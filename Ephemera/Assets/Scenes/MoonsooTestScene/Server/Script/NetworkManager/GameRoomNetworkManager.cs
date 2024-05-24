using DunGen;
using Mirror;
using Mirror.Examples.NetworkRoom;
using System.Collections;
using System.Collections.Generic;
using Telepathy;
using UnityEngine;
using UnityEngine.UIElements;

public class GameRoomNetworkManager : NetworkRoomManager
{
    [SerializeField]
    GameObject gamePlayerObjectPrefab;
    [SerializeField]
    NetworkRoomPlayer roomPlayerObjectPrefab;
    [SerializeField]
    GameObject spaceSystem;

    int SpawnCount = 0;

    public static GameRoomNetworkManager Instance => NetworkRoomManager.singleton as GameRoomNetworkManager;
     
    public override void Start()
    {
        base.Start();
        playerPrefab = gamePlayerObjectPrefab;
        roomPlayerPrefab = roomPlayerObjectPrefab;

        /*NetworkServer.RegisterHandler<CreateDungeonMessage>(CreateDungeonHandler);
        NetworkServer.RegisterHandler<CreateObjectMessage>(CreateObjectHandler);
        NetworkServer.RegisterHandler<SetObjectPosRotMessage>(SetObjectPosRotHandler);
        NetworkServer.RegisterHandler<SetObjectHierarchyMessage>(SetObjectHierarchyHandler);
        NetworkServer.RegisterHandler<SetObjectActive>(SetActiveObjectHandler);
        NetworkServer.RegisterHandler<SetAnimation>(SetAnimationHandler);*/
    }

    #region NetworkRoomManager Function
    //새로운 클라이언트가 서버에 연결되었을 때에 서버에서 호출되는 함수
    public override void OnRoomServerConnect(NetworkConnectionToClient conn)//GameObject OnRoomServerCreateRoomPlayer(NetworkConnectionToClient conn)
    {
        Debug.Log("OnRoomServerCreateRoomPlayer");
        GameObject gameobject = Instantiate(ResourceManager.Instance.GetPrefab("RoomPlayer")); //Instantiate(roomPlayerObjectPrefab.gameObject);
        //gameobject의 컴포넌트를 가져와 message로 초기화

        Debug.Log(gameobject.name);
        NetworkServer.AddPlayerForConnection(conn, gameobject);

        GameObject roomCharacter = Instantiate(ResourceManager.Instance.GetPrefab("LobbyScavenger"));
        NetworkServer.Spawn(roomCharacter, conn);

        NetworkClient.RegisterHandler<CreateDungeonMessage>(CreateDungeonHandler);
        NetworkClient.RegisterHandler<CreateObjectMessage>(CreateObjectHandler);
        NetworkClient.RegisterHandler<SetObjectPosRotMessage>(SetObjectPosRotHandler);
        NetworkClient.RegisterHandler<SetObjectHierarchyMessage>(SetObjectHierarchyHandler);
        NetworkClient.RegisterHandler<SetObjectActive>(SetActiveObjectHandler);
        NetworkClient.RegisterHandler<SetAnimation>(SetAnimationHandler);
    }
    //클라이언트가 접속했을 때 클라이언트에서 호출되는 함수
    public override void OnRoomClientConnect() 
    {
        Debug.Log("OnRoomClientConnect");
        //ResourceManager.Instance.GetPrefab("RoomPlayer");
    }
    //GamePlayer를 생성할 때 호출하는 함수
    /*public override GameObject OnRoomServerCreateGamePlayer(NetworkConnectionToClient conn, GameObject roomPlayer)
    {
        GameObject playerController = Instantiate(ResourceManager.Instance.GetPrefab("Player"));
        NetworkServer.AddPlayerForConnection(conn, playerController);
        return playerController;
    }*/
    public override void OnRoomServerSceneChanged(string sceneName)
    {
        if(sceneName == "Assets/Scenes/GamePlay.unity")
        {
            GameObject gameManager = Instantiate(ResourceManager.Instance.GetPrefab("GameManager"));
            NetworkServer.Spawn(gameManager);

            OnCreateObject("SpaceShip", new Vector3(3000, 0, 0));
            OnCreateObject("SpaceObject", Vector3.zero);
            OnCreateObject("Terrain", Vector3.zero);
        }
    }
    #endregion

    #region MessageHandler Function
    //던전 생성 MessageHandler
    public void CreateDungeonHandler(NetworkConnectionToClient conn, CreateDungeonMessage msg)
    {
        GameObject go = Instantiate(ResourceManager.Instance.GetPrefab(msg.addressableAssetKey));
        ObjectReference.Instance.AddGameObject(msg.objectReferenceId, go);
        RuntimeDungeon rd = go.GetComponent<RuntimeDungeon>();
        rd.Generator.Seed = msg.seed;
        rd.Generate();
    }
    //오브젝트 생성 MessageHandler
    private void CreateObjectHandler(NetworkConnectionToClient conn, CreateObjectMessage msg)
    {
        GameObject go = Instantiate(ResourceManager.Instance.GetPrefab(msg.addressableAssetKey));
        go.transform.localPosition = msg.position;
        ObjectReference.Instance.AddGameObject(msg.objectReferenceId, go);
    }
    //오브젝트 위치 설정 MessageHandler
    private void SetObjectPosRotHandler(NetworkConnectionToClient conn, SetObjectPosRotMessage msg)
    {
        GameObject go = ObjectReference.Instance.GetGameObjectById(msg.objectReferenceId);
        go.transform.localPosition = msg.pos;
        go.transform.localRotation = msg.rot;
    }
    //오브젝트 계층구조 구성 MessageHandler
    private void SetObjectHierarchyHandler(NetworkConnectionToClient conn, SetObjectHierarchyMessage msg)
    {
        GameObject go = ObjectReference.Instance.GetGameObjectById(msg.objectReferenceId);
        if(msg.parentObjectReferenceId == uint.MaxValue)
        {
            go.transform.SetParent(null);
        }
        else
        {
            GameObject parent = ObjectReference.Instance.GetGameObjectById(msg.parentObjectReferenceId);
            go.transform.SetParent(parent.transform);
        }
    }
    // 오브젝트 활성화 설정 MessageHandler
    private void SetActiveObjectHandler(NetworkConnection conn, SetObjectActive msg)
    {
        ObjectReference.Instance.GetGameObjectById(msg.objectReferenceId).SetActive(msg.isActive);
    }
    // 오브젝트 애니메이션 설정 MessageHandler
    private void SetAnimationHandler(NetworkConnection conn, SetAnimation msg)
    {

    }
    #endregion
    #region MessageHandler Function
    //던전 생성 MessageHandler
    public void CreateDungeonHandler(CreateDungeonMessage msg)
    {
        GameObject go = Instantiate(ResourceManager.Instance.GetPrefab(msg.addressableAssetKey));
        ObjectReference.Instance.AddGameObject(msg.objectReferenceId, go);
        RuntimeDungeon rd = go.GetComponent<RuntimeDungeon>();
        rd.Generator.Seed = msg.seed;
        rd.Generate();
    }
    //오브젝트 생성 MessageHandler
    private void CreateObjectHandler(CreateObjectMessage msg)
    {
        GameObject go = Instantiate(ResourceManager.Instance.GetPrefab(msg.addressableAssetKey));
        go.transform.localPosition = msg.position;
        ObjectReference.Instance.AddGameObject(msg.objectReferenceId, go);
    }
    //오브젝트 위치 설정 MessageHandler
    private void SetObjectPosRotHandler(SetObjectPosRotMessage msg)
    {
        GameObject go = ObjectReference.Instance.GetGameObjectById(msg.objectReferenceId);
        go.transform.localPosition = msg.pos;
        go.transform.localRotation = msg.rot;
    }
    //오브젝트 계층구조 구성 MessageHandler
    private void SetObjectHierarchyHandler(SetObjectHierarchyMessage msg)
    {
        GameObject go = ObjectReference.Instance.GetGameObjectById(msg.objectReferenceId);
        if (msg.parentObjectReferenceId == uint.MaxValue)
        {
            go.transform.SetParent(null);
        }
        else
        {
            GameObject parent = ObjectReference.Instance.GetGameObjectById(msg.parentObjectReferenceId);
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = Vector3.zero;
        }
    }
    // 오브젝트 활성화 설정 MessageHandler
    private void SetActiveObjectHandler(SetObjectActive msg)
    {
        ObjectReference.Instance.GetGameObjectById(msg.objectReferenceId).SetActive(msg.isActive);
    }
    // 오브젝트 애니메이션 설정 MessageHandler
    private void SetAnimationHandler(SetAnimation msg)
    {

    }
    #endregion
    #region Server Communication Function
    //던전 생성
    public uint OnCreateDungeon(string addressableAssetKey, int seed)
    {
        uint id = ObjectReference.Instance.GenerationObjectReferenceId;
        CreateDungeonMessage msg = new CreateDungeonMessage { objectReferenceId = id, addressableAssetKey = addressableAssetKey };
        NetworkServer.SendToAll(msg);
        return id;
    }
    //오브젝트 생성
    public uint OnCreateObject(string addressableAssetKey, Vector3 position)
    {
        uint id = ObjectReference.Instance.GenerationObjectReferenceId;
        CreateObjectMessage msg = new CreateObjectMessage { objectReferenceId = id, addressableAssetKey = addressableAssetKey, position = position };
        NetworkServer.SendToAll(msg);
        return id;
    }
    //오브젝트 위치 설정
    public void OnSetObjectPosRot(uint objectReferenceId, Vector3 position, Quaternion quaternion)
    {
        SetObjectPosRotMessage msg = new SetObjectPosRotMessage { objectReferenceId = objectReferenceId, pos = position, rot = quaternion };
        NetworkServer.SendToAll(msg);
    }
    //오브젝트 계층구조 구성, parentObjectReferenceId에 uint.MaxValue를 할당함으로써 계층구조 해제
    public void OnSetObjectHierarchy(uint objectReferenceId, uint parentObjectReferenceId)
    {
        SetObjectHierarchyMessage msg = new SetObjectHierarchyMessage { objectReferenceId = objectReferenceId, parentObjectReferenceId = parentObjectReferenceId};
        NetworkServer.SendToAll(msg);
    }
    //오브젝트 활성화 설정
    public void OnSetActiveObject(uint objectReferenceId, bool isActive)
    {
        SetObjectActive msg = new SetObjectActive { objectReferenceId = objectReferenceId, isActive = isActive };
        NetworkServer.SendToAll(msg);
    }
    //오브젝트 애니메이션 설정
    public void OnSetAnimation(uint objectReferenceId)
    {
        SetAnimation msg = new SetAnimation { objectReferenceId = objectReferenceId};
        NetworkServer.SendToAll(msg);
    }
    #endregion
}
