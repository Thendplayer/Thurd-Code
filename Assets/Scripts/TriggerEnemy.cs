using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerEnemy : MonoBehaviour
{

    public GameObject v_enemy;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (v_enemy != null)
        {
            v_enemy.SetActive(true);

        }


    }
}
