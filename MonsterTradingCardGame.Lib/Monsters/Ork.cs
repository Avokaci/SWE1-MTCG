using System;
using System.Collections.Generic;
using System.Text;

namespace MonsterTradingCardGame.Lib.Monsters
{
    public class Ork : MonsterCard
    {
        //instances

        //constructors
        public Ork(ElementType element, string name, int damage) : base(element, name, damage)
        {
            element = ElementType.normal;
            name = "Garrosh Hellscream";
            damage = 80;
        }
        //access modifiers

        //methods

    }
}
