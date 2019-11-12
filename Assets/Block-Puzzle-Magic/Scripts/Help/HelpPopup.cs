using UnityEngine;

/// <summary>
///     Help popup.
/// </summary>
public class HelpPopup : MonoBehaviour
{
    /// <summary>
    ///     Raises the close button pressed event.
    /// </summary>
    public void OnCloseButtonPressed()
    {
        if (InputManager.Instance.canInput())
        {
            AudioManager.Instance.PlayButtonClickSound();
            //StackManager.Instance.OnCloseButtonPressed ();
            Destroy(gameObject);
        }
    }
}