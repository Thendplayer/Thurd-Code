using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float v_jumpForce = 650f;
    [SerializeField] private float v_gravity = 32f;
    [SerializeField] private float v_jumpLength = 0.2f;
    [SerializeField] private float v_baseWallFall = 2f;
    [SerializeField] private float v_wallFallAcceleration = 1.008f;
    [SerializeField] private float v_parryTime = 2f;
    [SerializeField] private float v_exitWallTime = 0.15f;
    [SerializeField] private float v_wallJumpLength = 0.25f;
    [SerializeField] private float v_knockbackForce = 400f;
    [SerializeField] private float v_maxFallSpeed = 80f;

    [SerializeField] private Vector2 v_wallJumpForce = new Vector2(2f, 3f);

    public float v_speed = 10f;

    [Space(10)]
    [SerializeField] public Rigidbody2D i_rb;
    [SerializeField] public BoxCollider2D i_myCollider;
    [SerializeField] public Shield i_shield;
    [SerializeField] private Animator i_animator;
    [SerializeField] private SpriteRenderer i_renderer;
    public PlayerSound i_sound;
    [SerializeField] private AudioClip i_gameOverClip;
    [SerializeField] private GameObject i_shadow;
    [HideInInspector] public bool vAux_rightWall = false;
    [HideInInspector] public bool vAux_leftWall = false;
    [HideInInspector] public bool vAux_canGetDamage = true;
    [HideInInspector] public bool vAux_isParrying = false;

    private int v_xInput;
    private Coroutine stopTimeCoroutine;
    private Coroutine shakeCameraCoroutine;
    private float vAux_exitWallTimer;

    private bool vAux_jumpNearWall = false;
    private bool vAux_deadActivated = false;

    [HideInInspector] public S_PlayerAnimation v_currentAnimation = S_PlayerAnimation.Idle;
    private S_PlayerAnimation v_lastAnimation;

    [HideInInspector] public bool vAux_TutorialCantMove = false;

    #region STATES

    public S_PlayerState v_playerState;
    public S_JumpState v_jumpState;
    public S_Direction v_playerDirection;

    private bool v_isStatic;
    private bool vAux_CanMove = true;

    [HideInInspector] public bool vAux_knocback = false;

    #endregion

    #region Auxiliar Functions
    private float vAux_WallJumpMoveTimer = 0f;
    [HideInInspector] public float vAux_TemporalSpeed;
    private float vAux_DeadTimer = 2.5f;
    private float vAux_DamageTimer = 0.6f;
    #endregion

    #region ParticleVariables

    [SerializeField] private GameObject i_particleJump;
    [SerializeField] private GameObject i_particleDoubleJump;
    [SerializeField] private GameObject i_wallSlide;

    private float vAux_runParticleTimer = 0.2f;

    private bool vAux_shieldFall = false;

    #endregion

    private void Start()
    {
        v_playerState = S_PlayerState.Spawn;

        vAux_TemporalSpeed = v_speed;
        vAux_exitWallTimer = v_exitWallTime;

        Physics2D.gravity = new Vector2(0, -v_gravity);

        transform.position = LevelManager.instance.L_checkpointList[0].transform.position;
    }

    private void Update()
    {
        if (!LevelManager.instance.v_pausedLevel)
        {
            PlayerStateMachine();

            #region Flip
            if (v_jumpState != S_JumpState.Climb)
            {
                if (v_playerDirection == S_Direction.Right)
                {
                    i_renderer.flipX = false;
                    i_renderer.transform.localPosition = new Vector3(0.18f, i_renderer.transform.localPosition.y);
                    i_shadow.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                    if (i_shield.v_shieldState != S_ShieldState.Hide)
                        i_shadow.transform.localPosition = new Vector3(-0.18f, i_shadow.transform.localPosition.y);
                    else
                        i_shadow.transform.localPosition = new Vector3(0, i_shadow.transform.localPosition.y);

                }
                else
                {
                    i_renderer.flipX = true;
                    i_renderer.transform.localPosition = new Vector3(-0.18f, i_renderer.transform.localPosition.y);
                    i_shadow.transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                    if (i_shield.v_shieldState != S_ShieldState.Hide)
                        i_shadow.transform.localPosition = new Vector3(0.18f, i_shadow.transform.localPosition.y);
                    else
                        i_shadow.transform.localPosition = new Vector3(0, i_shadow.transform.localPosition.y);

                }
            }
            else
            {
                if (vAux_rightWall)
                {
                    i_renderer.flipX = true;
                    i_renderer.transform.localPosition = new Vector3(-0.27f, i_renderer.transform.localPosition.y);
                    i_shadow.transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                    if (i_shield.v_shieldState != S_ShieldState.Hide)
                        i_shadow.transform.localPosition = new Vector3(0.27f, i_shadow.transform.localPosition.y);
                    else
                        i_shadow.transform.localPosition = new Vector3(0, i_shadow.transform.localPosition.y);

                }
                else
                {
                    i_renderer.flipX = false;
                    i_renderer.transform.localPosition = new Vector3(0.27f, i_renderer.transform.localPosition.y);
                    i_shadow.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                    if (i_shield.v_shieldState != S_ShieldState.Hide)
                        i_shadow.transform.localPosition = new Vector3(-0.27f, i_shadow.transform.localPosition.y);
                    else
                        i_shadow.transform.localPosition = new Vector3(0, i_shadow.transform.localPosition.y);

                }
            }
            #endregion
        }
    }

    #region StateMachine

    private void PlayerStateMachine()
    {
        switch (v_playerState)
        {
            case S_PlayerState.Spawn:
                i_rb.bodyType = RigidbodyType2D.Dynamic;

                if (i_shield.v_shieldState == S_ShieldState.Platform ||
                    i_shield.v_shieldState == S_ShieldState.Released)
                {
                    i_shield.ChangeShieldState(i_shield.v_shieldState, S_ShieldState.Returning);
                }
                else
                {
                    i_shield.ChangeShieldState(i_shield.v_shieldState, S_ShieldState.Hide);
                }

                if (CheckFloor())
                {
                    ChangePlayerState(v_playerState, S_PlayerState.Idle);
                    ChangeJumpState(v_jumpState, S_JumpState.Grounded);
                }
                else
                {
                    ChangePlayerState(v_playerState, S_PlayerState.Jump);
                    ChangeJumpState(v_jumpState, S_JumpState.Fall);
                }

                break;

            case S_PlayerState.Idle:

                //Shield Inputs


                i_shield.ShieldStateMachine();

                if (i_animator.GetCurrentAnimatorStateInfo(0).IsName("Fall"))
                    ChangeAnimation(S_PlayerAnimation.Idle);
                else if (i_animator.GetCurrentAnimatorStateInfo(0).IsName("FallNoShield"))
                    ChangeAnimation(S_PlayerAnimation.IdleNoShield);

                if (!vAux_TutorialCantMove)
                {
                    //Horizontal Inputs
                    v_xInput = (int)Input.GetAxis("Horizontal");

                    if (v_xInput < 0)
                    {
                        v_isStatic = true;
                        v_xInput = -1;
                        v_playerDirection = S_Direction.Left;
                        ChangePlayerState(v_playerState, S_PlayerState.Run);
                    }
                    else if (v_xInput > 0)
                    {
                        v_isStatic = true;
                        v_xInput = 1;
                        v_playerDirection = S_Direction.Right;
                        ChangePlayerState(v_playerState, S_PlayerState.Run);
                    }

                    //Jump Inputs
                    JumpStateMachine();
                }
                else //Block Tutorial Conditions
                {
                    if (v_playerDirection == S_Direction.Left)
                    {
                        v_playerDirection = S_Direction.Right;
                    }
                }




                break;

            case S_PlayerState.Run:

                //Horizontal Inputs
                v_xInput = (int)Input.GetAxis("Horizontal");

                if (v_xInput < 0)
                {
                    v_isStatic = false;
                    v_xInput = -1;
                    v_playerDirection = S_Direction.Left;
                }
                else if (v_xInput > 0)
                {
                    v_isStatic = false;
                    v_xInput = 1;
                    v_playerDirection = S_Direction.Right;
                }

                if (v_xInput != -1 && v_xInput != 1)
                {
                    v_isStatic = true;
                    v_xInput = 0;
                    ChangePlayerState(v_playerState, S_PlayerState.Idle);
                }

                HorizontalMovement();

                RunParticle();

                //Jump Inputs
                JumpStateMachine();

                //Shield Inputs

                i_shield.ShieldStateMachine();

                break;

            case S_PlayerState.Jump:

                //Horizontal Inputs
                v_xInput = (int)Input.GetAxis("Horizontal");

                if (CheckFloor())
                {
                    ChangePlayerState(v_playerState, S_PlayerState.Idle);
                }

                if (v_xInput < 0)
                {
                    v_isStatic = false;
                    v_xInput = -1;
                    v_playerDirection = S_Direction.Left;
                }
                else if (v_xInput > 0)
                {
                    v_isStatic = false;
                    v_xInput = 1;
                    v_playerDirection = S_Direction.Right;
                }

                if (v_xInput != -1 && v_xInput != 1)
                {
                    v_isStatic = true;
                    v_xInput = 0;
                }

                //Jump Inputs
                JumpStateMachine();

                //Shield Inputs
                i_shield.ShieldStateMachine();

                break;

            case S_PlayerState.Die:
                i_rb.bodyType = RigidbodyType2D.Static;

                if (Manager.instance.v_currentHealth <= 0)
                {
                    vAux_DamageTimer -= Time.unscaledDeltaTime;

                    //ACERCAR LA CAMERA Y RELANTIZAR TIEMPO
                    LevelManager.instance.F_GameOver();

                    if (vAux_DamageTimer <= 0)
                    {
                        if (!vAux_deadActivated)
                        {
                            i_sound.PlayDead();
                            i_renderer.sortingOrder = 50;
                            ChangeAnimation(S_PlayerAnimation.Death);
                            i_animator.updateMode = AnimatorUpdateMode.UnscaledTime;
                            MusicManager.instance.ChangeSong(i_gameOverClip);
                            vAux_deadActivated = true;
                        }
                        vAux_DeadTimer -= Time.deltaTime;
                    }
                }
                else
                {
                    vAux_DamageTimer -= Time.deltaTime;
                    if (vAux_DamageTimer <= 0)
                    {
                        vAux_DamageTimer = 0.6f;
                        ChangePlayerState(v_playerState, S_PlayerState.Spawn);
                    }
                }

                break;
        }
    }

    private void JumpStateMachine()
    {
        //Debug.Log("JUMP STATE: " + v_jumpState);

        switch (v_jumpState)
        {
            case S_JumpState.Grounded:

                //To Single State
                if (Input.GetButtonDown("Jump"))
                {
                    ChangePlayerState(v_playerState, S_PlayerState.Jump);
                    ChangeJumpState(v_jumpState, S_JumpState.Single);
                }

                //To Fall State
                if (!CheckFloor())
                {
                    ChangeJumpState(v_jumpState, S_JumpState.Fall);
                    ChangePlayerState(v_playerState, S_PlayerState.Jump);
                }

                break;
            case S_JumpState.Single:

                if (i_rb.velocity.y < 0 && v_currentAnimation != S_PlayerAnimation.ParryJump)
                {
                    if (i_shield.InHand() && i_shield.v_active)
                        ChangeAnimation(S_PlayerAnimation.Fall);
                    else
                        ChangeAnimation(S_PlayerAnimation.FallNoShield);

                    i_rb.velocity += Vector2.up * Physics2D.gravity.y * 1.05f * Time.deltaTime;
                }

                if (!(vAux_jumpNearWall && ((v_xInput == 1 && vAux_rightWall) || (v_xInput == -1 && vAux_leftWall))))
                {
                    HorizontalMovement();
                    JumpMovement();
                }

                //To Double State
                if (Input.GetButtonDown("Jump"))
                {
                    ChangeJumpState(v_jumpState, S_JumpState.Double);
                }

                //To Grounded State
                if (CheckFloor())
                {
                    ChangeJumpState(v_jumpState, S_JumpState.Grounded);

                    if (v_isStatic)
                        ChangePlayerState(v_playerState, S_PlayerState.Idle);
                    else
                        ChangePlayerState(v_playerState, S_PlayerState.Run);
                }

                //To Climb State
                if (CheckWalls() && !vAux_jumpNearWall)
                {
                    ChangeJumpState(v_jumpState, S_JumpState.Climb);
                }

                if (vAux_jumpNearWall && v_xInput != 0 && i_rb.velocity.y <= 0)
                {
                    vAux_jumpNearWall = false;
                }

                if (i_rb.velocity.y <= -v_maxFallSpeed)
                    i_rb.velocity = new Vector2(i_rb.velocity.x, -v_maxFallSpeed);

                break;
            case S_JumpState.Double:

                if (i_rb.velocity.y < 0 && v_currentAnimation != S_PlayerAnimation.ParryJump)
                {
                    if (i_shield.InHand() && i_shield.v_active)
                        ChangeAnimation(S_PlayerAnimation.Fall);
                    else
                        ChangeAnimation(S_PlayerAnimation.FallNoShield);

                    i_rb.velocity += Vector2.up * Physics2D.gravity.y * 1.05f * Time.deltaTime;
                }

                if (!(vAux_jumpNearWall && ((v_xInput == 1 && vAux_rightWall) || (v_xInput == -1 && vAux_leftWall))))
                {
                    HorizontalMovement();
                    JumpMovement();
                }
                //To Grounded State
                if (CheckFloor())
                {
                    ChangeJumpState(v_jumpState, S_JumpState.Grounded);

                    if (v_isStatic)
                        ChangePlayerState(v_playerState, S_PlayerState.Idle);
                    else
                        ChangePlayerState(v_playerState, S_PlayerState.Run);
                }

                //To Climb State
                if (CheckWalls() && !vAux_jumpNearWall)
                {
                    ChangeJumpState(v_jumpState, S_JumpState.Climb);
                }

                if (vAux_jumpNearWall && v_xInput != 0 && i_rb.velocity.y <= 0)
                {
                    vAux_jumpNearWall = false;
                }

                if (i_rb.velocity.y <= -v_maxFallSpeed)
                    i_rb.velocity = new Vector2(i_rb.velocity.x, -v_maxFallSpeed);

                break;
            case S_JumpState.Climb:

                //Climb Movement
                //To Wall Jump
                WallBehaviour();

                //To Grounded State
                if (CheckFloor())
                {
                    ChangeJumpState(v_jumpState, S_JumpState.Grounded);

                    if (v_isStatic)
                        ChangePlayerState(v_playerState, S_PlayerState.Idle);
                    else
                        ChangePlayerState(v_playerState, S_PlayerState.Run);
                }

                //To Fall State
                if (!CheckFloor() && !CheckWalls())
                {
                    ChangeJumpState(v_jumpState, S_JumpState.Fall);
                }

                WallSlideParticle();

                break;
            case S_JumpState.WallJump:

                vAux_WallJumpMoveTimer += Time.deltaTime;

                if (vAux_WallJumpMoveTimer >= v_wallJumpLength)
                {
                    vAux_WallJumpMoveTimer = 0;
                    //Debug.Log("TIMER OUT");
                    JumpMovement();
                }

                //To Climb State
                if (CheckWalls())
                {
                    ChangeJumpState(v_jumpState, S_JumpState.Climb);
                }

                if (CheckFloor())
                {
                    ChangeJumpState(v_jumpState, S_JumpState.Grounded);

                    if (v_isStatic)
                        ChangePlayerState(v_playerState, S_PlayerState.Idle);
                    else
                        ChangePlayerState(v_playerState, S_PlayerState.Run);
                }

                if (i_rb.velocity.y < 0)
                    i_rb.velocity += Vector2.up * Physics2D.gravity.y * 1.05f * Time.deltaTime;

                if (i_rb.velocity.y <= -10 && v_currentAnimation != S_PlayerAnimation.ParryJump)
                {
                    if (i_shield.InHand() && i_shield.v_active)
                        ChangeAnimation(S_PlayerAnimation.Fall);
                    else
                        ChangeAnimation(S_PlayerAnimation.FallNoShield);
                }

                if (i_rb.velocity.y <= -v_maxFallSpeed)
                    i_rb.velocity = new Vector2(i_rb.velocity.x, -v_maxFallSpeed);

                break;
            case S_JumpState.Fall:

                HorizontalMovement();
                JumpMovement();

                //To Climb State
                if (CheckWalls())
                {
                    ChangeJumpState(v_jumpState, S_JumpState.Climb);
                }

                if (CheckFloor())
                {
                    ChangeJumpState(v_jumpState, S_JumpState.Grounded);

                    if (v_isStatic)
                        ChangePlayerState(v_playerState, S_PlayerState.Idle);
                    else
                        ChangePlayerState(v_playerState, S_PlayerState.Run);
                }

                if (Input.GetButtonDown("Jump"))
                {
                    ChangeJumpState(v_jumpState, S_JumpState.Double);
                }

                if (i_rb.velocity.y <= -v_maxFallSpeed)
                    i_rb.velocity = new Vector2(i_rb.velocity.x, -v_maxFallSpeed);

                break;
        }
    }

    #endregion

    #region ChangeState

    public void ChangePlayerState(S_PlayerState currentState, S_PlayerState nextState)
    {
        switch (currentState)
        {
            case S_PlayerState.Idle:
                switch (nextState)
                {
                    case S_PlayerState.Run:

                        i_sound.PlayRun();

                        if (i_shield.InHand() && i_shield.v_active)
                        {
                            if (i_shield.v_shieldState == S_ShieldState.Protect)
                                ChangeAnimation(S_PlayerAnimation.WalkBlock);
                            else
                                ChangeAnimation(S_PlayerAnimation.Run);
                        }
                        else
                            ChangeAnimation(S_PlayerAnimation.RunNoShield);

                        v_isStatic = false;
                        v_playerState = nextState;

                        break;
                    case S_PlayerState.Jump:

                        v_playerState = nextState;

                        break;
                    case S_PlayerState.Die:
                        i_sound.PlayDamage();

                        if (i_shield.InHand() && i_shield.v_active)
                            ChangeAnimation(S_PlayerAnimation.Damage);
                        else
                            ChangeAnimation(S_PlayerAnimation.DamageNoShield);

                        GetDamage();
                        v_playerState = nextState;

                        break;
                }
                break;

            case S_PlayerState.Run:
                switch (nextState)
                {
                    case S_PlayerState.Idle:
                        i_sound.Stop();

                        if (i_shield.InHand() && i_shield.v_active)
                        {
                            if (i_shield.v_shieldState == S_ShieldState.Protect)
                                ChangeAnimation(S_PlayerAnimation.IdleBlockDirect);
                            else
                                ChangeAnimation(S_PlayerAnimation.Idle);
                        }
                        else
                            ChangeAnimation(S_PlayerAnimation.IdleNoShield);

                        v_isStatic = true;
                        v_playerState = nextState;

                        break;
                    case S_PlayerState.Jump:

                        v_playerState = nextState;

                        break;
                    case S_PlayerState.Die:
                        i_sound.PlayDamage();

                        if (i_shield.InHand() && i_shield.v_active)
                            ChangeAnimation(S_PlayerAnimation.Damage);
                        else
                            ChangeAnimation(S_PlayerAnimation.DamageNoShield);

                        GetDamage();
                        v_playerState = nextState;

                        break;
                }
                break;

            case S_PlayerState.Jump:

                switch (nextState)
                {
                    case S_PlayerState.Idle:

                        v_isStatic = true;
                        ChangeJumpState(v_jumpState, S_JumpState.Grounded);
                        v_playerState = nextState;

                        if (Input.GetButton("Protect"))
                            i_shield.ChangeShieldState(i_shield.v_shieldState, S_ShieldState.Protect);

                        /*if (i_shield.InHand())
                        {
                            if (i_shield.v_shieldState == S_ShieldState.Protect)
                                ChangeAnimation(S_PlayerAnimation.IdleBlock);
                            else
                                ChangeAnimation(S_PlayerAnimation.Idle);
                        }
                        else
                            ChangeAnimation(S_PlayerAnimation.IdleNoShield);*/

                        break;
                    case S_PlayerState.Run:
                        i_sound.PlayRun();

                        ChangeJumpState(v_jumpState, S_JumpState.Grounded);
                        v_playerState = nextState;

                        if (Input.GetButton("Protect"))
                            i_shield.ChangeShieldState(i_shield.v_shieldState, S_ShieldState.Protect);

                        if (i_shield.InHand() && i_shield.v_active)
                        {
                            if (i_shield.v_shieldState == S_ShieldState.Protect)
                                ChangeAnimation(S_PlayerAnimation.WalkBlock);
                            else
                                ChangeAnimation(S_PlayerAnimation.Run);
                        }
                        else
                            ChangeAnimation(S_PlayerAnimation.RunNoShield);

                        break;
                    case S_PlayerState.Die:
                        i_sound.PlayDamage();

                        if (i_shield.InHand() && i_shield.v_active)
                            ChangeAnimation(S_PlayerAnimation.Damage);
                        else
                            ChangeAnimation(S_PlayerAnimation.DamageNoShield);

                        GetDamage();
                        v_playerState = nextState;
                        ChangeJumpState(v_jumpState, S_JumpState.Grounded);

                        break;
                }
                break;

            case S_PlayerState.Spawn:
                if (nextState == S_PlayerState.Idle)
                {
                    if (i_shield.InHand() && i_shield.v_active)
                        ChangeAnimation(S_PlayerAnimation.Idle);
                    else
                        ChangeAnimation(S_PlayerAnimation.IdleNoShield);

                    v_isStatic = true;
                    v_playerState = nextState;

                }
                break;

            case S_PlayerState.Die:
                if (nextState == S_PlayerState.Spawn)
                {
                    vAux_deadActivated = false;
                    i_sound.Stop();

                    ChangeAnimation(S_PlayerAnimation.Fall);

                    i_rb.velocity = Vector2.zero;
                    Time.timeScale = 1;
                    v_playerState = nextState;

                    if (Manager.instance.v_currentHealth > 0)
                        LevelManager.instance.Respawn(this.gameObject);
                    else
                    {
                        LevelManager.instance.RespawnToBegining(this.gameObject);
                    }
                }
                break;
        }
    }

    private void ChangeJumpState(S_JumpState currentState, S_JumpState nextState)
    {
        switch (currentState)
        {
            case S_JumpState.Grounded:
                switch (nextState)
                {
                    case S_JumpState.Single:
                        i_sound.PlayJump();

                        if (CheckWalls())
                            vAux_jumpNearWall = true;

                        if (i_shield.InHand() && i_shield.v_active)
                            ChangeAnimation(S_PlayerAnimation.Jump);
                        else
                            ChangeAnimation(S_PlayerAnimation.JumpNoShield);

                        v_jumpState = nextState;

                        Jump();
                        break;
                    case S_JumpState.Fall:
                        i_sound.Stop();

                        if (i_shield.InHand() && i_shield.v_active)
                            ChangeAnimation(S_PlayerAnimation.Fall);
                        else
                            ChangeAnimation(S_PlayerAnimation.FallNoShield);

                        v_jumpState = nextState;
                        Gravity();
                        break;
                }
                break;

            case S_JumpState.Single:
                switch (nextState)
                {
                    case S_JumpState.Climb:
                        i_sound.PlayWallSlide();

                        if (i_shield.InHand() && i_shield.v_active)
                            ChangeAnimation(S_PlayerAnimation.Climb);
                        else
                            ChangeAnimation(S_PlayerAnimation.ClimbNoShield);

                        v_jumpState = nextState;

                        i_rb.velocity = Vector2.down * v_baseWallFall;
                        CancelInvoke("Gravity");
                        i_rb.gravityScale = 0;
                        break;
                    case S_JumpState.Double:
                        i_sound.PlayJump();

                        if (CheckWalls())
                            vAux_jumpNearWall = true;

                        if (i_shield.InHand() && i_shield.v_active)
                            ChangeAnimation(S_PlayerAnimation.DoubleJump);
                        else
                            ChangeAnimation(S_PlayerAnimation.DoubleJumpNoShield);

                        v_jumpState = nextState;

                        Jump();
                        break;
                    case S_JumpState.Grounded:
                        i_sound.PlayCrouch();

                        if (i_shield.InHand() && i_shield.v_active)
                            ChangeAnimation(S_PlayerAnimation.Crouch);
                        else
                            ChangeAnimation(S_PlayerAnimation.CrouchNoShield);

                        v_jumpState = nextState;
                        JumpParticle(i_particleJump);
                        break;
                }
                break;

            case S_JumpState.Double:
                switch (nextState)
                {
                    case S_JumpState.Climb:
                        i_sound.PlayWallSlide();

                        if (i_shield.InHand() && i_shield.v_active)
                            ChangeAnimation(S_PlayerAnimation.Climb);
                        else
                            ChangeAnimation(S_PlayerAnimation.ClimbNoShield);

                        v_jumpState = nextState;
                        i_rb.velocity = Vector2.down * v_baseWallFall;
                        CancelInvoke("Gravity");
                        i_rb.gravityScale = 0;
                        break;
                    case S_JumpState.Grounded:
                        i_sound.PlayCrouch();

                        if (i_shield.InHand() && i_shield.v_active)
                            ChangeAnimation(S_PlayerAnimation.Crouch);
                        else
                            ChangeAnimation(S_PlayerAnimation.CrouchNoShield);

                        v_jumpState = nextState;
                        JumpParticle(i_particleJump);
                        break;
                }
                break;

            case S_JumpState.Climb:

                switch (nextState)
                {
                    case S_JumpState.Fall:
                        i_sound.Stop();

                        if (i_shield.InHand() && i_shield.v_active)
                            ChangeAnimation(S_PlayerAnimation.Fall);
                        else
                            ChangeAnimation(S_PlayerAnimation.FallNoShield);

                        v_jumpState = nextState;
                        Gravity();

                        vAux_exitWallTimer = v_exitWallTime;

                        break;
                    case S_JumpState.WallJump:
                        i_sound.PlayJump();

                        if (i_shield.InHand() && i_shield.v_active)
                            ChangeAnimation(S_PlayerAnimation.Jump);
                        else
                            ChangeAnimation(S_PlayerAnimation.JumpNoShield);

                        v_jumpState = nextState;
                        Gravity();

                        vAux_exitWallTimer = v_exitWallTime;
                        vAux_WallJumpMoveTimer = 0;
                        WallJump();

                        if (v_playerDirection == S_Direction.Right)
                            v_playerDirection = S_Direction.Left;
                        else
                            v_playerDirection = S_Direction.Right;

                        break;
                    case S_JumpState.Grounded:
                        i_sound.PlayCrouch();

                        if (i_shield.InHand() && i_shield.v_active)
                            ChangeAnimation(S_PlayerAnimation.Crouch);
                        else
                            ChangeAnimation(S_PlayerAnimation.CrouchNoShield);

                        v_jumpState = nextState;
                        Gravity();

                        vAux_exitWallTimer = v_exitWallTime;
                        JumpParticle(i_particleJump);
                        break;
                }
                break;

            case S_JumpState.WallJump:
                switch (nextState)
                {
                    case S_JumpState.Climb:
                        i_sound.PlayWallSlide();

                        if (i_shield.InHand() && i_shield.v_active)
                            ChangeAnimation(S_PlayerAnimation.Climb);
                        else
                            ChangeAnimation(S_PlayerAnimation.ClimbNoShield);

                        v_jumpState = nextState;
                        i_rb.velocity = Vector2.down * v_baseWallFall;
                        CancelInvoke("Gravity");
                        i_rb.gravityScale = 0;

                        break;
                    case S_JumpState.Grounded:
                        i_sound.PlayCrouch();

                        if (i_shield.InHand() && i_shield.v_active)
                            ChangeAnimation(S_PlayerAnimation.Crouch);
                        else
                            ChangeAnimation(S_PlayerAnimation.CrouchNoShield);

                        v_jumpState = nextState;
                        Gravity();

                        vAux_exitWallTimer = v_exitWallTime;
                        JumpParticle(i_particleJump);
                        break;
                }
                break;

            case S_JumpState.Fall:
                switch (nextState)
                {
                    case S_JumpState.Climb:
                        i_sound.PlayWallSlide();

                        if (i_shield.InHand() && i_shield.v_active)
                            ChangeAnimation(S_PlayerAnimation.Climb);
                        else
                            ChangeAnimation(S_PlayerAnimation.ClimbNoShield);

                        v_jumpState = nextState;
                        i_rb.velocity = Vector2.down * v_baseWallFall;
                        CancelInvoke("Gravity");
                        i_rb.gravityScale = 0;
                        break;
                    case S_JumpState.Double:
                        i_sound.PlayJump();

                        if (CheckWalls())
                            vAux_jumpNearWall = true;

                        if (i_shield.InHand() && i_shield.v_active)
                            ChangeAnimation(S_PlayerAnimation.DoubleJump);
                        else
                            ChangeAnimation(S_PlayerAnimation.DoubleJumpNoShield);

                        v_jumpState = nextState;

                        Jump();

                        break;
                    case S_JumpState.Grounded:
                        i_sound.Stop();

                        if (i_shield.InHand() && i_shield.v_active)
                            ChangeAnimation(S_PlayerAnimation.Crouch);
                        else
                            ChangeAnimation(S_PlayerAnimation.CrouchNoShield);

                        v_jumpState = nextState;
                        JumpParticle(i_particleJump);
                        break;
                }
                break;
        }
    }
    #endregion

    #region Movement

    public void Knockback(int direction)
    {
        ChangeAnimation(S_PlayerAnimation.BlockProjectile);

        vAux_knocback = true;
        i_rb.velocity = Vector2.zero;
        i_rb.AddForce(new Vector2(direction * v_knockbackForce, 0));

        v_speed = 0;

        Invoke("Freeze", 0.5f);
    }

    private void Freeze()
    {
        #region Animation
        if (v_playerState == S_PlayerState.Idle)
        {
            if (i_shield.InHand() && i_shield.v_active)
            {
                if (i_shield.v_shieldState == S_ShieldState.Protect)
                    ChangeAnimation(S_PlayerAnimation.IdleBlockDirect);
                else
                    ChangeAnimation(S_PlayerAnimation.Idle);
            }
            else
                ChangeAnimation(S_PlayerAnimation.IdleNoShield);
        }

        else if (v_playerState == S_PlayerState.Run)
        {
            if (i_shield.InHand() && i_shield.v_active)
            {
                if (i_shield.v_shieldState == S_ShieldState.Protect)
                    ChangeAnimation(S_PlayerAnimation.WalkBlock);
                else
                    ChangeAnimation(S_PlayerAnimation.Run);
            }
            else
                ChangeAnimation(S_PlayerAnimation.RunNoShield);
        }
        #endregion

        v_speed = i_shield.v_shieldState == S_ShieldState.Protect ? i_shield.v_blockingSpeed : vAux_TemporalSpeed;

        vAux_knocback = false;
    }

    private void HorizontalMovement()
    {
        if (v_speed != 0 && !vAux_shieldFall && vAux_CanMove)
            i_rb.velocity = new Vector2((v_xInput * v_speed), i_rb.velocity.y);
    }

    private void JumpMovement()
    {
        if (vAux_CanMove)
        {
            if (!vAux_shieldFall)
            {
                if (Mathf.Abs(i_rb.velocity.x + v_xInput * v_speed) > v_speed)
                    i_rb.velocity = new Vector2((v_xInput * v_speed), i_rb.velocity.y);
                else
                    i_rb.velocity += new Vector2((v_xInput * v_speed), 0);
            }
        }
    }

    private void FallWhenCollidingWithShield()
    {
        if (i_rb.velocity.y + (v_gravity / 25) <= v_gravity)
        {
            i_rb.velocity = new Vector2(0, i_rb.velocity.y);
            i_rb.velocity += Vector2.down * (v_gravity / 25);
        }
    }

    private void Jump()
    {
        if (v_jumpState == S_JumpState.Single)
        {
            JumpParticle(i_particleJump);
        }
        else if (v_jumpState == S_JumpState.Double)
        {
            JumpParticle(i_particleDoubleJump);
        }

        CancelInvoke("Gravity");

        i_rb.velocity = new Vector2(i_rb.velocity.x, 0);

        Physics2D.gravity = Vector2.zero;

        transform.position += new Vector3(0, 0.15f, 0);

        i_rb.AddForce(new Vector2(0, v_jumpForce));

        i_rb.velocity = new Vector2(i_rb.velocity.x, 0.1f);

        Invoke("Gravity", v_jumpLength);
    }

    private void WallMove()
    {
        if (vAux_exitWallTimer < 0)
        {
            vAux_exitWallTimer = v_exitWallTime;

            transform.position += new Vector3((v_xInput * 0.15f), 0, 0);

            ChangeJumpState(v_jumpState, S_JumpState.Fall);
            JumpMovement();
        }
        else
        {
            if ((v_xInput == -1 && vAux_rightWall) || (v_xInput == 1 && vAux_leftWall))
                vAux_exitWallTimer -= Time.deltaTime;

            if (Input.GetButtonDown("Jump"))
            {
                vAux_exitWallTimer = v_exitWallTime;
                ChangeJumpState(v_jumpState, S_JumpState.WallJump);
            }
        }
    }

    private void WallJump()
    {
        CancelInvoke("Gravity");

        i_rb.velocity = new Vector2((vAux_rightWall ? -1 : 1) * 0.1f, 0.1f);
        transform.position += new Vector3((vAux_rightWall ? -1 : 1) * 0.15f, 0, 0);

        if (vAux_rightWall)
            i_rb.AddForce(new Vector2(-1 * v_jumpForce / 2 * v_wallJumpForce.x, v_jumpForce / 2 * v_wallJumpForce.y));
        else
            i_rb.AddForce(new Vector2(v_jumpForce / 2 * v_wallJumpForce.x, v_jumpForce / 2 * v_wallJumpForce.y));

        Invoke("Gravity", v_wallJumpLength);
    }

    private void WallBehaviour()
    {
        WallMove();

        i_rb.velocity += new Vector2(0, i_rb.velocity.y * v_wallFallAcceleration * Time.deltaTime);
    }

    #endregion

    #region Collision

    private bool CheckFloor() //Returns if player collided with the ground
    {
        RaycastHit2D[] hit = new RaycastHit2D[3];

        hit[0] = Physics2D.Raycast(new Vector2(i_myCollider.bounds.min.x, i_myCollider.bounds.min.y), -Vector2.up, 0.1f);
        hit[1] = Physics2D.Raycast(new Vector2(transform.position.x, i_myCollider.bounds.min.y), -Vector2.up, 0.1f);
        hit[2] = Physics2D.Raycast(new Vector2(i_myCollider.bounds.max.x, i_myCollider.bounds.min.y), -Vector2.up, 0.1f);

        foreach (RaycastHit2D ray in hit)
        {
            if (ray.collider != null && ray.collider.tag != "Projectile")
            {
                if (ray.collider.tag == "Shield")
                {
                    if (ray.collider.GetComponent<Shield>().v_shieldState != S_ShieldState.Parry)
                    {
                        vAux_CanMove = true;
                        return true;
                    }
                }
                else
                {
                    vAux_CanMove = true;
                    return true;
                }
            }
        }
        return false;
    }

    private bool CheckWalls()
    {
        RaycastHit2D[] rightHit = new RaycastHit2D[5];

        rightHit[0] = Physics2D.Raycast(new Vector2(i_myCollider.bounds.max.x, i_myCollider.bounds.max.y), Vector2.right, 0.1f);
        rightHit[2] = Physics2D.Raycast(new Vector2(i_myCollider.bounds.max.x, i_myCollider.bounds.center.y), Vector2.right, 0.1f);
        rightHit[3] = Physics2D.Raycast(new Vector2(i_myCollider.bounds.max.x, i_myCollider.bounds.min.y), Vector2.right, 0.1f);
        rightHit[1] = Physics2D.Raycast(new Vector2(i_myCollider.bounds.max.x, i_myCollider.bounds.center.y + (i_myCollider.size.y * .35f)), Vector2.right, 0.1f);
        rightHit[4] = Physics2D.Raycast(new Vector2(i_myCollider.bounds.max.x, i_myCollider.bounds.center.y - (i_myCollider.size.y * .25f)), Vector2.right, 0.1f);

        RaycastHit2D[] leftHit = new RaycastHit2D[5];

        leftHit[0] = Physics2D.Raycast(new Vector2(i_myCollider.bounds.min.x, i_myCollider.bounds.max.y), Vector2.left, 0.1f);
        leftHit[2] = Physics2D.Raycast(new Vector2(i_myCollider.bounds.min.x, i_myCollider.bounds.center.y), Vector2.left, 0.1f);
        leftHit[3] = Physics2D.Raycast(new Vector2(i_myCollider.bounds.min.x, i_myCollider.bounds.min.y), Vector2.left, 0.1f);
        leftHit[1] = Physics2D.Raycast(new Vector2(i_myCollider.bounds.min.x, i_myCollider.bounds.center.y + (i_myCollider.size.y * .35f)), Vector2.left, 0.1f);
        leftHit[4] = Physics2D.Raycast(new Vector2(i_myCollider.bounds.min.x, i_myCollider.bounds.center.y - (i_myCollider.size.y * .25f)), Vector2.left, 0.1f);

        vAux_rightWall = false;
        vAux_leftWall = false;

        for (int i = 0; i < 5; i++)
        {
            if (rightHit[i].collider != null)
            {
                if (rightHit[i].collider.tag == "Wall" || rightHit[i].collider.tag == "Indestructible")
                {
                    if (rightHit[i].collider.bounds.min.y != i_myCollider.bounds.max.y)
                    {
                        if (i > 1)
                        {
                            vAux_CanMove = false;

                            return false;
                        }
                        else
                        {
                            vAux_CanMove = true;
                            vAux_rightWall = true;
                            return true;
                        }
                    }
                    else
                    {
                        Gravity();
                    }
                }
                else if ((rightHit[i].collider.tag == "Shield" || rightHit[i].collider.tag == "Floor") && !CheckFloor())
                {
                    this.transform.position -= new Vector3(0.2f, 0, 0);
                }
            }

            if (leftHit[i].collider != null)
            {
                if (leftHit[i].collider.tag == "Wall" || leftHit[i].collider.tag == "Indestructible")
                {
                    if (leftHit[i].collider.bounds.min.y != i_myCollider.bounds.max.y)
                    {
                        if (i > 1)
                        {
                            vAux_CanMove = false;

                            return false;
                        }
                        else
                        {
                            vAux_leftWall = true;
                            return true;
                        }
                    }
                    else
                    {
                        Gravity();
                    }
                }
                else if ((leftHit[i].collider.tag == "Shield" || leftHit[i].collider.tag == "Floor") && !CheckFloor())
                {
                    this.transform.position += new Vector3(0.2f, 0, 0);
                }
            }
        }

        vAux_CanMove = true;

        return false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Projectile" && vAux_canGetDamage)
        {
            if (i_shield.v_shieldState != S_ShieldState.Protect)
            {
                ChangePlayerState(v_playerState, S_PlayerState.Die);
                ChangeJumpState(v_jumpState, S_JumpState.Grounded);

                Destroy(collision.gameObject);
            }
            else
            {
                if (v_playerDirection == S_Direction.Left && collision.transform.position.x > transform.position.x ||
                    v_playerDirection == S_Direction.Right && collision.transform.position.x < transform.position.x)
                {
                    ChangePlayerState(v_playerState, S_PlayerState.Die);
                    ChangeJumpState(v_jumpState, S_JumpState.Grounded);

                    Destroy(collision.gameObject);
                }
            }

        }

        if (collision.tag == "Spikes" || collision.tag == "ProjectileSpawner")
        {
            ChangePlayerState(v_playerState, S_PlayerState.Die);
            ChangeJumpState(v_jumpState, S_JumpState.Grounded);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Shield")
        {
            if (collision.collider.bounds.max.y >= i_myCollider.bounds.min.y)
            {
                if (v_jumpState != S_JumpState.Grounded)
                {
                    v_speed = 0;
                    vAux_shieldFall = true;
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.tag == "Shield")
        {
            vAux_shieldFall = false;

            v_speed = vAux_TemporalSpeed;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.transform.tag == "Shield")
        {
            if (v_jumpState == S_JumpState.Grounded)
            {
                v_speed = vAux_TemporalSpeed;

                vAux_shieldFall = false;
            }
        }
    }

    #endregion

    #region Parry

    public void Parry(S_Parry parryType)
    {
        switch (parryType)
        {
            case S_Parry.GroundHorizontal:

                #region Invincibility

                vAux_canGetDamage = false;
                Invoke("CancelInvincibility", 0.4f);

                #endregion

                #region Effects

                if (shakeCameraCoroutine != null)
                    StopCoroutine(shakeCameraCoroutine);

                shakeCameraCoroutine = StartCoroutine(CameraShake.instance.Shake(0.2f, 0.5f));

                if (stopTimeCoroutine != null)
                    StopCoroutine(stopTimeCoroutine);

                stopTimeCoroutine = StartCoroutine(CAuxiliarFunctions.Couroutine_SlowTime(v_parryTime));

                #endregion

                break;

            //case S_Parry.GroundVertical:
            //    //NOTHING
            //    break;

            case S_Parry.JumpHorizontal:
                #region Invincibility

                vAux_canGetDamage = false;
                Invoke("CancelInvincibility", 0.4f);

                #endregion

                Jump();

                #region Effects

                if (shakeCameraCoroutine != null)
                    StopCoroutine(shakeCameraCoroutine);

                shakeCameraCoroutine = StartCoroutine(CameraShake.instance.Shake(0.2f, 0.5f));

                if (stopTimeCoroutine != null)
                    StopCoroutine(stopTimeCoroutine);

                stopTimeCoroutine = StartCoroutine(CAuxiliarFunctions.Couroutine_SlowTime(v_parryTime));

                #endregion

                break;

            case S_Parry.JumpVertical:

                #region Invincibility

                vAux_canGetDamage = false;
                Invoke("CancelInvincibility", 0.4f);

                #endregion

                Jump();

                #region Effects

                if (shakeCameraCoroutine != null)
                    StopCoroutine(shakeCameraCoroutine);

                shakeCameraCoroutine = StartCoroutine(CameraShake.instance.Shake(0.2f, 0.5f));

                if (stopTimeCoroutine != null)
                    StopCoroutine(stopTimeCoroutine);

                stopTimeCoroutine = StartCoroutine(CAuxiliarFunctions.Couroutine_SlowTime(v_parryTime));

                #endregion

                break;
        }
    }

    #endregion

    #region Functions

    private void GetDamage()
    {
        #region Effects
        StartCoroutine(CameraShake.instance.Shake(0.3f, 0.4f));
        //Vibration.instance.StartVibration(0.5f, 0.5f, 0.2f);
        #endregion

        Manager.instance.DecreaseCurrentHealth();
        vAux_canGetDamage = false;

        Invoke("CancelInvincibility", 0.4f);
    }

    private void Gravity()
    {
        Physics2D.gravity = new Vector2(0, -v_gravity);
        i_rb.gravityScale = 1;
    }

    private void CancelInvincibility()
    {
        vAux_canGetDamage = true;
    }

    #region Animations

    public void ChangeAnimation(S_PlayerAnimation _animation)
    {
        if (NotShieldActionAnimation(v_currentAnimation))
            v_lastAnimation = v_currentAnimation;

        if (!((v_lastAnimation == S_PlayerAnimation.Damage && _animation == S_PlayerAnimation.Crouch)
            || (v_lastAnimation == S_PlayerAnimation.DamageNoShield && _animation == S_PlayerAnimation.CrouchNoShield)))
        {
            v_currentAnimation = _animation;
            i_animator.Play(_animation.ToString());
        }
    }

    private void LoadLastAnimation()
    {
        bool skip = false;
        #region Exeptions
        if (v_lastAnimation == S_PlayerAnimation.Crouch)
            v_lastAnimation = S_PlayerAnimation.Idle;
        else if (v_lastAnimation == S_PlayerAnimation.CrouchNoShield)
            v_lastAnimation = S_PlayerAnimation.IdleNoShield;

        if (v_lastAnimation == S_PlayerAnimation.Run && v_currentAnimation == S_PlayerAnimation.Idle)
            v_lastAnimation = S_PlayerAnimation.Idle;
        else if (v_lastAnimation == S_PlayerAnimation.Run && v_currentAnimation == S_PlayerAnimation.IdleNoShield)
            v_lastAnimation = S_PlayerAnimation.IdleNoShield;
        else if (v_lastAnimation == S_PlayerAnimation.Idle && v_currentAnimation == S_PlayerAnimation.Run)
            v_lastAnimation = S_PlayerAnimation.Run;
        else if (v_lastAnimation == S_PlayerAnimation.Idle && v_currentAnimation == S_PlayerAnimation.RunNoShield)
            v_lastAnimation = S_PlayerAnimation.RunNoShield;

        if ((i_shield.v_shieldState == S_ShieldState.Released || i_shield.v_shieldState == S_ShieldState.Platform)
            && v_lastAnimation == S_PlayerAnimation.Idle)
        {
            if (v_playerState == S_PlayerState.Run)
                v_lastAnimation = S_PlayerAnimation.RunNoShield;
            else if (v_playerState == S_PlayerState.Idle)
                v_lastAnimation = S_PlayerAnimation.IdleNoShield;
            else
                skip = true;
        }
        else if ((i_shield.v_shieldState == S_ShieldState.Released || i_shield.v_shieldState == S_ShieldState.Platform)
            && v_lastAnimation == S_PlayerAnimation.Run)
        {
            if (v_playerState == S_PlayerState.Run)
                v_lastAnimation = S_PlayerAnimation.RunNoShield;
            else if (v_playerState == S_PlayerState.Idle)
                v_lastAnimation = S_PlayerAnimation.IdleNoShield;
            else
                skip = true;
        }
        #endregion

        if (!skip)
        {
            v_currentAnimation = v_lastAnimation;
            i_animator.Play(v_lastAnimation.ToString());
        }
    }

    public void ChangeToLastAnimation(float time)
    {
        Invoke("LoadLastAnimation", time);
    }

    public void CancelChangeToLastAnimation()
    {
        CancelInvoke("LoadLastAnimation");
    }

    private bool NotShieldActionAnimation(S_PlayerAnimation _animation)
    {
        if (_animation != S_PlayerAnimation.Throw && _animation != S_PlayerAnimation.ParryGround && _animation != S_PlayerAnimation.ParryJump
            && _animation != S_PlayerAnimation.IdleBlock && _animation != S_PlayerAnimation.IdleBlockDirect && _animation != S_PlayerAnimation.WalkBlock && _animation != S_PlayerAnimation.BlockProjectile)
            return true;
        else
            return false;
    }

    #endregion

    #region Particles

    private void JumpParticle(GameObject particleType)
    {
        GameObject particle = Instantiate(particleType);

        if (particleType == i_particleDoubleJump)
            particle.transform.position = new Vector3(i_myCollider.bounds.center.x, i_myCollider.bounds.min.y + 0.45f, 0);
        else
            particle.transform.position = new Vector3(i_myCollider.bounds.center.x, i_myCollider.bounds.min.y, 0);
    }

    private void RunParticle()
    {
        vAux_runParticleTimer += Time.deltaTime;

        if (vAux_runParticleTimer > 0.2f)
        {
            vAux_runParticleTimer = 0;

            GameObject particle = Instantiate(i_particleJump);

            particle.transform.position = new Vector3(i_myCollider.bounds.center.x, i_myCollider.bounds.min.y, 0);
        }
    }

    private void WallSlideParticle()
    {
        GameObject particle = Instantiate(i_wallSlide);
        GameObject particle2 = Instantiate(i_wallSlide);


        if (i_renderer.flipX)
        {
            particle.transform.position = new Vector3(i_myCollider.bounds.max.x, i_myCollider.bounds.min.y, 0);
            particle2.transform.position = new Vector3(i_myCollider.bounds.max.x, i_myCollider.bounds.max.y - 0.2f, 0);
        }
        else
        {
            particle.transform.position = new Vector3(i_myCollider.bounds.min.x, i_myCollider.bounds.min.y, 0);
            particle2.transform.position = new Vector3(i_myCollider.bounds.min.x, i_myCollider.bounds.max.y - 0.2f, 0);
        }
    }

    #endregion

    #endregion
}

