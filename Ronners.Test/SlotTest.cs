using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ronners.Bot.Services;
using System;
using System.Diagnostics;

namespace Ronners.Test
{
    [TestClass]
    public class SlotTest
    {
        [TestMethod]
        public void TestPayout()
        {
            SlotService ss = new SlotService(new Random());
            int x = 100000000;
            int y = x;
            int winnings = 0;
            int temp =0;
            while(x > 0)
            {
                x--;
                (temp,_)=ss.Play(1);
                winnings+=temp;
            }
            double RTP = (double)winnings/(double)y;
            Debug.WriteLine($"Winnings:{winnings}, Runs:{y}, RTP:{RTP}\n");
            Assert.IsTrue(RTP>=.99 && RTP <= 1.01);
        }

    }
}