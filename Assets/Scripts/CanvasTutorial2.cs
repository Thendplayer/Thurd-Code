using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasTutorial2 : CanvasTutorial
{
    [SerializeField] private BoxCollider2D v_trigger;
    [SerializeField] private Animator v_animator;

    [SerializeField] private float v_timerToShowAgain = 30f;

    private float vAux_currentTimer = 0f;
    private bool vAux_isBeingDisabled = false;
    private bool vAux_canBeDisplayed = true;

    void Update()
    {
        if (vAux_controllerConnected)
        {
            if (Input.GetMouseButton(0) || Input.GetKeyDown(KeyCode.Return))
            {
                v_tutorial1Controller.SetActive(false);
                v_tutorial1Keyboard.SetActive(true);
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

        //CHECK IF MENU ISACTIVE
        if (v_pauseMenu.activeSelf == true && v_tutorialCanvas.activeSelf == true)
        {
            DisableTutorialCanvas();
        }

        //TIMER TO SHOW AGAIN THE MENU
        if (vAux_currentTimer < v_timerToShowAgain && !vAux_canBeDisplayed)
        {
            vAux_currentTimer += Time.deltaTime;
        }
        else
        {
            vAux_currentTimer = 0f;
            vAux_canBeDisplayed = true;
        }
    }

    private void DisableTutorialCanvas()
    {
        v_tutorialCanvas.SetActive(false);
        vAux_isBeingDisabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (v_trigger != null && vAux_canBeDisplayed)
        {
            if (collision.gameObject.tag == "Player" && v_tutorialCanvas.activeSelf == false)
            {
                v_tutorialCanvas.SetActive(true);
                print(vAux_canBeDisplayed);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (v_trigger != null)
        {
            if (collision.gameObject.tag == "Player" && v_tutorialCanvas.activeSelf == true && !vAux_isBeingDisabled)
            {
                CancelInvoke();

                Invoke("DisableTutorialCanvas", 0.3f);

                if (v_animator.runtimeAnimatorController.name == "Tutorial1stPart")
                {
                    v_animator.Play("TutorialMenuClose");
                }
                else
                {
                    v_animator.Play("TutorialMenuClose2ndPart");
                }

                vAux_isBeingDisabled = true;
                vAux_canBeDisplayed = false;
            }
        }
    }
}
