using System;
using UnityEngine;

public class DailyRewardsManager : Singleton<DailyRewardsManager>
{
    public FortuneWheelManager fortuneWheel;

    // Start is called before the first frame update
    private void Awake()
    {
        if (fortuneWheel == null) throw new NullReferenceException("Missing FortuneWheelManager reference");
        Debug.Log("Daily Rewards Manager is awake");
        Invoke("PerformDailyReward", 0);
    }

    private void PerformDailyReward()
    {
        // no daily rewards while game is on
        if (StackManager.Instance.GamePlayActive()) return;

        Debug.Log("Checking Daily Rewards eligibility");

        if (fortuneWheel.FreeTurnAvailable())
        {
            Debug.Log("Earned Daily Reward");
            ShowFortuneWheel();
        }
        else
        {
            HideFortuneWheel();
        }
    }

    public void ShowFortuneWheel()
    {
        fortuneWheel.gameObject.SetActive(true);
    }

    public void HideFortuneWheel()
    {
        if (fortuneWheel.IsSpinning()) return;
        fortuneWheel.gameObject.SetActive(false);
    }
}