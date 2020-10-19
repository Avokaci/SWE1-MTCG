using MonsterTradingCardGame.Lib.Monsters;
using MonsterTradingCardGame.Lib.Spells;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace MonsterTradingCardGame.Lib
{
    public class Game
    {
      
       
        public Card battle(Card ownCard, Card enemyCard)
        {

            #region Special Rules
            //Goblin vs Dragon
            if ((ownCard is Goblin && enemyCard is Dragon) || (enemyCard is Goblin && ownCard is Dragon))
            {
                //dragon win               
                if(ownCard is Dragon)
                {return ownCard;}
                else
                {return enemyCard;}                
            }
            //Wizzard vs Ork
            else if((ownCard is Wizzard && enemyCard is Ork)||(enemyCard is Wizzard && ownCard is Ork) )
            {
                //wizard win
                if (ownCard is Wizzard)
                {return ownCard;}
                else
                {return enemyCard;}
            }
            //Knight vs Waterspell
            else if((ownCard is Knight && enemyCard is WaterSpell) || (enemyCard is Knight && ownCard is WaterSpell))
            {
                //waterspell win
                if (ownCard is WaterSpell)
                {return ownCard;}
                else
                {return enemyCard;}
            }
            //Kraken vs Spell
            else if((ownCard is Kraken && enemyCard is SpellCard) ||(enemyCard is Kraken && ownCard is SpellCard))
            {
                //kraken win
                if (ownCard is Kraken)
                {return ownCard;}
                else
                {return enemyCard;}
            }
            //FireElve vs Dragon
            else if((ownCard is FireElve && enemyCard is Dragon) || (enemyCard is FireElve && ownCard is Dragon))
            {
                //Fire Elve win
                if (ownCard is FireElve)
                {return ownCard;}
                else
                {return enemyCard;}
            }
            #endregion

            #region Normal rules
            #region Monster vs Monster
            if (ownCard is MonsterCard && enemyCard is MonsterCard)
            {
                //battle without element effectiveness
                if (ownCard.Damage > enemyCard.Damage)
                {return ownCard;}
                else
                {return enemyCard;}
            }
            #endregion

            #region Monster vs Spell
            else if ((ownCard is SpellCard && enemyCard is MonsterCard) || (ownCard is MonsterCard && enemyCard is SpellCard))
            {
                //spellcard calculate effectiveness
                if (ownCard is SpellCard)
                {
                    if (checkEffectiveness(ownCard, enemyCard))
                    {ownCard.Damage = ownCard.Damage * 2;}
                    else
                    {ownCard.Damage = (int)(ownCard.Damage * 0.5);}
                }
                else
                {
                    if (checkEffectiveness(enemyCard, ownCard))
                    {enemyCard.Damage = enemyCard.Damage * 2;}
                    else
                    {enemyCard.Damage = (int)(enemyCard.Damage * 0.5);}
                }

                //Damage Comparisson
                if (ownCard.Damage > enemyCard.Damage)
                {return ownCard;}
                else
                {return enemyCard;}
            }
            #endregion

            #region Spell vs Spell
            else if (ownCard is SpellCard && enemyCard is SpellCard)
            {
                //checkeffectiveness for both spells. Both cards are affected
                if (checkEffectiveness(ownCard, enemyCard))
                {ownCard.Damage = ownCard.Damage * 2;}
                else
                {ownCard.Damage = (int)(ownCard.Damage * 0.5);}

                if (checkEffectiveness(enemyCard, ownCard))
                {enemyCard.Damage = enemyCard.Damage * 2;}
                else
                {enemyCard.Damage = (int)(enemyCard.Damage * 0.5);}


                //Damage Comparisson
                if (ownCard.Damage > enemyCard.Damage)
                {return ownCard;}
                else
                {return enemyCard;}
            }
            #endregion
            #endregion

            //if none of the above apply --> enemycard win
            return enemyCard;
        }

        public bool checkEffectiveness(Card ownCard, Card enemyCard)
        {
            if(ownCard.Element == ElementType.water && enemyCard.Element == ElementType.fire)
            {
                return true;
            }
          
            if (ownCard.Element == ElementType.fire && enemyCard.Element == ElementType.normal)
            {
                return true;
            }
           
            if (ownCard.Element == ElementType.normal && enemyCard.Element == ElementType.water)
            {
                return true;
            }
         
            return false;
        }


       
        
    }
}
