using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTrigger : MonoBehaviour
{

    [SerializeField] public bool v_requeiresShieldAlways;
    [SerializeField] private Door I_door;
    [SerializeField] private Shield I_shield;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Shield" && !v_requeiresShieldAlways && I_shield.v_shieldState == S_ShieldState.Platform || I_shield.v_shieldState == S_ShieldState.Released)
        {
            if (I_door.v_doorState != S_Door.Opened)
            {
                I_door.ChangeDoorState(I_door.v_doorState, S_Door.Opening);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Shield" && v_requeiresShieldAlways && I_shield.v_shieldState == S_ShieldState.Platform)
        {
            if (I_door.v_doorState != S_Door.Opened)
            {
                I_door.ChangeDoorState(I_door.v_doorState, S_Door.Opening);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Shield")
        {
            if (v_requeiresShieldAlways && I_door.v_doorState == S_Door.Opened)
            {
                Debug.LogWarning("CLOSe");
                I_door.ChangeDoorState(I_door.v_doorState, S_Door.Closed);
            }
        }
    }
}
