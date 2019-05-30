using System;

namespace GameServerExample2B
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            GameTransportIPv4 transport = new GameTransportIPv4();
            
            transport.Bind("127.0.0.1", 9999);

            GameClock clock = new GameClock();

            GameServer server = new GameServer(transport,clock);

           

            server.Run();
        }


    }
}
