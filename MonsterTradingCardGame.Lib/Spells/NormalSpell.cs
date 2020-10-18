using System;
using System.Collections.Generic;
using System.Text;

namespace MonsterTradingCardGame.Lib.Spells
{
    public class NormalSpell : SpellCard
    {
        public NormalSpell()
        {
            Element = ElementType.normal;
            name = "Allmighty push";
            damage = 60;
        }
    }
}
