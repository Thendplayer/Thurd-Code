using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSpawner : MonoBehaviour
{
    public S_Direction s_direction = S_Direction.Left;
    public float v_currentDirectionInDegree = 0f;
    public float v_maxRotationInDegree = 0f;
    [SerializeField] private float v_smooth = 0f;
    public bool v_isEnemy = false;
    private float t;
    [Space(10)]
    [SerializeField] private Player I_player;

    [Space(10)]
    [SerializeField] private Projectile I_projectile;
    [SerializeField] private SpriteRenderer v_spriteRenderer;
    [SerializeField] private Animator v_animator;
    [SerializeField] private Transform v_spawnPoint;
    [SerializeField] private Transform v_spawnVector;

    [SerializeField] private Collider2D v_collider;





    [Space(10)]
    [SerializeField] float v_shakeLength = 0.4f;
    [SerializeField] float v_shakeMagnitude = 0.7f;

    [Space(20)]
    [HideInInspector] public bool v_hasWaves = false;

    [HideInInspector] public float v_intervalBetweenProjectiles;
    [HideInInspector] public float v_intervalBetweenWaves;

    [HideInInspector] public int v_numberOfProjectilesPerWave;
    [HideInInspector] public int v_numberOfLifes;

    [HideInInspector] public bool v_isDead;

    #region Setters
    public void SetIntervalBetweenProjectiles(float _intervalBetweemProjectiles)
    {
        v_intervalBetweenProjectiles = _intervalBetweemProjectiles;
    }
    public void SetIntervalBetweenWaves(float _intervalBetweenWaves)
    {
        v_intervalBetweenWaves = _intervalBetweenWaves;
    }
    public void SetNumberOfProjectilesPerWave(int _numberOfProjectilesPerWave)
    {
        v_numberOfProjectilesPerWave = _numberOfProjectilesPerWave;
    }
    public void SetWaves(bool _hasWaves)
    {
        v_hasWaves = _hasWaves;
    }
    #endregion


    private bool vAux_hasFlipped = false;
    private void Start()
    {
        I_projectile.I_player = I_player;

        Initializate();

    }

    private void Update()
    {
        RotateUntilMaxDirection();
    }
    private void StartAnimationAndThenInstantiateProjectile()
    {
        if(v_animator != null)
        {
            v_animator.Play("fire");
        }

        if (this.tag != "Indestructible")
            Invoke("InstantiateProjectile", 0.2f);
        else
            Invoke("InstantiateProjectile", 0.25f);
    }

    private void InstantiateProjectile()
    {
        Projectile l_toSpawn = I_projectile;
        l_toSpawn.s_direction = this.s_direction;
        if(s_direction == S_Direction.Custom)
        {
            l_toSpawn.v_forward = (v_spawnVector.transform.position - this.transform.position).normalized;
        }
        l_toSpawn.transform.rotation = this.transform.rotation;

        GameObject l_obj = Instantiate(l_toSpawn.gameObject, SpawnPoint(), Quaternion.identity);
        l_obj.transform.rotation = l_toSpawn.transform.rotation;
    }

    private void InstantiateWave()
    {
        for (int i = 0; i < v_numberOfProjectilesPerWave; i++)
        {
            Invoke("StartAnimationAndThenInstantiateProjectile", v_intervalBetweenProjectiles * i);
        }
    }

    public void Die()
    {
        v_isDead = true;
        v_spriteRenderer.enabled = false;
        v_collider.enabled = false;
        CancelInvoke();

        StartCoroutine(CameraShake.instance.Shake(v_shakeLength, v_shakeMagnitude));
        
    }

    public void Revive()
    {
        v_isDead = false;
        v_spriteRenderer.enabled = true;
        v_collider.enabled = true;

        Initializate();
    }

    public void ReviveWithDelay(float time)
    {
        Invoke("Revive", time);
    }

    public void Initializate()
    {
        if (v_hasWaves)
        {
            InvokeRepeating("InstantiateWave", 2f, v_intervalBetweenWaves);
        }
        else
        {
            InvokeRepeating("StartAnimationAndThenInstantiateProjectile", 2f, v_intervalBetweenProjectiles);
        }
    }

    private Vector3 SpawnPoint()
    {
        Vector3 l_spawnPoint = v_spawnPoint.position;

        switch (s_direction)
        {
            case S_Direction.Left:
                l_spawnPoint -= new Vector3(I_projectile.i_renderer.bounds.size.x / 4, 0, 0);
                break;

            case S_Direction.Right:
                l_spawnPoint += new Vector3(I_projectile.i_renderer.bounds.size.x / 4, 0, 0);
                break;

            case S_Direction.Up:
                l_spawnPoint += new Vector3(0, I_projectile.i_renderer.bounds.size.y / 4, 0);
                break;

            case S_Direction.Down:
                l_spawnPoint -= new Vector3(0, I_projectile.i_renderer.bounds.size.y / 4, 0);
                break;
        }

        return l_spawnPoint;
    }

    private void RotateUntilMaxDirection()
    {
        float vAux_currentRotationZ = transform.rotation.eulerAngles.z % 360;
        if(vAux_currentRotationZ > 180)
        {
            vAux_currentRotationZ = vAux_currentRotationZ - 360;
        }

        transform.Rotate(0, 0, v_smooth * Time.deltaTime);
        if(vAux_currentRotationZ < -v_maxRotationInDegree)
        {
            if(v_smooth < 0)
            {
                v_smooth = -v_smooth;
            }
        }
        if(vAux_currentRotationZ > v_maxRotationInDegree)
        {
            if(v_smooth > 0)
            {
                v_smooth = -v_smooth;
            }
        }
    }
}
