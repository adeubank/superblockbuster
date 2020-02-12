using System;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [SerializeField] private float timeRemaining = 30.0F;
    [SerializeField] private Image imageProgress;

    [SerializeField] private int maxTimeCounter = 30;

    [SerializeField] private float timerRate = 0.1f;
    [SerializeField] private Text txtTimeRemaining;

    private void Start()
    {
        timeRemaining = RemoteConfigController.Instance.GameLengthInSeconds;
        maxTimeCounter = RemoteConfigController.Instance.GameLengthInSeconds;
        SetTimeSlider(GetRemainingTime());
        if (!IsInvoking(nameof(ElapseTimer))) InvokeRepeating(nameof(ElapseTimer), timerRate, timerRate);
    }

    public void ElapseTimer()
    {
        if (GamePlay.Instance.isHelpOnScreen) return;
        timeRemaining -= timerRate;
        SetTimeSlider(timeRemaining);
    }

    public void SetTimeSlider(float timeRemaining)
    {
        if (timeRemaining <= Mathf.Epsilon)
        {
            timeRemaining = 0;
            GamePlayUI.Instance.ShowRescue(GameOverReason.TIME_OVER);
        }

        var sliderValue = timeRemaining / maxTimeCounter;
        imageProgress.fillAmount = sliderValue;
        var minutesRemaining = ((int) this.timeRemaining / 60).ToString("F0");
        var secondsRemaining = ((int) this.timeRemaining % 60).ToString("F0");
        txtTimeRemaining.text = minutesRemaining + ":" +
                                secondsRemaining.PadLeft(Math.Min(secondsRemaining.Length + 1, 2), '0');
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
        timeRemaining = Mathf.Clamp(timeRemaining, 0, maxTimeCounter);
    }

    public void PauseTimer()
    {
        if (IsInvoking(nameof(ElapseTimer))) CancelInvoke(nameof(ElapseTimer));
    }

    public void ResumeTimer()
    {
        if (!IsInvoking(nameof(ElapseTimer))) InvokeRepeating(nameof(ElapseTimer), timerRate, timerRate);
    }

    public int GetRemainingTime()
    {
        return (int) timeRemaining;
    }

    public void SetTime(int timeSeconds)
    {
        timeRemaining = timeSeconds;
    }

    public void ActivateLagPowerup()
    {
        AddSeconds(10);
    }
}