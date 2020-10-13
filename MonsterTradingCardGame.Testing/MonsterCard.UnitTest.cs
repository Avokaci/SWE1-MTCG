using MonsterTradingCardGame.Lib;
using NUnit.Framework;

namespace MonsterTradingCardGame.Testing
{
    public class MonsterCardUnitTests
    {
        private MonsterCard _monsterCard;

        [SetUp]
        public void Setup()
        {
            _monsterCard = new MonsterCard(ElementType.fire, "Fire boi", 80);
        }

        [Test]
        public void MonsterCard_IsNotNull()
        {
            Assert.IsNotNull(_monsterCard);
            Assert.That(_monsterCard, Is.Not.Null);
        }
        [Test]
        public void MonsterCard_IsCorrectElementType()
        {

            Assert.AreEqual(ElementType.fire, _monsterCard.Element);
        }
        [Test] //doesn't work
        public void MonsterCard_HasAlphabeticNameIncudingCaseAndSpace()
        {

            StringAssert.IsMatch(_monsterCard.Name, "^[a-zA-Z ]*$");
        }
        [Test] 
        public void MonsterCard_DamageIsInRange()
        {

            Assert.That(_monsterCard.Damage,Is.InRange(0,100) );
        }
    }
}