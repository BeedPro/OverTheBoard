﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OverTheBoard.Data.Entities.Applications;
using OverTheBoard.Data.Repositories;
using OverTheBoard.Infrastructure.Extensions;
using OverTheBoard.ObjectModel;

namespace OverTheBoard.Infrastructure.Queueing
{
    public class GameService : IGameService
    {
        private readonly IRepository<ChessGameEntity> _repositoryChessGame;
        private readonly IRepository<GamePlayerEntity> _repositoryGamePlayer;

        public GameService(IRepository<ChessGameEntity> repositoryChessGame, IRepository<GamePlayerEntity> repositoryGamePlayer)
        {
            _repositoryChessGame = repositoryChessGame;
            _repositoryGamePlayer = repositoryGamePlayer;
        }

        List<ChessGame> _chessGames = new List<ChessGame>();
        public async Task<bool> CreateGameAsync(string identifier, List<UnrankedGameQueueItem> queueItems, DateTime startTime, int periodInMinutes)
        {
            ChessGameEntity game = new ChessGameEntity();
            game.Identifier = identifier.ToGuid();
            game.Players = new List<GamePlayerEntity>();
            game.StartTime = startTime;
            game.Period = periodInMinutes;
            game.LastMoveAt = startTime;

            var colour = "white";
            foreach (var item in queueItems)
            {
                var player = new GamePlayerEntity()
                {
                    UserId = item.UserId.ToGuid(),
                    Colour = colour,
                    ConnectionId = item.ConnectionId,
                    TimeRemain = new TimeSpan(0,0,periodInMinutes, 0)
                };

                colour = "black";
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
                Fen = gameEntity.Fen
            };

            game.Players = gameEntity.Players.Select(e => new GamePlayer()
            {
                UserId = e.UserId.ToString(),
                ConnectionId = e.ConnectionId,
                Colour = e.Colour
            }).ToList();
            

            return game;
        }

        public async Task<bool> UpdateConnectionAsync(string userId, string gameId, string connectionId)
        {
            var gameEntity = GetGameEntity(gameId);

            var player = _repositoryGamePlayer.Query()
                .FirstOrDefault(e => e.GameId == gameEntity.GameId && e.UserId == userId.ToGuid());

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
            current.Pgn = $"{current.Pgn}#{move.Pgn}";
            current.TimeRemain = current.TimeRemain - (DateTime.Now - game.LastMoveAt);
            

            var opponent = game?.Players.FirstOrDefault(u => u.UserId != userId.ToGuid());

            game.LastMoveAt = DateTime.Now;
            _repositoryChessGame.Save();

            return opponent?.ConnectionId;
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

        public async Task<List<GameInfo>> GetGameByUserIdAsync(string userId)
        {
            var gamesInProgress = _repositoryChessGame.Query().Include(i => i.Players).Where(e => e.Players.Any(f => f.UserId == userId.ToGuid())).ToList();
            foreach(var game in gamesInProgress)
            {
                var whitePlayer = game.Players.Where(e => e.Colour == "white");
                var blackPlayer = game.Players.Where(e => e.Colour == "black");
            }
            

            var gamePlayerEntities = _repositoryGamePlayer.Query().Where(e => e.UserId == userId.ToGuid());
            var sss = gamePlayerEntities.Select(e => new GameInfo()
            {
                Identifier = e.Game.Identifier.ToString(),
                WhiteUser = null,
                BlackUser = null
            }).ToList();
            return sss;
        }
    }
}
