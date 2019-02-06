using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;


public class FinalBoss : MonoBehaviour
{
    [SerializeField] private List<ProjectileSpawner> L_spawners = new List<ProjectileSpawner>();

    [SerializeField] private ProjectileSpawner i_bigSpawner;

    [SerializeField] private int v_numberOfPhases;
    [SerializeField] private int v_numberOfSpawners;

    [SerializeField] private float v_maxPhaseTime;

    [SerializeField] private float v_shakeLength;
    [SerializeField] private float v_shakeMagnitude;

    [SerializeField] private GameObject v_videoCanvas;
    [SerializeField] private GameObject v_stopVideoCanvas;

    [SerializeField] private Color[] WaveColourSprite;

    [SerializeField] private SpriteRenderer v_bossSprite;

    [SerializeField] private VideoManager v_videoManager;
    [SerializeField] private AudioClip v_deathSound;
    [SerializeField] private AudioSource i_audioSource;

    private int v_currentPhase = 1;

    private float vAux_phaseTimer = 0;
    private bool vAux_isWaitingNewWave = false;
    private bool v_isBigSpawnerActive = false;

    private IEnumerator v_shakeCouritine = null;

    [HideInInspector] public bool v_isDead = false;

    //new
    [SerializeField] private List<GameObject> L_phaseEnemies = new List<GameObject>();
    private List<ProjectileSpawner> L_currentEnemies = new List<ProjectileSpawner>();
    private List<ProjectileSpawner> L_currentEnemiesThatCantDie = new List<ProjectileSpawner>();

    [SerializeField] private List<Transform> L_bloodPositions = new List<Transform>();
    [SerializeField] private Transform blood;
    [SerializeField] private float bloodSpeed = 1f;

    float l_vAux_numberOfDeadSpanwers = 0;
    [SerializeField] private float v_lastPhaseTimer = 5f;
    private float v_lastPhaseCurrentTimer = 0f;
    private bool vAux_bossHasBeenDefeated = false;
    [SerializeField] PostProcessingProfile v_postProcessingProfile;
    [SerializeField] float v_timeBetweenPhases = 2f;
    [SerializeField] GameObject v_firstPlatformToDisapear;
    [SerializeField] GameObject v_thirdPlatformToDisapear;
    [SerializeField] GameObject v_secondPlatformToDisapear;
    //[SerializeField] GameObject v_eyeToDodge;



    //private void Start()
    //{
    //    v_bossSprite.color = WaveColourSprite[v_currentPhase - 1];
    //}

    //private void Update()
    //{
    //    if (v_isBigSpawnerActive)
    //    {
    //        CheckStatusOfBigSpawner();
    //    }
    //    else
    //    {
    //        CheckStatusOfActiveSpawners();
    //    }
    //}

    //private void CheckStatusOfActiveSpawners()
    //{
    //    int l_numberOfDeadSpawners = 0;

    //    foreach (ProjectileSpawner spawner in L_spawners)
    //    {
    //        if (spawner.v_isDead)
    //        {
    //            l_numberOfDeadSpawners++;
    //        }
    //    }

    //    if (l_numberOfDeadSpawners >= v_numberOfSpawners)
    //    {
    //        vAux_phaseTimer = 0;

    //        v_isBigSpawnerActive = true;

    //        i_bigSpawner.gameObject.SetActive(true);

    //        if (i_bigSpawner.v_isDead)
    //        {
    //            i_bigSpawner.Revive();
    //        }
    //    }
    //}

    //private void CheckStatusOfBigSpawner()
    //{
    //    if (i_bigSpawner.v_isDead)
    //    {
    //        if (v_currentPhase <= v_numberOfPhases && !vAux_isWaitingNewWave)
    //        {
    //            i_audioSource.Play();
    //            Invoke("ChangeToNextPhase", 0.5f);
    //            vAux_isWaitingNewWave = true;
    //            //ChangeToNextPhase(false);
    //        }
    //        else if (v_currentPhase >= v_numberOfPhases && vAux_isWaitingNewWave)
    //        {
    //            //BOSS HAS BEEN DEFEATED
    //            //Destroy(gameObject);
    //            print("Im being executed");
    //            if (v_shakeCouritine == null)
    //            {
    //                i_audioSource.clip = v_deathSound;
    //                i_audioSource.Play();
    //                v_shakeCouritine = CameraShake.instance.Shake(v_shakeLength, v_shakeMagnitude);
    //                StartCoroutine(v_shakeCouritine);
    //                Invoke("FinalVideo", v_shakeLength / 2);

    //            }
    //            else
    //            {
    //                if (v_videoCanvas.activeSelf == true && v_stopVideoCanvas.activeSelf == true)
    //                {
    //                    //FINISH LEVEL
    //                    Manager.instance.LoadScene(0);
    //                    Debug.Log("YOU HAVE FINISHED THE GAME");
    //                }
    //            }


    //        }
    //    }

    //    ResetTimer();
    //}

    //private void ChangeToNextPhase(bool reset)
    //{
    //    if (!reset)
    //    {
    //        v_currentPhase++;
    //        v_bossSprite.color = WaveColourSprite[v_currentPhase - 1];
    //    }

    //    v_isBigSpawnerActive = false;

    //    i_bigSpawner.gameObject.SetActive(false);

    //    foreach (ProjectileSpawner spawner in L_spawners)
    //    {
    //        spawner.gameObject.SetActive(true);

    //        if (spawner.v_isDead)
    //        {
    //            //spawner.ReviveWithDelay(0.5f);
    //            spawner.Revive();
    //        }
    //    }
    //}
    //private void ChangeToNextPhase()
    //{
    //    if (v_currentPhase < v_numberOfPhases)
    //    {
    //        vAux_isWaitingNewWave = false;

    //        v_currentPhase++;

    //        v_bossSprite.color = WaveColourSprite[v_currentPhase - 1];

    //        v_isBigSpawnerActive = false;

    //        i_bigSpawner.gameObject.SetActive(false);

    //        foreach (ProjectileSpawner spawner in L_spawners)
    //        {
    //            spawner.gameObject.SetActive(true);

    //            if (spawner.v_isDead)
    //            {
    //                //spawner.ReviveWithDelay(0.5f);
    //                spawner.Revive();
    //            }
    //        }
    //    }

    //}

    //private void ResetTimer()
    //{
    //    vAux_phaseTimer += Time.deltaTime;

    //    if (vAux_phaseTimer >= v_maxPhaseTime)
    //    {
    //        vAux_phaseTimer = 0;

    //        ChangeToNextPhase(true);
    //    }
    //}

    //private void FinalVideo()
    //{
    //    VideoManager.instance.PlayVideo();
    //    CancelInvoke();
    //    StopCoroutine(v_shakeCouritine);
    //    Vibration.instance.StopVibration();
    //}

    private void Start()
    {
        v_currentPhase = 0;

        FillCurrentEnemiesList();

        v_postProcessingProfile = Camera.main.GetComponent<PostProcessingBehaviour>().profile;


    }


    private void Update()
    {
        print(v_currentPhase);
        CheckDeadSpawners();

        CheckNewPhase();

        MoveBlood();

        if(v_currentPhase == 2 && v_firstPlatformToDisapear.activeSelf == true)
        {
            v_firstPlatformToDisapear.SetActive(false);
        }
        else if (v_currentPhase == 5 && v_secondPlatformToDisapear.activeSelf == true)
        {
            v_secondPlatformToDisapear.SetActive(false);
        }
        if (v_currentPhase == 6 && v_thirdPlatformToDisapear.activeSelf == true)
        {
            v_thirdPlatformToDisapear.SetActive(false);
        }


        if (v_currentPhase == 1 || v_currentPhase == 4)
        {
            if (vAux_phaseTimer <= v_maxPhaseTime)
            {
                vAux_phaseTimer += Time.deltaTime;
            }
            else
            {
                vAux_phaseTimer = 0f;
                L_currentEnemies[0].CancelInvoke();
                if (L_currentEnemies[0].v_intervalBetweenProjectiles < 1)
                {
                    L_currentEnemies[0].v_intervalBetweenProjectiles = L_currentEnemies[0].v_intervalBetweenProjectiles * 18;
                }
                else
                {
                    L_currentEnemies[0].v_intervalBetweenProjectiles = L_currentEnemies[0].v_intervalBetweenProjectiles / 18;
                }
                if (!L_currentEnemies[0].v_isDead)
                {
                    L_currentEnemies[0].Initializate();
                }

            }
        }
        if (v_currentPhase == 5)
        {
            if (v_lastPhaseCurrentTimer <= v_lastPhaseTimer)
            {
                v_lastPhaseCurrentTimer += Time.deltaTime;

                //if(v_lastPhaseCurrentTimer >= 10 && v_eyeToDodge.activeSelf == false)
                //{
                //    v_eyeToDodge.SetActive(true);
                //}
            }
            else
            {
                v_lastPhaseCurrentTimer = 0f;
                i_audioSource.Play();
                EnableEffects(v_timeBetweenPhases);
                Invoke("DisableEffects", v_timeBetweenPhases);
                NextPhase();
            }
            
        }

        if (v_currentPhase == 7)
        {
            bloodSpeed = 10;

            if (!vAux_bossHasBeenDefeated)
            {
                FinalVideo();
                vAux_bossHasBeenDefeated = true;
            }
            if (v_videoCanvas.activeSelf == true && v_stopVideoCanvas.activeSelf == true)
            {
                Manager.instance.LoadScene(0);
            }
        }
    }

    private void NextPhase()
    {
        vAux_isWaitingNewWave = false;

        foreach (ProjectileSpawner enemy in L_currentEnemiesThatCantDie)
        {
            if (!enemy.v_isDead)
            {
                enemy.Die();
            }
        }
        foreach (ProjectileSpawner enemy in L_currentEnemies)
        {
            if (!enemy.v_isDead)
            {
                enemy.Die();
            }
        }
        if (v_currentPhase < L_phaseEnemies.Count)
        {
            L_phaseEnemies[v_currentPhase].SetActive(false);
        }
        v_currentPhase++;
        FillCurrentEnemiesList();

        if (v_currentPhase < L_phaseEnemies.Count)
        {
            v_bossSprite.color = WaveColourSprite[v_currentPhase];
        }

    }

    private void MoveBlood()
    {

        if (v_currentPhase < L_phaseEnemies.Count)
        {
            blood.transform.position = Vector3.MoveTowards(blood.transform.position, L_bloodPositions[v_currentPhase].position, bloodSpeed * Time.deltaTime);
        }
    }

    private void CheckDeadSpawners()
    {
        l_vAux_numberOfDeadSpanwers = 0;

        foreach (ProjectileSpawner enemy in L_currentEnemies)
        {
            if (enemy.v_isDead)
            {
                l_vAux_numberOfDeadSpanwers++;
            }
        }
    }

    private void FillCurrentEnemiesList()
    {
        L_currentEnemies.Clear();
        if ((v_currentPhase < L_phaseEnemies.Count))
        {
            ProjectileSpawner[] allChildren = L_phaseEnemies[v_currentPhase].GetComponentsInChildren<ProjectileSpawner>();
            foreach (ProjectileSpawner child in allChildren)
            {
                if (child.v_isEnemy)
                {
                    L_currentEnemies.Add(child);
                }
                else
                {
                    L_currentEnemiesThatCantDie.Add(child);
                }
            }
            L_phaseEnemies[v_currentPhase].SetActive(true);
        }

    }
    private void CheckNewPhase()
    {
        if (l_vAux_numberOfDeadSpanwers >= L_currentEnemies.Count && !vAux_isWaitingNewWave)
        {
            Invoke("NextPhase", v_timeBetweenPhases);
            vAux_isWaitingNewWave = true;
            if (v_currentPhase != 6 && v_currentPhase < 6)
            {
                i_audioSource.Play();
                EnableEffects(v_timeBetweenPhases);
                Invoke("DisableEffects", v_timeBetweenPhases);

            }
            else if (v_currentPhase == 6)
            {
                i_audioSource.clip = v_deathSound;
                i_audioSource.Play();
                EnableEffects(v_timeBetweenPhases * 3);
                Invoke("DisableEffects", v_timeBetweenPhases * 2.5f);
                v_timeBetweenPhases = v_timeBetweenPhases * 3;

            }
        }
    }

    private void FinalVideo()
    {
        DisableEffects();
        VideoManager.instance.PlayVideo();
        CancelInvoke();
        Vibration.instance.StopVibration();
    }

    private void EnableEffects(float length)
    {
        v_postProcessingProfile.chromaticAberration.enabled = true;
        v_postProcessingProfile.grain.enabled = true;

        StartCoroutine(CameraShake.instance.Shake(length, 0.7f));

    }
    private void DisableEffects()
    {
        v_postProcessingProfile.chromaticAberration.enabled = false;
        v_postProcessingProfile.grain.enabled = false;

        StopAllCoroutines();

    }

    private void OnDestroy()
    {
        DisableEffects();
    }
}
