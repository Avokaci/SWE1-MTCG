using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MonsterTradingCardGame.Lib
{
    public class Card
    {
        //instances
        private string name;
        protected ElementType _element;
        private int damage;

        //constructors
        public Card(ElementType element, string name, int damage)
        {
            this.Name = name;
            this._element = element;
            this.Damage = damage;
        }

        //access modifiers
        public ElementType Element { get => _element; set => _element = value; }
        public string Name { get => name; set => name = value; }
        public int Damage { get => damage; set { if (value >= 0 && value <= 100) { damage = value; } } }



        //methods



    }
}
