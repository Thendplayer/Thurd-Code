using UnityEngine;

public class Shield : MonoBehaviour
{
    public bool v_active;

    [SerializeField] private float v_forceAmount = 8f;
    [SerializeField] private float v_speed = 15f;
    [SerializeField] public float v_blockingSpeed = 5f;
    [SerializeField] public float v_decrementSpeed = 4.78f;

    [SerializeField] private BoxCollider2D i_shieldCollider;
    [SerializeField] private BoxCollider2D i_groundParryCollider;
    [SerializeField] private BoxCollider2D i_jumpParryCollider;
    [SerializeField] private SpriteRenderer i_myRenderer;

    [SerializeField] public Player i_player;

    [Header("Parry")]
    private float vAux_currentParryColdown;
    [SerializeField] private float v_coolDownParryTimer = 2f;

    [HideInInspector] public bool vAux_canParry = true;
    private bool vAux_canMove = false;

    private Vector3 v_destination;
    private Vector3 v_playerPosition;
    public Vector3 v_secondPlayerPosition = new Vector3(-1, -0.07f, 0);

    private Coroutine stopTimeCoroutine;
    private Coroutine shakeCameraCoroutine;
    private BoxCollider2D i_currentCollider;

    public S_ShieldState v_shieldState;

    public float vAux_parryLenght = 0.2f;
    public float vAux_parryTimer = 0;
    public float vAux_raycastLenght = 0.2f;

    public GameObject i_colliderParticle;
    public GameObject i_parryParticle;

    private float vAux_originalX;
    private float vAux_releasedShieldTimer = 1.5f;

    [HideInInspector] public bool vAux_firstTutorial = false;

    public TrailRenderer i_trail;

    void Start()
    {
        i_trail = GetComponent<TrailRenderer>();
        i_trail.enabled = false;
        vAux_currentParryColdown = v_coolDownParryTimer;
        i_currentCollider = i_shieldCollider;
        v_shieldState = S_ShieldState.Hide;
        v_playerPosition = transform.localPosition;
        vAux_originalX = this.transform.localPosition.x;
    }

    #region StateMachine

    public void ShieldStateMachine()
    {
        if (v_active)
        {
            switch (v_shieldState)
            {
                case S_ShieldState.Hide:

                    #region Flip

                    if (i_player.v_playerDirection == S_Direction.Right)
                    {
                        this.transform.localPosition = new Vector3(Mathf.Abs(this.transform.localPosition.x), this.transform.localPosition.y, this.transform.localPosition.z);
                        i_groundParryCollider.offset = new Vector2(Mathf.Abs(i_groundParryCollider.offset.x), i_groundParryCollider.offset.y);
                        i_jumpParryCollider.offset = new Vector2(Mathf.Abs(i_jumpParryCollider.offset.x) * -1, i_jumpParryCollider.offset.y);
                    }
                    else
                    {
                        this.transform.localPosition = new Vector3(Mathf.Abs(this.transform.localPosition.x) * -1, this.transform.localPosition.y, this.transform.localPosition.z);
                        i_groundParryCollider.offset = new Vector2(Mathf.Abs(i_groundParryCollider.offset.x) * -1, i_groundParryCollider.offset.y);
                        i_jumpParryCollider.offset = new Vector2(Mathf.Abs(i_jumpParryCollider.offset.x), i_jumpParryCollider.offset.y);
                    }

                    #endregion

                    ParryTimer();
                    transform.rotation = Quaternion.identity;

                    if (!vAux_firstTutorial)
                    {

                        //To Released State
                        if (Input.GetButtonDown("Throw") && i_player.v_jumpState != S_JumpState.Climb && CanThrow())
                        {
                            ChangeShieldState(v_shieldState, S_ShieldState.Released);
                        }

                        //Check Parry
                        if (Input.GetButtonDown("Parry"))
                        {
                            ChangeShieldState(v_shieldState, S_ShieldState.Parry);
                        }
                    }
                    //To Protect State
                    if (Input.GetAxisRaw("Protect") != 0 && (i_player.v_playerState == S_PlayerState.Idle || i_player.v_playerState == S_PlayerState.Run))
                    {
                        ChangeShieldState(v_shieldState, S_ShieldState.Protect);
                    }



                    break;

                case S_ShieldState.Protect:

                    #region Flip

                    if (i_player.v_playerDirection == S_Direction.Right)
                    {
                        this.transform.localPosition = new Vector3(Mathf.Abs(this.transform.localPosition.x), this.transform.localPosition.y, this.transform.localPosition.z);
                        i_groundParryCollider.offset = new Vector2(Mathf.Abs(i_groundParryCollider.offset.x), i_groundParryCollider.offset.y);
                        i_jumpParryCollider.offset = new Vector2(Mathf.Abs(i_jumpParryCollider.offset.x) * -1, i_jumpParryCollider.offset.y);
                    }
                    else
                    {
                        this.transform.localPosition = new Vector3(Mathf.Abs(this.transform.localPosition.x) * -1, this.transform.localPosition.y, this.transform.localPosition.z);
                        i_groundParryCollider.offset = new Vector2(Mathf.Abs(i_groundParryCollider.offset.x) * -1, i_groundParryCollider.offset.y);
                        i_jumpParryCollider.offset = new Vector2(Mathf.Abs(i_jumpParryCollider.offset.x), i_jumpParryCollider.offset.y);
                    }

                    #endregion

                    CheckCollisions();

                    ParryTimer();

                    //To Hide State
                    if (!Input.GetButton("Protect"))
                    {
                        ChangeShieldState(v_shieldState, S_ShieldState.Hide);
                    }

                    if (i_player.v_playerState != S_PlayerState.Idle && i_player.v_playerState != S_PlayerState.Run)
                    {
                        ChangeShieldState(v_shieldState, S_ShieldState.Hide);
                    }

                    break;

                case S_ShieldState.Released:
                    ParryTimer();
                    Move();

                    if (v_speed > 1)
                        v_speed -= v_speed * v_decrementSpeed * Time.deltaTime;

                    vAux_releasedShieldTimer -= Time.deltaTime;
                    if (vAux_releasedShieldTimer <= 0)
                    {
                        vAux_releasedShieldTimer = 1.5f;
                        ChangeShieldState(v_shieldState, S_ShieldState.Hide);
                    }

                    break;

                case S_ShieldState.Platform:
                    ParryTimer();

                    if (Input.GetButtonDown("Throw") || Input.GetButtonDown("Protect") || i_player.v_playerState == S_PlayerState.Spawn)
                    {
                        ChangeShieldState(v_shieldState, S_ShieldState.Returning);
                    }

                    break;

                case S_ShieldState.Returning:
                    ParryTimer();
                    Move();

                    break;

                case S_ShieldState.Parry:

                    #region Flip

                    if (i_player.v_playerDirection == S_Direction.Right)
                    {
                        this.transform.localPosition = new Vector3(Mathf.Abs(this.transform.localPosition.x), this.transform.localPosition.y, this.transform.localPosition.z);
                        i_groundParryCollider.offset = new Vector2(Mathf.Abs(i_groundParryCollider.offset.x), i_groundParryCollider.offset.y);
                        i_jumpParryCollider.offset = new Vector2(Mathf.Abs(i_jumpParryCollider.offset.x) * -1, i_jumpParryCollider.offset.y);
                    }
                    else
                    {
                        this.transform.localPosition = new Vector3(Mathf.Abs(this.transform.localPosition.x) * -1, this.transform.localPosition.y, this.transform.localPosition.z);
                        i_groundParryCollider.offset = new Vector2(Mathf.Abs(i_groundParryCollider.offset.x) * -1, i_groundParryCollider.offset.y);
                        i_jumpParryCollider.offset = new Vector2(Mathf.Abs(i_jumpParryCollider.offset.x), i_jumpParryCollider.offset.y);
                    }

                    #endregion

                    ParryTimer();

                    vAux_parryTimer += Time.deltaTime;

                    if (vAux_parryTimer >= vAux_parryLenght)
                    {
                        ChangeShieldState(v_shieldState, S_ShieldState.Hide);
                        vAux_parryTimer = 0;
                    }
                    break;
            }
        }
    }

    #endregion

    #region ChangeState

    public void ChangeShieldState(S_ShieldState current, S_ShieldState next)
    {
        if (v_active)
        {
            switch (current)
            {
                case S_ShieldState.Hide:

                    switch (next)
                    {
                        case S_ShieldState.Protect:

                            i_player.i_sound.PlayPrepareBlock();

                            if (i_player.v_playerDirection == S_Direction.Right)
                                this.transform.localPosition = new Vector3(vAux_originalX, this.transform.localPosition.y);
                            else
                                this.transform.localPosition = new Vector3(-vAux_originalX, this.transform.localPosition.y);

                            if (!i_player.vAux_knocback)
                                i_player.v_speed = v_blockingSpeed;

                            i_currentCollider = i_shieldCollider;

                            i_currentCollider.enabled = true;

                            if (i_player.v_playerState == S_PlayerState.Idle)
                                i_player.ChangeAnimation(S_PlayerAnimation.IdleBlock);
                            else if (i_player.v_playerState == S_PlayerState.Run)
                                i_player.ChangeAnimation(S_PlayerAnimation.WalkBlock);

                            v_shieldState = next;
                            break;

                        case S_ShieldState.Released:
                            if (CanThrow())
                            {
                                i_player.i_sound.PlayThrow();
                                i_trail.enabled = true;
                                vAux_releasedShieldTimer = 1.5f;
                                i_currentCollider = i_shieldCollider;

                                i_player.ChangeAnimation(S_PlayerAnimation.Throw);
                                i_player.ChangeToLastAnimation(0.32f);

                                Invoke("Throw", 0.24f);

                                v_shieldState = next;
                            }
                            break;

                        case S_ShieldState.Parry:
                            if (i_player.v_playerDirection == S_Direction.Right)
                                this.transform.localPosition = new Vector3(vAux_originalX, this.transform.localPosition.y);
                            else
                                this.transform.localPosition = new Vector3(-vAux_originalX, this.transform.localPosition.y);

                            if (i_player.v_playerState == S_PlayerState.Jump)
                            {
                                i_player.i_sound.PlayParry();
                                i_player.ChangeAnimation(S_PlayerAnimation.ParryJump);
                                i_player.ChangeToLastAnimation(0.25f);

                                i_currentCollider = i_jumpParryCollider;
                                i_currentCollider.enabled = true;

                                v_shieldState = next;
                            }
                            else if (i_player.v_playerState == S_PlayerState.Idle || i_player.v_playerState == S_PlayerState.Run)
                            {
                                i_player.i_sound.PlayParry();
                                i_player.ChangeAnimation(S_PlayerAnimation.ParryGround);
                                i_player.ChangeToLastAnimation(0.25f);

                                if (i_player.v_playerDirection == S_Direction.Right)
                                {
                                    transform.localPosition = v_playerPosition;

                                }
                                else
                                {
                                    transform.localPosition = v_secondPlayerPosition;
                                }
                                i_currentCollider = i_groundParryCollider;
                                i_currentCollider.enabled = true;

                                v_shieldState = next;
                            }
                            break;
                    }

                    break;

                case S_ShieldState.Protect:

                    switch (next)
                    {
                        case S_ShieldState.Hide:

                            if (i_player.v_playerDirection == S_Direction.Right)
                                this.transform.localPosition = new Vector3(vAux_originalX, this.transform.localPosition.y);
                            else
                                this.transform.localPosition = new Vector3(-vAux_originalX, this.transform.localPosition.y);

                            if (!i_player.vAux_knocback)
                                i_player.v_speed = i_player.vAux_TemporalSpeed;

                            i_currentCollider.enabled = false;
                            i_myRenderer.enabled = false;

                            if (i_player.v_playerState == S_PlayerState.Idle)
                                i_player.ChangeAnimation(S_PlayerAnimation.Idle);
                            else if (i_player.v_playerState == S_PlayerState.Run)
                                i_player.ChangeAnimation(S_PlayerAnimation.Run);

                            v_shieldState = next;
                            break;

                        case S_ShieldState.Released:
                            i_trail.enabled = true;

                            vAux_releasedShieldTimer = 1.5f;

                            if (!i_player.vAux_knocback)
                                i_player.v_speed = i_player.vAux_TemporalSpeed;

                            if (Throw())
                                v_shieldState = next;

                            break;
                    }

                    break;

                case S_ShieldState.Released:

                    switch (next)
                    {
                        case S_ShieldState.Platform:

                            v_shieldState = next;
                            break;
                        case S_ShieldState.Returning:

                            Return();
                            v_shieldState = next;
                            break;
                        case S_ShieldState.Hide:

                            if (i_player.v_playerDirection == S_Direction.Right)
                                this.transform.localPosition = new Vector3(vAux_originalX, this.transform.localPosition.y);
                            else
                                this.transform.localPosition = new Vector3(-vAux_originalX, this.transform.localPosition.y);

                            if (i_player.v_playerState == S_PlayerState.Idle)
                                i_player.ChangeAnimation(S_PlayerAnimation.Idle);
                            else if (i_player.v_playerState == S_PlayerState.Run)
                                i_player.ChangeAnimation(S_PlayerAnimation.Run);

                            i_currentCollider = i_shieldCollider;
                            i_currentCollider.enabled = false;
                            i_myRenderer.enabled = false;

                            v_shieldState = next;

                            break;
                    }

                    break;

                case S_ShieldState.Platform:

                    switch (next)
                    {
                        case S_ShieldState.Returning:
                            i_player.i_sound.PlayShieldReturn();
                            Return();
                            i_currentCollider.enabled = false;
                            v_shieldState = next;
                            break;
                    }

                    break;

                case S_ShieldState.Returning:

                    switch (next)
                    {
                        case S_ShieldState.Hide:
                            i_player.i_sound.PlayCatch();
                            i_trail.enabled = false;

                            if (i_player.v_playerDirection == S_Direction.Right)
                                this.transform.localPosition = new Vector3(vAux_originalX, this.transform.localPosition.y);
                            else
                                this.transform.localPosition = new Vector3(-vAux_originalX, this.transform.localPosition.y);

                            if (i_player.v_playerState == S_PlayerState.Idle)
                                i_player.ChangeAnimation(S_PlayerAnimation.Idle);
                            else if (i_player.v_playerState == S_PlayerState.Run)
                                i_player.ChangeAnimation(S_PlayerAnimation.Run);

                            i_currentCollider = i_shieldCollider;
                            i_currentCollider.enabled = false;
                            i_myRenderer.enabled = false;

                            v_shieldState = next;
                            break;

                        case S_ShieldState.Protect:
                            i_player.i_sound.PlayPrepareBlock();
                            i_trail.enabled = false;

                            if (i_player.v_playerDirection == S_Direction.Right)
                                this.transform.localPosition = new Vector3(vAux_originalX, this.transform.localPosition.y);
                            else
                                this.transform.localPosition = new Vector3(-vAux_originalX, this.transform.localPosition.y);

                            if (i_player.v_playerState == S_PlayerState.Idle)
                                i_player.ChangeAnimation(S_PlayerAnimation.IdleBlock);
                            else if (i_player.v_playerState == S_PlayerState.Run)
                                i_player.ChangeAnimation(S_PlayerAnimation.WalkBlock);

                            i_currentCollider = i_shieldCollider;
                            i_currentCollider.enabled = true;
                            i_myRenderer.enabled = false;


                            i_player.v_speed = v_blockingSpeed;

                            v_shieldState = next;
                            break;
                    }

                    break;

                case S_ShieldState.Parry:

                    switch (next)
                    {
                        case S_ShieldState.Hide:
                            if (i_player.v_playerDirection == S_Direction.Right)
                                this.transform.localPosition = new Vector3(vAux_originalX, this.transform.localPosition.y);
                            else
                                this.transform.localPosition = new Vector3(-vAux_originalX, this.transform.localPosition.y);

                            i_currentCollider.enabled = false;
                            v_shieldState = next;
                            break;
                    }

                    break;
            }
        }
    }

    #endregion

    #region Functions

    private bool Throw()
    {
        if (CanThrow())
        {
            i_currentCollider.enabled = true;
            i_myRenderer.enabled = true;

            transform.parent = null;

            vAux_canMove = true;

            this.transform.position += new Vector3(0, 0.88f, 0);
            v_destination = transform.position + (i_player.v_playerDirection == S_Direction.Right ? Vector3.right : Vector3.left) * v_forceAmount;

            if (v_destination.y - Mathf.RoundToInt(v_destination.y) < 0.25f || v_destination.y - Mathf.RoundToInt(v_destination.y) > 0.75f)
                v_destination = new Vector3(v_destination.x, Mathf.RoundToInt(v_destination.y));
            else if (v_destination.y - Mathf.RoundToInt(v_destination.y) < 0.5f)
                v_destination = new Vector3(v_destination.x, Mathf.RoundToInt(v_destination.y) + 0.5f);
            else
                v_destination = new Vector3(v_destination.x, Mathf.RoundToInt(v_destination.y) - 0.5f);

            v_speed = 40;

            transform.Rotate(new Vector3(0, 0, 90));

            vAux_canParry = false;

            return true;
        }
        else
            return false;
    }

    private bool CanThrow()
    {
        Vector2 dir = (i_player.v_playerDirection == S_Direction.Right ? Vector3.right : Vector3.left);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, i_currentCollider.size.y * transform.lossyScale.y);

        return (hit.transform != null && hit.transform.tag != "Player" && hit.transform.gameObject != this.gameObject) ? false : true;
    }

    public void Return()
    {
        vAux_canMove = true;

        i_currentCollider.isTrigger = true;

        transform.Rotate(new Vector3(0, 0, -90));

        transform.parent = i_player.transform;

        v_speed = 75;
    }

    private void Move()
    {
        if (vAux_canMove)
        {
            if (v_shieldState == S_ShieldState.Returning)
            {
                if (i_player.v_playerDirection == S_Direction.Right)
                    v_destination = i_player.transform.position + v_playerPosition;
                else
                    v_destination = i_player.transform.position + new Vector3(-v_playerPosition.x, v_playerPosition.y, v_playerPosition.z);
            }

            transform.position = Vector3.MoveTowards(transform.position, v_destination, v_speed * Time.deltaTime);

            if (transform.position == v_destination)
            {
                vAux_canMove = false;

                if (v_shieldState == S_ShieldState.Returning)
                {
                    if (Input.GetButton("Protect"))
                        ChangeShieldState(v_shieldState, S_ShieldState.Protect);
                    else
                        ChangeShieldState(v_shieldState, S_ShieldState.Hide);
                }
                else
                    ChangeShieldState(v_shieldState, S_ShieldState.Platform);
            }

            if (!vAux_canMove && !InHand())
            {
                if (v_shieldState == S_ShieldState.Returning)
                {
                    vAux_canParry = true;
                }
                else
                    i_currentCollider.isTrigger = false;
            }
        }
        else if (InHand())
        {
            if (i_player.v_playerDirection == S_Direction.Right)
            {
                transform.localPosition = v_playerPosition;
            }
            else
            {
                transform.localPosition = new Vector3(-v_playerPosition.x, v_playerPosition.y, v_playerPosition.z);
            }
        }
    }

    public bool InHand()
    {
        if (v_shieldState == S_ShieldState.Hide || v_shieldState == S_ShieldState.Parry || v_shieldState == S_ShieldState.Protect)
            return true;
        else
            return false;
    }

    #endregion

    #region Collisions

    private void CheckCollisions()
    {
        RaycastHit2D[] hit = new RaycastHit2D[5];

        if (i_player.v_playerDirection == S_Direction.Right)
        {

            Debug.DrawLine(new Vector2(i_currentCollider.bounds.min.x, i_currentCollider.bounds.center.y), new Vector2(i_currentCollider.bounds.min.x + vAux_raycastLenght, i_currentCollider.bounds.center.y));
            Debug.DrawLine(new Vector2(i_currentCollider.bounds.min.x, i_currentCollider.bounds.min.y), new Vector2(i_currentCollider.bounds.min.x + vAux_raycastLenght, i_currentCollider.bounds.min.y));
            Debug.DrawLine(new Vector2(i_currentCollider.bounds.min.x, i_currentCollider.bounds.max.y), new Vector2(i_currentCollider.bounds.min.x + vAux_raycastLenght, i_currentCollider.bounds.max.y));
            Debug.DrawLine(new Vector2(i_currentCollider.bounds.min.x, i_currentCollider.bounds.max.y - (i_currentCollider.size.y * .25f)), new Vector2(i_currentCollider.bounds.min.x + vAux_raycastLenght, i_currentCollider.bounds.max.y - (i_currentCollider.size.y * .25f)));
            Debug.DrawLine(new Vector2(i_currentCollider.bounds.min.x, i_currentCollider.bounds.min.y + (i_currentCollider.size.y * .25f)), new Vector2(i_currentCollider.bounds.min.x + vAux_raycastLenght, i_currentCollider.bounds.min.y + (i_currentCollider.size.y * .25f)));

            hit[0] = Physics2D.Raycast(new Vector2(i_currentCollider.bounds.min.x, i_currentCollider.bounds.max.y), Vector2.right, vAux_raycastLenght);
            hit[1] = Physics2D.Raycast(new Vector2(i_currentCollider.bounds.min.x, i_currentCollider.bounds.center.y), Vector2.right, vAux_raycastLenght);
            hit[2] = Physics2D.Raycast(new Vector2(i_currentCollider.bounds.min.x, i_currentCollider.bounds.min.y), Vector2.right, vAux_raycastLenght);
            hit[3] = Physics2D.Raycast(new Vector2(i_currentCollider.bounds.min.x, i_currentCollider.bounds.max.y - (i_currentCollider.size.y * .25f)), Vector2.right, vAux_raycastLenght);
            hit[4] = Physics2D.Raycast(new Vector2(i_currentCollider.bounds.min.x, i_currentCollider.bounds.min.y + (i_currentCollider.size.y * .25f)), Vector2.right, vAux_raycastLenght);
        }
        else
        {
            hit[0] = Physics2D.Raycast(new Vector2(i_currentCollider.bounds.max.x, i_currentCollider.bounds.max.y), Vector2.left, vAux_raycastLenght);
            hit[1] = Physics2D.Raycast(new Vector2(i_currentCollider.bounds.max.x, i_currentCollider.bounds.center.y), Vector2.left, vAux_raycastLenght);
            hit[2] = Physics2D.Raycast(new Vector2(i_currentCollider.bounds.max.x, i_currentCollider.bounds.min.y), Vector2.left, vAux_raycastLenght);
            hit[3] = Physics2D.Raycast(new Vector2(i_currentCollider.bounds.max.x, i_currentCollider.bounds.max.y - (i_currentCollider.size.y * .75f)), Vector2.left, vAux_raycastLenght);
            hit[4] = Physics2D.Raycast(new Vector2(i_currentCollider.bounds.max.x, i_currentCollider.bounds.min.y - (i_currentCollider.size.y * .25f)), Vector2.left, vAux_raycastLenght);
        }

        for (int i = 0; i < 5; i++)
        {
            if (hit[i].collider != null && hit[i].collider.tag == "Projectile")
            {
                Destroy(hit[i].collider.gameObject);

                //Vibration.instance.StartVibration(0.35f, 0.35f, 0.1f);
                StartCoroutine(CameraShake.instance.Shake(0.2f, 0.4f));
                i_player.Knockback(i_player.v_playerDirection == S_Direction.Right ? -1 : 1);

                CollideParticle(hit[i].collider);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (v_shieldState == S_ShieldState.Released)
        {
            if (collision.tag == "Spikes" || collision.tag == "ProjectileSpawner" || collision.tag == "Indestructible")
            {
                i_player.i_sound.PlayShildWall();
                ChangeShieldState(v_shieldState, S_ShieldState.Returning);
            }
            else if (collision.tag == "Wall" || collision.tag == "Ground")
            {
                float distance;

                if (collision.transform.position.x > transform.position.x)
                {
                    distance = i_currentCollider.bounds.max.x - collision.bounds.min.x;
                    transform.position += Vector3.left * distance;
                }
                else
                {
                    distance = collision.bounds.max.x - i_currentCollider.bounds.min.x;
                    transform.position += Vector3.right * distance;
                }

                i_player.i_sound.PlayShildWall();

                if (transform.position.y - Mathf.RoundToInt(transform.position.y) < 0.25f || transform.position.y - Mathf.RoundToInt(transform.position.y) > 0.75f)
                    v_destination = new Vector3(transform.position.x, Mathf.RoundToInt(transform.position.y));
                else if (transform.position.y - Mathf.RoundToInt(transform.position.y) < 0.5f)
                    v_destination = new Vector3(transform.position.x, Mathf.RoundToInt(transform.position.y) + 0.5f);
                else
                    v_destination = new Vector3(transform.position.x, Mathf.RoundToInt(transform.position.y) - 0.5f);

                PlatformParticle();
            }

        }

        if (v_shieldState == S_ShieldState.Parry)
        {
            if (collision.tag == "Projectile")
            {
                if (i_player.v_playerState == S_PlayerState.Jump) //Parry Jump
                {
                    ParryJump(collision);
                }
                else if (i_player.v_playerState == S_PlayerState.Idle || i_player.v_playerState == S_PlayerState.Run) //Parry Ground
                {
                    ParryGround(collision);
                }
            }
        }
    }

    #endregion

    #region Parry

    private void ParryGround(Collider2D collision)
    {

        #region Projectile
        Projectile projectile = collision.GetComponent<Projectile>();

        if (projectile.s_direction != S_Direction.Up && projectile.s_direction != S_Direction.Down)
        {
            projectile.Reflect(S_Parry.GroundHorizontal);


            ParryParticle(collision);
            #endregion

            #region Player


            if (projectile.s_direction == S_Direction.Right || projectile.s_direction == S_Direction.Left)
                i_player.Parry(S_Parry.GroundHorizontal);
            //else
            //    player.Parry(S_Parry.GroundVertical);

            #endregion

            vAux_currentParryColdown = 0f; //updates the timer because the parry has been done
            i_currentCollider.enabled = false;
        }
    }

    private void ParryJump(Collider2D collision)
    {
        #region Projectile
        Projectile projectile = collision.GetComponent<Projectile>();

        if (projectile.s_direction == S_Direction.Left || projectile.s_direction == S_Direction.Right)
            projectile.Reflect(S_Parry.JumpHorizontal);
        else
            projectile.Reflect(S_Parry.JumpVertical);
        #endregion

        #region Player


        if (projectile.s_direction == S_Direction.Right || projectile.s_direction == S_Direction.Left)
            i_player.Parry(S_Parry.JumpHorizontal);
        else
            i_player.Parry(S_Parry.JumpVertical);

        #endregion


        vAux_currentParryColdown = 0f; //updates the timer because the parry has been done
        i_currentCollider.enabled = false;
    }

    private void ParryTimer()
    {

        if (v_shieldState == S_ShieldState.Parry && vAux_currentParryColdown >= v_coolDownParryTimer)
        {
            vAux_canParry = true;
        }
        else if (v_shieldState != S_ShieldState.Parry)
        {
            vAux_currentParryColdown += Time.deltaTime;
        }
        else if (v_shieldState == S_ShieldState.Parry)
        {
            vAux_currentParryColdown = 0f;
        }
    }

    #endregion

    #region Particle

    private void ParryParticle(Collider2D collision)
    {
        GameObject particle = Instantiate(i_parryParticle);

        if (i_player.v_playerDirection == S_Direction.Right)
        {
            particle.transform.position = new Vector3(i_player.i_myCollider.bounds.max.x + .75f, i_player.i_myCollider.bounds.center.y, transform.position.z);
        }
        else
        {
            particle.transform.position = new Vector3(i_player.i_myCollider.bounds.min.x - .75f, i_player.i_myCollider.bounds.center.y, transform.position.z);
        }
    }

    private void CollideParticle(Collider2D collider)
    {
        GameObject particle = Instantiate(i_colliderParticle);

        if (i_player.v_playerDirection == S_Direction.Right)
        {
            particle.transform.eulerAngles = new Vector3(180, -90, 0);
            particle.transform.position = new Vector3(i_player.i_myCollider.bounds.max.x, collider.transform.position.y, transform.position.z);
        }
        else
        {
            particle.transform.eulerAngles = new Vector3(180, 90, 0);
            particle.transform.position = new Vector3(i_player.i_myCollider.bounds.min.x, collider.transform.position.y, transform.position.z);
        }
    }

    private void PlatformParticle()
    {
        GameObject particle = Instantiate(i_colliderParticle);

        if (i_player.v_playerDirection == S_Direction.Right)
        {
            particle.transform.eulerAngles = new Vector3(180, 90, 0);
            particle.transform.position = new Vector3(i_shieldCollider.bounds.max.x, transform.position.y, transform.position.z);
        }
        else
        {
            particle.transform.eulerAngles = new Vector3(180, -90, 0);
            particle.transform.position = new Vector3(i_shieldCollider.bounds.min.x, transform.position.y, transform.position.z);
        }
    }

    #endregion
}
