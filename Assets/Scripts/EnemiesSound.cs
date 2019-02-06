using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesSound : MonoBehaviour {

    public AudioSource i_audioSource;
    public AudioClip Sound;

    [Space(10)]
    [SerializeField] private GameObject i_gameOver;

    public void Play()
    {
        if(!i_gameOver.activeSelf)
            i_audioSource.PlayOneShot(Sound);
    }

}
