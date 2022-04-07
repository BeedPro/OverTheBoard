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
using OverTheBoard.Infrastructure.Queueing;
using OverTheBoard.ObjectModel;
using OverTheBoard.ObjectModel.Queues;

namespace OverTheBoard.Infrastructure.Services
{
    public class GameService : IGameService
    {
        private readonly IRepository<ChessGameEntity> _repositoryChessGame;
        private readonly IRepository<GamePlayerEntity> _repositoryGamePlayer;
        private readonly IUserService _userService;
        private readonly IEloService _eloService;
        public readonly UserManager<OverTheBoardUser> _userManager;
        private readonly IGameCompletionQueue _completionQueue;

        public GameService(
            IRepository<ChessGameEntity> repositoryChessGame, 
            IRepository<GamePlayerEntity> repositoryGamePlayer, 
            IUserService userService, 
            IEloService eloService, 
            UserManager<OverTheBoardUser> userManager,
            IGameCompletionQueue completionQueue)
        {
            _repositoryChessGame = repositoryChessGame;
            _repositoryGamePlayer = repositoryGamePlayer;
            _userService = userService;
            _eloService = eloService;
            _userManager = userManager;
            _completionQueue = completionQueue;
        }

        List<ChessGame> _chessGames = new List<ChessGame>();
        public async Task<bool> CreateGameAsync(string identifier, List<GameQueueItem> queueItems, DateTime startTime, int periodInMinutes, GameType type, string tournamentIdentifier)
        {
            ChessGameEntity game = new ChessGameEntity();
            game.Identifier = identifier.ToGuid();
            game.Players = new List<GamePlayerEntity>();
            game.StartTime = startTime;
            game.Period = periodInMinutes;
            game.Status = GameStatus.NotStarted;
            game.Type = type;
            
            if (!string.IsNullOrEmpty(tournamentIdentifier))
            {
                game.TournamentId = tournamentIdentifier.ToGuid();
            }
            
            foreach (var item in queueItems)
            {
               var player = new GamePlayerEntity()
                {
                    UserId = item.UserId.ToGuid(),
                    Colour = item.Colour,
                    ConnectionId = item.ConnectionId,
                    TimeRemaining = new TimeSpan(0, 0, periodInMinutes, 0),
                };
                game.Players.Add(player);
            }
            _repositoryChessGame.Context.Add(game);
            _repositoryChessGame.Save();
            return true;
        }


        public async Task<ChessGame> GetChessGameOnlyAsync(string gameId)
        {
            var gameEntity = GetGameEntity(gameId);

           return new ChessGame()
            {
                Identifier = gameEntity.Identifier.ToString(),
                Fen = gameEntity.Fen,
                LastMoveAt = gameEntity.LastMoveAt,
                NextMoveColour = gameEntity.NextMoveColour,
                Status = gameEntity.Status,
                Type = gameEntity.Type,
                StartTime = gameEntity.StartTime
            };
        }

        public async Task<ChessGame> GetChessGameWithPlayersAsync(string gameId)
        {
            var gameEntity = GetGameEntity(gameId);
            var game = PopulateChessGame(gameEntity);
            

            if (game.LastMoveAt.HasValue)
            {
                var player = game.Players.FirstOrDefault(e => e.Colour == game.NextMoveColour);
                player.TimeRemaining = player.TimeRemaining - (DateTime.Now - game.LastMoveAt.Value);
                var intTimeRemaining = Convert.ToInt32(player.TimeRemaining.TotalSeconds);
                if (intTimeRemaining <= 0)
                {
                    player.TimeRemaining = new TimeSpan(0, 0, 0);
                }
            }
            return game;
        }

        public async Task<bool> UpdateConnectionAsync(string userId, string gameId, string connectionId)
        {
            var userGuid = userId.ToGuid();
            var gameGuid = gameId.ToGuid();
            var entity = _repositoryGamePlayer.Query()
                .FirstOrDefault(e => e.UserId == userGuid && e.Game.Identifier == gameGuid);

            if (entity != null)
            {
                entity.ConnectionId = connectionId;
                entity.LastConnectedTime = DateTime.Now;
                _repositoryGamePlayer.Save();
            }
           
            return true;
        } 
        
        public async Task<bool> UpdateStatusAsync(string gameId, GameStatus status)
        {
            var gameGuid = gameId.ToGuid();
            var entity = _repositoryChessGame.Query()
                .FirstOrDefault(e => e.Identifier == gameGuid);

            if (entity != null)
            {
                entity.Status = status;
                _repositoryGamePlayer.Save();
            }
           
            return true;
        }
        
       


 //public async Task<bool> UpdateConnectionAsync(string userId, string gameId, string connectionId)
 //       {
 //           var gameEntity = GetGameEntity(gameId);

 //           if (!gameEntity.LastMoveAt.HasValue)
 //           {
 //               gameEntity.LastMoveAt = DateTime.Now;
 //               gameEntity.NextMoveColour = "white";
 //           }

 //           var player = gameEntity.Players.FirstOrDefault(e => e.UserId == userId.ToGuid());
 //           if (player != null)
 //           {
 //               player.ConnectionId = connectionId;
 //           }

 //           _repositoryGamePlayer.Save();
 //           return true;
 //       }



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
            var gameInProgressInfo = gameInProgress.Select(entity => PopulateChessGame(entity)).ToList();
            return gameInProgressInfo;
        }

        

        public async Task<bool> SaveGameOutcomeAsync(string gameId, EloOutcomesType whitePlayerOutcome, EloOutcomesType blackPlayerOutcome)
        {
            var gameEntity = GetGameEntity(gameId);
            if (gameEntity.Status != GameStatus.Completed)
            {
                var whitePlayer = gameEntity.Players.FirstOrDefault(f => f.Colour == "white");
                var blackPlayer = gameEntity.Players.FirstOrDefault(f => f.Colour == "black");
                
                var whiteUser = await _userService.GetUserAsync(whitePlayer?.UserId.ToString());
                var blackUser = await _userService.GetUserAsync(blackPlayer?.UserId.ToString());

                var newRatings = await _eloService.CalculateEloAsync(whiteUser.Rating, blackUser.Rating,whitePlayerOutcome, blackPlayerOutcome);
                //TODO change the indexing to properties
                //newRating index 0 is whitePlayers and newRating index 1 is blackPlayers
                whiteUser.Rating = newRatings.WhitePlayerRating;
                blackUser.Rating = newRatings.BlackPlayerRating;
                
                gameEntity.Status = GameStatus.Completed;
                whitePlayer.Outcome = whitePlayerOutcome.ToString();
                blackPlayer.Outcome = blackPlayerOutcome.ToString();


                await _userManager.UpdateAsync(whiteUser);
                await _userManager.UpdateAsync(blackUser);
                
                _repositoryChessGame.Save();

                await _completionQueue.AddQueueAsync(new GameCompletionQueueItem()
                {
                    GameId = gameId,
                    Level = gameEntity.Level
                });
            }
            
            return true;
        }

        public async Task<List<ChessGame>> GetMatchesByTournamentAsync(string tournamentIdentifier)
        {
            var gameInProgress = _repositoryChessGame.Query()
                .Include(i => i.Players)
                .Where(e => e.TournamentId.HasValue && 
                    e.TournamentId.Value == tournamentIdentifier.ToGuid());

            var gameInProgressInfo = gameInProgress.Select(entity => PopulateChessGame(entity)).ToList();

            return gameInProgressInfo;
        }

        public async Task<List<ChessGame>> GetMatchesByTournamentAndUserAsync(string userId, string tournamentIdentifier)
        {
            var gameInProgress = _repositoryChessGame.Query()
                .Include(i => i.Players)
                .Where(e =>
                    e.Players.Any(f => f.UserId == userId.ToGuid()) &&
                    e.TournamentId.HasValue &&
                    e.TournamentId.Value == tournamentIdentifier.ToGuid());

            var gameInProgressInfo = gameInProgress.Select(entity => PopulateChessGame(entity)).ToList();

            return gameInProgressInfo;
        }

        public async Task<Dictionary<string, ChessMove>> InitialiseChessGameAsync(string gameId, string userId)
        {
            Dictionary<string, ChessMove> chessMoves = new Dictionary<string, ChessMove>();
            var chessGame = await GetChessGameWithPlayersAsync(gameId);
            if (chessGame != null)
            {
                var whitePlayer = chessGame.Players.FirstOrDefault(e => e.Colour == "white");
                var blackPlayer = chessGame.Players.FirstOrDefault(e => e.Colour == "black");

                await UpdateStatusAsync(gameId, GameStatus.InProgress);
                
                //if (!string.IsNullOrEmpty(whitePlayer?.ConnectionId) &&
                //    !string.IsNullOrEmpty(blackPlayer?.ConnectionId))
                //{}
                
                int whiteTimer = Convert.ToInt32(whitePlayer?.TimeRemaining.TotalSeconds) * 10;
                var blackTimer = Convert.ToInt32(blackPlayer?.TimeRemaining.TotalSeconds) * 10;
                
                foreach (var player in chessGame.Players)
                {
                    var chessMove = new ChessMove() { Orientation = player.Colour };
                    if (!string.IsNullOrEmpty(userId) && player.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase))
                    {
                        chessMove.Fen = chessGame.Fen;
                    }
                    chessMove.whiteRemaining = whiteTimer;
                    chessMove.blackRemaining = blackTimer;
                    chessMoves.Add(player.ConnectionId, chessMove);
                }
            }

            return chessMoves;
        }


        private string GetDisplayNameById(string userId)
        {
            var user = _userService.GetUserAsync(userId).GetAwaiter().GetResult();
            return user?.DisplayName ?? "No user";
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

        private static ChessGame PopulateChessGame(ChessGameEntity e)
        {
            return new ChessGame()
            {
                Identifier = e.Identifier.ToString(),
                Fen = e.Fen,
                LastMoveAt = e.LastMoveAt,
                NextMoveColour = e.NextMoveColour,
                Players = e.Players.Select(p => new GamePlayer()
                {
                    UserId = p.UserId.ToString(),
                    ConnectionId = p.ConnectionId,
                    LastConnectedTime = p.LastConnectedTime,
                    Colour = p.Colour,
                    TimeRemaining = p.TimeRemaining,
                    Outcome = p.Outcome
                }).ToList(),
                Status = e.Status,
                Type = e.Type,
                StartTime = e.StartTime
            };
        }
    }
}
