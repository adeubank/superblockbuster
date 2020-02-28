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

    [MenuItem("SuperBlockBuster/Add Base 100 Score")]
    private static void AddBaseScore()
    {
        ScoreManager.Instance.AddScore(100);
    }
    
    [MenuItem("SuperBlockBuster/Add Low 1,000 Score")]
    private static void AddLowScore()
    {
        ScoreManager.Instance.AddScore(1000);
    }

    [MenuItem("SuperBlockBuster/Add Medium 10,000 Score")]
    private static void AddMediumScore()
    {
        ScoreManager.Instance.AddScore(10_000);
    }
    
    [MenuItem("SuperBlockBuster/Add High 100,000 Score")]
    private static void AddHighScore()
    {
        ScoreManager.Instance.AddScore(100_000);
    }
    
    [MenuItem("SuperBlockBuster/Add Combo 3x Score")]
    private static void AddCombo3XScore()
    {
        ScoreManager.Instance.AddScore(10_000, 3);
    }
    
    [MenuItem("SuperBlockBuster/Add Combo 4x Score")]
    private static void AddCombo4XScore()
    {
        ScoreManager.Instance.AddScore(100_000, 4);
    }
    
    [MenuItem("SuperBlockBuster/Add Combo 5x Score")]
    private static void AddCombo5XScore()
    {
        ScoreManager.Instance.AddScore(250_000, 5);
    }
    
    [MenuItem("SuperBlockBuster/Add Combo Mega Score")]
    private static void AddComboMegaScore()
    {
        ScoreManager.Instance.AddScore(500_000, 6);
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