using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSound : MonoBehaviour {

    [SerializeField] private AudioSource i_audioSource;
    [SerializeField] private AudioSource i_shildAudioSource;

    [Space(10)]
    [SerializeField] private AudioClip Run;
    [SerializeField] private List<AudioClip> Jump = new List<AudioClip>();
    [SerializeField] private AudioClip Crouch;
    [SerializeField] private AudioClip Block;
    [SerializeField] private AudioClip WallSlide;
    [SerializeField] private AudioClip Parry;
    [SerializeField] private AudioClip ShieldReturn;
    [SerializeField] private List<AudioClip> Damage = new List<AudioClip>();
    [SerializeField] private AudioClip Dead;
    [SerializeField] private AudioClip Throw;
    [SerializeField] private AudioClip Catch;
    [SerializeField] private AudioClip PrepareToBlock;
    [SerializeField] private AudioClip ThrowShield;
    [SerializeField] private AudioClip ReturnShield;

    [Header("Shild")]
    [SerializeField] private AudioClip ShildWall;

    public void PlayRun()
    {
        i_audioSource.Stop();
        i_audioSource.clip = Run;
        i_audioSource.loop = true;
        i_audioSource.Play();
    }

    public void PlayJump()
    {
        AudioClip l_jump = Jump[Random.Range(0, Jump.Count)];

        i_audioSource.Stop();
        i_audioSource.clip = l_jump;
        i_audioSource.loop = false;
        i_audioSource.Play();
    }

    public void PlayShildWall()
    {
        i_shildAudioSource.PlayOneShot(ShildWall);
    }

    public void PlayCrouch()
    {
        i_shildAudioSource.PlayOneShot(Crouch);
    }

    public void PlayBlock()
    {
        i_audioSource.Stop();
        i_audioSource.clip = Block;
        i_audioSource.loop = false;
        i_audioSource.Play();
    }

    public void PlayWallSlide()
    {
        i_audioSource.Stop();
        i_audioSource.clip = WallSlide;
        i_audioSource.loop = true;
        i_audioSource.Play();
    }

    public void PlayParry()
    {
        i_audioSource.Stop();
        i_audioSource.clip = Parry;
        i_audioSource.loop = false;
        i_audioSource.Play();
    }

    public void PlayShieldReturn()
    {
        i_shildAudioSource.PlayOneShot(ShieldReturn);
        i_shildAudioSource.PlayOneShot(ReturnShield);
    }

    public void PlayThrow()
    {
        i_shildAudioSource.PlayOneShot(Throw);
        i_shildAudioSource.PlayOneShot(ThrowShield);
    }

    public void PlayCatch()
    {
        i_shildAudioSource.PlayOneShot(Catch);
    }

    public void PlayPrepareBlock()
    {
        i_shildAudioSource.PlayOneShot(PrepareToBlock);
    }

    public void PlayDamage()
    {
        AudioClip l_damage = Damage[Random.Range(0, Damage.Count)];

        i_audioSource.Stop();
        i_audioSource.clip = l_damage;
        i_audioSource.loop = false;
        i_audioSource.Play();
    }

    public void PlayDead()
    {
        i_audioSource.Stop();
        i_audioSource.clip = Dead;
        i_audioSource.loop = false;
        i_audioSource.Play();
    }

    public void Stop()
    {
        i_audioSource.Stop();
    }
}
