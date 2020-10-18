using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MonsterTradingCardGame.Lib
{
    public class Card
    {
        //instances
        static protected string name;
        protected ElementType _element;
        static protected int damage;

        //constructors
        public Card()
        {
            
        }

        //access modifiers
        public ElementType Element { get => _element; set => _element = value; }
        public string Name { get => name; set => name = value; }
        public int Damage { get => damage; set { if (value >= 0 && value <= 100) { damage = value; } } }



        //methods



    }
}
