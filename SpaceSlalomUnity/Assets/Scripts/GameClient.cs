using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

public class GameClient : MonoBehaviour
{
    
    public string clientaddress;

    public int clientport;

    public string serverAddress;

    public int serverPort;

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
    private IPEndPoint clientendPoint;
    private static IPEndPoint serverEndPoint;
    private bool serverIsOnline;

    public GameObject backGround;
    public Camera cam;
    public GameObject startingMenu;
    public GameObject sceneElements;
    public GameObject loadingObj;

    private bool gameStarted;
    public bool GameStarted
    {
        get
        {
            return gameStarted;
        }
    }

    private float asteroidTimer;

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
    private GameObject myGameObject2;
    private Dictionary<uint,GameObject> asteroidsGameObject;

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
        clientendPoint = new IPEndPoint(IPAddress.Parse(clientaddress), clientport);
        socket.Bind(clientendPoint);

        serverEndPoint = new IPEndPoint(IPAddress.Parse(serverAddress), serverPort);
    }

    public bool Send(byte[] data)
    {
        bool success = false;
        try
        {
            int rlen = socket.SendTo(data, serverEndPoint);
            if (rlen == data.Length)
                success = true;
        }
        catch
        {
            success = false;
        }
        return success;
    }

    private void Startgame(byte[] data, EndPoint sender)
    {

        Debug.Log("game started");
        backGround.SetActive(true);
        cam.gameObject.SetActive(true);
        startingMenu.SetActive(false);
        sceneElements.SetActive(false);
        loadingObj.SetActive(false);
    }
    // Start is called before the first frame update
    void Start()
    {
        gameStarted = false;
       
        backGround.SetActive(false);
        cam.gameObject.SetActive(false);
        startingMenu.SetActive(true);
        sceneElements.SetActive(true);
        loadingObj.SetActive(false);

        commandsTable = new Dictionary<byte, GameCommand>();
        asteroidsGameObject = new Dictionary<uint, GameObject>();

        //commandsTable[0] = Join;
        commandsTable[1] = Welcome;
        commandsTable[2] = SpawnPlayer1;
        // commandsTable[3] = Update;
        commandsTable[5] = Startgame;
        commandsTable[6] = SpawnAsteroids;
        //commandsTable[8] = SetOneReady;
        //commandsTable[9] = DestroyAsteroid;
        //commandsTable[10] = SpaceShipCollision;
        commandsTable[11] = SpawnPlayer2;
        commandsTable[12] = MoveOtherPlayer;
        commandsTable[13] = MoveAsteroids;
        commandsTable[253] = StatusServer;
        //commandsTable[255] = Ack;
    }

    private void MoveAsteroids(byte[] data, EndPoint sender)
    {
        uint prefabType = BitConverter.ToUInt32(data, 1);
        uint netId = BitConverter.ToUInt32(data, 5);
        uint roomId = BitConverter.ToUInt32(data, 9);
        float x = BitConverter.ToSingle(data, 13);
        float y = BitConverter.ToSingle(data, 17);


        asteroidsGameObject[netId].transform.position = new Vector3(x, y);
    }

   
    

    private void SpawnAsteroids(byte[] data, EndPoint sender)
    {     
        //System.Random random = new System.Random();
        uint prefabType = BitConverter.ToUInt32(data, 1);
        uint netId = BitConverter.ToUInt32(data, 5);
        uint roomId = BitConverter.ToUInt32(data, 9);
        float x = BitConverter.ToSingle(data, 13);
        float y = BitConverter.ToSingle(data, 17);
        Debug.Log("x: " + x);
        Debug.Log("y: " + y);
        Debug.Log("spawnato");
        if (!netGameObjects.ContainsKey(netId) && netPrefabsCache.ContainsKey(prefabType))
        {
            GameObject prefab = netPrefabsCache[prefabType];
            GameObject asteroids = Instantiate(Resources.Load("Asteroid")) as GameObject;
            asteroidsGameObject.Add(netId,asteroids);
            asteroids.name = string.Format("NetObject {0}", netId);
            asteroids.transform.position = new Vector3(x, y);
        }

        //float rotationValue = random.Next(0, 360);
        //newGameObject.transform.rotation = Quaternion.Euler(new Vector3(rotationValue, rotationValue, rotationValue));
        //float randomScale = random.Next(-4, 5);
        //newGameObject.transform.localScale = new Vector3(randomScale, randomScale, randomScale) /10;
    }

    private void SpawnPlayer1(byte[] data, EndPoint sender)
    {
        uint prefabType = BitConverter.ToUInt32(data, 1);
        uint myNetId = BitConverter.ToUInt32(data, 5);
        uint roomId = BitConverter.ToUInt32(data, 9);
        float x = BitConverter.ToSingle(data, 13);
        float y = BitConverter.ToSingle(data, 17);
        

        if (netPrefabsCache.ContainsKey(prefabType) && myGameObject == null)
        {          
            GameObject prefab = netPrefabsCache[prefabType];
            myGameObject = Instantiate(Resources.Load("Player1")) as GameObject;            
            myGameObject.name = string.Format("Me {0}", myNetId);
            myGameObject.transform.position = new Vector3(x, y);
            netGameObjects[myNetId] = myGameObject;
        }       
    }

    private void SpawnPlayer2(byte[] data, EndPoint sender)
    {

        uint prefabType = BitConverter.ToUInt32(data, 1);
        uint myNetId = BitConverter.ToUInt32(data, 5);
        uint roomId = BitConverter.ToUInt32(data, 9);
        float x = BitConverter.ToSingle(data, 13);
        float y = BitConverter.ToSingle(data, 17);

        if(netPrefabsCache.ContainsKey(prefabType) && myGameObject2 == null)
        {
            GameObject prefab = netPrefabsCache[prefabType];
            myGameObject2 = Instantiate(Resources.Load("Player2")) as GameObject;
            myGameObject2.name = string.Format("Me {0}", myNetId);
            myGameObject2.transform.position = new Vector3(x, y);
            netGameObjects[myNetId] = myGameObject2;
        }
    }

    private void Welcome(byte[] data, EndPoint sender)
    {
        myNetId = BitConverter.ToUInt32(data, 1);
        roomId = BitConverter.ToUInt32(data, 5);        

        Debug.Log("Welcome Arrived");

        Packet ready = new Packet(8, myNetId, roomId);
        Send(ready.GetData());

    }



    private void StatusServer(byte[]data, EndPoint sender)
    {
        serverIsOnline = BitConverter.ToBoolean(data, 1);

        serverOfflineTImer = defaultOfflineTimer;
    }
   

    public void JoinButton()
    {
        Packet join = new Packet(0);
        Send(join.GetData());
        Debug.Log("join");

        startingMenu.SetActive(false);
        //sceneElements.SetActive(false);
        loadingObj.SetActive(true);
        backGround.SetActive(false);
        cam.gameObject.SetActive(false);
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


    

    private void MoveOtherPlayer(byte[] data, EndPoint sender)
    {
        uint myNetId = BitConverter.ToUInt32(data, 1);
        uint roomId = BitConverter.ToUInt32(data, 5);
        float x = BitConverter.ToSingle(data, 9);
        float y = BitConverter.ToSingle(data, 13);

        myGameObject2.transform.position = new Vector3(x, y);
        Debug.Log("move other player");
    }

    // Update is called once per frame
    void Update()
    {        
        Debug.Log(myNetId + " net id");
        
        defaultOfflineTimer -= Time.deltaTime;
        defaultServerStatusTimer -= Time.deltaTime;

        if(serverStatusTimer < 0)
        {
            Packet serverStatusPacket = new Packet(253, serverIsOnline);
            socket.SendTo(serverStatusPacket.GetData(), clientendPoint);

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

            //Vector3 myPosition = myGameObject.transform.position;
            Packet updatePosition = new Packet(3, myNetId, roomId, myGameObject.transform.position.x, myGameObject.transform.position.y);
            Debug.Log(myNetId);

            Send(updatePosition.GetData());


            //Debug.Log("player 1 x: " + myGameObject.transform.position);
           
            //Debug.Log("player 2 x: " + myGameObject2.transform.position);


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
                    commandsTable[command](data, clientendPoint);

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
            //    } uint prefabType = BitConverter.ToUInt32(data, 1);
            //uint netId = BitConverter.ToUInt32(data, 5);
            //float x = BitConverter.ToSingle(data, 9);
            //float y = BitConverter.ToSingle(data, 13);
            //float z = BitConverter.ToSingle(data, 17);

            //if (!netGameObjects.ContainsKey(netId) && netPrefabsCache.ContainsKey(prefabType))
            //{
            //    GameObject prefab = netPrefabsCache[prefabType];
            //    GameObject newGameObject = Resources.Load("Player") as GameObject;
            //    Vector3 position;
            //    position.x = x;
            //    position.y = y;
            //    position.z = z;
            //    newGameObject.name = string.Format("NetObject {0}", netId);
            //    newGameObject.transform.position = position;
            //    netGameObjects[netId] = newGameObject;

            //    Debug.Log("spaw");
            //}

        }
    }
}
