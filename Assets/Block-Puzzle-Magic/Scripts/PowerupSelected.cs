using UnityEngine;
using UnityEngine.UI;

public class PowerupSelected : MonoBehaviour
{
    private PowerupBlockSpawn _equippedPowerup;
    public Sprite noPowerupImage;
    [SerializeField] private Button powerupButton;
    public Image powerupImage;

    public void SetPowerup(PowerupBlockSpawn equippedPowerup)
    {
        _equippedPowerup = equippedPowerup;
        powerupImage.sprite = equippedPowerup.powerupBlockIcon.GetComponent<Image>().sprite;
        powerupButton.enabled = true;
        powerupButton.onClick.AddListener(() =>
        {
            PowerupSelectMenu.Instance.RemoveEquippedPowerup(_equippedPowerup);
            SetNoPowerup();
            powerupButton.onClick.RemoveAllListeners();
        });
    }

    public void SetNoPowerup()
    {
        powerupButton.enabled = false;
        powerupImage.sprite = noPowerupImage;
    }
}