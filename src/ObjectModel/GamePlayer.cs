﻿using System;
using System.Collections.Generic;
using System.Security.AccessControl;

namespace OverTheBoard.ObjectModel
{
    public class GamePlayer : Player
    {
        public string ConnectionId { get; set; }
        public string Colour { get; set; }
        public TimeSpan TimeRemaining { get; set; }
        public List<int> GameOutcome { get; set; }
        public List<GamePlayerEloOutcomes> EloOutcomes { get; set; }
        public DateTime LastConnectedTime { get; set; }
        public string Outcome { get; set; }
    }
}