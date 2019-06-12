using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;

public class PowerupSelectMenu : Singleton<PowerupSelectMenu>
{
    private List<PowerupBlockSpawn> _equippedPowerups;
    [SerializeField] private PowerupList availablePowerups;
    public GameObject emptySpacePrefab;
    public GameObject powerupOptionPrefab;
    public Transform powerupOptionsListTransform;
    public Transform powerupSelectedListTransform;
    public GameObject powerupSelectedPrefab;

    // Start is called before the first frame update
    private void Start()
    {
        InitMenuOptions();
    }

    [MenuItem("Powerups/Init Menu Options")]
    private static void InitMenuOptions()
    {
        Instance.LoadSavedEquippedPowerups();
        Instance.InitEquippedPowerups();
        Instance.InitAvailablePowerups();
    }

    private void InitAvailablePowerups()
    {
        // clean the list first
        foreach (Transform t in Instance.powerupOptionsListTransform)
        {
            if (t == Instance.powerupOptionsListTransform) continue;
            Destroy(t.gameObject);
        }

        // Add some empty space at the top of the list
        Instantiate(Instance.emptySpacePrefab, Instance.powerupOptionsListTransform);

        var equippedPowerupSpawns = Instance._equippedPowerups;
        foreach (var powerupBlockSpawn in Instance.availablePowerups.powerupBlockSpawns)
        {
            var powerupOption = Instantiate(Instance.powerupOptionPrefab, Instance.powerupOptionsListTransform)
                .GetComponent<PowerupOption>();

            powerupOption.SetPowerup(powerupBlockSpawn, equippedPowerupSpawns.Contains(powerupBlockSpawn));
        }

        // empty space at the bottom
        Instantiate(Instance.emptySpacePrefab, Instance.powerupOptionsListTransform);
    }

    private void InitEquippedPowerups()
    {
        Debug.Log("Initializing equipped powerups list " + _equippedPowerups);

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
            if (i < _equippedPowerups.Count)
                powerupSelected.SetPowerup(_equippedPowerups[i]);
            else
                powerupSelected.SetNoPowerup();
        }
    }

    public void RemoveEquippedPowerup(PowerupBlockSpawn equippedPowerup)
    {
        Debug.Log("Removing equipped powerup " + equippedPowerup);
        _equippedPowerups.Remove(equippedPowerup);
    }

    private void LoadSavedEquippedPowerups()
    {
        if (File.Exists(EquippedPowerupPath()))
        {
            var bf = new BinaryFormatter();
            var file = File.Open(EquippedPowerupPath(), FileMode.Open);
            _equippedPowerups = (List<PowerupBlockSpawn>) bf.Deserialize(file);
            file.Close();
            Debug.Log("Loaded saved equipped powerups " + _equippedPowerups);
        }
        else
        {
            Debug.Log("No saved equipped powerups " + _equippedPowerups);
            _equippedPowerups = new List<PowerupBlockSpawn>();
        }
    }

    private string EquippedPowerupPath()
    {
        return Application.persistentDataPath + "./equipped-powerups.json";
    }

    private void SaveEquippedPowerups()
    {
        var bf = new BinaryFormatter();
        var file = File.Create(EquippedPowerupPath());
        bf.Serialize(file, _equippedPowerups);
        file.Close();
        Debug.Log("Saved equipped powerups " + _equippedPowerups);
    }

    public bool AddEquippedPowerup(PowerupBlockSpawn powerup)
    {
        if (_equippedPowerups.Count < 3)
        {
            _equippedPowerups.Add(powerup);
            return true;
        }

        Debug.Log("Not equipping more than 3!");
        // TODO add a feedback to show we can't equip more than 3
        return false;
    }
}