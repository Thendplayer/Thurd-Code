using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{

    [SerializeField] protected Transform i_parent;

    [Space(10)]
    [Header("Main Menu and Pause Menu Variables")]
    [Space(10)]
    [SerializeField]
    protected Transform i_cloud;
    [SerializeField] protected Vector3 v_finalPosition;
    [SerializeField] protected float v_speed;
    [SerializeField] protected AudioMixer i_audioMixer;
    [SerializeField] protected AudioMixer i_audioMixerSound;
    [SerializeField] protected AudioSource i_audioSource;
    [SerializeField] protected AudioClip v_lowConfirmationSound;
    [SerializeField] protected AudioClip v_bigConfirmationSound;
    [SerializeField] protected AudioClip v_changeSound;


    [SerializeField] protected Text i_audioText;
    [SerializeField] protected Text i_qualityText;
    [SerializeField] protected Text i_fullscreenText;


    [Space(10)]
    [SerializeField] protected GameObject i_mainScreen;
    [SerializeField] protected GameObject i_optionsScreen;
    [SerializeField] protected EventSystem i_eventSystem;

    [Space(10)]
    [SerializeField] protected GameObject i_firstOptionInMainScreen;
    [SerializeField] protected GameObject i_firstOptionInOptionsScreen;
    [SerializeField] protected GameObject i_keyboardInfo;
    [SerializeField] protected GameObject i_controllerInfo;

    protected List<Transform> L_clouds = new List<Transform>();

    protected Vector3 v_spawnPoint;
    protected bool vAux_childSpawned = false;

    protected bool vAux_audioActive = true;
    protected int vAux_qualityLevel = 2;
    protected bool vAux_fullscreen = true;
    protected bool vAux_inOptionsMenu = false;
    protected bool vAux_controllerConnected = false;

    private bool vAux_isFading = false;

    private float vAux_initialVolume;
    private bool vAux_confirmationSound = false;

    Image fadeImage;


    private int vAux_sceneIndex;

    [SerializeField] private Animator I_fadeAnimator;

    void Start()
    {
        Cursor.visible = false;

        if (Manager.instance != null)
        {
            Destroy(Manager.instance.gameObject);
        }

        fadeImage = I_fadeAnimator.gameObject.GetComponent<Image>();
        vAux_initialVolume = MusicManager.instance.i_audioSource.volume;
        v_spawnPoint = i_cloud.position;
        SpawnCloud(i_cloud.gameObject);
        vAux_childSpawned = false;

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
    }

    void Update()
    {
        MenuSound();
        if (vAux_isFading && vAux_initialVolume >= 1 - fadeImage.color.a)
        {
            MusicManager.instance.i_audioSource.volume = 1 - fadeImage.color.a;
        }
        float l_volume;
        i_audioMixer.GetFloat("volume", out l_volume);
        i_audioMixerSound.SetFloat("volume", l_volume);

        #region Effects

        bool l_spawn = false;
        Transform l_toDestroy = null;

        foreach (Transform _cloud in L_clouds)
        {
            _cloud.position = Vector3.MoveTowards(_cloud.position, v_finalPosition, v_speed * Time.deltaTime);
            float l_distance = Vector3.Distance(_cloud.position, v_finalPosition);

            if (l_distance < 40 && l_distance > 30 && !vAux_childSpawned)
                l_spawn = true;
            else if (l_distance < 30)
                vAux_childSpawned = false;

            if (Vector3.Distance(_cloud.position, v_finalPosition) <= 0.01f)
                l_toDestroy = _cloud;
        }

        if (l_spawn)
            SpawnCloud(i_cloud.gameObject);

        if (l_toDestroy)
            RemoveCloud(l_toDestroy.gameObject);

        #endregion

        #region Back

        if (Input.GetButtonDown("Cancel") && vAux_inOptionsMenu)
        {
            Back();
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
    }

    protected void SpawnCloud(GameObject _cloud)
    {
        GameObject l_cloud = Instantiate(_cloud, i_parent);
        l_cloud.transform.position = v_spawnPoint;
        L_clouds.Add(l_cloud.transform);
        vAux_childSpawned = true;
    }

    protected void RemoveCloud(GameObject _toDestroy)
    {
        L_clouds.Remove(_toDestroy.transform);
        Destroy(_toDestroy);
    }

    protected void MenuSound()
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

    void OnDrawGizmos()
    {
        Gizmos.DrawSphere(v_finalPosition, 0.5f);
    }

    #region ChangeScene

    public void StartFade(int index)
    {
        vAux_isFading = true;
        vAux_sceneIndex = index;
        I_fadeAnimator.Play("FadeOut");
        Invoke("Play", 1.5f);
    }

    public void Play()
    {
        SceneManager.LoadScene(vAux_sceneIndex);
    }

    public void Options()
    {
        i_mainScreen.SetActive(false);
        i_optionsScreen.SetActive(true);
        vAux_inOptionsMenu = true;

        i_eventSystem.SetSelectedGameObject(i_firstOptionInOptionsScreen);
    }

    public void Back()
    {
        i_mainScreen.SetActive(true);
        i_optionsScreen.SetActive(false);
        vAux_inOptionsMenu = true;

        i_eventSystem.SetSelectedGameObject(i_firstOptionInMainScreen);
    }

    public void Exit()
    {
        Application.Quit();
    }

    #endregion

    #region Settings

    public void ChangeVolumenToggle()
    {
        if (vAux_audioActive)
        {
            i_audioMixer.SetFloat("volume", -80);
            i_audioText.text = "OFF";
            vAux_audioActive = false;
        }
        else
        {
            i_audioMixer.SetFloat("volume", 0);
            i_audioText.text = "ON";
            vAux_audioActive = true;
        }
    }

    public void ChangeQualityToggle()
    {
        vAux_qualityLevel++;
        if (vAux_qualityLevel > 3)
            vAux_qualityLevel = 0;

        QualitySettings.SetQualityLevel(vAux_qualityLevel);

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
    }

    public void ChangeFullscreenToggle()
    {
        if (vAux_fullscreen)
        {
            Screen.fullScreen = false;
            i_fullscreenText.text = "OFF";
            vAux_fullscreen = false;
        }
        else
        {
            Screen.fullScreen = true;
            i_fullscreenText.text = "ON";
            vAux_fullscreen = true;
        }
    }

    #endregion
}
