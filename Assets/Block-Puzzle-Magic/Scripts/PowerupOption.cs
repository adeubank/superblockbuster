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

        //region update equippable status
        equippedIcon.gameObject.SetActive(purchased);
        equippedIcon.color = equipped ? Color.white : Color.clear;
        //endregion

        //region update pricing
        powerupPrice.gameObject.SetActive(!purchased);
        powerupPrice.text = PriceForPowerup(powerup).ToString();
        //endregion

        //region rebind event handlers
        powerupButton.onClick.RemoveAllListeners();
        if (equipped)
            powerupButton.onClick.AddListener(UnequipThisPowerup);
        else if (purchased)
            powerupButton.onClick.AddListener(EquipThisPowerup);
        else
            // TODO implement buying powerups modal with demo video
            powerupButton.onClick.AddListener(BuyThisPowerup);
        //endregion

        powerupIcon.sprite = powerup.powerupBlockIcon.GetComponent<Image>().sprite;
        var powerupNameSplit = powerup.shapeBlock.name.Split('-');
        if (powerupNameSplit.Length == 3)
            powerupName.text = powerupNameSplit[2].ToUpper();
        else
            throw new Exception("Failed to parse powerup name from " + powerup.shapeBlock.name);
    }

    private void BuyThisPowerup()
    {
        if (InputManager.Instance.canInput())
        {
            var coinDeduced = CurrencyManager.Instance.deductBalance(PriceForPowerup(_powerup));

            if (coinDeduced)
            {
                AudioManager.Instance.PlayButtonClickSound();
                PowerupSelectMenu.Instance.AddPurchasedPowerupId(_powerup);
            }
            else
            {
                StackManager.Instance.shopScreen.Activate();
            }
        }
    }

    private void EquipThisPowerup()
    {
        PowerupSelectMenu.Instance.AddEquippedPowerupId(_powerup);
    }

    private void UnequipThisPowerup()
    {
        PowerupSelectMenu.Instance.RemoveEquippedPowerupId(_powerup.BlockID);
    }

    public static int PriceForPowerup(PowerupBlockSpawn powerup)
    {
        switch (powerup.BlockID)
        {
            // region tier 1 powerups
            case (int) ShapeInfo.Powerups.Doubler:
                return 100;
            case (int) ShapeInfo.Powerups.ColorCoder:
                return 100;
            case (int) ShapeInfo.Powerups.Bandage:
                return 200;
            case (int) ShapeInfo.Powerups.Bomb:
                return 200;
            case (int) ShapeInfo.Powerups.SticksGalore:
                return 200;
            //endregion

            // region tier 2 powerups
            case (int) ShapeInfo.Powerups.Flood:
                return 500;
            case (int) ShapeInfo.Powerups.Dandelion:
                return 750;
            case (int) ShapeInfo.Powerups.Quake:
                return 500;
            //endregion

            // region tier 3 powerups
            case (int) ShapeInfo.Powerups.Frenzy:
                return 1500;
            case (int) ShapeInfo.Powerups.Storm:
                return 1500;
            case (int) ShapeInfo.Powerups.Lag:
                return 2000;
            case (int) ShapeInfo.Powerups.Avalanche:
                return 2000;
            //endregion

            default:
                throw new NotImplementedException("Price for powerup is not implemented. " + powerup.BlockID);
        }
    }
}