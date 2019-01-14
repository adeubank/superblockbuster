using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StackManager : Singleton<StackManager> 
{
	List<string> screenStack = new List<string>();

	public GameObject mainMenu;
	public GameObject selectModeScreen;
	public GameObject settingsScreen;
	public GameObject selectLanguageScreen;
	public GameObject pauseSceen;
	public GameObject recueScreen;
	public GameObject gameOverScreen;
	public GameObject shopScreen;
	public GameObject purchaseSuccessScreen;
	public GameObject purchaseFailScreen;
	public GameObject quitConfirmGameScreen;

	GameObject gamePlayScreen;

	/// <summary>
	/// Start is called on the frame when a script is enabled just before
	/// any of the Update methods is called the first time.
	/// </summary>
	void Start()
	{
		mainMenu.Activate();
	}
	public GameObject SpawnUIScreen(string name)
	{
		GameObject thisScreen = (GameObject)Instantiate (Resources.Load ("Prefabs/UIScreens/" + name));
		thisScreen.name = name;
		thisScreen.transform.SetParent (GameController.Instance.UICanvas.transform);
		thisScreen.transform.localPosition = Vector3.zero;
		thisScreen.transform.localScale = Vector3.one;
		thisScreen.GetComponent<RectTransform> ().sizeDelta = Vector3.zero;
		thisScreen.Activate();
		return thisScreen;
	}

	public void ActivateGamePlay()
	{
		if(gamePlayScreen == null)
		{
			gamePlayScreen = (GameObject)Instantiate (Resources.Load ("Prefabs/UIScreens/GamePlay"));
			gamePlayScreen.name = "GamePlay";
			gamePlayScreen.transform.SetParent (GameController.Instance.UICanvas.transform);
			gamePlayScreen.transform.localPosition = Vector3.zero;
			gamePlayScreen.transform.localScale = Vector3.one;
			gamePlayScreen.GetComponent<RectTransform> ().sizeDelta = Vector3.zero;
			gamePlayScreen.Activate();
		}
	}

	public void DeactivateGamePlay()
	{
		if(gamePlayScreen != null)
		{
			Destroy(gamePlayScreen);
		}
	}

	public void Push(string screenName)
	{
		if(!screenStack.Contains(screenName))
		{
			screenStack.Add(screenName);
		}
	}

	public string Peek()
	{
		if(screenStack.Count > 0)
		{
			return screenStack[screenStack.Count-1];
		}
		return "";
	}

	public void Pop(string screenName)
	{
		if(screenStack.Contains(screenName))
		{
			screenStack.Remove(screenName);
		}
	}

	void Update()
	{
		if(Input.GetKeyDown(KeyCode.Escape))
		{
			if(InputManager.Instance.canInput())
			{
				if(screenStack.Count > 0) {
					ProcessBackButton(Peek());
				}
			}
		}
	}

	void ProcessBackButton(string currentScreen)
	{
		Debug.Log(currentScreen);
		switch(currentScreen)
		{
			case "MainScreen":
			quitConfirmGameScreen.Activate();
			break;

			case "SelectMode":
			selectModeScreen.Deactivate();
			break;

			case "Quit-Confirm-Game":
			quitConfirmGameScreen.Deactivate();
			break;

			case "Shop":
			//shopScreen.Deactivate();
			break;

			case "Settings":
			settingsScreen.Deactivate();
			break;

			case "SelectLanguage":
			selectLanguageScreen.Deactivate();
			break;

			case "GamePlay":
			pauseSceen.Activate();
			break;

			case "Paused":
			pauseSceen.Deactivate();
			break;

			case "GameOver":
			gameOverScreen.Deactivate();
			mainMenu.Activate();
			break;

			case "PurchaseFail":
			purchaseFailScreen.Deactivate();
			break;

			case "PurchaseSuccess":
			purchaseSuccessScreen.Deactivate();
			shopScreen.Deactivate();
			break;
		}
	}
}
