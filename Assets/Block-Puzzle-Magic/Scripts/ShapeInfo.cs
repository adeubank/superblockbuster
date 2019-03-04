using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ShapeInfo : MonoBehaviour
{
    [HideInInspector] public Sprite blockImage;

    [HideInInspector] public ShapeBlock firstBlock;

    public List<ShapeBlock> ShapeBlocks;
    public int ShapeID;

    [HideInInspector] public int startOffsetX;
    [HideInInspector] public int startOffsetY;

    public bool isBandageShape;
    
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

    public void ConvertToBandageShape()
    {
        var bandageBlockIcon = (GameObject) Instantiate(Resources.Load("Prefabs/UIScreens/GamePlay/PowerupBlockIcons/Powerup-Icon-1003-Bandage"));
        isBandageShape = true;
        foreach (var block in ShapeBlocks)
        {
            Instantiate(bandageBlockIcon, block.block, false);
        }
    }

    public virtual bool IsPowerup()
    {
        return false;
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