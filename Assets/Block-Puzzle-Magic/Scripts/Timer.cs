using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [SerializeField] private Image imageProgress;

    [SerializeField] private int MaxTimeCounter = 60;

    private float timeRemaining = 60.0F;

    private float timerRate = 0.1f;

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