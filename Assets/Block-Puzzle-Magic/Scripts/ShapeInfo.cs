using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ShapeInfo : MonoBehaviour
{
    [HideInInspector] public Sprite blockImage;

    [HideInInspector] public ShapeBlock firstBlock;

    //Status whether block is a bandage block capable of being played over other blocks.
    [HideInInspector] public bool isBandageShape;

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

        Debug.Log("Started a new shape! " + this);
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

    public void ConvertToBandageShape()
    {
        Debug.Log("Converting shape to bandage. " + this);
        isBandageShape = true;
        var powerupInfo = BlockShapeSpawner.Instance.FindPowerupById((int) PowerupInfo.Powerups.Bandage);

        foreach (var block in ShapeBlocks)
        {
            Instantiate(powerupInfo.powerupBlockIcon, block.block, false);
        }
    }

    public void ConvertToPowerup(PowerupBlockSpawn powerupInfo)
    {
        if (ShapeBlocks == null || ShapeBlocks.Count == 0)
        {
            Debug.Log("No Shape Blocks, not converting to powerup. shape=" + this + " powerup=" + powerupInfo);
            return;
        }

        ShapeBlocks.ForEach(b => { Instantiate(powerupInfo.powerupBlockIcon, b.block, false); });
    }

    public bool IsPowerup()
    {
        return ShapeID >= 1000;
    }

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

    public IEnumerator PerformPowerup(List<Block> currentBlocks)
    {
        switch (ShapeID)
        {
            case (int) Powerups.Flood:

                Debug.Log("Played Flood Powerup");
                yield return HandleFloodBlocks(currentBlocks);

                break;
            case (int) Powerups.Doubler:

                Debug.Log("Played Doubler Powerup");
                HandleDoublerBlocks(currentBlocks);
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

            case (int) Powerups.SticksGalore:

                Debug.Log("Played Sticks Galore Powerup");
                foreach (var block in currentBlocks) block.ConvertToSticksGalore();
                break;

            case (int) Powerups.Lag:

                Debug.Log("Played Lag Powerup");
                foreach (var block in currentBlocks) block.ConvertToLagBlock();
                break;

            case (int) Powerups.Storm:

                Debug.Log("Played Storm Powerup");
                foreach (var block in currentBlocks) block.ConvertToStormBlock();
                break;

            case (int) Powerups.Quake:

                Debug.Log("Played Quake Powerup");
                foreach (var block in currentBlocks) block.ConvertToQuakeBlock();
                break;

            case (int) Powerups.Avalanche:

                Debug.Log("Played Avalanche Powerup");
                foreach (var block in currentBlocks) block.ConvertToAvalancheBlock();
                break;

            case (int) Powerups.Frenzy:

                Debug.Log("Played Frenzy Powerup");
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
        StartCoroutine(GamePlay.Instance.ShowPowerupActivationSprite(
            BlockShapeSpawner.Instance.FindPowerupById(powerupBlock.blockID), powerupBlock));

        var surroundingBlocks = GamePlay.Instance.SurroundingBlocksInRadius(powerupBlock, 2, true);
        foreach (var block in surroundingBlocks)
        {
            block.ConvertToFilledBlock(0);
            block.colorId = powerupBlock.colorId;
            block.blockImage.sprite = powerupBlock.blockImage.sprite;
            yield return new WaitForSeconds(0.01f);
        }
    
    }

    private void HandleDoublerBlocks(List<Block> currentBlocks)
    {
        var powerupBlock = currentBlocks[Random.Range(0, currentBlocks.Count)];

        // since doubler is activation once played, show sprite here
        StartCoroutine(GamePlay.Instance.ShowPowerupActivationSprite(
            BlockShapeSpawner.Instance.FindPowerupById(powerupBlock.blockID), powerupBlock));
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