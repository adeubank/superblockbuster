using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if HBDOTween

#endif


public class GameBoardGenerator : Singleton<GameBoardGenerator>
{
    private int blockHeight;

    /// Space between each blocks, Configurable from inspector.
    public int blockSpace = 5;

    private int blockWidth;

    /// The content of the board.
    public GameObject BoardContent;

    private int cellIndex;

    /// The empty block template.
    public GameObject emptyBlockTemplate;

    public PreviousSessionData previousSessionData;

    private int startPosx;
    private int startPosy;

    /// Total Column Count, Configurable from inspector.
    public int TotalColumns;

    /// Total Rows, Configurable from inspector.
    public int TotalRows;

    private void Start()
    {
        ///checks if level needs to start from previos session or start new session.
        previousSessionData = GetComponent<GameProgressManager>().GetPreviousSessionData();
    }

    /// <summary>
    ///     Generates the board.
    /// </summary>
    public void GenerateBoard()
    {
        if (previousSessionData != null)
            if (previousSessionData.blockGridInfo.Count > 0)
            {
                TotalRows = previousSessionData.blockGridInfo.Count;
                TotalColumns = previousSessionData.blockGridInfo[0].Split(',').Length;
            }

        blockHeight = (int) emptyBlockTemplate.GetComponent<RectTransform>().sizeDelta.x;
        blockWidth = (int) emptyBlockTemplate.GetComponent<RectTransform>().sizeDelta.y;

        startPosx = -((TotalColumns - 1) * (blockHeight + blockSpace) / 2);
        startPosy = (TotalRows - 1) * (blockWidth + blockSpace) / 2;

        var newPosX = startPosx;
        var newPosY = startPosy;

        for (var row = 0; row < TotalRows; row++)
        {
            var thisRowCells = new List<Block>();
            for (var column = 0; column < TotalColumns; column++)
            {
                var newCell = GenerateNewBlock(row, column);
                newCell.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(newPosX, newPosY, 0);
                newPosX += blockWidth + blockSpace;
                var thisCellInfo = newCell.GetComponent<Block>();
                thisCellInfo.blockImage = newCell.transform.GetChild(0).GetComponent<Image>();
                thisCellInfo.rowID = row;
                thisCellInfo.columnID = column;

                #region EdgePiece

                //mark whether or not this is an edge piece
                if (row == 0 || row == TotalRows - 1 || column == 0 || column == TotalRows - 1)
                    thisCellInfo.isEdge = true;

                #endregion

                thisRowCells.Add(thisCellInfo);
                cellIndex++;
            }

            GamePlay.Instance.blockGrid.AddRange(thisRowCells);
            newPosX = startPosx;
            newPosY -= blockHeight + blockSpace;
        }

        SetupPreviousSessionBoard();
    }

    /// <summary>
    ///     Setups the previous session board.
    /// </summary>
    private void SetupPreviousSessionBoard()
    {
        if (previousSessionData != null)
        {
            #region normal blocks setup on board

            if (previousSessionData.blockGridInfo.Count > 0)
            {
                var rowIndex = 0;
                foreach (var gridRow in previousSessionData.blockGridInfo)
                {
                    var columnIndex = 0;
                    foreach (var blockID in gridRow.Split(','))
                    {
                        var thisBlockID = blockID.TryParseInt();
                        if (thisBlockID >= 0)
                        {
                            var thisBlock =
                                GamePlay.Instance.blockGrid.Find(o => o.rowID == rowIndex && o.columnID == columnIndex);
                            if (thisBlock != null) thisBlock.ConvertToFilledBlock(thisBlockID);
                        }

                        columnIndex++;
                    }

                    rowIndex++;
                }
            }

            #endregion

            #region place previous session bombs

            if (GameController.gameMode == GameMode.BLAST || GameController.gameMode == GameMode.CHALLENGE)
                if (previousSessionData.placedBombInfo != null)
                    foreach (var bomb in previousSessionData.placedBombInfo)
                    {
                        var thisBlock =
                            GamePlay.Instance.blockGrid.Find(o => o.rowID == bomb.rowID && o.columnID == bomb.columnID);
                        thisBlock.ConvertToBomb(bomb.bombCounter);
                    }

            #endregion

            #region set score

            ScoreManager.Instance.AddScore(previousSessionData.score, false);

            #endregion

            #region moves count

            GamePlay.Instance.MoveCount = previousSessionData.movesCount;

            #endregion

            #region set timer for time and challenge mode

            if (GameController.gameMode == GameMode.TIMED || GameController.gameMode == GameMode.CHALLENGE)
                GamePlay.Instance.timeSlider.SetTime(previousSessionData.remainingTime);

            #endregion
        }

        GetComponent<GameProgressManager>().ClearProgress();
    }

    /// <summary>
    ///     Generates the new block.
    /// </summary>
    /// <returns>The new block.</returns>
    /// <param name="rowIndex">Row index.</param>
    /// <param name="columnIndex">Column index.</param>
    private GameObject GenerateNewBlock(int rowIndex, int columnIndex)
    {
        var newBlock = Instantiate(emptyBlockTemplate);
        newBlock.GetComponent<RectTransform>().sizeDelta = new Vector2(blockWidth, blockHeight);
        newBlock.transform.SetParent(BoardContent.transform);
        newBlock.transform.localScale = Vector3.one;
        newBlock.transform.SetAsLastSibling();
        newBlock.name = "Block-" + rowIndex + "-" + columnIndex;
        return newBlock;
    }
}