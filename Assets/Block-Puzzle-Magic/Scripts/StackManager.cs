using System.Collections.Generic;
using UnityEngine;

public class StackManager : Singleton<StackManager>
{
    private readonly List<string> screenStack = new List<string>();
    public GameObject gameOverScreen;

    private GameObject gamePlayScreen;

    public GameObject mainMenu;
    public GameObject pauseSceen;
    public GameObject powerupSelectScreen;
    public GameObject purchaseFailScreen;
    public GameObject purchaseSuccessScreen;
    public GameObject quitConfirmGameScreen;
    public GameObject recueScreen;
    public GameObject selectLanguageScreen;
    public GameObject selectModeScreen;
    public GameObject settingsScreen;
    public GameObject shopScreen;

    /// <summary>
    ///     Start is called on the frame when a script is enabled just before
    ///     any of the Update methods is called the first time.
    /// </summary>
    private void Start()
    {
        mainMenu.Activate();
    }

    public GameObject SpawnUIScreen(string name)
    {
        var thisScreen = (GameObject) Instantiate(Resources.Load("Prefabs/UIScreens/" + name));
        thisScreen.name = name;
        thisScreen.transform.SetParent(GameController.Instance.UICanvas.transform);
        thisScreen.transform.localPosition = Vector3.zero;
        thisScreen.transform.localScale = Vector3.one;
        thisScreen.GetComponent<RectTransform>().sizeDelta = Vector3.zero;
        thisScreen.Activate();
        return thisScreen;
    }

    public bool GamePlayActive()
    {
        return gamePlayScreen != null;
    }

    public void ActivateGamePlay()
    {
        if (!GamePlayActive())
        {
            gamePlayScreen = (GameObject) Instantiate(Resources.Load("Prefabs/UIScreens/GamePlay"));
            gamePlayScreen.name = "GamePlay";
            gamePlayScreen.transform.SetParent(GameController.Instance.UICanvas.transform);
            gamePlayScreen.transform.localPosition = Vector3.zero;
            gamePlayScreen.transform.localScale = Vector3.one;
            gamePlayScreen.GetComponent<RectTransform>().sizeDelta = Vector3.zero;
            gamePlayScreen.Activate();
        }
    }

    public void DeactivateGamePlay()
    {
        if (GamePlayActive()) Destroy(gamePlayScreen);
    }

    public void Push(string screenName)
    {
        if (!screenStack.Contains(screenName)) screenStack.Add(screenName);
    }

    public string Peek()
    {
        if (screenStack.Count > 0) return screenStack[screenStack.Count - 1];
        return "";
    }

    public void Pop(string screenName)
    {
        if (screenStack.Contains(screenName)) screenStack.Remove(screenName);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            if (InputManager.Instance.canInput())
                if (screenStack.Count > 0)
                    ProcessBackButton(Peek());
    }

    private void ProcessBackButton(string currentScreen)
    {
        Debug.Log(currentScreen);
        switch (currentScreen)
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