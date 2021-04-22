using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#pragma warning disable 649

public class EnemyShipScript : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float startMoveCooldown;
    [SerializeField] private Sprite sprite1;
    [SerializeField] private Sprite sprite2;
    [SerializeField] private GameObject enemyMissilePrefab;
    [SerializeField] private AudioClip shootAudio;
    private SpriteRenderer spriteRenderer;
    private Vector2 startPosition;
    private float moveCooldownTimer = 0f;
    private bool spriteBool = false;
    private float deathTimer = 0f;

    [HideInInspector] public float specialTimer = 0f;
    [HideInInspector] public bool moveDown = false;
    [HideInInspector] public bool shouldMove = true;
    [HideInInspector] public bool deathBool = false;
    [HideInInspector] public bool isSpecial = false;
    public int scoreToGive;
    public bool isDying = false;

    private void Start()
    {
        startPosition = transform.position;
        spriteRenderer = transform.GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        MoveCooldown();
        Death();
        Shoot();
        Special();
    }

    // Changing the sprite each time the ship moves.
    private void SpriteChange()
    {
        switch(spriteBool)
        {
            case true:
                spriteRenderer.sprite = sprite1;
                break;
            case false:
                spriteRenderer.sprite = sprite2;
                break;
        }
        spriteBool = !spriteBool;
    }

    // After stage clear we are calling this so that all ships change sprites synchronously.
    public void ResetSprite()
    {
        spriteRenderer.sprite = sprite1;
        spriteBool = false;
    }

    // Moving (either down or to the side, never up)
    private void Move()
    {
        if(!deathBool)
        {
            if(!moveDown)
            {
                // Simple moving with x offset, depending on what direction should the ship go.
                Vector2 newPos = new Vector2(transform.position.x + speed * ManagerScript.instance.moveMultiplier, transform.position.y);
                SpriteChange();
                transform.position = newPos;
            } else
            {
                // Dropping the spaceships down by 1 y coordinate.
                Vector2 newYPos = new Vector2(transform.position.x, transform.position.y - 1f);
                transform.position = newYPos;
                moveDown = false;
            }
        }
    }

    // Since ships' movement is not fluid, it has to have a cooldown.
    private void MoveCooldown()
    {
        if(shouldMove)
        {
            moveCooldownTimer += Time.deltaTime;
            if(moveCooldownTimer >= ManagerScript.instance.moveCooldown)
            {
                moveCooldownTimer = 0f;
                Move();
            }
        }
    }

    private void Shoot()
    {
        if(!PlayerScript.instance.isDying)
        {
            // The fewer ships are, the more likely that this ship is going to fire.
            float random = Random.Range(0, 400f);
            if(random <= ManagerScript.instance.randomShootingLimit)
            {
                // Raycast to check if there are ships below this ship. There has to be none for it to shoot.
                Vector2 startPos = new Vector2(transform.position.x, transform.position.y - 2.5f);
                RaycastHit2D ray = Physics2D.Raycast(startPos, -Vector2.up*100f);
                if(ray.collider != null) 
                {
                    // If the raycast doesn't collide with an enemy ship, that means that the path is clear.
                    if(!ray.collider.gameObject.name.StartsWith("EnemyShip"))
                    {
                        PoolingScript.instance.LaunchAvailableEnemyMissile(new Vector2(transform.position.x, transform.position.y -2f), transform.gameObject.name);
                        ManagerScript.instance.PlayAudio(shootAudio, 0.4f);
                    }
                }
            }
        }
    }

    // When dying, show a brief 'explosion' sprite.
    private void Death()
    {
        if(deathBool)
        {
            deathTimer += Time.deltaTime;
            if(deathTimer >= 0.2f)
            {
                deathTimer = 0f;
                deathBool = false;
                spriteRenderer.sprite = sprite1;
                transform.gameObject.SetActive(false);
                isDying = false;
                isSpecial = false;
                specialTimer = 0f;
                transform.GetComponent<FlashingScript>().ResetColor();
                moveCooldownTimer = 0f;
            }
        } 
    }

    // Mostly HSV to RGB visuals for special ships
    private void Special()
    {
        if(isSpecial)
        {
            specialTimer += Time.deltaTime;
            // Special timer is up, revert back to white sprite.
            if(specialTimer >= 7f)
            {
                isSpecial = false;
                transform.GetComponent<FlashingScript>().ResetColor();
                specialTimer = 0f;
            }
        }
    }

    // If ship touches player or the grid surrounding the stage, the game is over.
    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.name.StartsWith("Tilemap") || col.name.StartsWith("Player"))
        {
            GameObject.Find("Manager").GetComponent<HighScoreScript>().SaveHighScore(ManagerScript.instance.score);
            SceneManager.LoadScene("GameOverScene");
        }
    }
}

#pragma warning restore 0649
