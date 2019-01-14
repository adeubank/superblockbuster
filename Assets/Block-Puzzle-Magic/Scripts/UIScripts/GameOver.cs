using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameOver : MonoBehaviour {

	[SerializeField] Text txtScore;
	[SerializeField] private Text txtBestScore;
	[SerializeField] private Text txtCoinReward;

	public void SetLevelScore(int score, int coinReward)
	{
		int bestScore = PlayerPrefs.GetInt ("BestScore_" + GameController.gameMode.ToString (), score);

		if (score >= bestScore) 
		{
			PlayerPrefs.SetInt ("BestScore_" + GameController.gameMode.ToString (), score);
			bestScore = score;
		}

		txtScore.text = string.Format("{0:#,#.}", score.ToString("0"));
		txtBestScore.text = string.Format("{0:#,#.}", bestScore.ToString("0"));
		txtCoinReward.text = string.Format("{0:#,#.}", coinReward.ToString("0"));

		CurrencyManager.Instance.AddCoinBalance (coinReward);
	}

	public void OnHomeButtonPressed()
	{
		if (InputManager.Instance.canInput ()) {
			AudioManager.Instance.PlayButtonClickSound ();
			StackManager.Instance.mainMenu.Activate();
			gameObject.Deactivate();
		}
	}

	public void OnReplayButtonPressed()
	{
		if (InputManager.Instance.canInput ()) {
			AudioManager.Instance.PlayButtonClickSound ();
			StackManager.Instance.ActivateGamePlay();
			gameObject.Deactivate();
		}
	}
}
