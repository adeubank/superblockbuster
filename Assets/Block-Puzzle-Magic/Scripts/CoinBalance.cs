using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CoinBalance : MonoBehaviour
{
    private float _lastKnownCoinBalance;
    public Text txtCoinBalance;
    
    private void OnEnable()
    {
        var coinBalance = CurrencyManager.Instance.GetCoinBalance();

        if (Math.Abs(_lastKnownCoinBalance - coinBalance) > Mathf.Epsilon)
        {
            OnCoinBalanceUpdated(coinBalance);
            StartCoroutine(SetCoinBalance(coinBalance));
        }
        
        CurrencyManager.OnCoinBalanceUpdated += OnCoinBalanceUpdated;
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
        _lastKnownCoinBalance = coinBalance;
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
            SetTextCoinBalance(oldBalance);
            yield return new WaitForEndOfFrame();
        }

        SetTextCoinBalance(coinBalance);
    }

    private void SetTextCoinBalance(int coinBalance)
    {
        txtCoinBalance.text = string.Format("{0:#,#.}", coinBalance);
    }
}