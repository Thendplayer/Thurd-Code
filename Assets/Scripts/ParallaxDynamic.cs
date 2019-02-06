using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxDynamic : MonoBehaviour
{

    public float scrollSpeed;

    private Vector3 startPosition;


    public Transform[] spriteTransforms;
    public Transform finalPosition;


    private Transform auxTransform;

    [SerializeField] private float v_offsetX;

    void Start()
    {

        startPosition = transform.position;

    }

    void Update()
    {
        //float newPosition = Mathf.Repeat(Time.time * scrollSpeed, tileSizeX);
        //transform.position = startPosition + Vector3.left * newPosition;

        foreach (Transform t in spriteTransforms)
        {
            t.position = new Vector3(t.position.x - scrollSpeed * Time.deltaTime, t.position.y, t.position.z);
        }


        if (spriteTransforms[0].position.x <= finalPosition.position.x)
        {
            spriteTransforms[0].position = new Vector3(spriteTransforms[spriteTransforms.Length - 1].position.x + v_offsetX, spriteTransforms[0].position.y, spriteTransforms[0].position.z);
            auxTransform = null;
            for (int i = 0; i < spriteTransforms.Length - 1; i++)
            {
                auxTransform = spriteTransforms[i + 1];
                spriteTransforms[i + 1] = spriteTransforms[i];
                spriteTransforms[i] = auxTransform;
            }
        }


    }
}
