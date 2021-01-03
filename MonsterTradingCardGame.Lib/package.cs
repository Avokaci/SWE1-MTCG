using MonsterTradingCardGame.Lib;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTCG.Lib
{
    public class package
    {
        int packageId;
        Card _card1;
        Card _card2;
        Card _card3;
        Card _card4;
        Card _card5;
        public package(Card card1, Card card2, Card card3, Card card4, Card card5)
        {
            Card1 = card1;
            Card2 = card2;
            Card3 = card3;
            Card4 = card4;
            Card5 = card5;
        }

        public Card Card1 { get => _card1; set => _card1 = value; }
        public Card Card2 { get => _card2; set => _card2 = value; }
        public Card Card3 { get => _card3; set => _card3 = value; }
        public Card Card4 { get => _card4; set => _card4 = value; }
        public Card Card5 { get => _card5; set => _card5 = value; }
        public int PackageId { get => packageId; set => packageId = value; }
    }
}
