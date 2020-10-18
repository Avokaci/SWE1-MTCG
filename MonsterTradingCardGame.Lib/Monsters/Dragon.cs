﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MonsterTradingCardGame.Lib.Monsters
{
    public class Dragon : MonsterCard
    {

        //instances

        //constructors
        public Dragon(ElementType element, string name, int damage) : base(element, name, damage)
        {
            element = ElementType.fire;
            name = "Smaug";
            damage = 90;
        }
        //access modifiers

        //methods

    }
}
            