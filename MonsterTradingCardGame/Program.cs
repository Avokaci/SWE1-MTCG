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
            Console.WriteLine(gb.Name);
            Console.WriteLine(dg.Name);

        }
    }
}
