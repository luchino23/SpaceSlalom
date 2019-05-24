using System;

namespace GameServerExample2B
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            GameTransportIPv4 transport = new GameTransportIPv4();
            transport.Bind("192.168.3.115", 9999);

            GameServer server = new GameServer(transport,null);

           

            server.Run();
        }


    }
}
