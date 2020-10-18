using System;
using System.Collections.Generic;
using System.Text;

namespace MonsterTradingCardGame.Lib.Spells
{
    public class FireSpell : SpellCard
    {
        public FireSpell(ElementType element, string name, int damage) : base(element, name, damage)
        {
            element = ElementType.fire;
            name = "Inferno";
            damage = 70; 

        }
    }
}
