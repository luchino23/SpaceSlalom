using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace GameServerExample2B.Test
{
    public class FakeEndPoint : EndPoint
    {
        private string address;
        private int port;

        public string Address
        {
            get
            {
                return address;
            }
        }

        public int Port
        {
            get
            {
                return port;
            }
        }

        public FakeEndPoint()
        {

        }

        public FakeEndPoint(string address, int port)
        {
            this.address = address;
            this.port = port;
        }

        public override int GetHashCode()
        {
            return address.GetHashCode() + Port;
        }

        //public override AddressFamily AddressFamily => base.AddressFamily;

        public override EndPoint Create(SocketAddress socketAddress)
        {
            return base.Create(socketAddress);
        }

        public override bool Equals(object obj)
        {
            FakeEndPoint other = obj as FakeEndPoint;
            if (other == null)
                return false;

            return (this.address == other.address) && (this.port == other.port);
        }

    }
}
