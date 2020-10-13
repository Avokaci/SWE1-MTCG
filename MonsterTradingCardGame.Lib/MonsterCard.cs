using System;
using System.Collections.Generic;
using System.Text;

namespace MonsterTradingCardGame.Lib
{
    public class MonsterCard:Card
    {
        //instances
        protected int _damage;
   

        //constructors
        public MonsterCard(ElementType element, string name, int damage) : base(element, name)
        {
          
            _damage = damage;
           
        }

        //access modifiers
        public int Damage { get => _damage; set { if (value >= 0 && value <= 100) { _damage = value; } } }
        //methods

        

      
    }
}
