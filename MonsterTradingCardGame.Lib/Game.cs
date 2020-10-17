using MonsterTradingCardGame.Lib.Monsters;
using MonsterTradingCardGame.Lib.Spells;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace MonsterTradingCardGame.Lib
{
    public class Game
    {
        bool attackerToken;
        public void battle(Card ownCard, Card enemyCard)
        {
            //Special Rules
            //Goblin vs Dragon
            if((ownCard is Goblin && enemyCard is Dragon) || (enemyCard is Goblin && ownCard is Dragon))
            {
                //dragon win 
            }
            //Wizzard vs Ork
            if((ownCard is Wizzard && enemyCard is Ork)||(enemyCard is Wizzard && ownCard is Ork) )
            { 
                //wizard win
            }
            //Knight vs Waterspell
            if((ownCard is Knight && enemyCard is WaterSpell) || (enemyCard is Knight && ownCard is WaterSpell))
            {
                //waterspell win
            }
            //Kraken vs Spell
            if((ownCard is Kraken && enemyCard is SpellCard) ||(enemyCard is Kraken && ownCard is SpellCard))
            {
                //kraken win
            }
            //FireElve vs Dragon
            if((ownCard is FireElve && enemyCard is Dragon) || (enemyCard is FireElve && ownCard is Dragon))
            {
                //Fire Elve win
            }

        }
        

    }
}
