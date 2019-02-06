using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSound : MonoBehaviour {

    [SerializeField] private AudioSource i_audioSource;

    [Space(10)]
    [SerializeField] private AudioClip DefaultSound;
    [SerializeField] private AudioClip HitSound;
    [SerializeField] private AudioClip ReflectedSound;

    public void PlayHit()
    {
        i_audioSource.PlayOneShot(HitSound);
    }

    public void PlayDefault()
    {
        i_audioSource.Stop();
        i_audioSource.clip = DefaultSound;
        i_audioSource.loop = true;
        i_audioSource.Play();
    }

    public void PlayReflected()
    {
        i_audioSource.Stop();
        i_audioSource.volume = 1f;    
        i_audioSource.clip = ReflectedSound;
        i_audioSource.loop = true;
        i_audioSource.Play();
    }

    public void Stop()
    {
        i_audioSource.Stop();
    }
}
