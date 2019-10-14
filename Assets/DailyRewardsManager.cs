using System;
using System.Collections;
using System.Globalization;
using UnityEngine;

public class DailyRewardsManager : MonoBehaviour
{
    private const string DAILY_REWARDS_PREFS_KEY = "last-daily-rewards-shown";

    private GameObject _dailyRewardsGameObject;

    public GameObject DailyRewardsPrefab;

    // Start is called before the first frame update
    private void Awake()
    {
        StartCoroutine(ShowDailyRewards());
    }

    private IEnumerator ShowDailyRewards()
    {
        if (DailyRewardsPrefab == null)
        {
            Debug.LogError("Did not find DailyRewardsPrefab");
            yield break;
        }

        Debug.Log("Checking Daily Rewards eligibility");

        if (ShouldShowDailyRewards())
        {
            Debug.Log("Earned Daily Reward");
            _dailyRewardsGameObject = Instantiate(DailyRewardsPrefab);
            SetDailyRewardsCheckedAt();
        }
        else
        {
            var lastDailyRewardsCheck = LastDailyRewardsCheckedAt();
            Debug.Log("No Daily Reward lastDailyRewardsCheck=" + lastDailyRewardsCheck.GetValueOrDefault(DateTime.Now));
        }

        yield return null;
    }

    private void SetDailyRewardsCheckedAt()
    {
        var now = DateTime.Now;
        PlayerPrefs.SetString(DAILY_REWARDS_PREFS_KEY, now.ToString("o"));
    }

    private DateTime? LastDailyRewardsCheckedAt()
    {
        if (!PlayerPrefs.HasKey(DAILY_REWARDS_PREFS_KEY)) return null;

        try
        {
            return DateTime.Parse(PlayerPrefs.GetString(DAILY_REWARDS_PREFS_KEY), null, DateTimeStyles.RoundtripKind);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private bool ShouldShowDailyRewards()
    {
        var lastDailyRewardsCheck = LastDailyRewardsCheckedAt();

        if (!lastDailyRewardsCheck.HasValue)
        {
            SetDailyRewardsCheckedAt();
            return false;
        }

        return lastDailyRewardsCheck.Value.Day < DateTime.Now.Day;
    }
}