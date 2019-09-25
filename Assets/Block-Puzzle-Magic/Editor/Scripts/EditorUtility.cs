using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class EditorUtility : MonoBehaviour
{
    [MenuItem("Super Block Buster/Plugin Setup/Check Setup", false, 1)]
    private static void SetUp()
    {
        DependencyChecker.OpenWelcomeWindow();
    }

    [MenuItem("Super Block Buster/Plugin Setup/Setup IAP Catalog", false, 2)]
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

    [MenuItem("Super Block Buster/Clear PlayerPrefs")]
    private static void ClearPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    [MenuItem("Super Block Buster/Persistent Data Path/Remove All Files")]
    private static void DeleteAllFilesFromPersistentDataPath()
    {
        FileUtil.DeleteFileOrDirectory(Application.persistentDataPath);
    }

    [MenuItem("Super Block Buster/Capture Screenshot/1X")]
    private static void Capture1XScreenshot()
    {
        var imgName = "IMG-" + DateTime.Now.Year + DateTime.Now.Month.ToString("00") +
                      DateTime.Now.Day.ToString("00") + "-" + DateTime.Now.Hour.ToString("00") +
                      DateTime.Now.Minute.ToString("00") + DateTime.Now.Second.ToString("00") + ".png";
        //ScreenCapture.CaptureScreenshot ((Application.dataPath + "/" + imgName),1);
    }

    [MenuItem("Super Block Buster/Capture Screenshot/2X")]
    private static void Capture2XScreenshot()
    {
        var imgName = "IMG-" + DateTime.Now.Year + DateTime.Now.Month.ToString("00") +
                      DateTime.Now.Day.ToString("00") + "-" + DateTime.Now.Hour.ToString("00") +
                      DateTime.Now.Minute.ToString("00") + DateTime.Now.Second.ToString("00") + ".png";
        //ScreenCapture.CaptureScreenshot ((Application.dataPath + "/" + imgName),2);
    }

    [MenuItem("Super Block Buster/Execute Rescue")]
    private static void ExecuteRescue()
    {
        GamePlayUI.Instance.currentGameOverReson = GameOverReason.OUT_OF_MOVES;
        GamePlay.Instance.OnUnableToPlaceShape();
    }


    [MenuItem("Super Block Buster/Init Powerups Menu Options")]
    private static void InitMenuOptions()
    {
        PowerupSelectMenu.Instance.InitMenuOptions();
    }

    [MenuItem("Super Block Buster/Show Help In Game")]
    private static void ShowHelpInGame()
    {
        PlayerPrefs.SetInt("isHelpShown_" + GameController.gameMode, 0);
        PlayerPrefs.SetInt("isBasicHelpShown", 0);
        GamePlay.Instance.ShowBasicHelp();
    }
}