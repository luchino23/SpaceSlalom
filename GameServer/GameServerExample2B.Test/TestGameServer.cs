using System;
using NUnit.Framework;

namespace GameServerExample2B.Test
{
    public class TestGameServer
    {
        private FakeTransport transport;
        private FakeClock clock;
        private GameServer server;

        [SetUp]
        public void SetupTests()  //chiama il blocco di codice prima di ogni test per resettare la classe (torna ad uno stato piu pulito) il costruttore verrebbe chiamato una sola volta all inizio di tutto e non per ogni test
        {
            transport = new FakeTransport();
            clock = new FakeClock();
            server = new GameServer(transport, clock);
        }

        [Test]
        public void TestZeroNow()
        {
            Assert.That(server.Now, Is.EqualTo(0));
        }

        [Test]
        public void TestClientsOnStart()
        {
            Assert.That(server.NumClients, Is.EqualTo(0));
        }

        [Test]
        public void TestGameObjectsOnStart()
        {
            Assert.That(server.NumGameObjects, Is.EqualTo(0));
        }

        [Test]
        public void TestJoin()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet,"tester",0);
            server.SingleStep(); //controlliamo se il server sia attivo
            Assert.That(server.NumClients, Is.EqualTo(1));
        }

        [Test]
        public void TestJoinNumOfGameObject()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep(); //controlliamo se il server sia attivo
            Assert.That(server.NumGameObjects, Is.EqualTo(1));
        }

        [Test]
        public void TestWelcomeAfterJoin()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep(); //controlliamo se il server sia attivo
            FakeData welcome = transport.ClientDequeue();
            Assert.That(welcome.data[0], Is.EqualTo(1));
        }

        [Test]
        public void TestSpawnAvatarAfterJoin()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep(); //controlliamo se il server sia attivo
            transport.ClientDequeue();
            Assert.That(() => transport.ClientDequeue(), Throws.InstanceOf<FakeQueueEmpty>());
        }

        [Test]
        public void TestJoinSameClient()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep(); //controlliamo se il server sia attivo
            Assert.That(server.NumClients,Is.EqualTo(1));
        }

        [Test]
        public void TestJoinSameAddressClient()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            transport.ClientEnqueue(packet, "tester", 1);
            server.SingleStep(); //controlliamo se il server sia attivo
            Assert.That(server.NumClients, Is.EqualTo(2));
        }

        [Test]
        public void TestJoinSameAddressAvatars()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            transport.ClientEnqueue(packet, "tester", 1);
            server.SingleStep(); //controlliamo se il server sia attivo
            Assert.That(server.NumGameObjects, Is.EqualTo(2));
        }

        [Test]
        public void TestJoinTwoClientsSamePort()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            transport.ClientEnqueue(packet, "foobar", 1);
            server.SingleStep(); //controlliamo se il server sia attivo
            Assert.That(server.NumClients, Is.EqualTo(2));
        }

        [Test]
        public void TestJoinTwoClientsWelcome()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            transport.ClientEnqueue(packet, "foobar", 1);
            server.SingleStep(); //controlliamo se il server sia attivo

            Assert.That(transport.ClientQueueCount, Is.EqualTo(5));
            Assert.That(transport.ClientDequeue().endPoint.Address, Is.EqualTo("tester"));
            Assert.That(transport.ClientDequeue().endPoint.Address, Is.EqualTo("tester"));
            Assert.That(transport.ClientDequeue().endPoint.Address, Is.EqualTo("tester"));
            Assert.That(transport.ClientDequeue().endPoint.Address, Is.EqualTo("foobar"));
            Assert.That(transport.ClientDequeue().endPoint.Address, Is.EqualTo("foobar"));

        }

        //[Test]
        //public void TestEvilUpdate()
        //{
        //    Packet packet = new Packet(0);
        //    transport.ClientEnqueue(packet, "tester", 0);
        //    server.SingleStep();
        //    transport.ClientEnqueue(packet, "foobar", 1);
        //    server.SingleStep();

        //    Packet move = new Packet(3, 9999u, 1.0f, 1.0f, 2.0f);
        //    //TODO get the id from the welcome packets
        //    //try to move the id from the other player
        //}

    }
}
