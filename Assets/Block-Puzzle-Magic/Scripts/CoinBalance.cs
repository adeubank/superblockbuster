using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CoinBalance : MonoBehaviour
{
    public Text txtCoinBalance;

    /// <summary>
    ///     Raises the enable event.
    /// </summary>
    private void OnEnable()
    {
        CurrencyManager.OnCoinBalanceUpdated += OnCoinBalanceUpdated;

        var coinBalance = CurrencyManager.Instance.GetCoinBalance();
        OnCoinBalanceUpdated(coinBalance);
    }

    /// <summary>
    ///     Raises the disable event.
    /// </summary>
    private void OnDisable()
    {
        CurrencyManager.OnCoinBalanceUpdated -= OnCoinBalanceUpdated;
    }

    /// <summary>
    ///     Raises the coin balance updated event.
    /// </summary>
    /// <param name="coinBalance">Coin balance.</param>
    private void OnCoinBalanceUpdated(int coinBalance)
    {
        StartCoroutine(SetCoinBalance(coinBalance));
    }

    /// <summary>
    ///     Sets the coin balance.
    /// </summary>
    /// <returns>The coin balance.</returns>
    /// <param name="coinBalance">Coin balance.</param>
    private IEnumerator SetCoinBalance(int coinBalance)
    {
        var oldBalance = 0;
        int.TryParse(txtCoinBalance.text.Replace(",", ""), out oldBalance);

        var IterationSize = (coinBalance - oldBalance) / 50;

        for (var index = 1; index < 50; index++)
        {
            oldBalance += IterationSize;
            txtCoinBalance.text = string.Format("{0:#,#.}", oldBalance);
            yield return new WaitForEndOfFrame();
        }

        txtCoinBalance.text = string.Format("{0:#,#.}", coinBalance);
    }
}