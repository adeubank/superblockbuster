using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ShapeInfo : MonoBehaviour
{
    public enum Powerups
    {
        Flood = 1000,
        Doubler = 1001,
        Dandelion = 1002,
        Bandage = 1003,
        Bomb = 1004,
        ColorCoder = 1005,
        SticksGalore = 1006,
        Lag = 1007,
        Storm = 1008,
        Quake = 1009,
        Avalanche = 1010,
        Frenzy = 1011
    }

    [HideInInspector] public Sprite blockImage;

    [HideInInspector] public ShapeBlock firstBlock;

    public List<ShapeBlock> ShapeBlocks;
    public int ShapeID;

    [HideInInspector] public int startOffsetX;
    [HideInInspector] public int startOffsetY;

    private void Start()
    {
        if (ShapeBlocks == null)
            CreateBlockList();

        firstBlock = ShapeBlocks[0];
        blockImage = firstBlock.block.GetComponent<Image>().sprite;
        startOffsetX = firstBlock.rowID;
        startOffsetY = firstBlock.columnID;
    }

    public void CreateBlockList()
    {
        ShapeBlocks = new List<ShapeBlock>();
        var shapeAllBlocks = transform.GetComponentsInChildren<Transform>().ToList();

        if (shapeAllBlocks.Contains(transform)) shapeAllBlocks.Remove(transform);


        foreach (var block in shapeAllBlocks)
        {
            var blockNameSplit = block.name.Split('-');

            if (blockNameSplit.Length == 3)
            {
                var rowID = blockNameSplit[1].TryParseInt();
                var columnID = blockNameSplit[2].TryParseInt();

                var thisBlock = new ShapeBlock(block, rowID, columnID);
                if (!ShapeBlocks.Contains(thisBlock)) ShapeBlocks.Add(thisBlock);
            }
        }
    }

    public bool IsBandageShape()
    {
        return ShapeID == (int) Powerups.Bandage;
    }

    public void ConvertToPowerup(PowerupBlockSpawn powerupInfo)
    {
        if (ShapeBlocks == null || ShapeBlocks.Count == 0)
        {
            return;
        }

        ShapeBlocks.ForEach(b => { Instantiate(powerupInfo.powerupBlockIcon, b.block, false); });
    }

    public bool IsPowerup()
    {
        return ShapeID >= 1000;
    }

    public IEnumerator PerformPowerup(List<Block> currentBlocks)
    {
        switch (ShapeID)
        {
            case (int) Powerups.Flood:
                yield return HandleFloodBlocks(currentBlocks);
                break;
            case (int) Powerups.Doubler:
                HandleDoublerBlocks(currentBlocks);
                break;
            case (int) Powerups.Dandelion:
                foreach (var block in currentBlocks) block.ConvertToDandelion();
                break;
            case (int) Powerups.Bandage:
                var randomBandageBlock = currentBlocks[Random.Range(0, currentBlocks.Count)];
                StartCoroutine(GamePlay.Instance.ShowPowerupActivationSprite((int) Powerups.Bandage,
                    randomBandageBlock.moveID, randomBandageBlock, true));
                break;
            case (int) Powerups.Bomb:
                foreach (var block in currentBlocks) block.ConvertToBomb();
                break;
            case (int) Powerups.ColorCoder:
                foreach (var block in currentBlocks) block.ConvertToColorCoder();
                break;
            case (int) Powerups.SticksGalore:
                foreach (var block in currentBlocks) block.ConvertToSticksGalore();
                break;
            case (int) Powerups.Lag:
                foreach (var block in currentBlocks) block.ConvertToLagBlock();
                break;
            case (int) Powerups.Storm:
                foreach (var block in currentBlocks) block.ConvertToStormBlock();
                break;
            case (int) Powerups.Quake:
                foreach (var block in currentBlocks) block.ConvertToQuakeBlock();
                break;
            case (int) Powerups.Avalanche:
                foreach (var block in currentBlocks) block.ConvertToAvalancheBlock();
                break;
            case (int) Powerups.Frenzy:
                foreach (var block in currentBlocks) block.ConvertToFrenzyBlock();
                break;
            default:
                Debug.Log("Cannot perform powerup with ShapeID=" + ShapeID + " (" + gameObject.name + ")");
                break;
        }
    }

    private IEnumerator HandleFloodBlocks(List<Block> currentBlocks)
    {
        var powerupBlock = currentBlocks[Random.Range(0, currentBlocks.Count)];

        // since flood is activation once played, show sprite here
        StartCoroutine(GamePlay.Instance.ShowPowerupActivationSprite(powerupBlock.blockID, powerupBlock.moveID,
            powerupBlock, true));

        var surroundingBlocks = GamePlay.Instance.SurroundingBlocksInRadius(powerupBlock, 2, true).ToList();
        if (surroundingBlocks.Any())
        {
            var floodSequence = DOTween.Sequence();
            foreach (var block in surroundingBlocks)
            {
                floodSequence.AppendCallback(() =>
                {
                    if (!block.isFilled) block.ConvertToFilledBlock(0);
                    block.colorId = powerupBlock.colorId;
                    block.blockImage.sprite = powerupBlock.blockImage.sprite;
                });
                floodSequence.AppendInterval(0.04f);
            }

            yield return floodSequence.WaitForCompletion();
            yield return new WaitForEndOfFrame();
        }
    }

    private void HandleDoublerBlocks(List<Block> currentBlocks)
    {
        var powerupBlock = currentBlocks[Random.Range(0, currentBlocks.Count)];

        // since doubler is activation once played, show sprite here
        StartCoroutine(GamePlay.Instance.ShowPowerupActivationSprite(powerupBlock.blockID, powerupBlock.moveID,
            powerupBlock, false));
        GamePlay.Instance.SurroundingBlocksInRadius(powerupBlock, 2, true)
            .Where(block => block.isFilled)
            .ToList()
            .ForEach(b => b.ConvertToDoublerBlock());
    }
}

public class ShapeBlock
{
    public Transform block;
    public int columnID;
    public int rowID;

    public ShapeBlock(Transform _block, int _rowID, int _columnID)
    {
        block = _block;
        rowID = _rowID;
        columnID = _columnID;
    }
}