using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour
{

    [SerializeField] private ParticleSystem particle;

    void Start()
    {
        Invoke("Destroy", particle.main.duration);
    }

    private void Destroy()
    {
        Destroy(gameObject);
    }
}
