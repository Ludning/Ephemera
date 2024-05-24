using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TreeEditor;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    public Transform spawnPoint;
    [SerializeField]
    MovePlatform movePlatform;

    private void Awake()
    {
        Debug.Log("Awake!!");
    }

    public void StartLanding(Vector3 destination)
    {
        // 대상 위치에서 현재 위치를 빼서 방향 벡터 계산
        Vector3 direction = destination - transform.position;
        direction.y = 0;

        uint id = ObjectReference.Instance.GetIdByGameObject(gameObject);
        GameRoomNetworkManager.Instance.OnSetObjectPosRot(id, transform.position ,Quaternion.LookRotation(direction));

        StartCoroutine(Landing(destination));
    }

    IEnumerator Landing(Vector3 destination)
    {
        uint id = ObjectReference.Instance.GetIdByGameObject(gameObject);
        while (true)
        {
            if(Vector3.Distance(transform.position, destination) < 0.1f)
            {
                GameRoomNetworkManager.Instance.OnSetObjectPosRot(id, destination, transform.rotation);
                GameManager.Instance.OnServerActiveLocalPlayerCamera();
                GameManager.Instance.OnServerSetActivePlayer(true);
                UIController.Instance.SetActivateUI(typeof(UI_Setup));
                yield break;
            }
            GameRoomNetworkManager.Instance.OnSetObjectPosRot(id, Vector3.Slerp(transform.position, destination, 0.01f), transform.rotation);
            yield return null;
        }
    }
}
