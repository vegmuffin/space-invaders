using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 649

public class ApproachScript : MonoBehaviour
{
    public static ApproachScript instance;

    [SerializeField] private AudioClip approachSound;
    private AudioSource audioSource;
    private float timer = 0f;

    public float frequency = 1f;

    private void Awake()
    {
        instance = this;    
    }

    private void Start()
    {
        audioSource = transform.GetComponent<AudioSource>();
    }

    private void Update()
    {
        PlayAudio();
    }

    // The spaceship approach sound has to have a separate audio source because we are modifying its pitch.
    public void PlayAudio()
    {
        timer += Time.deltaTime;
        if(timer >= frequency)
        {
            timer = 0f;
            audioSource.PlayOneShot(approachSound, 0.7f);
            audioSource.pitch -= 0.15f;
            if(audioSource.pitch <= 0.4f)
                audioSource.pitch = 0.85f;
        }
    }
}

#pragma warning restore 649
