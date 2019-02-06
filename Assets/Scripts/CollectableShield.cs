using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableShield : MonoBehaviour
{

    [SerializeField] Shield v_shield;
    [SerializeField] GameObject i_light;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            VideoManager.instance.PlayVideo();
            v_shield.v_active = true;

            i_light.SetActive(false);

            Destroy(this.gameObject);
        }
    }

}
