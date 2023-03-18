using System;
using pjank.BossaAPI;
using pjank.BossaAPI.src;

namespace ConsoleAppTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Bossa.ConnectNOL3();
            Bossa.Disconnect();

            /*
            var bossa = new BossaWrapper();
            bossa.ConnectNOL3();

            var account = bossa.GetAccount("00-55-048082");
            var instrument = bossa.GetInstrument("TAURONPE");

            var order = instrument.Buy(1.9m, 1, new DateTime(2023, 3, 20));

            order.Cancel();

            bossa.Disconnect();*/
        }
    }
}
