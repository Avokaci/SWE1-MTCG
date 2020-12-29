using System;
using System.Collections.Generic;
using System.Text;

namespace MonsterTradingCardGame.Lib
{
    public class Deck
    {
        List<Card> deck;
        public Deck()
        {
            deck = new List<Card>();
        }

        public List<Card> deckk { get => deck; set => deck = value; }
    }
}
