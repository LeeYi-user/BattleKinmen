using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip audioClip;

    private void Start()
    {
        audioSource.volume = SampleSceneManager.Instance.volume;
        audioSource.PlayOneShot(audioClip);
    }
}
