using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAudio : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(PlayAudioCoroutine());
    }

    private IEnumerator PlayAudioCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);
            audioSource.Play();
            Debug.Log("¼Ò¸® ³Â¾î¿ä");
        }
    }
}
