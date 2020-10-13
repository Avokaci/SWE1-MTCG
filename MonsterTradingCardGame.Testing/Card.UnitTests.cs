using MonsterTradingCardGame.Lib;
using NUnit.Framework;

namespace MonsterTradingCardGame.Testing
{
    public class CardUnitTest
    {
        private Card _card;

        [SetUp]
        public void Setup()
        {
            _card = new Card(ElementType.fire, "Fire boi");
        }

        [Test]
        public void Card_IsNotNull()
        {
            Assert.IsNotNull(_card);
            Assert.That(_card, Is.Not.Null);
        }
        [Test]
        public void Card_IsCorrectElementType()
        {

            Assert.AreEqual(ElementType.fire, _card.Element);
        }
        [Test] //doesn't work
        public void Card_HasAlphabeticNameIncudingCaseAndSpace()
        {

            StringAssert.IsMatch(_card.Name, "^[a-zA-Z ]*$");
        }
    }
}