using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PowerupInfo : ShapeInfo
{
    public override bool IsPowerup()
    {
        return true;
    }

    public void PerformPowerup(List<Block> CurrentBlocks)
    {
        switch (ShapeID)
        {
            case (int) Powerups.Flood:
                Debug.Log("Played Flood Powerup");

                foreach (var powerupBlock in CurrentBlocks)
                    for (var row = powerupBlock.rowID - 2; row <= powerupBlock.rowID + 2; row++)
                    for (var col = powerupBlock.columnID - 2; col <= powerupBlock.columnID + 2; col++)
                    {
                        Debug.Log("Played Flood Powerup: Filling row=" + row + " col=" + col);

                        var block = GamePlay.Instance.blockGrid.Find(b => b.rowID == row && b.columnID == col);
                        block.ConvertToFilledBlock(powerupBlock.blockID);
                        block.blockImage.DOColor(Color.blue, 1f);
                    }

                break;
            default:
                Debug.Log("Cannot perform powerup with ShapeID=" + ShapeID + " (" + gameObject.name + ")");
                break;
        }
    }

    private enum Powerups
    {
        Flood = 1000
    }
}