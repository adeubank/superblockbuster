﻿using UnityEngine;

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

    /// <summary>
    ///     Raises the destroy event.
    /// </summary>
    private void OnDestroy()
    {
        if (GamePlay.Instance != null) GamePlay.Instance.OnHelpPopupClosed();
    }
}