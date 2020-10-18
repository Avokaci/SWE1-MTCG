using System;
using System.Collections.Generic;
using System.Text;

namespace MonsterTradingCardGame.Lib.Monsters
{
    public class FireElve : MonsterCard
    {
        //instances

        //constructors
        public FireElve(ElementType element, string name, int damage) : base(element, name, damage)
        {
            element = ElementType.fire;
            name = "Ifrit";
            damage = 60;

        }
        //access modifiers

        //methods

    }
}
