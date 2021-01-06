using MTCG.Lib;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace MTCG.Testing
{
    public class TradeDealsTest
    {
        [Test]
        public void Deal_JSONProperlyInitialized()
        {
            string jsonstring = "{\"Id\": \"6cd85277-4590-49d4-b0cf-ba0a921faad0\", \"CardToTrade\": \"1cb6ab86-bdb2-47e5-b6e4-68c5ab389334\", \"Type\": \"monster\", \"MinimumDamage\": 15}";

            TradeDeal trade = JsonSerializer.Deserialize<TradeDeal>(jsonstring);

            Assert.AreEqual("6cd85277-4590-49d4-b0cf-ba0a921faad0", trade.Id);
            Assert.AreEqual("1cb6ab86-bdb2-47e5-b6e4-68c5ab389334", trade.CardToTrade);
            Assert.AreEqual("monster", trade.Type);
            Assert.AreEqual(15, trade.MinimumDamage);
        }
    }
}
