using MonsterTradingCardGame.Lib;
using MTCG.Lib;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTCG.Testing
{
    public class PackageTest
    {
        [Test]
        public void PackageInitializedCorrectly()
        {
            Card card1 = new Card();
            card1.Id = "845f0dc7-37d0-426e-994e-43fc3ac83c08";
            card1.Name = "WaterGoblin";
            card1.Damage = 10;
            Card card2 = new Card();
            card2.Id = "99f8f8dc-e25e-4a95-aa2c-782823f36e2a";
            card2.Name = "Dragon";
            card2.Damage = 50;
            Card card3 = new Card();
            card3.Id = "e85e3976-7c86-4d06-9a80-641c2019a79f";
            card3.Name = "WaterSpell";
            card3.Damage = 20;
            Card card4 = new Card();
            card4.Id = "1cb6ab86-bdb2-47e5-b6e4-68c5ab389334";
            card4.Name = "Ork";
            card4.Damage = 45;
            Card card5 = new Card();
            card5.Id = "dfdd758f-649c-40f9-ba3a-8657f4b3439f";
            card5.Name = "\"FireSpell";
            card5.Damage = 25;

            package pack = new package(card1,card2,card3,card4,card5);

            Assert.AreEqual(pack.Card1.Id, "845f0dc7-37d0-426e-994e-43fc3ac83c08");
            Assert.AreEqual(pack.Card2.Damage, 50);
            Assert.AreEqual(pack.Card3.Name, "WaterSpell");
            Assert.AreEqual(pack.Card4.Name, "Ork");
            Assert.AreEqual(pack.Card5.Damage, 25);
        }
    }
}
