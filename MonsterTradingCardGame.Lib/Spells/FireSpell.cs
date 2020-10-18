using System;
using System.Collections.Generic;
using System.Text;

namespace MonsterTradingCardGame.Lib.Spells
{
    public class FireSpell : SpellCard
    {
        public FireSpell()
        {
            Element = ElementType.fire;
            name = "Inferno";
            damage = 70; 

        }
    }
}
