using Mirror;
using UnityEngine;
public struct CreateCharacterMessage : NetworkMessage
{
    public string name;
}


public struct CreateDungeonMessage : NetworkMessage
{
    public uint objectReferenceId;
    public string addressableAssetKey;
    public int seed;
}
public struct CreateObjectMessage : NetworkMessage
{
    public uint objectReferenceId;
    public string addressableAssetKey;
    public Vector3 position;
}
public struct SetObjectPosRotMessage : NetworkMessage
{
    public uint objectReferenceId;
    public Vector3 pos;
    public Quaternion rot;
}
public struct SetObjectHierarchyMessage : NetworkMessage
{
    public uint objectReferenceId;
    public uint parentObjectReferenceId;
    public bool isPositionReset;
}
public struct SetObjectActive : NetworkMessage
{
    public uint objectReferenceId;
    public bool isActive;
}
public struct SetAnimation : NetworkMessage
{
    public uint objectReferenceId;
}