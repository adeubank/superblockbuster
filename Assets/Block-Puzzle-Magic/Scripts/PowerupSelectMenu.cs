using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;

public class PowerupSelectMenu : Singleton<PowerupSelectMenu>
{
    private List<int> _purchasedPowerupIds;
    [SerializeField] public PowerupList availablePowerups;
    public GameObject emptySpacePrefab;
    [HideInInspector] public List<int> equippedPowerupIds;
    public GameObject powerupOptionPrefab;
    public Transform powerupOptionsListTransform;
    public Transform powerupSelectedListTransform;
    public GameObject powerupSelectedPrefab;

    // Start is called before the first frame update
    private void Start()
    {
        InitMenuOptions();
    }

#if UNITY_EDITOR
    [MenuItem("Powerups/Init Menu Options")]
#endif
    private static void InitMenuOptions()
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

        var equippedPowerupSpawns = Instance.equippedPowerupIds;
        foreach (var powerupBlockSpawn in Instance.availablePowerups.powerupBlockSpawns)
        {
            var powerupOption = Instantiate(Instance.powerupOptionPrefab, Instance.powerupOptionsListTransform)
                .GetComponent<PowerupOption>();

            powerupOption.SetPowerup(powerupBlockSpawn, equippedPowerupSpawns.Contains(powerupBlockSpawn.BlockID),
                _purchasedPowerupIds.Contains(powerupBlockSpawn.BlockID));
        }

        // empty space at the bottom
        Instantiate(Instance.emptySpacePrefab, Instance.powerupOptionsListTransform);
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

    public static void LoadSavedPowerups(string path, out List<int> list)
    {
        if (File.Exists(path))
        {
            var bf = new BinaryFormatter();
            var file = File.Open(path, FileMode.Open);
            list = (List<int>) bf.Deserialize(file);
            file.Close();
            Debug.Log("Loaded saved powerups. path=" + path);
        }
        else
        {
            Debug.Log("No saved powerups. path=" + path);
            list = new List<int>();
        }
    }

    private void LoadSavedEquippedPowerups()
    {
        LoadSavedPowerups(EquippedPowerupPath(), out equippedPowerupIds);
    }

    private void LoadSavedPurchasedPowerups()
    {
        LoadSavedPowerups(PurchasedPowerupPath(), out _purchasedPowerupIds);
    }

    public static string EquippedPowerupPath()
    {
        return Application.persistentDataPath + "/equipped-powerups.dat";
    }

    private string PurchasedPowerupPath()
    {
        return Application.persistentDataPath + "/purchased-powerups.dat";
    }

    private void SaveEquippedPowerups()
    {
        SavePowerups(EquippedPowerupPath(), equippedPowerupIds);
    }

    private void SavePurchasedPowerups()
    {
        SavePowerups(PurchasedPowerupPath(), _purchasedPowerupIds);
    }

    private void SavePowerups(string path, List<int> powerups)
    {
        var bf = new BinaryFormatter();
        var file = File.Create(path);
        bf.Serialize(file, powerups);
        file.Close();
        Debug.Log("Saved powerups " + path);
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
}