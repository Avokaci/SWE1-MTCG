using MonsterTradingCardGame.Lib;
using NUnit.Framework;

namespace MonsterTradingCardGame.Testing
{
    public class CardTest
    {
        private Card _card;

        [SetUp]
        public void Setup()
        {
            //arrange
            _card = new Card();
        }

        [Test]
        public void Card_IsNotNull()
        {
            //assert
            Assert.IsNotNull(_card);
            Assert.That(_card, Is.Not.Null);
        }
        [Test]
        public void Card_IsCorrectElementType()
        {
            //assert
            Assert.AreEqual(ElementType.fire, _card.Element);
        }

     
    }
}