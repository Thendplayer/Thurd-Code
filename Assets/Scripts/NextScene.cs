using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NextScene : MonoBehaviour
{
    [SerializeField] Animator I_fadeAnimator;

    private bool vAux_isFading = false;

    private float vAux_initialVolume;

    private Image fadeImage;

    private AudioSource v_playerAudio;

    private void Start()
    {
        fadeImage = I_fadeAnimator.gameObject.GetComponent<Image>();
        
        vAux_initialVolume = MusicManager.instance.i_audioSource.volume;
    }

    public void Load()
    {
        SceneManager.LoadScene(LevelManager.instance.v_levelNum + 1);
    }

    private void Update()
    {
        if (vAux_isFading && vAux_initialVolume >= 1 - fadeImage.color.a)
        {
            MusicManager.instance.i_audioSource.volume = 1 - fadeImage.color.a;
            v_playerAudio.volume = 1 - fadeImage.color.a;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            v_playerAudio = collision.gameObject.GetComponentInChildren<AudioSource>();

            vAux_isFading = true;

            I_fadeAnimator.Play("FadeOut");
            foreach (GameObject light in LevelManager.instance.L_lightingSystem)
            {
                if (light != null)
                    light.SetActive(false);
            }

            Invoke("Load", 1.5f);
        }
    }
}
