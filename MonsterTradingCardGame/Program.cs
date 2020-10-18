using MonsterTradingCardGame.Lib;
using MonsterTradingCardGame.Lib.Monsters;
using System;

namespace MonsterTradingCardGame
{
    class Program
    {
        static void Main(string[] args)
        {
            Goblin gb = new Goblin();       
            Dragon dg = new Dragon();
            Game spiel = new Game();
            if (spiel.battle(gb, dg) == gb)
            {
                Console.WriteLine("Goblin won");
            }
            else if(spiel.battle(gb, dg) == gb)
            {
                Console.WriteLine("Dragon won");
            }
            Console.WriteLine("hi");
        }
    }
}
