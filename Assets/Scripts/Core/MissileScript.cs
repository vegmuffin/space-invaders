using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

#pragma warning disable 649

public class MissileScript : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Sprite deathSprite;
    [SerializeField] private TMP_Text stageClearTextPrefab;
    [SerializeField] private TMP_Text mothershipScoreTextPrefab;
    [SerializeField] private GameObject tilemap;

    private void Awake()
    {
        // Instantly disabling the missile since it's only instantiated when the pool is generated.
        transform.gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        // Simple moving with y offset.
        Vector2 newPos = new Vector2(transform.position.x, transform.position.y + speed);
        transform.position = newPos;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        // Colliding with enemy ship.
        if(col.gameObject.name.StartsWith("EnemyShip") && !col.GetComponent<EnemyShipScript>().deathBool)
        {
            col.GetComponent<BoxCollider2D>().enabled = false;

            // Increase the score.
            EnemyShipScript enemyShipScript = col.GetComponent<EnemyShipScript>();
            ManagerScript.instance.score += enemyShipScript.scoreToGive;
            GameObject.Find("ScoreText").GetComponent<TMP_Text>().text = "SCORE: " + ManagerScript.instance.score;

            // Initiate enemy ship death.
            --ManagerScript.instance.shipsLeft;
            enemyShipScript.isDying = true;
            enemyShipScript.deathBool = true;
            enemyShipScript.shouldMove = false;
            col.GetComponent<SpriteRenderer>().sprite = deathSprite;

            // Game, along with the approach sounds speed up as we kill enemy ships.
            ManagerScript.instance.moveCooldown -= 0.0075f;
            if(ManagerScript.instance.moveCooldown <= 0.03f)
                ManagerScript.instance.moveCooldown = 0.03f;
            ManagerScript.instance.randomShootingLimit += 0.02f;
            ApproachScript.instance.frequency = ManagerScript.instance.moveCooldown;

            // Rapid fire. If already on rapid fire - refresh.
            if(enemyShipScript.isSpecial)
            {
                if(!PlayerScript.instance.rapidFire)
                {
                    enemyShipScript.specialTimer = 8f;
                    PlayerScript.instance.rapidFire = true;
                    ManagerScript.instance.tilemapFlashingScript.colorSaturation = 0.3f;
                    ManagerScript.instance.tilemapFlashingScript.colorBool = true;

                }
                else
                    PlayerScript.instance.rapidFireTimer = 0f;
            }
            
            // If there are no more enemy ships, initiate stage clear text.
            if(ManagerScript.instance.shipsLeft == 0)
            {
                Instantiate(stageClearTextPrefab, Vector3.zero, Quaternion.identity, GameObject.Find("WorldSpaceCanvas").transform);
                ManagerScript.instance.checkShipsBool = false;
                ApproachScript.instance.frequency = 1f;
                ApproachScript.instance.enabled = false;
            }
        } else if(col.gameObject.name.StartsWith("Mothership") && !col.GetComponent<MothershipScript>().isDead)
        {
            col.GetComponent<MothershipScript>().isDead = true;

            // Missile collides with the mothership - adding randomized score as well as flashing it on the screen where the mothership was destroyed.
            int ran = Random.Range(5, 10);
            int scoreAddition = ran*100;
            ManagerScript.instance.score += scoreAddition;
            GameObject.Find("ScoreText").GetComponent<TMP_Text>().text = "SCORE: " + ManagerScript.instance.score;
            TMP_Text text = Instantiate(mothershipScoreTextPrefab, col.transform.position, Quaternion.identity, GameObject.Find("WorldSpaceCanvas").transform);
            text.text = scoreAddition.ToString();
            Destroy(col.gameObject);

            // When hitting the mothership, player gets increased power.
            if(PlayerScript.instance.missileUpgrade < 4)
            {
                Instantiate(ManagerScript.instance.missilePowerIncreasedText, Vector2.zero, Quaternion.identity, GameObject.Find("WorldSpaceCanvas").transform);
                ++PlayerScript.instance.missileUpgrade;
            }
        }
        --PlayerScript.instance.missileCount;

        // Putting the missile back in the pool.
        transform.gameObject.SetActive(false);
        transform.position = PoolingScript.instance.missilePoolPosition;
    }

    // Destroying specific tile from tilemap.
    private void OnCollisionEnter2D(Collision2D col)
    {
        if(col.collider.gameObject.name.StartsWith("Barrier"))
        {
            // Due to the Dynamic rigidbody, the missile slightly rotates itself, so we have to set that back.
            Rigidbody2D rb = transform.GetComponent<Rigidbody2D>();
            rb.angularVelocity = 0f;
            rb.velocity = Vector3.zero;
            transform.rotation = Quaternion.Euler(Vector3.zero);

            Tilemap tilemap = col.collider.GetComponent<Tilemap>();
            GridLayout gridLayout = col.collider.GetComponentInParent<GridLayout>();
            Vector3Int cellPosition = gridLayout.WorldToCell(col.contacts[0].point);

            // For fast missiles, there has to be a lot of tilemap checking :(
            if(!tilemap.HasTile(cellPosition))
            {
                for(float y = -0.2f; y < 0.3f; y += 0.1f)
                {
                    for(float x = -0.2f; x < 0.3f; x += 0.1f)
                    {
                        cellPosition = gridLayout.WorldToCell(new Vector2(col.contacts[0].point.x + x, col.contacts[0].point.y + y));
                        if(tilemap.HasTile(cellPosition))
                        {
                            // Break out conditions
                            y = 0.3f;
                            x = 0.3f;
                        } 
                    }
                }
            }

            tilemap.SetTile(cellPosition, null);
            --PlayerScript.instance.missileCount;

            // Putting the missile back in the pool.
            transform.gameObject.SetActive(false);
            transform.position = PoolingScript.instance.missilePoolPosition;
        }
    }
}

#pragma warning restore 0649
