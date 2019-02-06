using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


[RequireComponent(typeof(MusicManager))]
public class LevelManager : MonoBehaviour
{
    public static LevelManager instance = null;
    [HideInInspector] public bool v_pausedLevel = false;

    [Header("Lists")]
    public List<Heart> L_heartsList = new List<Heart>();
    public List<Checkpoint> L_checkpointList = new List<Checkpoint>();

    [SerializeField] public List<GameObject> L_lightingSystem = new List<GameObject>();

    [HideInInspector] public uint v_numberOfHeartsSpawned;
    [HideInInspector] public uint v_currentHeartFragmentsCollected = 0;

    private int v_heartListIndex;
    public int v_levelNum;

    [SerializeField] private GameObject i_pauseMenu;
    [SerializeField] private GameObject i_gameOverMenu;

    [SerializeField] protected AudioMixer i_audioMixerSound;

    #region Singletone
    void Awake()
    {
        if (instance == null) instance = this;
        if (instance != this) Destroy(this);
    }
    #endregion

    void Start()
    {
        //Checkpoints
        L_checkpointList[0].v_currentCheckpoint = true;
        if (VideoManager.instance != null && v_levelNum == 1)
        {
            VideoManager.instance.PlayVideo();
        }
    }

    void Update()
    {
        if (Input.GetButtonDown("Pause") && !i_gameOverMenu.activeSelf)
        {
            if (VideoManager.instance == null || !VideoManager.instance.vAux_isPlaying)
            {
                i_audioMixerSound.SetFloat("volume", -80);
                v_pausedLevel = true;
                i_pauseMenu.SetActive(true);
            }
        }
    }

    #region RESPAWN

    public void Respawn(GameObject _player)
    {
        if (L_checkpointList.Count >= 1)
        {
            foreach (Checkpoint checkpoint in L_checkpointList)
            {
                if (checkpoint.v_currentCheckpoint)
                {
                    _player.transform.position = checkpoint.transform.position;
                }
            }
        }

        //ADD ANIMATIONS AND MORE BEAUTIFUL THINGS
    }

    public void RespawnToBegining(GameObject _player)
    {
        Manager.instance.v_currentHealth = Manager.instance.v_currentNumOfHealthContainers * Manager.instance.v_numOfFragmentsForHealthContainer;

        if (L_checkpointList.Count >= 1)
        {
            _player.transform.position = L_checkpointList[0].transform.position;
            Manager.instance.ReloadLevel();
        }

        //ADD ANIMATIONS AND GAME OVER AND YOU KNOW
    }

    public void F_GameOver()
    {
        i_gameOverMenu.SetActive(true);
    }

    #endregion
}
