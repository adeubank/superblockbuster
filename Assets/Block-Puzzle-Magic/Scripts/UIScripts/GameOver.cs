﻿using UnityEngine;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    [SerializeField] private Text txtBestScore;
    [SerializeField] private Text txtCoinReward;

    [SerializeField] private Text txtScore;

    public void SetLevelScore(int score, int coinReward)
    {
        var bestScore = PlayerPrefs.GetInt("BestScore_" + GameController.gameMode, score);

        if (score >= bestScore)
        {
            PlayerPrefs.SetInt("BestScore_" + GameController.gameMode, score);
            bestScore = score;
        }

        txtScore.text = string.Format("{0:#,#.}", score.ToString("0"));
        txtBestScore.text = string.Format("{0:#,#.}", bestScore.ToString("0"));
        txtCoinReward.text = string.Format("{0:#,#.}", coinReward.ToString("0"));

        CurrencyManager.Instance.AddCoinBalance(coinReward);
    }

    public void OnHomeButtonPressed()
    {
        if (InputManager.Instance.canInput())
        {
            AudioManager.Instance.PlayButtonClickSound();
            StackManager.Instance.mainMenu.Activate();
            gameObject.Deactivate();
        }
    }

    public void OnReplayButtonPressed()
    {
        if (InputManager.Instance.canInput())
        {
            AudioManager.Instance.PlayButtonClickSound();
            StackManager.Instance.ActivateGamePlay();
            gameObject.Deactivate();
        }
    }
}