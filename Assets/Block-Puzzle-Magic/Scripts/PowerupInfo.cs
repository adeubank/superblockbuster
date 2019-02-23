using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class PowerupInfo : ShapeInfo
{
    public override bool IsPowerup()
    {
        return true;
    }

    public IEnumerator PerformPowerup(IEnumerable<Block> currentBlocks)
    {
        switch (ShapeID)
        {
            case (int) Powerups.Flood:
                Debug.Log("Played Flood Powerup");

                var tweeners = currentBlocks.Select(powerupBlock =>
                {
                    Tweener tweener = null;
                    for (var row = powerupBlock.rowID - 2; row <= powerupBlock.rowID + 2; row++)
                    for (var col = powerupBlock.columnID - 2; col <= powerupBlock.columnID + 2; col++)
                    {
//                        Debug.Log("Played Flood Powerup: Filling row=" + row + " col=" + col);

                        var block = GamePlay.Instance.blockGrid.Find(b =>
                            b.rowID == row && b.columnID == col && !b.isFilled);
                        if (block)
                        {
                            block.ConvertToFilledBlock(powerupBlock.blockID);
                            var row1 = row;
                            var col1 = col;
                            tweener = block.blockImage.DOColor(Color.blue, 1f).OnComplete(() =>
                            {
//                                Debug.Log("Played Flood Powerup: Tween Complete row=" + row1 + " col=" + col1 +
//                                          " currentColor=" + block.blockImage.color);
                            });
                        }
                    }

                    return tweener;
                });

                yield return new WaitWhile(() => tweeners.Any(t => t != null && !t.IsComplete()));
                DOTween.CompleteAll(true);
                yield return new WaitForEndOfFrame();
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