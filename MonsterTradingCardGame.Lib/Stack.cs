using System;
using System.Collections.Generic;
using System.Text;

namespace MonsterTradingCardGame.Lib
{
    public class Stack
    {
        List<Card> deck;
        public Stack()
        {
            deck = new List<Card>();
        }

        public List<Card> Deck { get => deck; set => deck = value; }
    }
}
