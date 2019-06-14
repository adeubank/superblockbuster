﻿using UnityEngine;

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
            StackManager.Instance.powerupSelectScreen.Activate();
        }
    }
}