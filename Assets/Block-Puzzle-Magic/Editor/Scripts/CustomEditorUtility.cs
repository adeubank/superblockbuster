using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CustomEditorUtility : MonoBehaviour
{
    [MenuItem("SuperBlockBuster/Plugin Setup/Setup IAP Catalog", false, 2)]
    private static void SetUpIAPCatalog()
    {
        var sourcePath = Application.dataPath + "/Block-Puzzle-Magic/UnityIAPCatalog/IAPProductCatalog.json";
        var destPath = Application.dataPath + "/Plugins/UnityPurchasing/Resources/IAPProductCatalog.json";

        if (File.Exists(sourcePath))
        {
            if (!File.Exists(destPath))
            {
                File.Copy(sourcePath, destPath);
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogWarning("IAP Catalog already exists.");
            }
        }
    }

    [MenuItem("SuperBlockBuster/Clear PlayerPrefs")]
    private static void ClearPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    [MenuItem("SuperBlockBuster/Persistent Data Path/Remove All Files")]
    private static void DeleteAllFilesFromPersistentDataPath()
    {
        FileUtil.DeleteFileOrDirectory(Application.persistentDataPath);
    }

    [MenuItem("SuperBlockBuster/Capture Screenshot/1X")]
    private static void Capture1XScreenshot()
    {
        var imgName = "IMG-" + DateTime.Now.Year + DateTime.Now.Month.ToString("00") +
                      DateTime.Now.Day.ToString("00") + "-" + DateTime.Now.Hour.ToString("00") +
                      DateTime.Now.Minute.ToString("00") + DateTime.Now.Second.ToString("00") + ".png";
        //ScreenCapture.CaptureScreenshot ((Application.dataPath + "/" + imgName),1);
    }

    [MenuItem("SuperBlockBuster/Capture Screenshot/2X")]
    private static void Capture2XScreenshot()
    {
        var imgName = "IMG-" + DateTime.Now.Year + DateTime.Now.Month.ToString("00") +
                      DateTime.Now.Day.ToString("00") + "-" + DateTime.Now.Hour.ToString("00") +
                      DateTime.Now.Minute.ToString("00") + DateTime.Now.Second.ToString("00") + ".png";
        //ScreenCapture.CaptureScreenshot ((Application.dataPath + "/" + imgName),2);
    }

    [MenuItem("SuperBlockBuster/Execute Rescue")]
    private static void ExecuteRescue()
    {
        GamePlayUI.Instance.currentGameOverReson = GameOverReason.OUT_OF_MOVES;
        GamePlay.Instance.OnUnableToPlaceShape();
    }

    [MenuItem("SuperBlockBuster/Add Minus Score")]
    private static void AddMinusScore()
    {
        ScoreManager.Instance.AddScore(-100);
    }

    [MenuItem("SuperBlockBuster/Add Plus Score")]
    private static void AddPlusScore()
    {
        ScoreManager.Instance.AddScore(100);
    }

    [MenuItem("SuperBlockBuster/Init Powerups Menu Options")]
    private static void InitMenuOptions()
    {
        PowerupSelectMenu.Instance.InitMenuOptions();
    }

    [MenuItem("SuperBlockBuster/Show Help In Game")]
    private static void ShowHelpInGame()
    {
        PlayerPrefs.SetInt("isHelpShown_" + GameController.gameMode, 0);
        PlayerPrefs.SetInt("isBasicHelpShown", 0);
        GamePlay.Instance.ShowBasicHelp();
    }

    [MenuItem("SuperBlockBuster/Add 500 coins")]
    private static void AddFiftyCoins()
    {
        CurrencyManager.Instance.AddCoinBalance(500);
    }
    
    [MenuItem("SuperBlockBuster/Set Level Score C")]
    private static void SetLevelScoreC()
    {
        var gameOverScreen = StackManager.Instance.gameOverScreen;
        gameOverScreen.Activate();
        gameOverScreen.GetComponent<GameOver>().SetLevelScore(499999);
    } 
    
    [MenuItem("SuperBlockBuster/Set Level Score B")]
    private static void SetLevelScoreB()
    {
        var gameOverScreen = StackManager.Instance.gameOverScreen;
        gameOverScreen.Activate();
        gameOverScreen.GetComponent<GameOver>().SetLevelScore(999999);
    }
    
    [MenuItem("SuperBlockBuster/Set Level Score A")]
    private static void SetLevelScoreA()
    {
        var gameOverScreen = StackManager.Instance.gameOverScreen;
        gameOverScreen.Activate();
        gameOverScreen.GetComponent<GameOver>().SetLevelScore(1499999);
    }
    
    [MenuItem("SuperBlockBuster/Set Level Score S")]
    private static void SetLevelScoreS()
    {
        var gameOverScreen = StackManager.Instance.gameOverScreen;
        gameOverScreen.Activate();
        gameOverScreen.GetComponent<GameOver>().SetLevelScore(9999999);
    }

}