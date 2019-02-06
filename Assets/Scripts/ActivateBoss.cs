using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateBoss : MonoBehaviour {


    //public List<GameObject> activateWhenPassing;
    //public List<GameObject> deActivateWhenPassing;

    [SerializeField] private CameraMovement I_camera;

    public GameObject boss;
    public GameObject shortcut;
    public AudioSource playerAudioSource;
    public AudioClip musicBoss;

    private bool musicActive = false;

    private bool vAux_playerHasColisioned;
    [SerializeField] private float vAux_newSmooth = 0.005f;
    [SerializeField] private float vAux_originalSmooth;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        I_camera.ChangeStaticTarget(true, boss.transform.position);
        I_camera.v_smoothSpeed = vAux_newSmooth;
        vAux_playerHasColisioned = true;
        Vibration.instance.CameraForTheBoss();
        MusicManager.instance.ChangeSong(musicBoss);
        LevelManager.instance.GetComponent<AudioSource>().volume = 0.4f;
        LevelManager.instance.v_pausedLevel = true;
        Time.timeScale = 0.01f;
        playerAudioSource.enabled = false;
    }


    private void Start()
    {
        vAux_originalSmooth = I_camera.v_smoothSpeed;
    }

    private void Update()
    {
        Vector3 v = I_camera.transform.position - boss.transform.position;
        if (v.magnitude <= 12f && vAux_playerHasColisioned)
        {
            I_camera.ChangeStaticTarget(false);
            I_camera.v_smoothSpeed = vAux_originalSmooth/2;
            boss.SetActive(true);
            vAux_playerHasColisioned = false;
            Time.timeScale = 1;
            this.transform.GetComponentInParent<Animator>().updateMode = AnimatorUpdateMode.Normal;
            LevelManager.instance.v_pausedLevel = false;
            playerAudioSource.enabled = true;


            Destroy(this.gameObject);
        }
    }
}
