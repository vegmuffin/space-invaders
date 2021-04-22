using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyMissileScript : MonoBehaviour
{
    [HideInInspector] public float speed = 0.2f; // Default values for testing purposes
    [HideInInspector] public int whichLine = 1;

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
        Vector2 newPos = new Vector2(transform.position.x, transform.position.y - speed);
        transform.position = newPos;
        
        // Visuals for different missiles: 3rd line - standard; 2nd line - rotation; 1st line - rotation + trails.
        if(whichLine == 1 || whichLine == 2)
            transform.Rotate(new Vector3(0, 0, 15));
    }

    // If colliding with player - initiate player death. After that, destroy the missile.
    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.name.StartsWith("Player") && !PlayerScript.instance.isDying)
        {
            PlayerScript.instance.Death();
        }
        if(!col.gameObject.name.StartsWith("EnemyShip"))
        {
            // Returning the enemy missile to the pool.
            if(whichLine == 1)
            {
                foreach(TrailRenderer tr in transform.GetComponentsInChildren<TrailRenderer>())
                    tr.enabled = false;
            }
            transform.gameObject.SetActive(false);
            transform.eulerAngles = Vector3.zero;
            transform.position = PoolingScript.instance.enemyMissilePoolPosition;
        }
    }

    // Destorying specific tile.
    private void OnCollisionEnter2D(Collision2D col)
    {
        if(col.collider.gameObject.name.StartsWith("Barrier"))
        {
            Rigidbody2D rb = transform.GetComponent<Rigidbody2D>();
            rb.angularVelocity = 0f;
            rb.velocity = Vector3.zero;
            transform.rotation = Quaternion.Euler(Vector3.zero);

            Tilemap tilemap = col.collider.GetComponent<Tilemap>();
            GridLayout gridLayout = col.collider.GetComponentInParent<GridLayout>();
            Vector3Int cellPosition = gridLayout.WorldToCell(col.contacts[0].point);

            // For fast missiles, there has to be a lot of tilemap checking. :(
            if(!tilemap.HasTile(cellPosition))
            {
                for(float y = -0.2f; y < 0.3f; y += 0.1f)
                {
                    for(float x = -0.2f; x < 0.3f; x += 0.1f)
                    {
                        cellPosition = gridLayout.WorldToCell(new Vector2(col.contacts[0].point.x + x, col.contacts[0].point.y + y));
                        if(tilemap.HasTile(cellPosition))
                        {
                            // Break out conditions.
                            y = 0.3f;
                            x = 0.3f;
                        } 
                    }
                }
            }
            tilemap.SetTile(cellPosition, null);
            
            // Returning the enemy missile to the pool.
            transform.gameObject.SetActive(false);
            transform.eulerAngles = Vector3.zero;
            transform.position = PoolingScript.instance.enemyMissilePoolPosition;
        }
    }
}
