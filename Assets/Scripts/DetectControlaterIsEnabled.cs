using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectControlaterIsEnabled : MonoBehaviour {

    [SerializeField] BoxCollider2D v_trigger;
    [SerializeField] GameObject v_joysticTutorial;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Vibration.GamePadConnected)
        {
            v_trigger.enabled = true;
        }
        else
        {
            v_trigger.enabled = false;
            if (v_joysticTutorial.activeSelf == true)
            {
                v_joysticTutorial.SetActive(false);
            }
        }
    }
}
