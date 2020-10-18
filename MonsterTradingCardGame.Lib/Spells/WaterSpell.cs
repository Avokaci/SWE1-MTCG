using System;
using System.Collections.Generic;
using System.Text;

namespace MonsterTradingCardGame.Lib.Spells
{
    public class WaterSpell : SpellCard
    {
        public WaterSpell()
        {
            Element = ElementType.water;
            name = "Splash";
            damage = 55;
        }
    }
}
