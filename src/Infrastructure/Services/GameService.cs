﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OverTheBoard.Data.Entities;
using OverTheBoard.Data.Entities.Applications;
using OverTheBoard.Data.Repositories;
using OverTheBoard.Infrastructure.Extensions;
using OverTheBoard.ObjectModel;

namespace OverTheBoard.Infrastructure.Services
{
    public class GameService : IGameService
    {
        private readonly IRepository<ChessGameEntity> _repositoryChessGame;
        private readonly IRepository<GamePlayerEntity> _repositoryGamePlayer;
        private readonly IUserService _userService;
        private readonly IEloService _eloService;
        public readonly UserManager<OverTheBoardUser> _userManager;

        public GameService(IRepository<ChessGameEntity> repositoryChessGame, IRepository<GamePlayerEntity> repositoryGamePlayer, IUserService userService, IEloService eloService, UserManager<OverTheBoardUser> userManager)
        {
            _repositoryChessGame = repositoryChessGame;
            _repositoryGamePlayer = repositoryGamePlayer;
            _userService = userService;
            _eloService = eloService;
            _userManager = userManager;
        }

        List<ChessGame> _chessGames = new List<ChessGame>();
        public async Task<bool> CreateGameAsync(string identifier, List<UnrankedGameQueueItem> queueItems, DateTime startTime, int periodInMinutes)
        {
            ChessGameEntity game = new ChessGameEntity();
            game.Identifier = identifier.ToGuid();
            game.Players = new List<GamePlayerEntity>();
            game.StartTime = startTime;
            game.Period = periodInMinutes;
            game.Status = GameStatus.InProgress;
            List<string> colours = new List<string> { "white", "black" };
            var colour = "";
            foreach (var item in queueItems)
            {
                Random rand = new Random();
                int index = rand.Next(colours.Count);
                colour = colours[index];
                var player = new GamePlayerEntity()
                {
                    UserId = item.UserId.ToGuid(),
                    Colour = colour,
                    ConnectionId = item.ConnectionId,
                    TimeRemaining = new TimeSpan(0, 0, periodInMinutes, 0),
                };
                colours.Remove(colour);
                game.Players.Add(player);
            }
            _repositoryChessGame.Context.Add(game);
            _repositoryChessGame.Save();
            return true;
        }


        public async Task<ChessGame> GetPlayersAsync(string gameId)
        {
            var id = gameId.ToGuid();
            var gameEntity = GetGameEntity(gameId);

            var game = new ChessGame()
            {
                Identifier = gameEntity.Identifier.ToString(),
                Fen = gameEntity.Fen,
                LastMoveAt = gameEntity.LastMoveAt,
                NextMoveColour = gameEntity.NextMoveColour,
                Status = gameEntity.Status
            };

            game.Players = gameEntity.Players.Select(e => new GamePlayer()
            {
                UserId = e.UserId.ToString(),
                ConnectionId = e.ConnectionId,
                Colour = e.Colour,
                TimeRemaining = e.TimeRemaining,
            }).ToList();

            if (game.LastMoveAt.HasValue)
            {
                var player = game.Players.FirstOrDefault(e => e.Colour == game.NextMoveColour);
                player.TimeRemaining = player.TimeRemaining - (DateTime.Now - game.LastMoveAt.Value);
                var intTimeRemaining = Convert.ToInt32(player.TimeRemaining.TotalSeconds);
                if (intTimeRemaining <= 0)
                {
                    player.TimeRemaining = new TimeSpan(0, 0, 0);
                    if (player.Colour == "white")
                    {
                        await SaveGameOutcomeAsync(gameId, GameStatus.Completed, EloOutcomesType.Lose, EloOutcomesType.Win);
                    }
                    else
                    {
                        await SaveGameOutcomeAsync(gameId, GameStatus.Completed, EloOutcomesType.Win,
                            EloOutcomesType.Lose);
                    }
                }
            }
            return game;
        }

        public async Task<bool> UpdateConnectionAsync(string userId, string gameId, string connectionId)
        {
            var gameEntity = GetGameEntity(gameId);

            if (!gameEntity.LastMoveAt.HasValue)
            {
                gameEntity.LastMoveAt = DateTime.Now;
                gameEntity.NextMoveColour = "white";
            }

            var player = gameEntity.Players.FirstOrDefault(e => e.UserId == userId.ToGuid());
            if (player != null)
            {
                player.ConnectionId = connectionId;
            }
            _repositoryGamePlayer.Save();
            return true;
        }



        public async Task<string> SaveGameMoveAsync(string userId, ChessMove move)
        {
            //var game = _chessGames.FirstOrDefault(e => e.Identifier.Equals(move.GameId, StringComparison.OrdinalIgnoreCase));
            var game = GetGameEntity(move.GameId);
            game.Fen = move.Fen;

            var current = game.Players.FirstOrDefault(u => u.UserId == userId.ToGuid());
            var opponent = game?.Players.FirstOrDefault(u => u.UserId != userId.ToGuid());

            current.Pgn = $"{current.Pgn}#{move.Pgn}";
            if (game.LastMoveAt.HasValue)
            {
                current.TimeRemaining = current.TimeRemaining - (DateTime.Now - game.LastMoveAt.Value);
            }
            game.LastMoveAt = DateTime.Now;
            game.NextMoveColour = opponent.Colour;

            _repositoryChessGame.Save();
            return opponent?.ConnectionId;
        }

        public async Task<List<GameInfo>> GetGameByUserIdAsync(string userId)
        {
            var games = _repositoryChessGame.Query().Include(i => i.Players).Where(e => e.Players.Any(f => f.UserId == userId.ToGuid())).ToList();
            //var gamesInProgress = _repositoryChessGame.Query().Include(i => i.Players).Where(e => e.Players.Any(f => f.UserId == userId.ToGuid())).ToList();
            var gamesInfo = games.Select(e => new GameInfo()
            {
                Identifier = e.Identifier.ToString(),
                WhiteUser = GetDisplayNameById(e.Players.FirstOrDefault(f => f.Colour == "white")?.UserId.ToString()),
                BlackUser = GetDisplayNameById(e.Players.FirstOrDefault(f => f.Colour == "black")?.UserId.ToString()),
                Status = e.Status
            }).ToList();
            return gamesInfo;
        }

        public async Task<List<ChessGame>> GetGamesInProgress()
        {
            var gameInProgress = _repositoryChessGame.Query().Include(i => i.Players).Where(e => e.Status == GameStatus.InProgress);
            var gameInProgressInfo = gameInProgress.Select(e => new ChessGame()
            {
                Identifier = e.Identifier.ToString(),
                Fen = e.Fen,
                LastMoveAt = e.LastMoveAt,
                NextMoveColour = e.NextMoveColour,
                Players = e.Players.Select(p => new GamePlayer()
                {
                    UserId = p.UserId.ToString(),
                    ConnectionId = p.ConnectionId,
                    Colour = p.Colour,
                    TimeRemaining = p.TimeRemaining,
                }).ToList(),
                Status = e.Status
            }).ToList();
            return gameInProgressInfo;
        }

        public async Task<bool> SaveGameOutcomeAsync(string gameId, GameStatus status, EloOutcomesType whitePlayerOutcome, EloOutcomesType blackPlayerOutcome)
        {
            var id = gameId.ToGuid();
            var gameEntity = GetGameEntity(gameId);
            if (gameEntity.Status != GameStatus.Completed)
            {
                var whiteUser =
                    await _userService.GetUserAsync(gameEntity.Players.FirstOrDefault(f => f.Colour == "white")?.UserId
                        .ToString());
                var blackUser =
                    await _userService.GetUserAsync(gameEntity.Players.FirstOrDefault(f => f.Colour == "black")?.UserId
                        .ToString());

                var newRatings = await _eloService.CalculateEloAsync(whiteUser.Rating, blackUser.Rating,
                    whitePlayerOutcome, blackPlayerOutcome);
                //TODO change the indexing to properties
                //newRating index 0 is whitePlayers and newRating index 1 is blackPlayers
                whiteUser.Rating = newRatings.WhitePlayerRating;
                blackUser.Rating = newRatings.BlackPlayerRating;
                gameEntity.Status = status;
                await _userManager.UpdateAsync(whiteUser);
                await _userManager.UpdateAsync(blackUser);
                _repositoryChessGame.Save();
            }
            
            return true;
        }


        private string GetDisplayNameById(string userId)
        {
            var user = _userService.GetUserAsync(userId).GetAwaiter().GetResult();
            return user.DisplayName;
        }
        private ChessGameEntity GetGameEntity(string gameId)
        {
            var id = gameId.ToGuid();
            var gameEntity = _repositoryChessGame
                .Query()
                .Include(i => i.Players)
                .FirstOrDefault(e => e.Identifier == id);
            return gameEntity;
        }
    }
}