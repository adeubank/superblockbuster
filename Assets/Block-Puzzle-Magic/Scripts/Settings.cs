﻿using UnityEngine;

public class Settings : MonoBehaviour
{
    public void OnRateButtonPressed()
    {
        if (InputManager.Instance.canInput())
        {
            AudioManager.Instance.PlayButtonClickSound();
            gameObject.GetComponent<AppRating>()?.Open();
        }
    }

    public void OnCloseButtonPressed()
    {
        if (InputManager.Instance.canInput())
        {
            AudioManager.Instance.PlayButtonClickSound();
            gameObject.Deactivate();
        }
    }

    public void OnSelectLanguageButtonPressed()
    {
        if (InputManager.Instance.canInput())
        {
            StackManager.Instance.selectLanguageScreen.Activate();
            gameObject.Deactivate();
        }
    }
}