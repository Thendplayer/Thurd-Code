using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager instance = null;

    public AudioSource i_audioSource;

    [SerializeField] AudioClip v_music;

    [SerializeField] bool vAux_playOnAwake = false;

    void Awake()
    {
        #region Singletone
        if (instance == null) instance = this;
        if (instance != this) Destroy(this);
        #endregion

        if (vAux_playOnAwake)
        {
            ChangeSong(v_music);
        }
    }

    public void ChangeSong(AudioClip clip)
    {
        i_audioSource.Stop();
        i_audioSource.clip = clip;
        i_audioSource.Play();
    }

    public void Play()
    {
        i_audioSource.Play();
    }

    public void Pause()
    {
        i_audioSource.Stop();
    }
}
