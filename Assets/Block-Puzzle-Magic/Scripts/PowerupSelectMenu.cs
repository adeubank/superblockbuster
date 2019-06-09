using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PowerupSelectMenu : Singleton<PowerupSelectMenu>
{
    private List<PowerupBlockSpawn> _equippedPowerups;
    [SerializeField] private PowerupList availablePowerups;
    public GameObject emptySpacePrefab;
    public GameObject powerupOptionPrefab;

    // Start is called before the first frame update
    private void Start()
    {
        InitMenuOptions();
    }

    [MenuItem("Powerups/Init Menu Options")]
    private static void InitMenuOptions()
    {
        Instance.LoadEquippedPowerups();

        // clean the list first
        foreach (Transform t in Instance.transform)
        {
            if (t == Instance.transform) continue;
            Destroy(t);
        }

        // Add some empty space at the top of the list
        Instantiate(Instance.emptySpacePrefab, Instance.transform);

        var equippedPowerupSpawns = Instance._equippedPowerups;
        foreach (var powerupBlockSpawn in Instance.availablePowerups.powerupBlockSpawns)
        {
            var powerupOption = Instantiate(Instance.powerupOptionPrefab, Instance.transform)
                .GetComponent<PowerupOption>();

            if (equippedPowerupSpawns.Contains(powerupBlockSpawn))
            {
                powerupOption.checkedIcon.gameObject.SetActive(true);
                powerupOption.powerupPrice.gameObject.SetActive(false);
            }
            else
            {
                powerupOption.checkedIcon.gameObject.SetActive(false);
                powerupOption.powerupPrice.gameObject.SetActive(true);
            }

            powerupOption.powerupIcon.sprite = powerupBlockSpawn.powerupBlockIcon.GetComponent<Image>().sprite;
            var powerupNameSplit = powerupBlockSpawn.shapeBlock.name.Split('-');
            if (powerupNameSplit.Length == 3)
                powerupOption.powerupName.text = powerupNameSplit[2].ToUpper();
            else
                throw new Exception("Failed to parse powerup name from " + powerupBlockSpawn.shapeBlock.name);
        }

        // empty space at the bottom
        Instantiate(Instance.emptySpacePrefab, Instance.transform);
    }

    private void LoadEquippedPowerups()
    {
        if (File.Exists(EquippedPowerupPath()))
        {
            var bf = new BinaryFormatter();
            var file = File.Open(EquippedPowerupPath(), FileMode.Open);
            _equippedPowerups = (List<PowerupBlockSpawn>) bf.Deserialize(file);
            file.Close();
        }
        else
        {
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
    }
}