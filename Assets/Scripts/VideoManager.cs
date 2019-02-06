using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
    public static VideoManager instance = null;

    [SerializeField] private VideoPlayer I_videoPlayer;
    [SerializeField] private List<VideoClip> L_videos = new List<VideoClip>();

    [SerializeField] private GameObject I_playVideo;
    [SerializeField] private GameObject I_stopVideo;
    [SerializeField] public GameObject I_canvasVideo;

    [SerializeField] private Color v_cameraLightingColor;
    [SerializeField] private Color vAuxOriginalCameraLighting;

    [SerializeField] private Camera v_cameraLighting;

    private int vAuxCurrentVideoIndex = 0;
    private bool vAuxWasPlayingVideo = false;
    [HideInInspector] public bool vAux_isPlaying = false;

    #region Singletone
    void Awake()
    {
        if (instance == null) instance = this;
        if (instance != this) Destroy(this);
    }
    #endregion

    private void Start()
    {
        LoadNextVideo();
    }

    private void Update()
    {
        if (vAux_isPlaying)
        {
            vAuxWasPlayingVideo = true;

            if (Input.anyKey) //CHANGE
            {
                ExitVideo();
                LoadNextVideo();
            }

        }
        else if (vAuxWasPlayingVideo)
        {
            ExitVideo();
            LoadNextVideo();
        }

        vAux_isPlaying = I_videoPlayer.isPlaying;
    }

    public void ExitVideo()
    {
        I_videoPlayer.Stop();

        I_playVideo.SetActive(false);
        I_stopVideo.SetActive(true);

        EnableLights();

        v_cameraLighting.backgroundColor = vAuxOriginalCameraLighting;

        Invoke("DisableCanvas", 2f);

        Time.timeScale = 1f;
        LevelManager.instance.v_pausedLevel = false;


        vAuxWasPlayingVideo = false;

        MusicManager.instance.Play();
    }

    public void LoadNextVideo()
    {
        if (vAuxCurrentVideoIndex < L_videos.Count)
        {
            I_videoPlayer.clip = L_videos[vAuxCurrentVideoIndex];
        }
        vAuxCurrentVideoIndex++;
        vAux_isPlaying = false;
    }

    public void PlayVideo()
    {
        InvokeRepeating("SlowTime", 0, 0.05f);

        LevelManager.instance.v_pausedLevel = true;
        I_canvasVideo.SetActive(true);
        I_stopVideo.SetActive(false);
        I_playVideo.SetActive(true);
        DisableLights();
        v_cameraLighting.backgroundColor = v_cameraLightingColor;
    }

    private void Play()
    {
        I_videoPlayer.Play();
        Time.timeScale = 0;


        MusicManager.instance.Pause();
    }

    private void DisableCanvas()
    {
        I_canvasVideo.SetActive(false);
    }

    private void DisableLights()
    {
        foreach (GameObject light in LevelManager.instance.L_lightingSystem)
        {
            if (light != null)
            {
                light.SetActive(false);
            }
        }
    }

    private void EnableLights()
    {
        foreach (GameObject light in LevelManager.instance.L_lightingSystem)
        {
            if (light != null)
            {
                light.SetActive(true);
            }
        }
    }

    private void SlowTime()
    {
        if (Time.timeScale - 0.07f > 0)
            Time.timeScale -= 0.07f;
        else
        {
            Play();
            CancelInvoke("SlowTime");
        }

    }
}
