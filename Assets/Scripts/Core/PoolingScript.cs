using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 649

public class PoolingScript : MonoBehaviour
{
    public static PoolingScript instance;

    [SerializeField] private GameObject missilePrefab;
    [SerializeField] private GameObject enemyMissilePrefab;

    [HideInInspector] public Vector2 missilePoolPosition = new Vector2(-150, 0);
    [HideInInspector] public Vector2 enemyMissilePoolPosition = new Vector2(-150, 10);
    [HideInInspector] public List<GameObject> missilePool = new List<GameObject>();
    [HideInInspector] public List<GameObject> enemyMissilePool = new List<GameObject>();

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        CreateMissilePool();
        CreateEnemyMissilePool();
    }

    // Creating two pools of missiles and enemy missiles because instantiating new missiles every time is resource extensive.
    private void CreateMissilePool()
    {
        for(int i = 0; i < 100; ++i)
        {
            GameObject missile = Instantiate(missilePrefab, missilePoolPosition, Quaternion.identity, transform);
            missilePool.Add(missile);
        }
    }

    private void CreateEnemyMissilePool()
    {
        for(int i = 0; i < 100; ++i)
        {
            GameObject enemyMissile = Instantiate(enemyMissilePrefab, enemyMissilePoolPosition, Quaternion.identity, transform);
            enemyMissilePool.Add(enemyMissile);
        }
    }

    // Getting a missile that is not already in use in the stage.
    public void LaunchAvailableMissile(Vector2 position)
    {
        foreach(GameObject missile in missilePool)
        {
            if(!missile.activeSelf)
            {
                missile.transform.position = position;
                missile.SetActive(true);
                break;
            }
        }
    }

    // Getting an enemy missile that is not already in use in the stage.
    public void LaunchAvailableEnemyMissile(Vector2 position, string enemyShipName)
    {
        foreach(GameObject enemyMissile in enemyMissilePool)
        {
            if(!enemyMissile.activeSelf)
            {
                enemyMissile.transform.position = position;
                EnemyMissileScript ems = enemyMissile.GetComponent<EnemyMissileScript>();

                // Missile's speed is dependant on what ship launched it. Also programmed visuals.
                if(enemyShipName.StartsWith("EnemyShip3"))
                {
                    ems.speed = 0.25f;
                    ems.whichLine = 3;
                } else if(enemyShipName.StartsWith("EnemyShip2"))
                {
                    ems.speed = 0.35f;
                    ems.whichLine = 2;
                } else
                {
                    ems.speed = 0.5f;
                    ems.whichLine = 1;
                    foreach(TrailRenderer tr in enemyMissile.transform.GetComponentsInChildren<TrailRenderer>())
                        tr.enabled = true;
                }
                enemyMissile.SetActive(true);
                break;
            }
        }
    }
}

#pragma warning restore 649