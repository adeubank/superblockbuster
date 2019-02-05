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

    private void Start()
    {
        CreateBlockList();
        
        firstBlock = ShapeBlocks[0];
        blockImage = firstBlock.block.GetComponent<Image>().sprite;
        startOffsetX = firstBlock.rowID;
        startOffsetY = firstBlock.columnID;
    }

    private void CreateBlockList()
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