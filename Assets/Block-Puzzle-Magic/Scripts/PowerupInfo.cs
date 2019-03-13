using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class PowerupInfo : ShapeInfo
{
    // todo these should go in the BlockShapeSpawner
    public GameObject dandelionBlockIcon;
    public GameObject doublerBlockIcon;

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

                var floodPowerups = HandleFloodBlocks(currentBlocks);

                yield return new WaitWhile(() => floodPowerups.Any(t => t.IsActive()));

                yield return new WaitForEndOfFrame();

                break;
            case (int) Powerups.Doubler:

                Debug.Log("Played Doubler Powerup");

                HandleDoublerBlocks(currentBlocks);

                yield return new WaitForEndOfFrame();

                break;
            case (int) Powerups.Dandelion:

                Debug.Log("Played Dandelion Powerup");

                foreach (var block in currentBlocks) block.ConvertToDandelion();

                break;
            case (int) Powerups.Bandage:

                Debug.Log("Played Bandage Powerup");

                foreach (var block in currentBlocks) block.ConvertToBandage();

                break;
            case (int) Powerups.Bomb:

                Debug.Log("Played Bomb Powerup");

                foreach (var block in currentBlocks) block.ConvertToBomb();

                break;
            case (int) Powerups.ColorCoder:
                Debug.Log("Played Coder Coder Powerup");

                foreach (var block in currentBlocks) block.ConvertToColorCoder();

                break;
            default:
                Debug.Log("Cannot perform powerup with ShapeID=" + ShapeID + " (" + gameObject.name + ")");
                break;
        }
    }

    private List<Tweener> HandleFloodBlocks(IEnumerable<Block> currentBlocks)
    {
        return currentBlocks.Select(powerupBlock =>
        {
            Tweener tweener = null;
            for (var row = powerupBlock.rowID - 2; row <= powerupBlock.rowID + 2; row++)
            for (var col = powerupBlock.columnID - 2; col <= powerupBlock.columnID + 2; col++)
            {
                //                        Debug.Log("Played Flood Powerup: Filling row=" + row + " col=" + col);

                var block = GamePlay.Instance.blockGrid.Find(b =>
                    b.rowID == row && b.columnID == col);
                if (block)
                {
                    block.ConvertToFilledBlock(powerupBlock.blockID);
                    block.colorId = powerupBlock.colorId;
                    block.blockImage.sprite = powerupBlock.blockImage.sprite;
                }
            }

            return tweener;
        }).Where(t => t != null).ToList();
    }

    private void HandleDoublerBlocks(IEnumerable<Block> currentBlocks)
    {
        foreach (var currentBlock in currentBlocks)
            for (var row = currentBlock.rowID - 2; row <= currentBlock.rowID + 2; row++)
            for (var col = currentBlock.columnID - 2; col <= currentBlock.columnID + 2; col++)
            {
                Debug.Log("Played Doubler Powerup: Filling row=" + row + " col=" + col);

                var block = GamePlay.Instance.blockGrid.Find(b =>
                    b.rowID == row && b.columnID == col && b.isFilled);
                if (block)
                {
                    block.isDoublePoints = true;
                    // add the doubler block icon
                    Instantiate(doublerBlockIcon, block.blockImage.transform, false);
                }
            }
    }

    private enum Powerups
    {
        Flood = 1000,
        Doubler = 1001,
        Dandelion = 1002,
        Bandage = 1003,
        Bomb = 1004,
        ColorCoder = 1005
    }
}