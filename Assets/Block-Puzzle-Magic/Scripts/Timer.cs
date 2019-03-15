using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [SerializeField] private Image imageProgress;

    [SerializeField] private int MaxTimeCounter = 120;

    private float _timeRemaining = 120.0F;

    [SerializeField] private float timerRate = 0.1f;
    [SerializeField] private Text txtTimeRemaining;

    private void Start()
    {
        if (!IsInvoking(nameof(ElapseTimer))) InvokeRepeating(nameof(ElapseTimer), timerRate, timerRate);
    }

    private void ElapseTimer()
    {
        _timeRemaining -= timerRate;
        SetTimeSlider(_timeRemaining);
    }

    private void SetTimeSlider(float timeRemaining)
    {
        if (timeRemaining <= Mathf.Epsilon)
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
        _timeRemaining += secondsToAdd;
        _timeRemaining = Mathf.Clamp(_timeRemaining, 0, MaxTimeCounter);
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
        return (int) _timeRemaining;
    }

    public void SetTime(int timeSeconds)
    {
        _timeRemaining = timeSeconds;
    }

    public void ActivateLagPowerup()
    {
        Debug.Log("Activating Lag Powerup! _timeRemaining=" + _timeRemaining);
        AddSeconds(10);
    }
}