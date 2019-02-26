using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class PowerupInfo : ShapeInfo
{
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

                yield return new WaitWhile(
                    () => { return HandleFloodBlocks(currentBlocks).Any(t => t != null && !t.IsComplete()); });

                DOTween.CompleteAll(true);

                yield return new WaitForEndOfFrame();

                break;
            case (int) Powerups.Doubler:

                Debug.Log("Played Flood Doubler");

                HandleDoublerBlocks(currentBlocks);

                yield return new WaitForEndOfFrame();

                break;
            case (int) Powerups.Dandelion:

                Debug.Log("Played Flood Dandelion");

                HandleDandelionBlocks(currentBlocks);

                yield return new WaitForEndOfFrame();

                break;
            default:
                Debug.Log("Cannot perform powerup with ShapeID=" + ShapeID + " (" + gameObject.name + ")");
                break;
        }
    }

    private IEnumerable<Tweener> HandleFloodBlocks(IEnumerable<Block> currentBlocks)
    {
        return currentBlocks.Select(powerupBlock =>
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
                    block.ConvertToFilledBlock(currentBlock.blockID);
                    block.isDoublePoints = true;
                    // add the doubler block icon
                    Instantiate(doublerBlockIcon, block.blockImage.transform, false);
                }
            }
    }

    private void HandleDandelionBlocks(IEnumerable<Block> currentBlocks)
    {
        var placedBlock = currentBlocks.First();
        var seedBlocks = new List<Block>();

        // give open blocks more priority over empty
        var availableBlocks = GamePlay.Instance.blockGrid.Where(b => !b.isFilled).ToList();
        availableBlocks.ToList().AddRange(GamePlay.Instance.blockGrid);

        while (seedBlocks.Count < 5)
        {
            var randomIndex = Random.Range(0, availableBlocks.Count());
            seedBlocks.Add(availableBlocks[randomIndex]);
        }

        seedBlocks.ForEach(b =>
        {
            b.isDandelionSeed = true;
            var newSeedBlock = Instantiate(dandelionBlockIcon, placedBlock.transform.position, Quaternion.identity,
                b.blockImage.transform);
            newSeedBlock.transform.DOMove(b.blockImage.transform.position, 0.4f);
        });
    }

    private enum Powerups
    {
        Flood = 1000,
        Doubler = 1001,
        Dandelion = 1002
    }
}