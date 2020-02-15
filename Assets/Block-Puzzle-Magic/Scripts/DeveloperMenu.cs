using UnityEngine;
using UnityEngine.UI;

public class DeveloperMenu : MonoBehaviour
{
    public Image doublePowerupSpawn;
    
    // Start is called before the first frame update
    void Start()
    {
        SetDoublePowerupSpawnUIState();
    }

    public void ToggleDoublePowerupSpawn()
    {
        GameController.Instance.DoublePowerupSpawn = !GameController.Instance.DoublePowerupSpawn;
        SetDoublePowerupSpawnUIState();
    }

    void SetDoublePowerupSpawnUIState()
    {
        doublePowerupSpawn.color = GameController.Instance.DoublePowerupSpawn ? Color.yellow : Color.white;
    }

    public void AddCoins()
    {
        CurrencyManager.Instance.AddCoinBalance(500);
    }
    
}
