using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

public class ObjectReference : MonoBehaviour
{
    private static ObjectReference instance;
    public static ObjectReference Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ObjectReference>();
            }
            return instance;
        }
    }

    Dictionary<uint, GameObject> objectDic = new Dictionary<uint, GameObject>();

    private uint currentObjectReferenceId = 0;
    public uint GenerationObjectReferenceId => currentObjectReferenceId++;

    public bool hasKey(uint objectId)
    {
        return objectDic.ContainsKey(objectId);
    }
    public void AddGameObject(uint objectId, GameObject gameObject)
    {
        Debug.Log(gameObject);
        objectDic.Add(objectId, gameObject);
    }
    public void RemoveGameObject(uint objectId)
    {
        if (hasKey(objectId))
        {
            if(objectDic[objectId] != null)
                Destroy(objectDic[objectId]);
            objectDic.Remove(objectId);
        }
        currentObjectReferenceId = 0;
    }
    public GameObject GetGameObjectById(uint objectId)
    {
        if(objectDic.TryGetValue(objectId, out GameObject obj))
            return obj;
        return null;
    }
    public uint GetIdByGameObject(GameObject gameObject)
    {
        uint objectId = uint.MaxValue;
        foreach (var pair in objectDic)
        {
            if (pair.Value == gameObject)
            {
                objectId = pair.Key;
                break;
            }
        }
        return objectId;
    }
    public uint GetIdByName(string name)
    {
        uint objectId = uint.MaxValue;
        foreach (var pair in objectDic)
        {
            if (pair.Value.name == name)
            {
                objectId = pair.Key;
                break;
            }
        }
        return objectId;
    }
    public GameObject GetGameObjectByName(string name)
    {
        foreach (var pair in objectDic)
        {
            if (pair.Value.name == name)
            {
                return pair.Value;
            }
        }
        return null;
    }
}