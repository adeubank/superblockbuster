using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayUI : Singleton<GamePlayUI>
{
    [SerializeField] private GameObject alertWindow;

    public GameOverReason currentGameOverReson;
    [SerializeField] private Image imgAlert;
    private Text txtAlertText;

    /// <summary>
    ///     Start this instance.
    /// </summary>
    private void Start()
    {
        txtAlertText = alertWindow.transform.GetChild(0).GetComponentInChildren<Text>();
    }

    public void OnPauseButtonPressed()
    {
        if (InputManager.Instance.canInput())
        {
            AudioManager.Instance.PlayButtonClickSound();
            StackManager.Instance.pauseSceen.Activate();
        }
    }

    public void ShowAlert()
    {
        alertWindow.SetActive(true);
        if (!IsInvoking("CloseAlert")) Invoke("CloseAlert", 2F);
    }

    /// <summary>
    ///     Closes the alert.
    /// </summary>
    private void CloseAlert()
    {
        alertWindow.SetActive(false);
    }

    /// <summary>
    ///     Shows the rescue.
    /// </summary>
    /// <param name="reason">Reason.</param>
    public void ShowRescue(GameOverReason reason)
    {
        currentGameOverReson = reason;
        StartCoroutine(ShowRescueScreen(reason));
    }

    /// <summary>
    ///     Shows the rescue screen.
    /// </summary>
    /// <returns>The rescue screen.</returns>
    /// <param name="reason">Reason.</param>
    private IEnumerator ShowRescueScreen(GameOverReason reason)
    {
        #region time mode

        if (GameController.gameMode == GameMode.TIMED || GameController.gameMode == GameMode.CHALLENGE)
            GamePlay.Instance.timeSlider.PauseTimer();

        #endregion

        yield return DisplayAlert(reason);

        GamePlay.Instance.OnGameOver();
    }

    public IEnumerator DisplayAlert(GameOverReason reason)
    {
        switch (reason)
        {
            case GameOverReason.OUT_OF_MOVES:
                txtAlertText.SetLocalizedTextForTag("txt-out-moves");
                break;
            case GameOverReason.BOMB_COUNTER_ZERO:
                txtAlertText.SetLocalizedTextForTag("txt-bomb-blast");
                break;
            case GameOverReason.TIME_OVER:
                txtAlertText.SetLocalizedTextForTag("txt-time-over");
                break;
            case GameOverReason.PLAYED_IN_LAVA:
                txtAlertText.SetLocalizedTextForTag("txt-in-lava");
                break;
        }

        alertWindow.SetActive(true);
        yield return new WaitForSeconds(2F);
        alertWindow.SetActive(false);
        txtAlertText.gameObject.SetActive(true);
    }
}

/// <summary>
///     Game over reason.
/// </summary>
public enum GameOverReason
{
    OUT_OF_MOVES = 0,
    BOMB_COUNTER_ZERO = 1,
    TIME_OVER = 2,
    PLAYED_IN_LAVA = 3
}