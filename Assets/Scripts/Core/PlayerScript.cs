using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#pragma warning disable 649

public class PlayerScript : MonoBehaviour
{
    public static PlayerScript instance;

    [SerializeField] private float speed;
    [SerializeField] private float missileCooldown;
    [SerializeField] private GameObject missilePrefab;
    [SerializeField] private AudioClip shootAudio;
    [SerializeField] private AudioClip deathAudio;
    private float leftLimit = -28f;
    private float rightLimit = 27f;
    private float shootColdown = 0f;

    [HideInInspector] public bool isDying = false;
    [HideInInspector] public bool rapidFire = false;
    [HideInInspector] public float rapidFireTimer = 0f;
    [HideInInspector] public int missileCount = 0;
    public float missileUpgrade = 1f;
    public int lives = 3;

    private void Awake()
    {
        instance = this;
    }

    private void FixedUpdate()
    {
        Move();
        Shoot();
        RapidFire();
        ShootCooldown();
    }

    // The player can move only to the sides and to a certain limit.
    private void Move()
    {
        if((Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)) && !isDying)
        {
            // Getting the direction.
            float dir = 0;
            if(Input.GetKey(KeyCode.LeftArrow))
                dir = -1f;
            else 
                dir = 1f;

            // If the player position is valid, move it to the new position.
            Vector2 oldPos = transform.position;
            Vector2 newPos = new Vector2(oldPos.x + dir*speed, oldPos.y);
            if(newPos.x > leftLimit && newPos.x < rightLimit) transform.position = newPos;
        }
    }

    // The player can only shoot if the missile count ingame is 0. If rapid fire is enabled, then the shooting cooldown is 0.1.
    private void Shoot()
    {
        if(Input.GetKey(KeyCode.Space) && !isDying && (missileCount == 0 || (rapidFire && shootColdown <= 0f)))
        {
            // Initiating the missiles slightly above the player.
            Vector2 startPos = Vector2.zero;
            
            // If shooting only one missile, set it at the center.
            if(missileUpgrade == 1)
            {
                startPos = new Vector2(transform.position.x, transform.position.y + 1.5f);
                PoolingScript.instance.LaunchAvailableMissile(startPos);
                ++missileCount;
            }
            else
            {
                // If shooting more than one missile, divide the X distance of 2 units into (missileUpgrade-1) parts so that we can set them symetrically.
                float leftEnd = transform.position.x - 1f;
                float rightEnd = transform.position.x + 1f;
                float interval = 2/(missileUpgrade-1);
                for(float f = leftEnd; f <= rightEnd + 0.1f; f += interval)
                {
                    startPos = new Vector2(f, transform.position.y + 1.5f);
                    PoolingScript.instance.LaunchAvailableMissile(startPos);
                    ++missileCount;
                }
            }

            // On shooting we generate sound. When on rapid fire, it gets atrocious with those sounds so the least we can do is take care of the volume.
            if(!rapidFire)
                ManagerScript.instance.PlayAudio(shootAudio, 0.5f);
            else
                ManagerScript.instance.PlayAudio(shootAudio, 0.2f);

            if(rapidFire)
                shootColdown = 0.05f;
        }
    }

    // Dying plays an animation and removes a life and if there are no more lives to spare, game ends.
    public void Death()
    {
        --lives;
        if(lives >= 0)
        {   
            // If we have not run out of lives, put the player into a dying animation.
            ManagerScript.instance.lives[lives].SetActive(false);
            isDying = true;
            transform.GetComponent<Animator>().SetBool("DeathBool", true);
            ManagerScript.instance.PlayAudio(deathAudio, 0.7f);
        } else
        {
            // If we have run out of lives, save the high score and enter the game over scene.
            GameObject.Find("Manager").GetComponent<HighScoreScript>().SaveHighScore(ManagerScript.instance.score);
            SceneManager.LoadScene("GameOverScene");
        }
    }

    // Keeping track of the rapid fire time. Resetting some values when it ends.
    private void RapidFire()
    {
        if(rapidFire)
        {
            rapidFireTimer += Time.deltaTime;
            if(rapidFireTimer >= 5f)
            {
                rapidFire = false;
                rapidFireTimer = 0f;
                ManagerScript.instance.tilemapFlashingScript.ResetColor();
            }
        }
    }

    private void ShootCooldown()
    {
        if(rapidFire)
            if(shootColdown >= 0f)
                shootColdown -= Time.deltaTime;
    }

    // This function is used as an event in the death animation.
    private void OnAnimationEnd()
    {
        // No longer play the animation and reset the position.
        transform.GetComponent<Animator>().SetBool("DeathBool", false);
        isDying = false;
        transform.position = new Vector2(-22, -23);
    }
}

#pragma warning restore 0649
