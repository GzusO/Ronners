using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Ronners.Bot;
using Ronners.Bot.Models;
using Ronners.Bot.Services;

namespace Ronners.Test
{
    [TestClass]
    public class PoliticsTest
    {
        [TestMethod]
        public void TestHash()
        {
            
            var message ="In loving memory of Donovan \"Petals\" Shoefield, we've all shit our pants in our cars on our way to work"+312292317161586690;
            Assert.IsTrue(AdminService.sha1(message));
        }
        [TestMethod]
        public void TestHash2()
        {
            
            var message ="test\"test"+309487366794379265;
            Assert.IsTrue(AdminService.sha1(message));
        }
    }
}
