using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlatform : NetworkBehaviour
{
    #region OnTrigger Function
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Plane"))
            return;
        uint otherId = ObjectReference.Instance.GetIdByGameObject(other.gameObject);
        uint parentId = ObjectReference.Instance.GetIdByGameObject(other.gameObject);
        GameRoomNetworkManager.Instance.OnSetObjectHierarchy(otherId, parentId);
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Plane"))
            return;
        uint otherId = ObjectReference.Instance.GetIdByGameObject(other.gameObject);
        uint parentId = ObjectReference.Instance.GetIdByGameObject(other.gameObject);
        GameRoomNetworkManager.Instance.OnSetObjectHierarchy(otherId, uint.MaxValue);
    }
    #endregion
}
