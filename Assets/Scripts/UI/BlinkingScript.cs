using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

#pragma warning disable 649

public class BlinkingScript : MonoBehaviour
{
    [SerializeField] private float blinkFrequency;
    [SerializeField] private int blinkCount;
    [SerializeField] private bool isCountdown;
    [SerializeField] private bool isStageClear;
    [SerializeField] private bool isMissilePowerIncreased;
    private TMP_Text tmpText;
    private float blinkTimer = 0f;
    private bool blinkBool = false;
    private GameObject player;

    private void Start()
    {
        tmpText = transform.GetComponent<TMP_Text>();
        if(isMissilePowerIncreased)
            player = GameObject.Find("Player");
    }

    private void Update()
    {
        Blinking();
        Move();
    }

    // Since there are many texts in the UI that blinks, this method (or rather script) encompasses them all.
    private void Blinking()
    {
        blinkTimer += Time.deltaTime;
        if(blinkTimer >= blinkFrequency)
        {
            blinkTimer = 0f;
            if(blinkCount > 0)
            {
                // If blinkBool is false, don't show the text, if it's true - show the text. Simple!
                switch(blinkBool)
                {
                    case false:
                        tmpText.enabled = false;
                        break;
                    case true:
                        // This is used on the countdown script when the game starts, so we have to account for the number shown.
                        if(isCountdown)
                            tmpText.text = (blinkCount/2).ToString();
                        tmpText.enabled = true;
                        break;
                }
                --blinkCount;
                blinkBool = !blinkBool;
            } else
            {
                // Blinking ends. If this script is attached to the stage clear text, reactivate the stage. If it's attached to the countdown, initialize the stage.
                if(isStageClear)
                    ManagerScript.instance.Reactivate();
                if(isCountdown)
                    ManagerScript.instance.InitializeStage();
                Destroy(transform.gameObject);
            }

        }
    }

    // If the text is the "MISSILE POWER INCREASED" text, it has to follow the player.
    private void Move()
    {
        if(isMissilePowerIncreased)
        {
            transform.position = new Vector2(player.transform.position.x, player.transform.position.y - 1.4f);
        }
    }
}

#pragma warning restore 649
