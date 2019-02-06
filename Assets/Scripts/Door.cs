using UnityEngine;

public class Door : MonoBehaviour {

    [SerializeField] public Collider2D v_trigger;
    [SerializeField] public Collider2D v_collider;

    [SerializeField] public CameraMovement I_camera;
    [SerializeField] public DoorTrigger I_doorTrigger;

    [SerializeField] public bool v_isFakeDoor;
    [SerializeField] public Collider2D v_fakeTrigger;




    public bool isOpen = false;

    private float vAux_firstTime = 0f;
    private float vAux_currentTime = 0f;
    [SerializeField] private float vAux_maxTime = 3f;

    public S_Door v_doorState;



    #region TOREMOVE
    [SerializeField] private GameObject I_sprite;
    #endregion

    private void Update()
    {
        if (!v_isFakeDoor)
        {
            DoorState();
        }
        else if(v_collider.enabled == true || I_sprite.activeSelf == true)
        {
            v_collider.enabled = false;
            I_sprite.SetActive(false);
        }

    }

    private void DoorState()
    {
        switch (v_doorState)
        {
            case S_Door.Closed:
                break;
            case S_Door.Opening:
                if (!I_camera.v_staticNewPos) //checks when the camera has just arrived to the target(this.position)
                {
                    if (TimerWithouTimeScale())
                    {
                        Time.timeScale = 1f;
                        I_camera.ChangeStaticTarget(false);
                        print("ESPUTOFALSO");
                        ChangeDoorState(v_doorState, S_Door.Opened);


                    }
                    //TO DO ANIMACION

                }




                break;
            case S_Door.Opened:
                break;
        }
            
    }

    public void ChangeDoorState(S_Door currentState, S_Door nextState)
    {
        switch(currentState)
        {
            case S_Door.Closed: //nextState is OPENING
                Time.timeScale = 0f;
                I_camera.ChangeStaticTarget(true, this.transform.position);

                v_doorState = nextState;


                v_collider.enabled = false;
                I_sprite.SetActive(false);
                break;

            case S_Door.Opening:
                switch(nextState)
                {
                    case S_Door.Closed:
                        Time.timeScale = 1f;


                        v_doorState = nextState;




                        break;


                    case S_Door.Opened:

                        Time.timeScale = 1f;

                        v_doorState = nextState;
                        break;
                }
                v_doorState = nextState;
                break;

            case S_Door.Opened: //nextState is CLOSED
                Time.timeScale = 1f;
                v_collider.enabled = true;

                I_sprite.SetActive(true);
                v_doorState = nextState;
                break;

        }
    }

    private bool TimerWithouTimeScale()
    {
        if (vAux_firstTime == 0)
        {
            vAux_firstTime = (int)Time.realtimeSinceStartup;
            vAux_maxTime = vAux_maxTime + vAux_firstTime;
        }
        else if (vAux_currentTime >= vAux_maxTime)
        {
            vAux_maxTime = vAux_maxTime - vAux_firstTime;
            vAux_firstTime = 0f;
            vAux_currentTime = 0f;

            return true;
        }
        else
        {
            vAux_currentTime = (int)Time.realtimeSinceStartup;

        }

        return false;

    }

    private void OntriggerExit2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            v_isFakeDoor = false;
            v_collider.enabled = true;
            I_sprite.SetActive(true);
            v_fakeTrigger.enabled = false;
        }
    }
}
