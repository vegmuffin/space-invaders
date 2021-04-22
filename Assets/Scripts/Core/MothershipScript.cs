using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

public class MothershipScript : MonoBehaviour
{
    [SerializeField] private AudioClip mothershipSound;
    private float mothershipSoundTimer = 0f;

    [HideInInspector] public bool isDead = false;
    [HideInInspector] public int moveMultiplier = 0;

    private void Update()
    {
        Move();
        MothershipSoundTimer();
    }

    private void Move()
    {
        if(moveMultiplier != 0)
        {
            // Simple moving with x offset, depending on what the multiplier was set to.
            Vector2 newPos = new Vector2(transform.position.x + 0.175f*moveMultiplier, transform.position.y);
            transform.position = newPos;
        }
    }

    // If it collides with the tilemap that's surrounding the entire stage, destroy the mothership.
    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.name.StartsWith("Tilemap"))
        {
            isDead = true;
            Destroy(transform.gameObject);
        }
    }

    // Wonky mothership sound (playing the same high-pitched sound at a high frequency).
    private void MothershipSoundTimer()
    {
        mothershipSoundTimer += Time.deltaTime;
        if(mothershipSoundTimer >= 0.15f)
        {
            ManagerScript.instance.PlayAudio(mothershipSound, 0.3f);
            mothershipSoundTimer = 0f;
        }
    }
}

#pragma warning restore 0649
