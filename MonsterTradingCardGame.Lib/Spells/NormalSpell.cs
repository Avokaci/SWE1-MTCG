using System;
using System.Collections.Generic;
using System.Text;

namespace MonsterTradingCardGame.Lib.Spells
{
    public class NormalSpell : SpellCard
    {
        public NormalSpell(ElementType element, string name, int damage) : base(element, name, damage)
        {
            element = ElementType.normal;
            name = "Allmighty push";
            damage = 60;
        }
    }
}
