using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using MonsterTradingCardGame.Lib;
using MonsterTradingCardGame.Lib.Monsters;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using MonsterTradingCardGame.Lib.Spells;

namespace MonsterTradingCardGame.Testing
{
    public class GameTest
    {

      
        [Test]
        public void TestMove_SpecialRuleDragonVSGoblinInstaWin()
        {
            //arrange
            var mockedA = new Mock<Goblin>();
            var mockedB = new Mock<Dragon>();
            var card = new Card();
            var game = new Game();

            //act
            card = game.battle(mockedA.Object, mockedB.Object);

            //assert
            Assert.AreEqual(card, mockedB.Object);
            

        }

        [Test]
        public void TestMove_SpecialRuleWizzardVSOrk()
        {
            //arrange
            var mockedA = new Mock<Wizzard>();
            var mockedB = new Mock<Ork>();
            var card = new Card();
            var game = new Game();

            //act
            card = game.battle(mockedA.Object, mockedB.Object);

            //assert
            Assert.AreEqual(card, mockedA.Object);
        }

        [Test]
        public void TestMove_SpecialRuleKnightVSWaterSpell()
        {
            //arrange
            var mockedA = new Mock<Knight>();
            var mockedB = new Mock<WaterSpell>();
            var card = new Card();
            var game = new Game();

            //act
            card = game.battle(mockedA.Object, mockedB.Object);

            //assert
            Assert.AreEqual(card, mockedB.Object);
        }

        [Test]
        public void TestMove_SpecialRuleKrakenVSSpell()
        {
            //arrange
            var mockedA = new Mock<Kraken>();
            var mockedB = new Mock<SpellCard>();
            var card = new Card();
            var game = new Game();

            //act
            card = game.battle(mockedA.Object, mockedB.Object);

            //assert
            Assert.AreEqual(card, mockedA.Object);
        }
        [Test]
        public void TestMove_SpecialRuleFireElveVSDragon()
        {
            //arrange
            var mockedA = new Mock<FireElve>();
            var mockedB = new Mock<Dragon>();
            var card = new Card();
            var game = new Game();

            //act
            card = game.battle(mockedA.Object, mockedB.Object);

            //assert
            Assert.AreEqual(card, mockedA.Object);
        }

        [Test]
        public void TestMove_NormalRuleMonsterVSMonster()
        {
            //arrange
            var mockedA = new Mock<Ork>();
            var mockedB = new Mock<Knight>();
            var card = new Card();
            var game = new Game();

            //act
            card = game.battle(mockedA.Object, mockedB.Object);

            //assert
            Assert.AreEqual(card, mockedA.Object);
        }
        [Test]
        public void TestMove_NormalRuleMonsterVSSpell()
        {
            //arrange
            var mockedA = new Mock<Dragon>();
            var mockedB = new Mock<WaterSpell>();
            var card = new Card();
            var game = new Game();

            //act
            card = game.battle(mockedA.Object, mockedB.Object);

            //assert
            Assert.AreEqual(card, mockedA.Object);
        }
        [Test]
        public void TestMove_NormalRuleSpellVSSpell()
        {
            //arrange
            var mockedA = new Mock<FireSpell>();
            var mockedB = new Mock<WaterSpell>();
            var card = new Card();
            var game = new Game();

            //act
            card = game.battle(mockedA.Object, mockedB.Object);

            //assert
            Assert.AreEqual(card, mockedB.Object);
        }
        [Test]
        public void TestMove_CheckEffectiveness()
        {
            //arrange
            var mockedA = new Mock<FireSpell>();
            var mockedB = new Mock<NormalSpell>();
            bool check;
            var game = new Game();

            //act
            check = game.checkEffectiveness(mockedA.Object, mockedB.Object);

            //assert
            Assert.That(check);
        }
    }
}
