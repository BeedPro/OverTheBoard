﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OverTheBoard.Data.Entities;
using OverTheBoard.ObjectModel;

namespace OverTheBoard.WebUI.Models
{
    public class BracketsViewModel
    {
        public List<GamePlayerStatsModel> StatModels { get; set; }
        public Dictionary<int, BracketsRoundItemModel> Items { get; set; }
    }

    public class BracketsRoundItemModel
    {
        public List<BracketsGameModel> Games { get; set; }
    }

    public class BracketsGameModel
    {
        public string Identifier { get; set; }
        public List<BracketsPlayerModel>  Players  { get; set; }
        public GameStatus Status { get; set; }
        public bool CanPlay { get; set; }
        public string StartDate { get; set; }
        public string StartAt { get; set; }
    }
    
    public class BracketsPlayerModel
    {
        public string DisplayName { get; set; }
        public bool IsSelf { get; set; }
        public string Outcome { get; set; }
        public string Colour { get; set; }
    }

    public class GamePlayerStatsModel
    {
        public string DisplayName { get; set; }
        public int Matches { get; set; }
        public int TotalMatches { get; set; }
        public int Win { get; set; }
        public int Lose { get; set; }
        public int Draw { get; set; }
        public decimal Point { get; set; }
    }

}
