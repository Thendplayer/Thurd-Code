using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameOver : MonoBehaviour {

    [SerializeField] private CameraMovement i_cameraMovement;
    [SerializeField] private GameObject i_blackBackground;
    [SerializeField] private GameObject i_canvas;

    [Space(10)]
    [SerializeField] private EventSystem i_eventSystem;
    [SerializeField] private GameObject i_firstOption;

    [Space(10)]
    [SerializeField] private Transform i_player;
    [SerializeField] private float v_verticalOffset;
    [SerializeField] private float v_deathZoom;
    [SerializeField] private float v_speed;
    [SerializeField] private float v_zoomTimer = 0.6f;

    [SerializeField] private AudioSource i_audioSource;
    [SerializeField] private AudioClip v_lowConfirmationSound;
    [SerializeField] private AudioClip v_bigConfirmationSound;
    [SerializeField] private AudioClip v_changeSound;

    private bool v_zoomReady = false;
    private bool vAux_targetSelected = false;
    private bool vAux_confirmationSound = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (!v_zoomReady)
        {
            i_canvas.SetActive(false);
            i_blackBackground.SetActive(true);
            Time.timeScale = Mathf.Lerp(Time.timeScale, 0, Time.deltaTime);

            v_zoomTimer -= Time.unscaledDeltaTime;

            if (v_zoomTimer <= 0)
            {
                MenuSound();
                Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, v_deathZoom, v_speed * Time.unscaledDeltaTime);

                foreach(GameObject light in LevelManager.instance.L_lightingSystem)
                {
                    if(light.activeSelf == true)
                    {
                        light.SetActive(false);
                    }
                }

                if (!vAux_targetSelected)
                {
                    Vector3 l_targetPosition = i_player.position + new Vector3(0, v_verticalOffset);
                    i_cameraMovement.ChangeStaticTarget(true, l_targetPosition);
                    vAux_targetSelected = true;
                }

                if (Camera.main.orthographicSize - v_deathZoom < 0.02f)
                {
                    Time.timeScale = 0;
                    v_zoomReady = true;
                }
            }
        }
    }

    private void MenuSound()
    {
        if (Input.GetAxisRaw("Vertical") != 0)
        {
            if (!vAux_confirmationSound)
            {
                // Call your event function here.
                i_audioSource.PlayOneShot(v_changeSound);
                vAux_confirmationSound = true;
            }
        }
        if (Input.GetAxisRaw("Vertical") == 0)
        {
            vAux_confirmationSound = false;
        }
    }

    public void PlayLowConfirmation()
    {
        i_audioSource.PlayOneShot(v_lowConfirmationSound);
    }

    public void PlayBigConfirmation()
    {
        i_audioSource.PlayOneShot(v_bigConfirmationSound);
    }

    private void ChangeFristSelected()
    {
        i_eventSystem.SetSelectedGameObject(i_firstOption);
    }

    public void Continue()
    {
        LevelManager.instance.RespawnToBegining(i_player.gameObject);
    }

    public void ExitMenu()
    {
        Time.timeScale = 1;
        Manager.instance.LoadScene(0);
    }
}
