using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CAuxiliarFunctions
{
    public static IEnumerator Couroutine_SlowTime(float secondsOfSlow)
    {
        float _pauseTime = secondsOfSlow;
        float _elapsedTime = 0f;
        Time.timeScale = 0.1f;

        while (_elapsedTime < _pauseTime)
        {
            Time.timeScale = Mathf.Lerp(Time.timeScale, 1, _elapsedTime / _pauseTime * 2); //CHANGE THE HARDCODE
            _elapsedTime += Time.fixedDeltaTime;

            yield return 0;
        }

        Time.timeScale = 1;
        yield break;
    }

    public static float SinValue(float amplitude, float frequency)
    {
        return amplitude * Mathf.Sin(frequency * Time.time) * Time.deltaTime; ;
    }

    public static IEnumerator Coroutine_CameraBoss(float orthographicSize)
    {
        while (Camera.main.orthographicSize < orthographicSize)
        {
            Camera.main.orthographicSize += 0.04f;
            yield return null;
        }
        Camera.main.orthographicSize = orthographicSize;
        yield break;
    }
}
