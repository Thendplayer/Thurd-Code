using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CanvasTutorial : MonoBehaviour {


    [SerializeField] protected GameObject v_pauseMenu;

    [SerializeField] protected GameObject v_tutorialCanvas;
    [SerializeField] protected Text v_tutorial1;
    [SerializeField] protected GameObject v_tutorial1Controller;
    [SerializeField] protected GameObject v_tutorial1Keyboard;


    [SerializeField] protected Text v_tutorial2;
    [SerializeField] protected GameObject v_tutorial2Controller;
    [SerializeField] protected GameObject v_tutorial2Keyboard;

    [SerializeField] private EnemyTutorial I_enemy;



    protected bool vAux_controllerConnected;


    // Use this for initialization
    void Start () {

        vAux_controllerConnected = false;

    }
	
	// Update is called once per frame
	void Update () {

        if (I_enemy.vAux_firstTutorial && Time.timeScale == 0f)
        {
            v_tutorialCanvas.SetActive(true);
            v_tutorial1.gameObject.SetActive(true);
            v_tutorial2.gameObject.SetActive(false);
        }
        else if (I_enemy.vAux_secondTutorial && Time.timeScale == 0f)
        {
            v_tutorialCanvas.SetActive(true);
            v_tutorial2.gameObject.SetActive(true);
            vAux_controllerConnected = false;
            v_tutorial1.gameObject.SetActive(false);
        }

        if (v_pauseMenu.activeSelf == true)
        {

            v_tutorialCanvas.SetActive(false);

        }

        #region Controller or Keyboard
        if (I_enemy.vAux_firstTutorial && Time.timeScale == 0f)
        {
            if (vAux_controllerConnected)
            {
                if (Input.GetMouseButton(0) || Input.GetKeyDown(KeyCode.Return))
                {
                    v_tutorial1Controller.SetActive(false);
                    v_tutorial2Keyboard.SetActive(true);
                }
            }
            else
            {
                if (Vibration.GamePadConnected)
                {
                    v_tutorial1Controller.SetActive(true);
                    v_tutorial1Keyboard.SetActive(false);
                    vAux_controllerConnected = true;
                }
            }
        }

        else if (I_enemy.vAux_secondTutorial && Time.timeScale == 0f)
        {
            if (vAux_controllerConnected)
            {
                if (Input.anyKey)
                {
                    v_tutorial2Controller.SetActive(false);
                    v_tutorial2Keyboard.SetActive(true);
                }
            }
            else
            {
                if (Vibration.GamePadConnected)
                {
                    v_tutorial2Controller.SetActive(true);
                    v_tutorial2Keyboard.SetActive(false);
                    vAux_controllerConnected = true;
                }
            }
        }

        
        #endregion
    }
}
