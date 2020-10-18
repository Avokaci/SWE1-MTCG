using MonsterTradingCardGame.Lib;
using NUnit.Framework;

namespace MonsterTradingCardGame.Testing
{
    public class MonsterCardTest
    {
        private MonsterCard _monsterCard;

        [SetUp]
        public void Setup()
        {
            //arrange
            _monsterCard = new MonsterCard();
        }

        [Test]
        public void MonsterCard_IsNotNull()
        {
            //assert
            Assert.IsNotNull(_monsterCard);
            Assert.That(_monsterCard, Is.Not.Null);
        }
        [Test]
        public void MonsterCard_IsCorrectElementType()
        {
            //assert
            Assert.AreEqual(ElementType.fire, _monsterCard.Element);
        }
 
        [Test] 
        public void MonsterCard_DamageIsInRange()
        {
            //assert
            Assert.That(_monsterCard.Damage,Is.InRange(0,100) );
        }
    }
}