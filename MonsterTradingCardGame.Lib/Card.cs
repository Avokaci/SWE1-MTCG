using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

namespace MonsterTradingCardGame.Lib
{
    
    public class Card
    {
        //instances
        private string id;
        protected string name;
        protected ElementType _element;
        protected double damage;

        //constructors
        public Card()
        {
            
        }

        //access modifiers
        public ElementType Element { get => _element; set => _element = value; }
        public string Name { get => name; set => name = value; }
        public double Damage { get => damage; set { if (value >= 0 && value <= 100) { damage = value; } } }

        public string Id { get => id; set => id = value; }

   



       



    }
}
