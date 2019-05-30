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

    public GameObject backGround;
    public Camera cam;
    public GameObject startingMenu;
    public GameObject sceneElements;



    private float asteroidTimer;

    public bool ServerIsOnline
    {
        get
        {
            return serverIsOnline;
        }
    }

    private bool isReady;

    public bool IsReady
    {
        get
        {
            return isReady;
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
        
        backGround.SetActive(false);
        cam.gameObject.SetActive(false);
        startingMenu.SetActive(true);
        sceneElements.SetActive(true);

        commandsTable = new Dictionary<byte, GameCommand>();

        //commandsTable[0] = Join;
        commandsTable[1] = Spawn;
        commandsTable[5] = JoinRoom;
        commandsTable[6] = SpawnAsteroids;
        
        commandsTable[8] = SetReady;
        commandsTable[9] = DestroyAsteroid;
        commandsTable[10] = SpaceShipCollision;
        commandsTable[253] = StatusServer;
        //commandsTable[255] = Ack;

    }

    private void SpaceShipCollision(byte[] data, EndPoint sender)
    {
        throw new NotImplementedException();
    }

    private void DestroyAsteroid(byte[] data, EndPoint sender)
    {
        
    }

    public void SetReady(byte[] data, EndPoint sender)
    {
        isReady = !isReady;
        Packet startPacket = new Packet(8, roomId);
        socket.SendTo(startPacket.GetData(), sender);
        Debug.Log("start");
    }

    private void SpawnAsteroids(byte[] data, EndPoint sender)
    {
        
        System.Random random = new System.Random();
        uint prefabType = BitConverter.ToUInt32(data, 1);
        uint netId = BitConverter.ToUInt32(data, 5);
        float x = BitConverter.ToSingle(data, 9);
        float y = BitConverter.ToSingle(data, 13);
        float z = BitConverter.ToSingle(data, 17);

        if (!netGameObjects.ContainsKey(netId) && netPrefabsCache.ContainsKey(prefabType))
        {
            
            GameObject prefab = netPrefabsCache[prefabType];
            GameObject newGameObject = Resources.Load("Asteroid") as GameObject;
            
            Vector3 position;
            float rotationValue = random.Next(0, 360);
            newGameObject.transform.rotation = Quaternion.Euler(new Vector3(rotationValue, rotationValue, rotationValue));

            float randomScale = random.Next(-4, 5);
            newGameObject.transform.localScale = new Vector3(randomScale, randomScale, randomScale) /10;

            position.x = x;
            position.y = y;
            position.z = z;
            newGameObject.name = string.Format("NetObject {0}", netId);
            newGameObject.transform.position = position;
        }

        Packet spawnAsteroids = new Packet(6, netId, roomId, asteroidTimer);
        socket.SendTo(spawnAsteroids.GetData(), sender);
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

    public void GameStarted(byte[] data, EndPoint sender)
    {
        backGround.SetActive(true);
        cam.gameObject.SetActive(true);
        startingMenu.SetActive(false);
        sceneElements.SetActive(false);

        Packet ackStart = new Packet(255);
        socket.SendTo(ackStart.GetData(), sender);

    }

    public void JoinButton()
    {
        Packet join = new Packet(0);
        socket.SendTo(join.GetData(), endPoint);
        
    }



    public void OnAsteroidCollision()
    {
        //Packet packet = new Packet(9, roomId, asteroid.id) 
        
        
            //for (int i = 0; i < items.Count; i++)
            //{
            //    if (items[i].GameObject.IsActive)
            //        items[i].Update();
            //}
        

            //for (uint i = 0; i < netGameObjects.Count - 1; i++)
            //{
            //    if (netGameObjects[i].GameObject.IsActive && netGameObjects[i].IsCollisionsAffected)
            //    {
            //        for (uint j = i + 1; j < netGameObjects.Count; j++)
            //        {
            //            if (netGameObjects[j].gameObject.isStatic && netGameObjects[j].IsCollisionsAffected)
            //            {
            //                bool checkFirst = netGameObjects[i].CheckCollisionWith(netGameObjects[j]);
            //                bool checkSecond = netGameObjects[j].CheckCollisionWith(netGameObjects[i]);

            //                if ((checkFirst || checkSecond) && netGameObjects[i].Collides(netGameObjects[j]))
            //                {
            //                    if (checkFirst)
            //                        netGameObjects[i].GameObject.OnCollide(netGameObjects[j].GameObject);
            //                    if (checkSecond)
            //                        items[j].GameObject.OnCollide(items[i].GameObject);
            //                }
            //            }
            //        }
            //    }
            //}
        
    }


    
    public void OnPlayerCollision()
    {
        //packet (10, netId, roomId)

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
            if (Input.GetKey(KeyCode.D))
            {
                myGameObject.transform.position += myGameObject.transform.right * 3 * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.A))
            {
                myGameObject.transform.position += -myGameObject.transform.right * 3 * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.W))
            {
                myGameObject.transform.position += myGameObject.transform.up * 3 * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.S))
            {
                myGameObject.transform.position += -myGameObject.transform.up * 3 * Time.deltaTime;
            }

            Vector3 myPosition = myGameObject.transform.position;
            Packet updatePosition = new Packet(3, myNetId, roomId, myPosition.x, myPosition.y, myPosition.z);
            socket.SendTo(updatePosition.GetData(), endPoint);

            //if(asteroidTimer <= 0)
            //{
            //    SpawnAsteroidsAtRAndomTime();
            //}
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
