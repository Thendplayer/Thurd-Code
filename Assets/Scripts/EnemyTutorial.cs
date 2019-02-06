using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class EnemyTutorial : MonoBehaviour {

    [SerializeField] private ProjectileSpawner v_enemy;
    [SerializeField] private Player v_player;
    [SerializeField] private Shield v_shield;



    [SerializeField] private float v_minDistanceWithPlayer;
    [SerializeField] private float v_minDistanceWithPlayer2;


    public bool vAux_firstTutorial;
    public bool vAux_secondTutorial;
    public bool vAux_thirdTutorial;
    public bool vAux_fourthTutorial;


    private bool vAux_playerIsProtecting;
    private bool vAux_playerHasDied;

    private bool vAux_hasCorrectedTheAnimation = false;


    private S_PlayerState vAux_previousPlayerState = S_PlayerState.Idle;

    [SerializeField] private GameObject v_tutorialMenu;
    [SerializeField] private GameObject v_pauseMenu;






    // Use this for initialization
    void Start () {
        vAux_firstTutorial = true;
        vAux_secondTutorial = false;
        vAux_playerIsProtecting = false;


	}
	
	// Update is called once per frame
	void Update () {


        if (v_enemy.enabled == true)
        {
            //Checks if the player has died during the tutorial and resets some values
            if (v_player.v_playerState == S_PlayerState.Idle && vAux_previousPlayerState == S_PlayerState.Spawn)
            {
                v_enemy.enabled = false;
                vAux_firstTutorial = true;
                vAux_secondTutorial = false;
                vAux_playerIsProtecting = false;
                vAux_playerHasDied = true;
                v_enemy.CancelInvoke();
                return;
            }
            vAux_previousPlayerState = v_player.v_playerState;
            
            //*****BLOCKING TUTORIAL*****//
            if (vAux_firstTutorial && v_pauseMenu.activeSelf == false)
            {
                CorrectPlayerState();
                if(!vAux_hasCorrectedTheAnimation)
                {
                    CorrectPlayerAnimation();
                }
                v_player.i_rb.velocity = Vector2.zero;

                if (vAux_playerIsProtecting)
                {
                    if(v_player.v_currentAnimation == S_PlayerAnimation.BlockProjectile) //Means that player has completed the tutorial
                    {
                        v_enemy.CancelInvoke();
                        v_enemy.enabled = false;

                        vAux_firstTutorial = false;
                        vAux_secondTutorial = true;
                        v_player.vAux_TutorialCantMove = false;
                        v_shield.vAux_firstTutorial = false;
                        vAux_hasCorrectedTheAnimation = false; //Resets the var to know if has to correct animation again


                    }
                    else if(v_player.v_currentAnimation == S_PlayerAnimation.Damage || v_player.v_currentAnimation == S_PlayerAnimation.DamageNoShield) //Player has died
                    {
                        v_enemy.CancelInvoke();
                        v_enemy.enabled = false;
                        v_player.vAux_TutorialCantMove = false;
                        v_shield.vAux_firstTutorial = false;
                        vAux_playerHasDied = true;
                        Time.timeScale = 1f;

                    }

                    if (Input.GetAxisRaw("Protect") == 0)
                    {
                        vAux_playerIsProtecting = false;
                    }
                }
                else //Pauses time and show the canvas
                {
                    Time.timeScale = 0f;
                    LevelManager.instance.v_pausedLevel = true;


                    v_tutorialMenu.SetActive(true); //shows the canvas message

                    if (Input.GetAxisRaw("Protect") != 0)
                    {
                        v_tutorialMenu.SetActive(false);

                        vAux_playerIsProtecting = true;
                        v_player.vAux_TutorialCantMove = true;
                        v_shield.vAux_firstTutorial = true;

                        LevelManager.instance.v_pausedLevel = false;
                        Time.timeScale = 1f;
                    }
                }


            }
            else if(vAux_secondTutorial && v_pauseMenu.activeSelf == false)
            {

                if(Input.GetAxisRaw("Parry") == 0)
                {
                    if(!vAux_hasCorrectedTheAnimation)
                    {
                        CorrectPlayerAnimation();
                        vAux_hasCorrectedTheAnimation = true;
                    }
                    CorrectPlayerState();

                    v_tutorialMenu.SetActive(true);

                    v_player.i_rb.velocity = Vector2.zero;

                    v_shield.StopAllCoroutines();
                    LevelManager.instance.v_pausedLevel = true;
                    Time.timeScale = 0f;


                }
                else
                {
                    
                    vAux_secondTutorial = false;
                    v_tutorialMenu.SetActive(false);
                    v_player.vAux_TutorialCantMove = false;

                    LevelManager.instance.v_pausedLevel = false;
                    Time.timeScale = 1f;

                }
            }
            if(v_enemy.v_isDead)
            {
                Destroy(this);
            }
        }

        else
        {
            CheckDistanceWithPlayer();
        }
    }


    private void CheckDistanceWithPlayer()
    {
        Vector3 l_distance = this.transform.position - v_player.transform.position;

        vAux_previousPlayerState = S_PlayerState.Idle;

        if(vAux_firstTutorial)
        {
            if (l_distance.magnitude <= v_minDistanceWithPlayer)
            {
                if (v_player.v_playerState == S_PlayerState.Run)
                {
                    CorrectPlayerState();
                    CorrectPlayerAnimation();
                    v_player.i_rb.velocity = Vector2.zero;
                    v_player.vAux_TutorialCantMove = true;


                }
                if (v_player.v_playerState == S_PlayerState.Idle)
                {


                    if (v_shield.v_shieldState == S_ShieldState.Hide)
                    {
                        v_enemy.enabled = true;

                        if (vAux_playerHasDied)
                        {
                            v_enemy.CancelInvoke();
                            v_enemy.Initializate();
                            vAux_playerHasDied = false;
                        }

                    }
                    else 
                    {
                        if (v_shield.v_shieldState != S_ShieldState.Hide && v_shield.v_shieldState != S_ShieldState.Returning)
                            v_shield.ChangeShieldState(v_shield.v_shieldState, S_ShieldState.Returning);

                        CorrectPlayerAnimation();
                    }
                }

            }


        }
        else if(vAux_secondTutorial)
        {
            if(l_distance.magnitude <= v_minDistanceWithPlayer2)
            {
                if (v_player.v_playerState == S_PlayerState.Run)
                {
                    CorrectPlayerState();
                    CorrectPlayerAnimation();
                    v_player.vAux_TutorialCantMove = true;

                }
                if (v_player.v_playerState == S_PlayerState.Idle)
                {
                    if (v_shield.InHand())
                    {
                        v_enemy.CancelInvoke();
                        v_enemy.enabled = true;
                        v_enemy.Initializate();
                    }
                    else
                    {
                        if (v_shield.v_shieldState != S_ShieldState.Returning)
                            v_shield.ChangeShieldState(v_shield.v_shieldState, S_ShieldState.Returning);

                        CorrectPlayerAnimation();
                    }
                }

            }


        }

        
    }

    private void CorrectPlayerAnimation()
    {
        if(v_player.v_playerState == S_PlayerState.Idle)
        {
            if (!v_shield.InHand())
            {
                v_player.ChangeAnimation(S_PlayerAnimation.IdleNoShield);
            }
            else
            {
                v_player.ChangeAnimation(S_PlayerAnimation.Idle);
            }
        }
        else if (v_player.v_playerState == S_PlayerState.Run)
        {
            if (!v_shield.InHand())
            {
                v_player.ChangeAnimation(S_PlayerAnimation.RunNoShield);
            }
            else
            {
                v_player.ChangeAnimation(S_PlayerAnimation.Run);
            }
        }

        vAux_hasCorrectedTheAnimation = true;



    }
    private void CorrectPlayerState()
    {
        if (v_player.v_playerState != S_PlayerState.Idle)
        {
            v_player.ChangePlayerState(v_player.v_playerState, S_PlayerState.Idle);
        }
        if((v_shield.transform.position - v_player.transform.position).magnitude > 0.5)
        {
            if (v_shield.v_shieldState != S_ShieldState.Hide && v_shield.v_shieldState != S_ShieldState.Protect)
            {
                v_shield.ChangeShieldState(v_shield.v_shieldState, S_ShieldState.Returning);
                v_shield.Return();

            }

            print("YEY");
        }
    }

    //*****************************************DEBUGGGG**********************************************//
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(this.transform.position, v_minDistanceWithPlayer);
        Gizmos.DrawWireSphere(this.transform.position, v_minDistanceWithPlayer2);

    }


}
