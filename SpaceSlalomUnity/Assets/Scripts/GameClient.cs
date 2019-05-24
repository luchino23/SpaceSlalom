using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

public class GameClient : MonoBehaviour
{
    [SerializeField]
    private string address;

    [SerializeField]
    private int port;

    [System.Serializable]
    struct NetPrefab
    {
        public uint Id;
        public GameObject Prefab;
    }

    [SerializeField]
    private NetPrefab[] netPrefabs;

    [SerializeField]
    private uint roomId;

    private Dictionary<uint, GameObject> netPrefabsCache;

    private Dictionary<uint, GameObject> netGameObjects;

    private delegate void GameCommand(byte[] data, EndPoint sender);

    private Dictionary<byte, GameCommand> commandsTable;

    private Socket socket;
    private IPEndPoint endPoint;

    private bool serverIsOnline;

    public bool ServerIsOnline
    {
        get
        {
            return serverIsOnline;
        }
    }

    float serverStatusTimer;
    float serverOfflineTImer;
    float defaultServerStatusTimer = 2.5f;  
    float defaultOfflineTimer = 3.0f;

    private uint myNetId;
    private GameObject myGameObject;

    void Awake()
    {
        netPrefabsCache = new Dictionary<uint, GameObject>();
        foreach (NetPrefab netPrefab in netPrefabs)
        {
            netPrefabsCache[netPrefab.Id] = netPrefab.Prefab;
        }
        netGameObjects = new Dictionary<uint, GameObject>();
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Blocking = false;
        endPoint = new IPEndPoint(IPAddress.Parse(address), port);
    }

    // Start is called before the first frame update
    void Start()
    {
        Packet join = new Packet(0);
        socket.SendTo(join.GetData(), endPoint);

        commandsTable = new Dictionary<byte, GameCommand>();

        //commandsTable[0] = Join;
        commandsTable[1] = Spawn;
        commandsTable[5] = JoinRoom;
        commandsTable[253] = StatusServer;

    }

    private void Spawn(byte[] data, EndPoint sender)
    {
        uint prefabType = BitConverter.ToUInt32(data, 1);
        uint netId = BitConverter.ToUInt32(data, 5);
        float x = BitConverter.ToSingle(data, 9);
        float y = BitConverter.ToSingle(data, 13);
        float z = BitConverter.ToSingle(data, 17);

        if (!netGameObjects.ContainsKey(netId) && netPrefabsCache.ContainsKey(prefabType))
        {
            GameObject prefab = netPrefabsCache[prefabType];
            GameObject newGameObject = Resources.Load("Player") as GameObject;
            Vector3 position;
            position.x = x;
            position.y = y;
            position.z = z;
            newGameObject.name = string.Format("NetObject {0}", netId);
            newGameObject.transform.position = position;
            netGameObjects[netId] = newGameObject;
        }
    }

    private void JoinRoom(byte[] data, EndPoint sender)
    {
        roomId = BitConverter.ToUInt32(data, 1);
    }

    private void StatusServer(byte[]data, EndPoint sender)
    {
        serverIsOnline = BitConverter.ToBoolean(data, 1);

        serverOfflineTImer = defaultOfflineTimer;
    }

    // Update is called once per frame
    void Update()
    {
        defaultOfflineTimer -= Time.deltaTime;
        defaultServerStatusTimer -= Time.deltaTime;

        if(serverStatusTimer < 0)
        {
            Packet serverStatusPacket = new Packet(253, serverIsOnline);
            socket.SendTo(serverStatusPacket.GetData(), endPoint);

            serverStatusTimer = defaultServerStatusTimer;
        }

        if (serverOfflineTImer < 0)
            serverIsOnline = false;

        if (myGameObject != null)
        {
            if (Input.GetKey(KeyCode.RightArrow))
            {
                myGameObject.transform.position += myGameObject.transform.right * 3 * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                myGameObject.transform.position += -myGameObject.transform.right * 3 * Time.deltaTime;
            }

            Vector3 myPosition = myGameObject.transform.position;
            Packet updatePosition = new Packet(3, myNetId, myPosition.x, myPosition.y, myPosition.z);
            socket.SendTo(updatePosition.GetData(), endPoint);
        }
        const int maxPackets = 100;
        byte[] data = new byte[256];
        for (int i = 0; i < maxPackets; i++)
        {
            int rlen = -1;
            try
            {
                rlen = socket.Receive(data);
            }
            catch
            {
                break;
            }

            if (rlen > 0)
            {
                byte command = data[0];
                Console.WriteLine();
                if (commandsTable.ContainsKey(command))
                    commandsTable[command](data, endPoint);

            }
            //    if (command == 2)
            //    {
            //        uint prefabType = BitConverter.ToUInt32(data, 1);
            //        uint netId = BitConverter.ToUInt32(data, 5);
            //        float x = BitConverter.ToSingle(data, 9);
            //        float y = BitConverter.ToSingle(data, 13);
            //        float z = BitConverter.ToSingle(data, 17);

            //        if (!netGameObjects.ContainsKey(netId) && netPrefabsCache.ContainsKey(prefabType))
            //        {
            //            GameObject prefab = netPrefabsCache[prefabType];
            //            GameObject newGameObject = Resources.Load("Player") as GameObject;
            //            Vector3 position;
            //            position.x = x;
            //            position.y = y;
            //            position.z = z;
            //            newGameObject.name = string.Format("NetObject {0}", netId);
            //            newGameObject.transform.position = position;
            //            netGameObjects[netId] = newGameObject;
            //        }
            //    }
            //    else if (command == 1)
            //    {
            //        uint prefabType = BitConverter.ToUInt32(data, 1);
            //        myNetId = BitConverter.ToUInt32(data, 5);
            //        float x = BitConverter.ToSingle(data, 9);
            //        float y = BitConverter.ToSingle(data, 13);
            //        float z = BitConverter.ToSingle(data, 17);

            //        if (netPrefabsCache.ContainsKey(prefabType) && myGameObject == null)
            //        {
            //            GameObject prefab = netPrefabsCache[prefabType];
            //            myGameObject = Resources.Load("Player") as GameObject;
            //            Vector3 position;
            //            position.x = x;
            //            position.y = y;
            //            position.z = z;
            //            myGameObject.name = string.Format("Me {0}", myNetId);
            //            myGameObject.transform.position = position;
            //            netGameObjects[myNetId] = myGameObject;
            //        }
            //    }
            //    else if (command == 3)
            //    {
            //        uint netId = BitConverter.ToUInt32(data, 1);
            //        float x = BitConverter.ToSingle(data, 5);
            //        float y = BitConverter.ToSingle(data, 9);
            //        float z = BitConverter.ToSingle(data, 13);

            //        if (netId != myNetId && netGameObjects.ContainsKey(netId))
            //        {
            //            GameObject updatedGameObject = netGameObjects[netId];
            //            Vector3 position;
            //            position.x = x;
            //            position.y = y;
            //            position.z = z;
            //            updatedGameObject.transform.position = position;
            //        }
            //    }
            
        }
    }
}
