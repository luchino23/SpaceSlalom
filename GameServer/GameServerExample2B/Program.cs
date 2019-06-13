using System;

namespace GameServerExample2B
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            GameTransportIPv4 transport = new GameTransportIPv4();
            
            transport.Bind("192.168.1.108", 9999);

            GameClock clock = new GameClock();

            GameServer server = new GameServer(transport,clock);

            server.Run();
        }


    }
}
