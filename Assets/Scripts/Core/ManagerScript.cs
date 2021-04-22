using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

#pragma warning disable 649

public class ManagerScript : MonoBehaviour
{
    public static ManagerScript instance;

    [SerializeField] private GameObject enemyShipPrefab1;
    [SerializeField] private GameObject enemyShipPrefab2;
    [SerializeField] private GameObject enemyShipPrefab3;
    [SerializeField] private GameObject mothershipPrefab;
    private float staticMoveCooldown;
    private float checkShipsTimer = 0f;
    private Vector2 leftRayPos = new Vector2(-28f, 24f);
    private Vector2 rightRayPos = new Vector2(28f, 24f);
    private float mothershipTime = 25f;
    private float mothershipTimer = 0f;
    private float specialShipTimer = 0f;
    private AudioSource audioSource;
    
    public GameObject missilePowerIncreasedText;
    [HideInInspector] public int moveMultiplier = 1;
    [HideInInspector] public int score = 0;
    [HideInInspector] public float randomShootingLimit = 0.1f;
    [HideInInspector] public int shipsLeft;
    [HideInInspector] public float moveCooldown = 1f;
    [HideInInspector] public bool checkShipsBool = true;
    [HideInInspector] public List<GameObject> enemyShips = new List<GameObject>();
    [HideInInspector] public List<GameObject> lives = new List<GameObject>();
    [HideInInspector] public FlashingScript tilemapFlashingScript;

    private void Awake()
    {
        instance = this;    
    }

    private void Start()
    {
        staticMoveCooldown = moveCooldown;
        lives.Add(GameObject.Find("Life 1"));
        lives.Add(GameObject.Find("Life 2"));
        lives.Add(GameObject.Find("Life 3"));
        audioSource = transform.GetComponent<AudioSource>();
        tilemapFlashingScript = GameObject.Find("Tilemap").GetComponent<FlashingScript>();
    }

    private void Update()
    {
        CheckShips();
        Mothership();
        SpecialShip();
    }

    // Creating ships and setting some needed variables as the game goes on.
    public void InitializeStage()
    {
        int x = 0;
        for(int i = -24; i <= 24; i += 2)
        {
            for(int y = 5; y <= 17; y += 3)
            {
                // Choosing the correct prefab based on what is the current loop 'y' coordinate.
                GameObject shipPrefab;
                if(y == 11 || y == 14) shipPrefab = enemyShipPrefab2;
                else if (y == 17) shipPrefab = enemyShipPrefab1;
                else shipPrefab = enemyShipPrefab3;

                // Creating the ship.
                Vector2 shipPos = new Vector2(i, y);
                GameObject ship = Instantiate(shipPrefab, shipPos, Quaternion.identity);
                enemyShips.Insert(x, ship);
                ++x;
            }
        }
        shipsLeft = x;
        ApproachScript.instance.enabled = true;
    }

    // Reactivating the stage after it has been cleared.
    public void Reactivate()
    {
        // Resetting some values.
        moveCooldown = staticMoveCooldown;
        moveMultiplier = 1;
        randomShootingLimit = 0.1f;
        int x = 0;
        for(int i = -24; i <= 24; i+= 2)
        {
            for(int y = 5; y <= 17; y += 3)
            {
                // Instantiating objects takes a bit more resources so we can just disable the ships instead.
                Vector2 shipPos = new Vector2(i, y);
                enemyShips[x].transform.position = shipPos;
                enemyShips[x].SetActive(true);
                enemyShips[x].GetComponent<BoxCollider2D>().enabled = true;
                ++x;
            }
        }
        shipsLeft = x;

        // Making sure that they are moving into the right direction and that the sprite that they begin with is correct.
        foreach(GameObject enemyShip in enemyShips)
        {
            EnemyShipScript ess = enemyShip.GetComponent<EnemyShipScript>();
            ess.shouldMove = true;
            ess.moveDown = false;
            ess.ResetSprite();
        }
        checkShipsBool = true;
        ApproachScript.instance.enabled = true;
    }

    // Checking if at least 1 ship is at the X limit (which is the condition to move down and change direction).
    private void CheckShips()
    {
        if(checkShipsBool)
        {
            checkShipsTimer += Time.deltaTime;
            if(checkShipsTimer >= moveCooldown)
            {
                // Two raycasts at each side to avoid checking for each ship if they are at the X limit.
                checkShipsTimer = 0f;
                RaycastHit2D rayLeft = Physics2D.Raycast(leftRayPos, -Vector2.up*100f);
                RaycastHit2D rayRight = Physics2D.Raycast(rightRayPos, -Vector2.up*100f);

                // If the left raycast is hitting something when the move multiplier is -1, move the ships down. Alternatively, if the right raycast is hitting something when the move multiplier is 1, move them down as well.
                if((rayLeft.collider.gameObject.name.StartsWith("EnemyShip") && moveMultiplier == -1) || (rayRight.collider.gameObject.name.StartsWith("EnemyShip") && moveMultiplier == 1))
                {
                    moveMultiplier *= -1;
                    foreach(GameObject enemyShip in enemyShips)
                        enemyShip.GetComponent<EnemyShipScript>().moveDown = true;
                }
            }
        }
    }

    // Mothership arrives each 50-60 seconds and gives a lotta points (and more!).
    private void Mothership()
    {
        mothershipTimer += Time.deltaTime;
        if(mothershipTimer >= mothershipTime)
        {
            mothershipTimer = 0f;
            mothershipTime = Random.Range(50f, 60f);
            int ran = Random.Range(1, 2);
            int multiplier = 0;
            Vector2 pos = Vector2.zero;
            switch(ran)
            {
                case 1:
                    pos = new Vector2(-28f, 22f);
                    multiplier = 1;
                    break;
                case 2:
                    pos = new Vector2(28f, 22f);
                    multiplier = -1;
                    break;
            }
            GameObject mothership = Instantiate(mothershipPrefab, pos, Quaternion.identity);
            mothership.GetComponent<MothershipScript>().moveMultiplier = multiplier;
        }
    }

    // Every 10 seconds, select a random ship that would be special for 7 seconds. If player hits the ship while it's in that state, player gains rapid fire.
    private void SpecialShip()
    {
        specialShipTimer += Time.deltaTime;
        if(specialShipTimer >= 10f)
        {
            specialShipTimer = 0f;
            bool exitCondition = false;
            List<GameObject> tempShipList = enemyShips;
            GameObject ship;
            int n = 0;

            // Taking care of edge cases (no ships remaining, ship count is 0, only one ship remaining but it's in a dying state).
            for(int i = 0; i < tempShipList.Count; ++i)
            {
                if(i == tempShipList.Count - 1)
                {
                    if(!tempShipList[i].activeSelf || tempShipList[i].GetComponent<EnemyShipScript>().deathBool)
                        exitCondition = true;
                }
                if(tempShipList[i].activeSelf && !tempShipList[i].GetComponent<EnemyShipScript>().deathBool)
                    break;
            }
            if(tempShipList.Count == 0)
                exitCondition = true;
            
            // Since the ship count is not that big, it takes about ~35 iterations to get to the ship when there is only one left.
            // Considering this fires only each 10 seconds - not a big deal.
            while(!exitCondition)
            {
                ++n;
                ship = tempShipList[Random.Range(0, tempShipList.Count)];
                EnemyShipScript shipScript = ship.GetComponent<EnemyShipScript>();
                if(ship.activeSelf && !shipScript.deathBool && !shipScript.isSpecial)
                {
                    FlashingScript flashingScript = ship.GetComponent<FlashingScript>();
                    flashingScript.colorSaturation = 0.3f;
                    flashingScript.colorBool = true;
                    shipScript.isSpecial = true;
                    exitCondition = true;
                }
            }        
        }
    }

    // Since the sound is 2D, we only need one AudioSource for all sounds except the approaching sound.
    public void PlayAudio(AudioClip audioClip, float volume)
    {
        audioSource.PlayOneShot(audioClip, volume);
    }
}

#pragma warning restore 0649
