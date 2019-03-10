﻿using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [SerializeField] private Image imageProgress;

    [SerializeField] private int MaxTimeCounter = 120;

    private float timeRemaining = 120.0F;

    [SerializeField] private float timerRate = 0.1f;
    [SerializeField] private Text txtTimeRemaining;

    private void Start()
    {
        if (!IsInvoking("ElapseTimer")) InvokeRepeating("ElapseTimer", timerRate, timerRate);
    }

    private void ElapseTimer()
    {
        timeRemaining -= timerRate;
        SetTimeSlider(timeRemaining);
    }

    private void SetTimeSlider(float timeRemaining)
    {
        if (timeRemaining <= 0)
        {
            GamePlayUI.Instance.ShowRescue(GameOverReason.TIME_OVER);
        }
        else
        {
            var sliderValue = timeRemaining / MaxTimeCounter;
            imageProgress.fillAmount = sliderValue;
            txtTimeRemaining.text = "Seconds left " + timeRemaining.ToString("F2");
        }
    }

    public void UpdateTimerSpeed(float newRate)
    {
        PauseTimer();
        timerRate = newRate;
        ResumeTimer();
    }

    public void AddSeconds(int secondsToAdd)
    {
        timeRemaining += secondsToAdd;
        timeRemaining = Mathf.Clamp(timeRemaining, 0, MaxTimeCounter);
    }

    public void PauseTimer()
    {
        if (IsInvoking("ElapseTimer")) CancelInvoke("ElapseTimer");
    }

    public void ResumeTimer()
    {
        if (!IsInvoking("ElapseTimer")) InvokeRepeating("ElapseTimer", timerRate, timerRate);
    }

    public int GetRemainingTime()
    {
        return (int) timeRemaining;
    }

    public void SetTime(int timeSeconds)
    {
        timeRemaining = timeSeconds;
    }
}