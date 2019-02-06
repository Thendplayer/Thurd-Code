using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intermitent : MonoBehaviour {
    public SpriteRenderer mylight;
    public float duration = 5;
    Color32 color;
    // Use this for initialization
    void Start () {
        color = mylight.color;
    }
	
	// Update is called once per frame
	void Update () {
        float lerp = Mathf.PingPong(Time.time, duration) / duration;
        float alpha = Mathf.Lerp(50, 255, lerp);
        mylight.color = new Color32(color.r, color.g, color.b, (byte)alpha);
    }
}
