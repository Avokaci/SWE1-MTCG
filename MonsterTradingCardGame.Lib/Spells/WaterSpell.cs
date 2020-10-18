using System;
using System.Collections.Generic;
using System.Text;

namespace MonsterTradingCardGame.Lib.Spells
{
    public class WaterSpell : SpellCard
    {
        public WaterSpell(ElementType element, string name, int damage) : base(element, name, damage)
        {
            element = ElementType.water;
            name = "Splash";
            damage = 50;
        }
    }
}
