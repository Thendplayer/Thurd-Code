using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heart : Item
{
    public bool isFragment;

    [SerializeField] private GameObject i_heartParticle;
    [SerializeField] private AudioSource i_audioSource;
    [SerializeField] private Collider2D i_collider;
    [SerializeField] private SpriteRenderer i_spriteRenderer;

    private bool vAux_sound = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && !collision.isTrigger && !vAux_sound)
        {
            if (isFragment)
            {
                Manager.instance.AddFragmentOfHeart();

                if (Manager.instance.v_numOfFragmentsForHealthContainer == LevelManager.instance.v_currentHeartFragmentsCollected)
                {
                    Manager.instance.AddHeartContainer();
                    //Manager.instance.L_collectedHearts[LevelManager.instance.LevelNum] = true;
                    LevelManager.instance.L_heartsList.Remove(this);
                }
            }
            else
            {
                Manager.instance.AddHeartContainer();
                //Manager.instance.L_collectedHearts[LevelManager.instance.LevelNum] = true;
                LevelManager.instance.L_heartsList.Remove(this);
            }

            GameObject particle = Instantiate(i_heartParticle);

            particle.transform.position = transform.position + new Vector3(0, 0.25f, 0);

            i_audioSource.Play();
            i_collider.enabled = false;
            i_spriteRenderer.enabled = false;
            vAux_sound = true;
            Destroy(this.gameObject, 0.5f);
        }
    }
}
