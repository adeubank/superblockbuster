using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PowerupOption : MonoBehaviour
{
    private PowerupBlockSpawn _powerup;
    [FormerlySerializedAs("checkedIcon")] public Image equippedIcon;
    [SerializeField] private Button powerupButton;
    public Image powerupIcon;
    public Text powerupName;
    public Text powerupPrice;

    public void SetPowerup(PowerupBlockSpawn powerup, bool equipped, bool purchased)
    {
        _powerup = powerup;
        powerupButton.onClick.RemoveAllListeners();
        equippedIcon.gameObject.SetActive(purchased);
        equippedIcon.color = equipped ? Color.white : Color.clear;
        powerupPrice.gameObject.SetActive(!purchased);

        if (equipped)
            powerupButton.onClick.AddListener(UnequipThisPowerup);
        else if (purchased)
            powerupButton.onClick.AddListener(EquipThisPowerup);
        else
            // TODO implement buying powerups modal with demo video
            powerupButton.onClick.AddListener(BuyThisPowerup);

        powerupIcon.sprite = powerup.powerupBlockIcon.GetComponent<Image>().sprite;
        var powerupNameSplit = powerup.shapeBlock.name.Split('-');
        if (powerupNameSplit.Length == 3)
            powerupName.text = powerupNameSplit[2].ToUpper();
        else
            throw new Exception("Failed to parse powerup name from " + powerup.shapeBlock.name);
    }

    private void BuyThisPowerup()
    {
        throw new NotImplementedException();
    }

    private void EquipThisPowerup()
    {
        PowerupSelectMenu.Instance.AddEquippedPowerupId(_powerup);
    }

    private void UnequipThisPowerup()
    {
        PowerupSelectMenu.Instance.RemoveEquippedPowerupId(_powerup.BlockID);
    }
}