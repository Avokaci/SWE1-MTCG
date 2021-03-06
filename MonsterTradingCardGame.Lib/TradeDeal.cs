﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MTCG.Lib
{
    public class TradeDeal
    {
        string _id;
        string _cardToTrade;
        string _cardToTradeName;
        int _cardToTradeDmg;
        string _type;
        int _minimumDamage;

        public TradeDeal()
        {
          
        }

        public string Id { get => _id; set => _id = value; }
        public string CardToTrade { get => _cardToTrade; set => _cardToTrade = value; }
        public string CardToTradeName { get => _cardToTradeName; set => _cardToTradeName = value; }
        public int CardToTradeDmg { get => _cardToTradeDmg; set => _cardToTradeDmg = value; }
        public string Type { get => _type; set => _type = value; }
        public int MinimumDamage { get => _minimumDamage; set => _minimumDamage = value; }
    }
}
