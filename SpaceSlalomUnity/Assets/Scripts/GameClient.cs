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



    private float asteroidTimer;

    public bool ServerIsOnline
    {
        get
        {
            return serverIsOnline;
        }
    }

    private bool isReadyPlayer1;

    public bool IsReadyPlayer1
    {
        get
        {
            return isReadyPlayer1;
        }
    }

    private bool isReadyPlayer2;

    public bool IsReadyPlayer2
    {
        get
        {
            return isReadyPlayer2;
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

    // Start is called before the first frame update
    void Start()
    {

        
        isReadyPlayer1 = false;

        backGround.SetActive(false);
        cam.gameObject.SetActive(false);
        startingMenu.SetActive(true);
        sceneElements.SetActive(true);
        loadingObj.SetActive(false);

        commandsTable = new Dictionary<byte, GameCommand>();

        //commandsTable[0] = Join;
        commandsTable[2] = Spawn;
        commandsTable[1] = Welcome;
        commandsTable[6] = SpawnAsteroids;
        
        commandsTable[8] = SetOneReady;
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

    public void SetOneReady(byte[] data, EndPoint sender)
    {
        //roomId = BitConverter.ToUInt32(data, 1);
        //myNetId = BitConverter.ToUInt32(data, 5);
                

        //if (isReadyPlayer1 ^ isReadyPlayer2)
        //{
        //    loadingObj.SetActive(true);
        //    startingMenu.SetActive(false);
        //}
        //else if (isReadyPlayer1 && IsReadyPlayer2)
        //{
        //    backGround.SetActive(true);
        //    cam.gameObject.SetActive(true);
        //    startingMenu.SetActive(false);
        //    sceneElements.SetActive(false);

        //    Packet start = new Packet(8, roomId, myNetId);
           
        //}
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

    }

    private void Spawn(byte[] data, EndPoint sender)
    {
        uint prefabType = BitConverter.ToUInt32(data, 1);
        myNetId = BitConverter.ToUInt32(data, 5);
        float x = BitConverter.ToSingle(data, 9);
        float y = BitConverter.ToSingle(data, 13);
        float z = BitConverter.ToSingle(data, 17);

        if (netPrefabsCache.ContainsKey(prefabType) && myGameObject == null)
        {
            GameObject prefab = netPrefabsCache[prefabType];
            myGameObject = Resources.Load("Player1") as GameObject;
            Vector3 position;
            position.x = x;
            position.y = y;
            position.z = z;
            myGameObject.name = string.Format("Me {0}", myNetId);
            myGameObject.transform.position = position;
            netGameObjects[myNetId] = myGameObject;
        }
    }

    private void Welcome(byte[] data, EndPoint sender)
    {
        roomId = BitConverter.ToUInt32(data, 1);
        myNetId = BitConverter.ToUInt32(data, 5);

        Debug.Log("Welcome Arrived");


        if (myNetId == 1)
            isReadyPlayer1 = true;
        else if (myNetId == 2)
            isReadyPlayer2 = true;

        if (isReadyPlayer1 ^ isReadyPlayer2)
        {
            loadingObj.SetActive(true);
            startingMenu.SetActive(false);
        }
        else if (isReadyPlayer1 && IsReadyPlayer2)
        {
            backGround.SetActive(true);
            cam.gameObject.SetActive(true);
            startingMenu.SetActive(false);
            sceneElements.SetActive(false);
        }


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

        //if (myNetId == 1)
        //    isReadyPlayer1 = true;
        //else if (myNetId == 2)
        //    isReadyPlayer2 = true;

        //Packet welcome = new Packet(this, 1, room.RoomId, newClient.Id);


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
        Debug.Log(isReadyPlayer1);
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

            Vector3 myPosition = myGameObject.transform.position;
            Packet updatePosition = new Packet(3, myNetId, roomId, myPosition.x, myPosition.y, myPosition.z);
            socket.SendTo(updatePosition.GetData(), clientendPoint);
            
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
