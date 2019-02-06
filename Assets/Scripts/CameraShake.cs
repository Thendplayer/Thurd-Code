using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{

    public static CameraShake instance = null;

    void Awake()
    {
        if (instance == null) instance = this;
        if (instance != this) Destroy(this);
    }

    public IEnumerator Shake(float _duration, float _magnitude)
    {
        Vector3 _originalPosition = this.transform.localPosition;

        Vibration.instance.StartVibration(_magnitude * 5, _magnitude * 5, _duration);

        float _elapsed = 0;

        while (_elapsed < _duration)
        {


            Vector2 movement = new Vector2(Random.Range(-1f, 1f) * _magnitude, Random.Range(-1f, 1f) * _magnitude);

            this.transform.localPosition += new Vector3(movement.x, movement.y, _originalPosition.z);

            _elapsed += Time.deltaTime;

            yield return null;
            this.transform.localPosition = _originalPosition;

        }

        this.transform.localPosition = _originalPosition;
        yield break;
    }

}
