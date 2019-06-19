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
    public GameObject gameOverPanel;

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

    private bool readyPlayer1;
    private bool readyPlayer2;

    float serverStatusTimer;
    float serverOfflineTImer;
    float defaultServerStatusTimer = 2.5f;  
    float defaultOfflineTimer = 3.0f;

    private uint myNetId;
    private GameObject myGameObject;
    private GameObject myGameObject2;
    private Dictionary<uint,GameObject> netGameObjects;

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
        readyPlayer1 = false;
        readyPlayer2 = false;
        backGround.SetActive(false);
        cam.gameObject.SetActive(false);
        startingMenu.SetActive(true);
        sceneElements.SetActive(true);
        loadingObj.SetActive(false);
        gameOverPanel.SetActive(false);

        commandsTable = new Dictionary<byte, GameCommand>();
        netGameObjects = new Dictionary<uint, GameObject>();

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
        commandsTable[15] = Collide;
        commandsTable[16] = DestroyObject;
        commandsTable[253] = StatusServer;
        //commandsTable[255] = Ack;
    }

    private void Collide(byte[] data, EndPoint sender)
    {
        uint netId = BitConverter.ToUInt32(data, 1);
        uint roomId = BitConverter.ToUInt32(data, 5);

        Debug.Log("collide ");
    }

    private void MoveAsteroids(byte[] data, EndPoint sender)
    {
        uint prefabType = BitConverter.ToUInt32(data, 1);
        uint netId = BitConverter.ToUInt32(data, 5);
        uint roomId = BitConverter.ToUInt32(data, 9);
        float x = BitConverter.ToSingle(data, 13);
        float y = BitConverter.ToSingle(data, 17);
        //Debug.Log("errore " + "prefabType : " + prefabType + "net id " + netId + "roomid :" + roomId + "x :" + x + "y :" + y);
        netGameObjects[netId].transform.position = new Vector3(x, y);      
    }

   
    

    private void SpawnAsteroids(byte[] data, EndPoint sender)
    {     
        uint prefabType = BitConverter.ToUInt32(data, 1);
        uint netId = BitConverter.ToUInt32(data, 5);
        uint roomId = BitConverter.ToUInt32(data, 9);
        float x = BitConverter.ToSingle(data, 13);
        float y = BitConverter.ToSingle(data, 17);
        //Debug.Log("x: " + x);
        //Debug.Log("y: " + y);
        //Debug.Log("spawnato");
        if (!netGameObjects.ContainsKey(netId) && netPrefabsCache.ContainsKey(prefabType))
        {
            GameObject prefab = netPrefabsCache[prefabType];
            GameObject asteroids = Instantiate(Resources.Load("Asteroid")) as GameObject;
            netGameObjects.Add(netId,asteroids);
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
            netGameObjects.Add(myNetId, myGameObject);
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
            netGameObjects.Add(myNetId, myGameObject2);
        }
    }

    private void Welcome(byte[] data, EndPoint sender)
    {
        myNetId = BitConverter.ToUInt32(data, 1);
        roomId = BitConverter.ToUInt32(data, 5);        
        Packet ready = new Packet(8, myNetId, roomId);
        Send(ready.GetData());

        if (myNetId == 1)
            readyPlayer1 = true;
        else if (myNetId == 2)
            readyPlayer2 = true;

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

    public void DestroyObject(byte[] data, EndPoint sender)
    {
        uint myNetId = BitConverter.ToUInt32(data, 1);
        uint roomId = BitConverter.ToUInt32(data, 5);
        DestroyImmediate(netGameObjects[myNetId]);
        netGameObjects.Remove(myNetId);
        Debug.Log("destroy");

        backGround.SetActive(false);
        cam.gameObject.SetActive(false);
        startingMenu.SetActive(false);
        sceneElements.SetActive(true);
        loadingObj.SetActive(false);
        gameOverPanel.SetActive(true);
    }

    private void MoveOtherPlayer(byte[] data, EndPoint sender)
    {
        uint myNetId = BitConverter.ToUInt32(data, 1);
        uint roomId = BitConverter.ToUInt32(data, 5);
        float x = BitConverter.ToSingle(data, 9);
        float y = BitConverter.ToSingle(data, 13);


        if (myNetId == 1)
            myGameObject.transform.position = new Vector3(x, y);

        else if (myNetId == 2)
            myGameObject2.transform.position = new Vector3(x, y);

       
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
       

        if (myGameObject != null && myGameObject2 != null)
        {
            Packet updatePosition = new Packet();           

            if (readyPlayer1)
            { 
                if (Input.GetKey(KeyCode.D))
                {
                    myGameObject.transform.position += myGameObject.transform.right * 2 * Time.deltaTime;
                }
                if (Input.GetKey(KeyCode.A))
                {
                    myGameObject.transform.position += -myGameObject.transform.right * 2 * Time.deltaTime;
                }
                if (Input.GetKey(KeyCode.W))
                {
                    myGameObject.transform.position += myGameObject.transform.up * 2 * Time.deltaTime;
                }
                if (Input.GetKey(KeyCode.S))
                {
                    myGameObject.transform.position += -myGameObject.transform.up * 2 * Time.deltaTime;
                }

               updatePosition = new Packet(3, myNetId, roomId, myGameObject.transform.position.x, myGameObject.transform.position.y);
            }

            if (readyPlayer2)
            {
                if (Input.GetKey(KeyCode.D))
                {
                    myGameObject2.transform.position += myGameObject.transform.right * 2 * Time.deltaTime;
                }
                if (Input.GetKey(KeyCode.A))
                {
                    myGameObject2.transform.position += -myGameObject.transform.right * 2 * Time.deltaTime;
                }
                if (Input.GetKey(KeyCode.W))
                {
                    myGameObject2.transform.position += myGameObject.transform.up * 2 * Time.deltaTime;
                }
                if (Input.GetKey(KeyCode.S))
                {
                    myGameObject2.transform.position += -myGameObject.transform.up * 2 * Time.deltaTime;
                }
                updatePosition = new Packet(3, myNetId, roomId, myGameObject2.transform.position.x, myGameObject2.transform.position.y);
            }

            if(updatePosition.GetData().Length != 0)
                Send(updatePosition.GetData());

            Debug.Log("player 1 x: " + myGameObject.transform.position);

            Debug.Log("player 2 x: " + myGameObject2.transform.position);
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
        }
    }
}
