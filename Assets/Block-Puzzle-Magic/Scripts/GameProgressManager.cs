using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class GameProgressManager : Singleton<GameProgressManager>
{
    /// <summary>
    ///     Gets the previous session data.
    /// </summary>
    /// <returns>The previous session data.</returns>
    public PreviousSessionData GetPreviousSessionData()
    {
        var oldProgrss = PlayerPrefs.GetString("GameProgress" + GameController.gameMode, string.Empty);

        if (oldProgrss != string.Empty)
        {
            var data = new PreviousSessionData();
            var xDoc = XDocument.Parse(oldProgrss);

            if (xDoc != null)
            {
                #region game mode

                data.currentMode = (GameMode) Enum.Parse(typeof(GameMode), xDoc.Root.Element("ModeName").Value);

                #endregion

                #region block grid

                var blockDataElement = xDoc.Root.Element("BlockData");
                if (blockDataElement != null)
                {
                    var allRows = blockDataElement.Elements("Row");
                    foreach (var ele in allRows) data.blockGridInfo.Add(ele.Value);
                }

                #endregion

                #region placing blocks

                data.shapeInfo = xDoc.Root.Element("ShapeInfo").Value;

                #endregion

                #region score

                data.score = xDoc.Root.Element("Score").Value.TryParseInt();

                #endregion

                #region total rescue done

                data.totalRescueDone = xDoc.Root.Element("TotalRescueDone").Value.TryParseInt();

                #endregion

                #region total free rescue done

                data.totalFreeRescueDone = xDoc.Root.Element("TotalFreeRescueDone").Value.TryParseInt();

                #endregion

                #region placed bombs

                if (GameController.gameMode == GameMode.BLAST || GameController.gameMode == GameMode.CHALLENGE)
                {
                    var placedBombElement = xDoc.Root.Element("PlacedBombs");
                    if (placedBombElement != null)
                    {
                        var allPlacedBombs = placedBombElement.Elements("Bomb");

                        foreach (var ele in allPlacedBombs) data.placedBombInfo.Add(ConverToPlacedBombInfo(ele));
                    }
                }

                #endregion

                #region remaining time

                if (GameController.gameMode == GameMode.TIMED || GameController.gameMode == GameMode.CHALLENGE)
                    data.remainingTime = xDoc.Root.Element("RemainingTime").Value.TryParseInt(60);

                #endregion

                #region total rescue done

                data.movesCount = xDoc.Root.Element("MovesCount").Value.TryParseInt();

                #endregion

                return data;
            }
        }

        return null;
    }

    /// <summary>
    ///     Convers to placed bomb info.
    /// </summary>
    /// <returns>The to placed bomb info.</returns>
    /// <param name="ele">Ele.</param>
    public PlacedBomb ConverToPlacedBombInfo(XElement ele)
    {
        return new PlacedBomb(ele.Attribute("rowID").Value.TryParseInt(), ele.Attribute("columnID").Value.TryParseInt(),
            ele.Attribute("bombCounter").Value.TryParseInt());
    }

    /// <summary>
    ///     Raises the application pause event.
    /// </summary>
    /// <param name="pause">If set to <c>true</c> pause.</param>
    private void OnApplicationPause(bool pause)
    {
        if (pause) SaveGame();
    }

    /// <summary>
    ///     Saves the game.
    /// </summary>
    public void SaveGame()
    {
        var xDocProgress = XDocument.Parse(Resources.Load("BoardTemplate").ToString());
        if (xDocProgress != null)
        {
            #region game mode

            var GameModeElement = xDocProgress.Root.Element("ModeName");
            GameModeElement.Value = GameController.gameMode.ToString();

            #endregion

            #region block grid

            var blockDataElement = xDocProgress.Root.Element("BlockData");
            var index = 0;

            for (var rowID = 0; rowID < GameBoardGenerator.Instance.TotalRows; rowID++)
            {
                var row = "";
                for (var columnID = 0; columnID < GameBoardGenerator.Instance.TotalColumns; columnID++)
                {
                    row += GamePlay.Instance.blockGrid[index].blockID + ",";
                    index++;
                }

                row = row.Remove(row.Length - 1);
                blockDataElement.Add(CreateXElement("Row", row));
            }

            #endregion

            #region placing blocks

            xDocProgress.Root.Element("ShapeInfo").Value = BlockShapeSpawner.Instance.GetAllOnBoardShapeNames();

            #endregion


            #region score

            xDocProgress.Root.Element("Score").Value = ScoreManager.Instance.GetScore().ToString();

            #endregion

            #region total rescue done

            xDocProgress.Root.Element("TotalRescueDone").Value = GamePlay.Instance.TotalRescueDone.ToString();

            #endregion

            #region total free rescue done

            xDocProgress.Root.Element("TotalFreeRescueDone").Value = GamePlay.Instance.TotalFreeRescueDone.ToString();

            #endregion

            #region placed bombs

            if (GameController.gameMode == GameMode.BLAST || GameController.gameMode == GameMode.CHALLENGE)
            {
                var placedBombsElement = xDocProgress.Root.Element("PlacedBombs");
                var bombBlocks = GamePlay.Instance.blockGrid.FindAll(o => o.isBomb);
                foreach (var block in bombBlocks)
                    placedBombsElement.Add(CreateBombElement(block.rowID, block.columnID, block.bombCounter));
            }

            #endregion

            #region remaining time

            if (GameController.gameMode == GameMode.TIMED || GameController.gameMode == GameMode.CHALLENGE)
                xDocProgress.Root.Element("RemainingTime").Value =
                    GamePlay.Instance.timeSlider.GetRemainingTime().ToString();

            #endregion

            #region moves count

            xDocProgress.Root.Element("MovesCount").Value = GamePlay.Instance.MoveCount.ToString();

            #endregion

            #region save progress

            PlayerPrefs.SetString("GameProgress" + GameController.gameMode, xDocProgress.ToString());

            #endregion
        }
    }

    /// <summary>
    ///     Clears the progress.
    /// </summary>
    public void ClearProgress()
    {
        PlayerPrefs.DeleteKey("GameProgress" + GameController.gameMode);
    }

    /// <summary>
    ///     Creates the X element.
    /// </summary>
    /// <returns>The X element.</returns>
    /// <param name="elementName">Element name.</param>
    /// <param name="value">Value.</param>
    private XElement CreateXElement(string elementName, string value)
    {
        var ele = new XElement(elementName);
        ele.Value = value;
        return ele;
    }

    /// <summary>
    ///     Creates the bomb element.
    /// </summary>
    /// <returns>The bomb element.</returns>
    /// <param name="rowID">Row I.</param>
    /// <param name="columnID">Column I.</param>
    /// <param name="bombCounter">Bomb counter.</param>
    private XElement CreateBombElement(int rowID, int columnID, int bombCounter)
    {
        var ele = new XElement("Bomb");
        ele.Add(new XAttribute("rowID", rowID));
        ele.Add(new XAttribute("columnID", columnID));
        ele.Add(new XAttribute("bombCounter", bombCounter));
        return ele;
    }
}

/// <summary>
///     Previous session data.
/// </summary>
public class PreviousSessionData
{
    public List<string> blockGridInfo = new List<string>();
    public int coinsEarned = 0;
    public GameMode currentMode;
    public int movesCount;
    public List<PlacedBomb> placedBombInfo = new List<PlacedBomb>();
    public int remainingTime;
    public int score;
    public string shapeInfo = string.Empty;
    public int totalFreeRescueDone;
    public int totalRescueDone;
}

/// <summary>
///     Placed bomb.
/// </summary>
public class PlacedBomb
{
    public int bombCounter;
    public int columnID;
    public int rowID;

    public PlacedBomb(int _rowID, int _columnID, int _bombCounter)
    {
        rowID = _rowID;
        columnID = _columnID;
        bombCounter = _bombCounter;
    }
}