using System;
using UnityEngine;
using UnityEngine.UI;

public class PowerupOption : MonoBehaviour
{
    private PowerupBlockSpawn _powerup;
    public Image checkedIcon;
    [SerializeField] private Button powerupButton;
    public Image powerupIcon;
    public Text powerupName;
    public Text powerupPrice;

    public void SetPowerup(PowerupBlockSpawn powerup, bool equipped)
    {
        _powerup = powerup;
        powerupButton.onClick.RemoveAllListeners();

        if (equipped)
        {
            checkedIcon.gameObject.SetActive(true);
            powerupPrice.gameObject.SetActive(false);
            powerupButton.enabled = true;
            powerupButton.onClick.AddListener(EquipThisPowerup);
        }
        else
        {
            checkedIcon.gameObject.SetActive(false);
            powerupPrice.gameObject.SetActive(true);
        }

        powerupIcon.sprite = powerup.powerupBlockIcon.GetComponent<Image>().sprite;
        var powerupNameSplit = powerup.shapeBlock.name.Split('-');
        if (powerupNameSplit.Length == 3)
            powerupName.text = powerupNameSplit[2].ToUpper();
        else
            throw new Exception("Failed to parse powerup name from " + powerup.shapeBlock.name);
    }

    public void EquipThisPowerup()
    {
        if (PowerupSelectMenu.Instance.AddEquippedPowerup(_powerup))
            SetPowerup(_powerup, true);
    }
}