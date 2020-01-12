using System.Linq;
using UnityEngine;

public class PowerupSelectMenu : Singleton<PowerupSelectMenu>
{
    public GameObject powerupOptionPrefab;
    public Transform powerupOptionsListTransform;
    public Transform powerupSelectedListTransform;
    public GameObject powerupSelectedPrefab;
    public GameObject powerupsEquippedTxt;
    public GameObject powerupsMenuHelpPrefab;
    public GameObject powerupsNoneEquippedTxt;

    // Start is called before the first frame update
    private void Start()
    {
        InitMenuOptions();
        CheckPowerupMenuHelp();
    }

    public void InitMenuOptions()
    {
        PowerupController.Instance.LoadSavedPurchasedPowerups();
        PowerupController.Instance.LoadSavedEquippedPowerups();
        UpdateMenu();
    }

    private void UpdateMenu()
    {
        UpdateAvailablePowerups();
        UpdateEquippedPowerups();
        SaveData();
    }

    private void SaveData()
    {
        PowerupController.Instance.SaveEquippedPowerups();
        PowerupController.Instance.SavePurchasedPowerups();
    }

    private void UpdateAvailablePowerups()
    {
        // clean the list first
        foreach (Transform t in powerupOptionsListTransform)
        {
            if (t == powerupOptionsListTransform) continue;
            Destroy(t.gameObject);
        }

        PowerupOption firstPowerupOption = null;
        foreach (var powerupBlockSpawn in PowerupController.Instance.availablePowerups.powerupBlockSpawns.OrderBy(PowerupPopup.PriceForPowerup))
        {
            if (PowerupController.Instance.equippedPowerupIds.Contains(powerupBlockSpawn.BlockID)) continue;
            var powerupOption = Instantiate(powerupOptionPrefab, powerupOptionsListTransform)
                .GetComponent<PowerupOption>();

            if (firstPowerupOption == null) firstPowerupOption = powerupOption;

            powerupOption.SetPowerup(powerupBlockSpawn);
        }
    }

    public void CheckPowerupMenuHelp()
    {
        ActivatePowerupMenuHelp(false);
    }

    public void ActivatePowerupMenuHelp(bool force = true)
    {
        ShowSelectPowerupHelp(force);
    }

    private void ShowSelectPowerupHelp(bool force = true)
    {
        if (force || !PlayerPrefs.HasKey("powerup_menu_help") || PlayerPrefs.GetInt("powerup_menu_help") != 1)
        {
            Instantiate(powerupsMenuHelpPrefab, transform);
            PlayerPrefs.SetInt("powerup_menu_help", 1);
        }
    }

    private void UpdateEquippedPowerups()
    {
        if (PowerupController.Instance.equippedPowerupIds.Any())
        {
            powerupsEquippedTxt.Activate();
            powerupsNoneEquippedTxt.Deactivate();
        }
        else
        {
            powerupsEquippedTxt.Deactivate();
            powerupsNoneEquippedTxt.Activate();
        }

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
            if (i < PowerupController.Instance.equippedPowerupIds.Count)
                powerupSelected.SetPowerup(PowerupController.Instance.equippedPowerupIds[i]);
            else
                powerupSelected.SetNoPowerup();
        }
    }

    public bool AddEquippedPowerupId(PowerupBlockSpawn powerup, bool updateMenu = true)
    {
        PowerupController.Instance.AddEquippedPowerupId(powerup.BlockID);

        if (updateMenu) UpdateMenu();

        return true;
    }

    public bool RemoveEquippedPowerupId(int equippedPowerup, bool updateMenu = true)
    {
        PowerupController.Instance.RemoveEquippedPowerupId(equippedPowerup);
        if (updateMenu) UpdateMenu();
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


    public bool AddPurchasedPowerupId(PowerupBlockSpawn purchasedPowerup, bool updateMenu = true)
    {
        PowerupController.Instance.AddPurchasedPowerupId(purchasedPowerup.BlockID);
        if (updateMenu) UpdateMenu();

        return true;
    }
}