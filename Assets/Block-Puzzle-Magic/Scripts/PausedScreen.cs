using System.Collections;
using UnityEngine;

public class PausedScreen : MonoBehaviour
{
    private bool hasGameExit;

    /// <summary>
    ///     Raises the enable event.
    /// </summary>
    private void OnEnable()
    {
        #region time mode

        if (GamePlay.Instance != null &&
            (GameController.gameMode == GameMode.TIMED || GameController.gameMode == GameMode.CHALLENGE))
            GamePlay.Instance.timeSlider.PauseTimer();

        #endregion
    }

    /// <summary>
    ///     Raises the disable event.
    /// </summary>
    private void OnDisable()
    {
        #region time mode

        if (!hasGameExit)
            if (GamePlay.Instance != null && (GameController.gameMode == GameMode.TIMED ||
                                              GameController.gameMode == GameMode.CHALLENGE))
                GamePlay.Instance.timeSlider.ResumeTimer();

        #endregion
    }

    /// <summary>
    ///     Raises the home button pressed event.
    /// </summary>
    public void OnHomeButtonPressed()
    {
        if (InputManager.Instance.canInput())
        {
            hasGameExit = true;
            AudioManager.Instance.PlayButtonClickSound();
            StackManager.Instance.mainMenu.Activate();
            StackManager.Instance.DeactivateGamePlay();
            gameObject.Deactivate();
        }
    }

    /// <summary>
    ///     Raises the reset button pressed event.
    /// </summary>
    public void OnResetButtonPressed()
    {
        if (InputManager.Instance.canInput())
        {
            AudioManager.Instance.PlayButtonClickSound();
            StartCoroutine(ResetGame());
        }
    }

    /// <summary>
    ///     Raises the close button pressed event.
    /// </summary>
    public void OnCloseButtonPressed()
    {
        if (InputManager.Instance.canInput())
        {
            AudioManager.Instance.PlayButtonClickSound();
            gameObject.Deactivate();
        }
    }

    private IEnumerator ResetGame()
    {
        StackManager.Instance.DeactivateGamePlay();
        yield return new WaitForSeconds(0.1F);
        StackManager.Instance.ActivateGamePlay();
        gameObject.Deactivate();
    }
}