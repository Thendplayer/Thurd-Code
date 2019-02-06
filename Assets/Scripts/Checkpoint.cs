using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public bool v_currentCheckpoint;
    public GameObject i_particles;

    void Awake()
    {
        v_currentCheckpoint = false;
        i_particles.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && !collision.isTrigger)
        {
            foreach (Checkpoint g in LevelManager.instance.L_checkpointList)
            {
                if (g != this)
                {
                    g.v_currentCheckpoint = false;
                    g.i_particles.SetActive(false);
                }
                else
                {
                    v_currentCheckpoint = true;
                    i_particles.SetActive(true);
                }
            }
        }
    }
}
