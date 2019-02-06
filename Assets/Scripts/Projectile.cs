using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : Damage
{
    public SpriteRenderer i_renderer;

    [HideInInspector] public Vector3 v_forward = Vector3.zero;

    private bool v_reflected = false;
    private bool v_oscillating = false;
    private bool vAux_dead = false;

    [SerializeField] private float v_speedAccelerationPerFrameWhenReflected;
    [SerializeField] private float v_speedMaxWhenReflected;
    [SerializeField] private float v_speed;
    [SerializeField] private float v_oscillationFrequency;
    [SerializeField] private float v_oscillationAmplitude;
    [SerializeField] private ProjectileSound i_sound;
    [SerializeField] private BoxCollider2D i_collider;

    [HideInInspector] public Player I_player;

    [SerializeField] private GameObject explosion;

    public S_Direction s_direction;

    private void Awake()
    {
        switch (s_direction)
        {
            case S_Direction.Up:
                v_forward = Vector3.up;
                //this.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 270));
                transform.forward = v_forward;
                break;
            case S_Direction.Down:
                v_forward = Vector3.down;
                //this.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90));
                transform.forward = v_forward;
                break;
            case S_Direction.Left:
                v_forward = Vector3.left;
                transform.forward = v_forward;
                break;
            case S_Direction.Right:
                v_forward = Vector3.right;
                //this.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 180));
                transform.forward = v_forward;
                break;
            case S_Direction.Custom:
                transform.LookAt(this.v_forward);
                transform.forward = v_forward;
                break;
        }
        //transform.LookAt(this.v_forward);

        if (LevelManager.instance != null)
            LevelManager.instance.L_lightingSystem.Add(this.gameObject);
    }

    private void Update()
    {
        if (!vAux_dead)
        {
            if (v_reflected && v_speed < v_speedMaxWhenReflected) //accelerates the speed when its reflected
            {
                v_speed += v_speedAccelerationPerFrameWhenReflected;
            }

            #region Move

            this.transform.position += v_forward * v_speed * Time.deltaTime;

            if (v_reflected && v_oscillating) //Change a little the trajectory of the proj when its rejected by the player
                Oscillate();
            #endregion
        }
        else
            i_renderer.enabled = false;
    }

    public void Reflect(S_Parry parryType) //Changes the direction of the projectile
    {
        i_sound.PlayReflected();
        i_sound.PlayHit();

        switch (parryType)
        {
            case S_Parry.GroundHorizontal:

                if (s_direction == S_Direction.Right || s_direction == S_Direction.Left)
                {
                    v_forward = new Vector3(v_forward.x * -1, v_forward.y, v_forward.z);

                    this.transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0)); //Flips the direction in the HORIZONTAL axis

                    v_oscillating = true;
                }

                break;

            case S_Parry.JumpHorizontal:

                if (s_direction == S_Direction.Right)
                {
                    v_forward = new Vector3(0.5f, -0.5f, v_forward.z);

                    this.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 45)); //Change the direction in the HORIZONTAL axis
                }
                else if (s_direction == S_Direction.Left)
                {
                    v_forward = new Vector3(-0.5f, -0.5f, v_forward.z);

                    this.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 45)); //Change the direction in the HORIZONTAL axis
                }

                break;

            case S_Parry.JumpVertical:

                v_forward = new Vector3(v_forward.x, v_forward.y * -1, v_forward.z);
                if(this.s_direction == S_Direction.Up)
                {
                    this.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 180)); //Flips the direction in the VERTICAL axis
                }
                else
                {
                    this.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90)); //Flips the direction in the VERTICAL axis
                }

                v_oscillating = true;

                break;
        }

        v_reflected = true;
    }

    private void Oscillate() //Oscilates the projectile in the X or Y axis depending on its direction
    {
        Vector3 _newPosition = transform.position;

        if (s_direction == S_Direction.Left || s_direction == S_Direction.Right)
        {
            _newPosition.y += CAuxiliarFunctions.SinValue(v_oscillationAmplitude, v_oscillationFrequency);
            transform.position = new Vector3(this.transform.position.x, _newPosition.y, this.transform.position.z);
        }
        else if (s_direction == S_Direction.Up || s_direction == S_Direction.Down)
        {
            _newPosition.x += CAuxiliarFunctions.SinValue(v_oscillationAmplitude, v_oscillationFrequency);
            transform.position = new Vector3(_newPosition.x, this.transform.position.y, this.transform.position.z);
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Projectile" && v_reflected)//Only the projectile that has been already rejected by the player
        {
            StartCoroutine(CameraShake.instance.Shake(0.2f, 0.5f));

            Destroy(collision.gameObject);
        }

        else if (collision.tag == "Wall")
        {
            Destroy(collision);
        }

        else if (collision.tag == "ProjectileSpawner")
        {
            ProjectileSpawner l_enemy = collision.gameObject.GetComponent<ProjectileSpawner>();

            if (l_enemy.v_isEnemy && this.v_reflected)
            {
                if (l_enemy.v_numberOfLifes > 1)
                {
                    l_enemy.v_numberOfLifes--;
                }
                else
                {
                    l_enemy.Die();
                }

                Destroy(collision);
            }


        }

        else if (collision.tag == "Indestructible" && this.v_reflected)
        {
            Destroy(collision);
        }
    }

    private void Destroy(Collider2D collision)
    {
        i_sound.Stop();
        i_sound.PlayHit();
        vAux_dead = true;

        SpawnParticle(collision);
        i_renderer.enabled = false;
        i_collider.enabled = false;

        Destroy(this.gameObject, 0.6f);
    }

    private void SpawnParticle(Collider2D collision)
    {
        GameObject explode = Instantiate(explosion);

        switch (s_direction)
        {
            case S_Direction.Left:

                explode.transform.position = new Vector3(collision.bounds.max.x, transform.position.y, 0);
                explode.transform.eulerAngles = new Vector3(0, 90, 0);
                break;
            case S_Direction.Right:

                explode.transform.position = new Vector3(collision.bounds.min.x, transform.position.y, 0);
                explode.transform.eulerAngles = new Vector3(180, 90, 0);
                break;
            case S_Direction.Up:

                explode.transform.position = new Vector3(transform.position.x, collision.bounds.min.y, 0);
                explode.transform.eulerAngles = new Vector3(90, 90, 0);
                break;
            case S_Direction.Down:

                explode.transform.position = new Vector3(transform.position.x, collision.bounds.max.y, 0);
                explode.transform.eulerAngles = new Vector3(-90, 90, 0);
                break;
            case S_Direction.Custom:
                {
                    explode.transform.position = new Vector3(collision.bounds.max.x, transform.position.y, 0);
                    explode.transform.eulerAngles = new Vector3(0, 90, 0);
                    break;
                }
        }
    }

    private void OnDestroy()
    {
        if (LevelManager.instance != null && LevelManager.instance.L_lightingSystem.Contains(this.gameObject))
            LevelManager.instance.L_lightingSystem.Remove(this.gameObject);
    }
}
