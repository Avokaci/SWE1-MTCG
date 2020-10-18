using System;
using System.Collections.Generic;
using System.Text;

namespace MonsterTradingCardGame.Lib.Monsters
{
    public class Wizzard : MonsterCard
    {
        //instances

        //constructors
        public Wizzard(ElementType element, string name, int damage) : base(element, name, damage)
        {
            element = ElementType.water;
            name = "Gandalf";
            damage = 70;
        }
        //access modifiers

        //methods

    }
}
