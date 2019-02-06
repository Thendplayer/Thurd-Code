using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause : Menu
{

    [SerializeField] private Transform v_cloudFinalPos;

    void Start()
    {
        vAux_fullscreen = Screen.fullScreen;

        if (vAux_fullscreen)
        {
            i_fullscreenText.text = "ON";
        }
        else
        {
            i_fullscreenText.text = "OFF";
        }

        vAux_qualityLevel = QualitySettings.GetQualityLevel();

        switch (vAux_qualityLevel)
        {
            case 0:
                i_qualityText.text = "LOW";
                break;
            case 1:
                i_qualityText.text = "GOOD";
                break;
            case 2:
                i_qualityText.text = "HIGH";
                break;
            case 3:
                i_qualityText.text = "ULTRA";
                break;
        }

        #region Effects
        v_spawnPoint = i_cloud.position;
        SpawnCloud(i_cloud.gameObject);
        vAux_childSpawned = false;
        ChangeOrderInHierarchy();
        #endregion

    }

    void Update()
    {
        if (Time.timeScale !=0)
        {
            PauseTime();
        }

        MenuSound();
        float l_volume;
        i_audioMixer.GetFloat("volume", out l_volume);
       // i_audioMixerSound.SetFloat("volume", l_volume);
        i_audioMixerSound.SetFloat("volume", -80);

        #region Back

        if (Input.GetButtonDown("Cancel") && vAux_inOptionsMenu)
        {
            Back();
        }
        else if (Input.GetButtonDown("Cancel") || Input.GetButtonDown("Pause"))
        {
            i_audioMixer.GetFloat("volume", out l_volume);
            i_audioMixerSound.SetFloat("volume", l_volume);

            CloseMenu();
        }

        #endregion

        #region Controller or Keyboard

        if (vAux_controllerConnected)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return))
            {
                i_controllerInfo.SetActive(false);
                i_keyboardInfo.SetActive(true);
            }
        }
        else
        {
            if (Vibration.GamePadConnected)
            {
                i_controllerInfo.SetActive(true);
                i_keyboardInfo.SetActive(false);
                vAux_controllerConnected = true;
            }
        }

        #endregion

        #region Effects
        Transform l_toDestroy = null;


        foreach (Transform _cloud in L_clouds)
        {
            if (_cloud != null)
            {
                _cloud.position = Vector3.MoveTowards(_cloud.position, v_cloudFinalPos.position, v_speed * Time.unscaledDeltaTime);
                float l_distance = Vector3.Distance(_cloud.position, v_cloudFinalPos.position);

                if (_cloud.position == v_cloudFinalPos.position)
                {
                    ChangeOrderInHierarchy();
                    l_toDestroy = _cloud;
                }
            }

        }
        if (l_toDestroy != null)
        {
            Destroy(l_toDestroy.gameObject);
            SpawnCloud(i_cloud.gameObject);
        }


        #endregion
    }

    public void PauseTime()
    {
        Time.timeScale = 0;
    }

    public void CloseMenu()
    {
        Time.timeScale = 1;
        LevelManager.instance.v_pausedLevel = false;
        i_parent.gameObject.SetActive(false);
    }

    public new void Exit()
    {
        Time.timeScale = 1;
        Manager.instance.LoadScene(0);
    }

    private void ChangeOrderInHierarchy()
    {
        foreach (Transform _cloud in L_clouds)
        {
            if (_cloud != null)
            {
                _cloud.SetAsFirstSibling();
            }
        }
    }

}
