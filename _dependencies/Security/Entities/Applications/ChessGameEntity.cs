﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace OverTheBoard.Data.Entities.Applications
{
    [Table("Games")]
    public class ChessGameEntity
    {
        [Key]
        public int GameId { get; set; }
        public Guid Identifier { get; set; }
        public string Fen { get; set; }
        public DateTime StartTime { get; set; }
        public int Period { get; set; } //in minutes
        public DateTime? LastMoveAt { get; set; }
        [ForeignKey("GameId")]
        public ICollection<GamePlayerEntity> Players { get; set; }
        public string NextMoveColour { get; set; }
        public GameStatus Status { get; set; }
        public GameType Type { get; set; }
        public int Level { get; set; }
        public Guid? TournamentId { get; set; }
        public int RoundNumber { get; set; }
    }
}
