using System;
using System.Collections.Generic;
using System.Text;

namespace MonsterTradingCardGame.Lib.Monsters
{
    public class Goblin : MonsterCard
    {
        //instances

        //constructors
        public Goblin(ElementType element, string name, int damage) : base(element, name, damage)
        {
            element = ElementType.normal;
            name = "Goburou";
            damage = 40;
        }
        //access modifiers

        //methods

    }
}
