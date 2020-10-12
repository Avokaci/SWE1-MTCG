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
            _damage = Damage;
        }

        //access modifiers
        public int Damage { get => _damage; set => _damage = value; }
        //methods

    }
}
