using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

#pragma warning disable 649

public class HighScoreScript : MonoBehaviour
{
    [SerializeField] private TMP_Text highScoreText;

    // Saving the high score in a PlayerPrefs file. Not an ideal solution but it gets some job done.
    public void SaveHighScore(int score)
    {
        int oldHighscore = PlayerPrefs.GetInt("highscore", 0);    
        if(score > oldHighscore)
        {
            PlayerPrefs.SetInt("highscore", score);
            highScoreText.text = "HIGH: " + score;
        }
    }

    // On awake, get the high score from PlayerPrefs.
    private void Awake()
    {
        int highScore = PlayerPrefs.GetInt("highscore");
        highScoreText.text = "HIGH: " + highScore;
    }
}

#pragma warning restore 649
