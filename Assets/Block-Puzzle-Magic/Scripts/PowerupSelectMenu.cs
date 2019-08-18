﻿using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PowerupSelectMenu : Singleton<PowerupSelectMenu>
{
    private GameObject _helpGameObject;
    private List<int> _purchasedPowerupIds;
    [SerializeField] public PowerupList availablePowerups;
    public GameObject emptySpacePrefab;
    [HideInInspector] public List<int> equippedPowerupIds;
    public GameObject helpIcon;
    public Scrollbar powerupMenuScrollView;
    public GameObject powerupOptionPrefab;
    public Transform powerupOptionsListTransform;
    public Transform powerupSelectedListTransform;
    public GameObject powerupSelectedPrefab;

    // Start is called before the first frame update
    private void Start()
    {
        InitMenuOptions();
    }

    public void InitMenuOptions()
    {
        Instance.LoadSavedPurchasedPowerups();
        Instance.LoadSavedEquippedPowerups();
        Instance.UpdateMenu();
    }

    private void UpdateMenu()
    {
        UpdateAvailablePowerups();
        UpdateEquippedPowerups();
        SaveData();
    }

    private void SaveData()
    {
        SaveEquippedPowerups();
        SavePurchasedPowerups();
    }

    private void UpdateAvailablePowerups()
    {
        // clean the list first
        foreach (Transform t in Instance.powerupOptionsListTransform)
        {
            if (t == Instance.powerupOptionsListTransform) continue;
            Destroy(t.gameObject);
        }

        // Add some empty space at the top of the list
        Instantiate(Instance.emptySpacePrefab, Instance.powerupOptionsListTransform);

        PowerupOption firstPowerupOption = null;
        var equippedPowerupSpawns = Instance.equippedPowerupIds;
        foreach (var powerupBlockSpawn in Instance.availablePowerups.powerupBlockSpawns.OrderBy(PowerupOption.PriceForPowerup))
        {
            var powerupOption = Instantiate(Instance.powerupOptionPrefab, Instance.powerupOptionsListTransform)
                .GetComponent<PowerupOption>();

            if (firstPowerupOption == null) firstPowerupOption = powerupOption;

            powerupOption.SetPowerup(powerupBlockSpawn, equippedPowerupSpawns.Contains(powerupBlockSpawn.BlockID),
                _purchasedPowerupIds.Contains(powerupBlockSpawn.BlockID));
        }

        // empty space at the bottom
        Instantiate(Instance.emptySpacePrefab, Instance.powerupOptionsListTransform);

        helpIcon.SetActive(true);

        // show view is scrollable
        var showScrollableSequence = DOTween.Sequence();
        showScrollableSequence.Insert(0, helpIcon.transform.DOLocalMoveY(5.0f, 1.0f));
        showScrollableSequence.Insert(0, DOTween.To(() => powerupMenuScrollView.value, value => powerupMenuScrollView.value = value, 0.25f, 1.0f));
        showScrollableSequence.Insert(1, helpIcon.transform.DOLocalMoveY(0, 1.0f));
        showScrollableSequence.Insert(1, DOTween.To(() => powerupMenuScrollView.value, value => powerupMenuScrollView.value = value, 0, 1.0f));
    }

    private void UpdateEquippedPowerups()
    {
        Debug.Log("Initializing equipped powerups list " + equippedPowerupIds);

        // clean the list first
        foreach (Transform t in powerupSelectedListTransform)
        {
            if (t == powerupSelectedListTransform) continue;
            Destroy(t.gameObject);
        }

        for (var i = 0; i < 3; i++)
        {
            var powerupSelected = Instantiate(powerupSelectedPrefab, powerupSelectedListTransform)
                .GetComponent<PowerupSelected>();
            if (i < equippedPowerupIds.Count)
                powerupSelected.SetPowerup(equippedPowerupIds[i]);
            else
                powerupSelected.SetNoPowerup();
        }
    }

    public static void LoadSavedPowerups(string prefsKey, out List<int> list)
    {
        if (PlayerPrefs.HasKey(prefsKey))
        {
            try
            {
                list = PlayerPrefs.GetString(prefsKey).Split(',').Select(int.Parse).ToList();
                Debug.Log("Loaded saved powerups. prefsKey=" + prefsKey + " " + PlayerPrefs.GetString(prefsKey));
            }
            catch (FormatException e)
            {
                Debug.LogError("Failed to parse powerups! prefsKey=" + prefsKey + " " + PlayerPrefs.GetString(prefsKey));
                list = new List<int>();
            }
        }
        else
        {
            Debug.Log("No saved powerups. prefsKey=" + prefsKey);
            list = new List<int>();
        }
    }

    private void LoadSavedEquippedPowerups()
    {
        LoadSavedPowerups(EquippedPowerupPrefsKey(), out equippedPowerupIds);
    }

    private void LoadSavedPurchasedPowerups()
    {
        LoadSavedPowerups(PurchasedPowerupPrefsKey(), out _purchasedPowerupIds);
    }

    public static string EquippedPowerupPrefsKey()
    {
        return "powerups_equipped";
    }

    private string PurchasedPowerupPrefsKey()
    {
        return "purchased_powerups";
    }

    private void SaveEquippedPowerups()
    {
        SavePowerups(EquippedPowerupPrefsKey(), equippedPowerupIds);
    }

    private void SavePurchasedPowerups()
    {
        SavePowerups(PurchasedPowerupPrefsKey(), _purchasedPowerupIds);
    }

    private void SavePowerups(string prefsName, List<int> powerups)
    {
        PlayerPrefs.SetString(prefsName, string.Join(",", powerups));
        PlayerPrefs.Save();
        Debug.Log("Saved powerups " + prefsName);
    }

    public bool AddEquippedPowerupId(PowerupBlockSpawn powerup)
    {
        Debug.Log("Adding equipped powerup " + powerup);

        if (equippedPowerupIds.Count == 3) RemoveEquippedPowerupId(equippedPowerupIds[equippedPowerupIds.Count - 1]);

        equippedPowerupIds.Add(powerup.BlockID);
        UpdateMenu();

        return true;
    }

    public bool RemoveEquippedPowerupId(int equippedPowerup)
    {
        Debug.Log("Removing equipped powerup " + equippedPowerup);
        equippedPowerupIds.Remove(equippedPowerup);
        UpdateMenu();
        return true;
    }

    public void OnPlayButtonClicked()
    {
        AudioManager.Instance.PlayButtonClickSound();
        GameController.gameMode = GameMode.TIMED;
        StackManager.Instance.ActivateGamePlay();
        StackManager.Instance.mainMenu.Deactivate();
        gameObject.Deactivate();
    }

    public void OnCloseButtonPressed()
    {
        if (InputManager.Instance.canInput())
        {
            AudioManager.Instance.PlayButtonClickSound();
            gameObject.Deactivate();
        }
    }

    public bool AddPurchasedPowerupId(PowerupBlockSpawn purchasedPowerup)
    {
        Debug.Log("Adding purchased powerup " + purchasedPowerup);
        _purchasedPowerupIds.Add(purchasedPowerup.BlockID);
        UpdateMenu();

        return true;
    }
}