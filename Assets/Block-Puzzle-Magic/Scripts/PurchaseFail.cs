﻿using UnityEngine;

public class PurchaseFail : MonoBehaviour
{
    public void OnOkButtonPressed()
    {
        if (InputManager.Instance.canInput()) gameObject.Deactivate();
    }

    public void OnCloseButtonPressed()
    {
        if (InputManager.Instance.canInput())
        {
            AudioManager.Instance.PlayButtonClickSound();
            gameObject.Deactivate();
        }
    }
}