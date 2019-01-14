using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectMode : MonoBehaviour 
{
	[SerializeField]
	private GameObject mainContent;

	void Awake()
	{
		SetUpModeSelection ();
	}

	void SetUpModeSelection()
	{
		Transform modeButton = null;
		foreach(GameModesInfo modeInfo in GameInfo.Instance.gameModesInfo)
		{
			switch (modeInfo.gameMode) 
			{
			case GameMode.CLASSIC:
				modeButton = mainContent.transform.Find ("btn-classic");
				if (modeButton != null) {
					modeButton.gameObject.SetActive (modeInfo.isActive);
					modeButton = null;
				}
				break;

			case GameMode.TIMED:
				modeButton = mainContent.transform.Find ("btn-timed");
				if (modeButton != null) {
					modeButton.gameObject.SetActive (modeInfo.isActive);
					modeButton = null;
				}
					break;
			case GameMode.BLAST:
				modeButton = mainContent.transform.Find ("btn-blast");
				if (modeButton != null) {
					modeButton.gameObject.SetActive (modeInfo.isActive);
					modeButton = null;
				}
					break;
			case GameMode.ADVANCE:
				modeButton = mainContent.transform.Find ("btn-advance");
				if (modeButton != null) {
					modeButton.gameObject.SetActive (modeInfo.isActive);
					modeButton = null;
				}
				break;
			case GameMode.CHALLENGE:
				modeButton = mainContent.transform.Find ("btn-challenge");
				if (modeButton != null) {
					modeButton.gameObject.SetActive (modeInfo.isActive);
					modeButton = null;
				}
				break;
			}
		}
	}

	public void OnClassicButtonPressed()
	{
		if (InputManager.Instance.canInput ()) {
			AudioManager.Instance.PlayButtonClickSound ();
			GameController.gameMode = GameMode.CLASSIC;
			StackManager.Instance.ActivateGamePlay();
			StackManager.Instance.mainMenu.Deactivate();
			gameObject.Deactivate();
		}
	}

	public void OnTimedButtonPressed()
	{
		if (InputManager.Instance.canInput ()) {
			AudioManager.Instance.PlayButtonClickSound ();
			GameController.gameMode = GameMode.TIMED;
			StackManager.Instance.ActivateGamePlay();
			StackManager.Instance.mainMenu.Deactivate();
			gameObject.Deactivate();
		}
	}

	public void OnBlastButtonPressed()
	{
		if (InputManager.Instance.canInput ()) {
			AudioManager.Instance.PlayButtonClickSound ();
			GameController.gameMode = GameMode.BLAST;
			StackManager.Instance.ActivateGamePlay();
			StackManager.Instance.mainMenu.Deactivate();
			gameObject.Deactivate();
		}
	}

	public void OnAdvanceButtonPressed()
	{
		if (InputManager.Instance.canInput ()) {
			AudioManager.Instance.PlayButtonClickSound ();
			GameController.gameMode = GameMode.ADVANCE;
			StackManager.Instance.ActivateGamePlay();
			StackManager.Instance.mainMenu.Deactivate();
			gameObject.Deactivate();
		}
	}

	public void OnChallengeButtonPressed()
	{
		if (InputManager.Instance.canInput ()) {
			AudioManager.Instance.PlayButtonClickSound ();
			GameController.gameMode = GameMode.CHALLENGE;
			StackManager.Instance.ActivateGamePlay();
			StackManager.Instance.mainMenu.Deactivate();
			gameObject.Deactivate();
		}
	}

	public void OnCloseButtonPressed()
	{
		if (InputManager.Instance.canInput ()) {
			AudioManager.Instance.PlayButtonClickSound ();
			gameObject.Deactivate();
		}
	}
}
