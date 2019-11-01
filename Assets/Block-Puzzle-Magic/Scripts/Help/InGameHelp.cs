using UnityEngine;
using UnityEngine.Analytics;

public class InGameHelp : MonoBehaviour
{
    public GameObject helpObject;

    /// <summary>
    ///     Starts the help.
    /// </summary>
    public void StartHelp()
    {
        switch (GameController.gameMode)
        {
            case GameMode.CLASSIC:
            case GameMode.CHALLENGE:
                ShowBasicHelp();
                break;
            case GameMode.BLAST:
                StackManager.Instance.SpawnUIScreen("Help-Bomb-Mode");
                PlayerPrefs.SetInt("isHelpShown_" + GameController.gameMode, 1);
                break;
            case GameMode.TIMED:
                GamePlay.Instance.timeSlider.PauseTimer();
                StackManager.Instance.SpawnUIScreen("Help-TimeMode");
                PlayerPrefs.SetInt("isHelpShown_" + GameController.gameMode, 1);
                break;
            case GameMode.ADVANCE:
                StackManager.Instance.SpawnUIScreen("Help-Advance-Mode");
                PlayerPrefs.SetInt("isHelpShown_" + GameController.gameMode, 1);
                break;
        }
    }

    /// <summary>
    ///     Shows the basic help.
    /// </summary>
    public void ShowBasicHelp()
    {
        AnalyticsEvent.TutorialStart("Help-Classic");
        helpObject = StackManager.Instance.SpawnUIScreen("Help-Classic");

        if (helpObject != null)
        {
            helpObject.transform.SetParent(transform);
            helpObject.transform.SetAsLastSibling();
            helpObject.transform.localScale = Vector3.one;
            helpObject.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            helpObject.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;

            GamePlay.Instance.isHelpOnScreen = true;
            helpObject.SetActive(true);
        }
    }

    /// <summary>
    ///     Stops the help.
    /// </summary>
    public void StopHelp()
    {
        //helpObject = StackManager.Instance.PeekWindow ();
        //StackManager.Instance.PopWindow ();

        if (helpObject != null && helpObject.name.Contains("Help-Classic"))
        {
            Destroy(helpObject);
            PlayerPrefs.SetInt("isBasicHelpShown", 1);
            AnalyticsEvent.TutorialComplete("Help-Classic");
        }

        GamePlay.Instance.isHelpOnScreen = false;
        Destroy(this);
    }
}