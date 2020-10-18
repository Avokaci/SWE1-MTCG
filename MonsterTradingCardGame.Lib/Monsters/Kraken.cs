using System;
using System.Collections.Generic;
using System.Text;

namespace MonsterTradingCardGame.Lib.Monsters
{
    public class Kraken : MonsterCard
    {
        //instances

        //constructors
        public Kraken(ElementType element, string name, int damage) : base(element, name, damage)
        {
            element = ElementType.water;
            name = "Cthulhu";
            damage = 100;
        }
        //access modifiers

        //methods

    }
}
