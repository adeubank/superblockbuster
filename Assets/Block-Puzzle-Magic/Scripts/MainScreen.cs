using UnityEngine;

public class MainScreen : MonoBehaviour
{
    /// <summary>
    ///     Raises the play button pressed event.
    /// </summary>
    public void OnPlayButtonPressed()
    {
        if (InputManager.Instance.canInput())
        {
            AudioManager.Instance.PlayButtonClickSound();
            
            // just go straight to timed mode
            // StackManager.Instance.selectModeScreen.Activate();
            AudioManager.Instance.PlayButtonClickSound();
            GameController.gameMode = GameMode.TIMED;
            StackManager.Instance.ActivateGamePlay();
            StackManager.Instance.mainMenu.Deactivate();
        }
    }
}