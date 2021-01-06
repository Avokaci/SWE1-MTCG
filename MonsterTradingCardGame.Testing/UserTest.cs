using MonsterTradingCardGame.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace MTCG.Testing
{
    public class UserTest
    {
        [Test]
        public void User_JSONProperlyInitialized()
        {
            string jsonstring = "{\"Username\":\"kienboec\", \"Password\":\"daniel\", \"Name\": \"Kienboeck\",  \"Bio\": \"me playin...\", \"Image\": \":-)\"}";

            User usr = JsonSerializer.Deserialize<User>(jsonstring);

            Assert.AreEqual("kienboec", usr.Username);
            Assert.AreEqual("daniel", usr.Password);
            Assert.AreEqual("Kienboeck", usr.Name);
            Assert.AreEqual("me playin...", usr.Bio);
            Assert.AreEqual(":-)", usr.Image);
            Assert.AreEqual(20, usr.Coins);
            Assert.AreEqual(100, usr.Elo);
            Assert.AreEqual(0, usr.Wins);
            Assert.AreEqual(0, usr.Losses);
            Assert.AreEqual(0, usr.PlayedGames);
        }

    }
}
