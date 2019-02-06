using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

public class Vibration : MonoBehaviour
{
    public static Vibration instance = null;
    public static bool GamePadConnected = false;

    [SerializeField] private CameraMovement i_cameraMovement;

    private float v_xValue = 0;
    private float v_yValue = 0;
    private float v_timer = 0;

    private bool v_vibrate = false;
    private bool v_playerIndexSet = false;
    private bool vAux_cameraBoss = false;

    private PlayerIndex s_playerIndex;

    private GamePadState s_state;
    private GamePadState s_prevState;
    private GamePadState s_testState;

    void Awake()
    {
        if (instance == null) instance = this;
        if (instance != this) Destroy(this);
    }

    void Update()
    {
        // Find a PlayerIndex, for a single player game
        // Will find the first controller that is connected ans use it
        if (!v_playerIndexSet || !s_prevState.IsConnected)
        {
            for (int i = 0; i < 4; ++i)
            {
                PlayerIndex testPlayerIndex = (PlayerIndex)i;
                s_testState = GamePad.GetState(testPlayerIndex);
                if (s_testState.IsConnected)
                {
                    Debug.Log(string.Format("GamePad found {0}", testPlayerIndex));
                    GamePadConnected = true;
                    s_playerIndex = testPlayerIndex;
                    v_playerIndexSet = true;
                }
            }
        }

        s_prevState = s_state;
        s_state = GamePad.GetState(s_playerIndex);

        if (i_cameraMovement)
        {
            if (v_vibrate)
            {
                i_cameraMovement.v_active = false;
                v_timer -= Time.deltaTime;

                if (v_timer <= 0)
                    StopVibration();
            }
            else
                i_cameraMovement.v_active = true;
        }
        else
            StopVibration();
    }

    void FixedUpdate()
    {
        // SetVibration should be sent in a slower rate.
        GamePad.SetVibration(s_playerIndex, v_xValue, v_yValue);
    }

    public void StartVibration(float x, float y, float t)
    {
        v_vibrate = true;
        v_xValue = x;
        v_yValue = y;
        v_timer = t;
    }

    public void StopVibration()
    {
        v_vibrate = false;
        v_xValue = 0;
        v_yValue = 0;
    }

    public void CameraForTheBoss()
    {
        if (!vAux_cameraBoss)
        {
            StartCoroutine(CAuxiliarFunctions.Coroutine_CameraBoss(11f));
            vAux_cameraBoss = true;
        }
    }
}
