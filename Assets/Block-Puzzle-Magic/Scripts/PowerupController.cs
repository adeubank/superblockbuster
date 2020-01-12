using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PowerupController : Singleton<PowerupController>
{
    [SerializeField] public PowerupList availablePowerups;
    public List<int> equippedPowerupIds;
    public List<int> purchasedPowerupIds;

    public void Start()
    {
        LoadSavedEquippedPowerups();
        LoadSavedPurchasedPowerups();
    }

    public void LoadSavedEquippedPowerups()
    {
        LoadSavedPowerups(EquippedPowerupPrefsKey(), out equippedPowerupIds);
    }

    public void LoadSavedPurchasedPowerups()
    {
        LoadSavedPowerups(PurchasedPowerupPrefsKey(), out purchasedPowerupIds);
    }

    public string EquippedPowerupPrefsKey()
    {
        return "powerups_equipped";
    }

    private string PurchasedPowerupPrefsKey()
    {
        return "purchased_powerups";
    }

    public void SaveEquippedPowerups()
    {
        SavePowerups(EquippedPowerupPrefsKey(), equippedPowerupIds);
    }

    public void SavePurchasedPowerups()
    {
        SavePowerups(PurchasedPowerupPrefsKey(), purchasedPowerupIds);
    }

    private void SavePowerups(string prefsName, List<int> powerups)
    {
        PlayerPrefs.SetString(prefsName, string.Join(",", powerups));
        PlayerPrefs.Save();
    }

    public bool AddPurchasedPowerupId(int blockId)
    {
        purchasedPowerupIds.Add(blockId);
        return true;
    }


    public bool AddEquippedPowerupId(int blockId)
    {
        if (equippedPowerupIds.Count == 3) RemoveEquippedPowerupId(equippedPowerupIds[equippedPowerupIds.Count - 1]);

        equippedPowerupIds.Add(blockId);

        return true;
    }

    public bool RemoveEquippedPowerupId(int equippedPowerup)
    {
        equippedPowerupIds.Remove(equippedPowerup);
        return true;
    }

    public static void LoadSavedPowerups(string prefsKey, out List<int> list)
    {
        if (PlayerPrefs.HasKey(prefsKey))
        {
            try
            {
                list = PlayerPrefs.GetString(prefsKey).Split(',').Select(int.Parse).ToList();
            }
            catch (FormatException e)
            {
                list = new List<int>();
            }
        }
        else
        {
            list = new List<int>();
        }
    }
}