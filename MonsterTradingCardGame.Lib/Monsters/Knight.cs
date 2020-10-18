using System;
using System.Collections.Generic;
using System.Text;

namespace MonsterTradingCardGame.Lib.Monsters
{
    public class Knight : MonsterCard
    {
        //instances

        //constructors
        public Knight(ElementType element, string name, int damage) : base(element, name, damage)
        {
            element = ElementType.normal;
            name = "Igris";
            damage = 50;

        }
        //access modifiers

        //methods

    }
}
