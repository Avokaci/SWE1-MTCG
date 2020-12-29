using MonsterTradingCardGame.Lib;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTCG.Lib
{
    public class package
    {
        List<Card> pack;

        public package()
        {
            


        }

        public List<Card> Pack { get => pack; set => pack = value; }
    }
}
