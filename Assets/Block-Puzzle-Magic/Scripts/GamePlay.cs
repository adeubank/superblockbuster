using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;
#if HBDOTween
using DG.Tweening;

#endif

// This script has main logic to run entire gameplay.
public class GamePlay : Singleton<GamePlay>, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler
{
    [HideInInspector] public List<Block> blockGrid;

    public AudioClip blockNotPlacedSound;

    public AudioClip blockPlaceSound;
    public AudioClip blockSelectSound;

    public Sprite BombSprite;
    [HideInInspector] public int currentRound = 1;

    private ShapeInfo currentShape;

    private List<Block> highlightingBlocks;
    private Transform hittingBlock;

    public bool isHelpOnScreen;

    // Line break sounds.
    [SerializeField] private AudioClip lineClear1;
    [SerializeField] private AudioClip lineClear2;
    [SerializeField] private AudioClip lineClear3;
    [SerializeField] private AudioClip lineClear4;

    [Tooltip("Max no. of times rescue can be used in 1 game. -1 is infinite")] [SerializeField]
    private int MaxAllowedRescuePerGame;

    [Tooltip("Max no. of times rescue can be used in 1 game using watch video. -1 is infinite")] [SerializeField]
    private int MaxAllowedVideoWatchRescue;

    [HideInInspector] public int MoveCount;

    public Timer timeSlider;

    [HideInInspector] public int TotalFreeRescueDone;

    [HideInInspector] public int TotalRescueDone;
    [HideInInspector] public Text txtCurrentRound;

    #region IBeginDragHandler implementation

    /// <summary>
    ///     Raises the begin drag event.
    /// </summary>
    /// <param name="eventData">Event data.</param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentShape != null)
        {
            var pos = Camera.main.ScreenToWorldPoint(eventData.position);
            pos.z = currentShape.transform.localPosition.z;
            currentShape.transform.localPosition = pos;
        }
    }

    #endregion

    #region IDragHandler implementation

    /// <summary>
    ///     Raises the drag event.
    /// </summary>
    /// <param name="eventData">Event data.</param>
    public void OnDrag(PointerEventData eventData)
    {
        if (currentShape != null)
        {
            var pos = Camera.main.ScreenToWorldPoint(eventData.position);
            pos = new Vector3(pos.x, pos.y + 1F, 0F);

            currentShape.transform.position = pos;

            var hit = Physics2D.Raycast(currentShape.GetComponent<ShapeInfo>().firstBlock.block.position, Vector2.zero,
                1);

            if (hit.collider != null)
            {
                if (hittingBlock == null || hit.collider.transform != hittingBlock)
                {
                    hittingBlock = hit.collider.transform;
                    CanPlaceShape(hit.collider.transform);
                }
            }
            else
            {
                StopHighlighting();
            }
        }
    }

    #endregion

    #region IPointerDownHandler implementation

    /// <summary>
    ///     Raises the pointer down event.
    /// </summary>
    /// <param name="eventData">Event data.</param>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject != null)
        {
            var clickedObject = eventData.pointerCurrentRaycast.gameObject.transform;

            if (clickedObject.GetComponent<ShapeInfo>() != null)
                if (clickedObject.transform.childCount > 0)
                {
                    currentShape = clickedObject.GetComponent<ShapeInfo>();
                    var pos = Camera.main.ScreenToWorldPoint(eventData.position);
                    currentShape.transform.localScale = Vector3.one;
                    currentShape.transform.localPosition = new Vector3(pos.x, pos.y, 0);
                    AudioManager.Instance.PlaySound(blockSelectSound);

                    if (isHelpOnScreen) GetComponent<InGameHelp>().StopHelp();
                }
        }
    }

    #endregion

    #region IPointerUpHandler implementation

    /// <summary>
    ///     Raises the pointer up event.
    /// </summary>
    /// <param name="eventData">Event data.</param>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (currentShape != null)
        {
            if (highlightingBlocks.Count > 0)
            {
                StartCoroutine(nameof(PlaceBlockCheckBoardStatus));
            }
            else
            {
#if HBDOTween
                currentShape.transform.DOLocalMove(Vector3.zero, 0.5F);
                currentShape.transform.DOScale(Vector3.one * 0.6F, 0.5F);
#endif
                currentShape = null;
                AudioManager.Instance.PlaySound(blockNotPlacedSound);
            }
        }
    }

    #endregion

    private void Start()
    {
        //Generate board from GameBoardGenerator Script Component.
        GetComponent<GameBoardGenerator>().GenerateBoard();
        highlightingBlocks = new List<Block>();

        #region time mode

        // Timer will start with TIME and CHALLENGE mode.
        if (GameController.gameMode == GameMode.TIMED || GameController.gameMode == GameMode.CHALLENGE)
            timeSlider.gameObject.SetActive(true);

        #endregion

        #region check for help

        Invoke("CheckForHelp", 0.5F);

        #endregion
    }

    /// <summary>
    ///     Determines whether this instance can place shape the specified currentHittingBlock.
    /// </summary>
    public bool CanPlaceShape(Transform currentHittingBlock)
    {
        var currentCell = currentHittingBlock.GetComponent<Block>();

        var currentRowID = currentCell.rowID;
        var currentColumnID = currentCell.columnID;

        StopHighlighting();

        var canPlaceShape = true;

        foreach (var c in currentShape.ShapeBlocks)
        {
            var checkingCell = blockGrid.Find(o =>
                o.rowID == currentRowID + c.rowID + currentShape.startOffsetX &&
                o.columnID == currentColumnID + (c.columnID - currentShape.startOffsetY));

            if (checkingCell == null || checkingCell != null && !currentShape.isBandageShape && checkingCell.isFilled)
            {
                canPlaceShape = false;
                highlightingBlocks.Clear();
                break;
            }

            if (!highlightingBlocks.Contains(checkingCell)) highlightingBlocks.Add(checkingCell);
        }

        if (canPlaceShape) SetHighLightImage();

        return canPlaceShape;
    }

    /// <summary>
    ///     Sets the high light image.
    /// </summary>
    private void SetHighLightImage()
    {
        foreach (var c in highlightingBlocks) c.SetHighlightImage(currentShape.blockImage);
    }

    /// <summary>
    ///     Stops the highlighting.
    /// </summary>
    private void StopHighlighting()
    {
        if (highlightingBlocks != null && highlightingBlocks.Count > 0)
            foreach (var c in highlightingBlocks)
                c.StopHighlighting();
        hittingBlock = null;
        highlightingBlocks.Clear();
    }

    /// <summary>
    ///     Sets the image to placing blocks.
    /// </summary>
    private void SetImageToPlacingBlocks()
    {
        if (highlightingBlocks != null && highlightingBlocks.Count > 0)
            foreach (var c in highlightingBlocks)
                c.SetBlockImage(currentShape.blockImage, currentShape.ShapeID);
    }

    /// <summary>
    ///     Checks the board status.
    /// </summary>
    public IEnumerator PlaceBlockCheckBoardStatus()
    {
        Debug.Log("Placing Block and Checking Board Status");

        SetImageToPlacingBlocks();
        AudioManager.Instance.PlaySound(blockPlaceSound);

        if (currentShape.IsPowerup())
            yield return currentShape.GetComponent<PowerupInfo>().PerformPowerup(highlightingBlocks);

        Destroy(currentShape.gameObject);
        currentShape = null;
        MoveCount += 1;

        yield return new WaitForEndOfFrame();

        var placingShapeBlockCount = highlightingBlocks.Count;
        var firstHighlightedBlock = highlightingBlocks.First();
        var touchingSameColor = blockGrid
            .FindAll(o => o.colorId == firstHighlightedBlock.colorId && o.blockID != firstHighlightedBlock.blockID).Any(
                o =>
                {
                    return highlightingBlocks.Any(hb =>
                    {
                        var touching = o.rowID == hb.rowID &&
                                       Mathf.Abs(o.columnID - hb.columnID) <= 1 ||
                                       o.columnID == hb.columnID &&
                                       Mathf.Abs(o.rowID - hb.rowID) <= 1;
                        if (touching) Debug.Log("Touching same color block! o=" + o + " hb=" + hb);
                        return touching;
                    });
                });

        if (touchingSameColor) placingShapeBlockCount *= 2;

        highlightingBlocks.Clear();

        var clearedLineBlocks = GetFilledRows().Concat(GetFilledColumns()).SelectMany(line => line).ToList();

        #region bomb powerup preperation

        // find any bomb blocks about to be detonated
        var activeBombPowerups = clearedLineBlocks.Where(b => b.isBombPowerup).ToList();
        if (activeBombPowerups.Any()) PrepDetonatingBombBlockPowerups(activeBombPowerups);

        #endregion

        #region color coder preparation

        // find any bomb blocks about to be detonated
        var clearedColorCoderBlocks = clearedLineBlocks.Where(b => b.isColorCoderPowerup).ToList();
        if (clearedColorCoderBlocks.Any()) yield return ActivateClearedColorCoderBlocks(clearedColorCoderBlocks);

        #endregion

        var breakingRows = GetFilledRows();
        var breakingColumns = GetFilledColumns();

        if (breakingRows.Count > 0 || breakingColumns.Count > 0)
        {
            yield return BreakAllCompletedLines(breakingRows, breakingColumns, placingShapeBlockCount);
        }
        else
        {
            StartCoroutine(nameof(AddShapesAndUpdateRound));
            ScoreManager.Instance.AddScore(10 * placingShapeBlockCount);
        }

        if (GameController.gameMode == GameMode.BLAST || GameController.gameMode == GameMode.CHALLENGE)
            UpdateBlockCount();
    }

    private IEnumerator ActivateClearedColorCoderBlocks(List<Block> clearedColorCoderBlocks)
    {
        var colorCoderTweeners = clearedColorCoderBlocks.SelectMany(powerupBlock =>
        {
            Debug.Log("Activating Cleared Color Coder Block. " + powerupBlock);

            var tweeners = new List<Block>();
            var rowId = powerupBlock.rowID;
            var colId = powerupBlock.columnID;
            for (var index = 1;
                index < GameBoardGenerator.Instance.TotalRows ||
                index < GameBoardGenerator.Instance.TotalColumns;
                index++)
            {
                var leftBlock = Instance.blockGrid.Find(b => b.rowID == rowId && b.columnID == colId - index);
                var rightBlock = Instance.blockGrid.Find(b => b.rowID == rowId && b.columnID == colId + index);
                var upBlock = Instance.blockGrid.Find(b => b.rowID == rowId + index && b.columnID == colId);
                var downBlock = Instance.blockGrid.Find(b => b.rowID == rowId - index && b.columnID == colId);
                if (leftBlock) tweeners.Add(leftBlock);

                if (rightBlock) tweeners.Add(rightBlock);

                if (upBlock) tweeners.Add(upBlock);

                if (downBlock) tweeners.Add(downBlock);
            }

            return tweeners.Select(block =>
                {
                    var prevColor = block.blockImage.color;
                    var prevImageSprite = block.blockImage.sprite;
                    block.blockImage.sprite = powerupBlock.blockImage.sprite;
                    // transition block to the next color
                    return block.blockImage.DOFade(0.1f, 0.4f).OnComplete(() =>
                    {
                        if (block.isFilled)
                        {
                            block.colorId = powerupBlock.colorId;
                            block.blockImage.color = prevColor;
                            block.blockImage.sprite = powerupBlock.blockImage.sprite;
                        }
                        else
                        {
                            block.blockImage.color = prevColor;
                            block.blockImage.sprite = prevImageSprite;
                        }
                    });
                }
            ).ToList();
        }).ToList();

        yield return new WaitWhile(() => colorCoderTweeners.Any(t => t.IsActive()));
    }

    public List<List<Block>> GetFilledRows()
    {
        var breakingRows = new List<List<Block>>();
        for (var row = 0; row < GameBoardGenerator.Instance.TotalRows; row++)
        {
            var currentRow = GetEntireRow(row);
            if (currentRow != null) breakingRows.Add(currentRow);
        }

        return breakingRows;
    }

    public List<List<Block>> GetFilledColumns()
    {
        var breakinColumns = new List<List<Block>>();
        for (var column = 0; column < GameBoardGenerator.Instance.TotalColumns; column++)
        {
            var currentColumn = GetEntireColumn(column);
            if (currentColumn != null) breakinColumns.Add(currentColumn);
        }

        return breakinColumns;
    }

    public IEnumerator AddShapesAndUpdateRound()
    {
        // if no blocks added, just return
        if (!BlockShapeSpawner.Instance.FillShapeContainer()) yield break;

        var newRound = currentRound + 1;
        Debug.Log("Updating round from currentRound=" + currentRound + " newRound=" + newRound);
        UpdateRound(newRound);

        #region dandelion seed sprout

        var possibleTweens = new List<Tweener>();
        foreach (var seedBlock in blockGrid.Where(b => b.isDandelionSeed))
        {
            Debug.Log("Found dandelion seed block rowId=" + seedBlock.rowID + " columnId=" + seedBlock.columnID);

            foreach (var surroundingBlock in SurroundingBlocksInRadius(seedBlock, 1, true))
                if (!surroundingBlock.isFilled)
                    possibleTweens.Add(surroundingBlock.ConvertToSeedSproutBlock());

            seedBlock.ClearExtraChildren();
            seedBlock.isDandelionSeed = false;
        }

        yield return new WaitWhile(() => possibleTweens.Any(t => t.IsPlaying()));

        var breakingRows = GetFilledRows();
        var breakingColumns = GetFilledColumns();

        if (breakingRows.Count > 0 || breakingColumns.Count > 0)
            yield return BreakAllCompletedLines(breakingRows, breakingColumns, 1);

        #endregion
    }

    private void UpdateRound(int newRound)
    {
        currentRound = newRound;
        var strCurrentRound = currentRound.ToString();
        txtCurrentRound.SetText(strCurrentRound.PadLeft(Math.Min(strCurrentRound.Length + 1, 2), '0'));

        // speed up game as rounds progress
        if (currentRound < 10)
        {
            timeSlider.UpdateTimerSpeed(0.1f);
        }
        else if (currentRound > 10)
        {
            BlockShapeSpawner.Instance.SetBlockShapeToSix();
            timeSlider.UpdateTimerSpeed(0.11f);
        }
        else if (currentRound > 20)
        {
            timeSlider.UpdateTimerSpeed(0.125f);
        }
    }

    /// <summary>
    ///     Gets the entire row.
    /// </summary>
    /// <returns>The entire row.</returns>
    /// <param name="rowID">Row I.</param>
    private List<Block> GetEntireRow(int rowID)
    {
        var thisRow = new List<Block>();
        for (var columnIndex = 0; columnIndex < GameBoardGenerator.Instance.TotalColumns; columnIndex++)
        {
            var block = blockGrid.Find(o => o.rowID == rowID && o.columnID == columnIndex);

            if (block.isFilled)
                thisRow.Add(block);
            else
                return null;
        }

        return thisRow;
    }

    /// <summary>
    ///     Gets the entire row for rescue.
    /// </summary>
    /// <returns>The entire row for rescue.</returns>
    /// <param name="rowID">Row I.</param>
    private List<Block> GetEntireRowForRescue(int rowID)
    {
        var thisRow = new List<Block>();
        for (var columnIndex = 0; columnIndex < GameBoardGenerator.Instance.TotalColumns; columnIndex++)
        {
            var block = blockGrid.Find(o => o.rowID == rowID && o.columnID == columnIndex);
            thisRow.Add(block);
        }

        return thisRow;
    }

    /// <summary>
    ///     Gets the entire column.
    /// </summary>
    /// <returns>The entire column.</returns>
    /// <param name="columnID">Column I.</param>
    private List<Block> GetEntireColumn(int columnID)
    {
        var thisColumn = new List<Block>();
        for (var rowIndex = 0; rowIndex < GameBoardGenerator.Instance.TotalRows; rowIndex++)
        {
            var block = blockGrid.Find(o => o.rowID == rowIndex && o.columnID == columnID);
            if (block.isFilled)
                thisColumn.Add(block);
            else
                return null;
        }

        return thisColumn;
    }

    /// <summary>
    ///     Gets the entire column for rescue.
    /// </summary>
    /// <returns>The entire column for rescue.</returns>
    /// <param name="columnID">Column I.</param>
    private List<Block> GetEntireColumnForRescue(int columnID)
    {
        var thisColumn = new List<Block>();
        for (var rowIndex = 0; rowIndex < GameBoardGenerator.Instance.TotalRows; rowIndex++)
        {
            var block = blockGrid.Find(o => o.rowID == rowIndex && o.columnID == columnID);
            thisColumn.Add(block);
        }

        return thisColumn;
    }

    /// <summary>
    ///     Breaks all completed lines.
    /// </summary>
    /// <returns>The all completed lines.</returns>
    /// <param name="breakingRows">Breaking rows.</param>
    /// <param name="breakingColumns">Breaking columns.</param>
    /// <param name="placingShapeBlockCount">Placing shape block count.</param>
    private IEnumerator BreakAllCompletedLines(List<List<Block>> breakingRows, List<List<Block>> breakingColumns,
        int placingShapeBlockCount)
    {
        var totalBreakingLines = breakingRows.Count + breakingColumns.Count;
        var totalBreakingRowBlocks =
            breakingRows.SelectMany(row => row.Select(b => b)).Sum(b => b.isDoublePoints ? 2 : 1);
        var totalBreakingColumnBlocks =
            breakingColumns.SelectMany(col => col.Select(b => b)).Sum(b => b.isDoublePoints ? 2 : 1);
        var totalBreakingBlocks = totalBreakingRowBlocks + totalBreakingColumnBlocks;
        var newScore = 0;

        if (totalBreakingLines == 1)
        {
            AudioManager.Instance.PlaySound(lineClear1);
            newScore = totalBreakingBlocks * 10 + placingShapeBlockCount * 10;
        }
        else if (totalBreakingLines == 2)
        {
            AudioManager.Instance.PlaySound(lineClear2);
            newScore = totalBreakingBlocks * 10 + placingShapeBlockCount * 10;
        }
        else if (totalBreakingLines == 3)
        {
            AudioManager.Instance.PlaySound(lineClear3);
            newScore = totalBreakingBlocks * 20 + placingShapeBlockCount * 10;
        }
        else if (totalBreakingLines >= 4)
        {
            AudioManager.Instance.PlaySound(lineClear4);
            newScore = totalBreakingBlocks * 30 + placingShapeBlockCount * 10;
        }

        // clearing row and column at same time multiplier
        var rowAndColumnBreakMultiplier = breakingRows.Any() && breakingColumns.Any() ? 1 : 0;

        // find rows/columns with same color and add to multiplier
        var rowsWithSameColor = breakingRows.Aggregate(0, (total, row) =>
        {
            var firstColorId = row.First().colorId;
            if (row.TrueForAll(b => b.colorId == firstColorId)) return total + 1;
            return total;
        });
        var columnsWithSameColor = breakingColumns.Aggregate(0, (total, column) =>
        {
            var firstColorId = column.First().colorId;
            if (column.TrueForAll(b => b.colorId == firstColorId)) return total + 1;
            return total;
        });

        var sameColorMultiplier = rowsWithSameColor + columnsWithSameColor;
        var multiplier = 1 + sameColorMultiplier + rowAndColumnBreakMultiplier;

        Debug.Log("Breaking lines! " +
                  "totalBreakingRowBlocks=" + totalBreakingRowBlocks +
                  " totalBreakingColumnBlocks=" + totalBreakingColumnBlocks +
                  " newScore=" + newScore +
                  " sameColorMultiplier=" + sameColorMultiplier +
                  " rowAndColumnBreakMultiplier=" + rowAndColumnBreakMultiplier);

        ScoreManager.Instance.AddScore(newScore * multiplier);

        yield return new WaitForEndOfFrame();

        #region time mode

        if (GameController.gameMode == GameMode.TIMED || GameController.gameMode == GameMode.CHALLENGE)
            timeSlider.PauseTimer();

        #endregion

        if (breakingRows.Count > 0)
            foreach (var thisLine in breakingRows)
                StartCoroutine(BreakThisLine(thisLine));

        if (breakingColumns.Count > 0)
            foreach (var thisLine in breakingColumns)
                StartCoroutine(BreakThisLine(thisLine));

        yield return new WaitWhile(() => DOTween.TotalPlayingTweens() > 0);

        yield return new WaitForEndOfFrame();

        Debug.Log("Finished breaking lines.");

        // remove still exploding blocks and reset them
        foreach (var wasExplodingBlock in blockGrid.Where(b => b.isExploding))
        {
            Debug.Log("Removing the isExploding flag from block. " + wasExplodingBlock);
            wasExplodingBlock.isFilled = false;
            wasExplodingBlock.isExploding = false;
        }

        StartCoroutine(nameof(AddShapesAndUpdateRound));

        #region time mode

        if (GameController.gameMode == GameMode.TIMED || GameController.gameMode == GameMode.CHALLENGE)
        {
            timeSlider.ResumeTimer();
            timeSlider.AddSeconds(totalBreakingLines * multiplier * 5);
        }

        #endregion
    }

    /// <summary>
    ///     Breaks the this line.
    /// </summary>
    /// <returns>The this line.</returns>
    /// <param name="breakingLine">Breaking line.</param>
    private IEnumerator BreakThisLine(List<Block> breakingLine)
    {
        Debug.Log("Breaking a line starting with block. " + breakingLine.First());

        foreach (var b in breakingLine)
        {
            if (b.isDandelionPowerup)
            {
                Debug.Log("Cleared a dandelion powerup! Scattering seeds. " + b);
                yield return HandleDandelionPowerup(b);
            }

            else if (b.isBandagePowerup)
            {
                Debug.Log("Cleared a bandage powerup! Next round is bandage shapes. " + b);
                BlockShapeSpawner.Instance.isNextRoundBandageBlock = true;
            }

            else if (b.isBombPowerup)
            {
                Debug.Log("Cleared a bomb powerup! Detonating this block! " + b);
                b.ConvertToExplosion();
            }

            b.ClearBlock();

            yield return new WaitForEndOfFrame();
        }

        yield return new WaitUntil(() => DOTween.TotalPlayingTweens() == 0);
    }

    /// <summary>
    ///     Determines whether this instance can existing blocks placed the specified OnBoardBlockShapes.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if this instance can existing blocks placed the specified OnBoardBlockShapes; otherwise,
    ///     <c>false</c>.
    /// </returns>
    /// <param name="OnBoardBlockShapes">On board block shapes.</param>
    public bool CanExistingBlocksPlaced(IEnumerable<ShapeInfo> OnBoardBlockShapes)
    {
        foreach (var block in blockGrid)
            if (!block.isFilled)
                foreach (var info in OnBoardBlockShapes)
                {
                    var canPlace = CheckShapeCanPlace(block, info);
                    if (canPlace) return true;
                }


        return false;
    }

    /// <summary>
    ///     Checks the shape can place.
    /// </summary>
    /// <returns><c>true</c>, if shape can place was checked, <c>false</c> otherwise.</returns>
    /// <param name="placingBlock">Placing block.</param>
    /// <param name="placingBlockShape">Placing block shape.</param>
    private bool CheckShapeCanPlace(Block placingBlock, ShapeInfo placingBlockShape)
    {
        var currentRowID = placingBlock.rowID;
        var currentColumnID = placingBlock.columnID;

        if (placingBlockShape != null && placingBlockShape.ShapeBlocks != null && !placingBlockShape.isBandageShape)
            foreach (var c in placingBlockShape.ShapeBlocks)
            {
                var checkingCell = blockGrid.Find(o =>
                    o.rowID == currentRowID + c.rowID + placingBlockShape.startOffsetX &&
                    o.columnID == currentColumnID + (c.columnID - placingBlockShape.startOffsetY));

                if (checkingCell == null || checkingCell != null && checkingCell.isFilled) return false;
            }

        return true;
    }

    /// <summary>
    ///     Raises the unable to place shape event.
    /// </summary>
    public void OnUnableToPlaceShape()
    {
        if (TotalRescueDone < MaxAllowedRescuePerGame || MaxAllowedRescuePerGame < 0)
        {
            GamePlayUI.Instance.ShowRescue(GameOverReason.OUT_OF_MOVES);
        }
        else
        {
            Debug.Log("GameOver Called..");
            OnGameOver();
        }
    }

    /// <summary>
    ///     Raises the bomb counter over event.
    /// </summary>
    public void OnBombCounterOver()
    {
        if (TotalRescueDone < MaxAllowedRescuePerGame || MaxAllowedRescuePerGame < 0)
        {
            GamePlayUI.Instance.ShowRescue(GameOverReason.BOMB_COUNTER_ZERO);
        }
        else
        {
            Debug.Log("GameOver Called..");
            OnGameOver();
        }
    }

    /// <summary>
    ///     Executes the rescue.
    /// </summary>
    private void ExecuteRescue()
    {
        if (GamePlayUI.Instance.currentGameOverReson == GameOverReason.OUT_OF_MOVES)
        {
            var TotalBreakingColumns = 3;
            var TotalBreakingRows = 3;

            var totalColumns = GameBoardGenerator.Instance.TotalColumns;
            var totalRows = GameBoardGenerator.Instance.TotalRows;

            var startingColumn = (int) (totalColumns / 2F - TotalBreakingColumns / 2F);
            var startingRow = (int) (totalRows / 2F - TotalBreakingRows / 2F);

            var breakingColums = new List<List<Block>>();

            for (var columnIndex = startingColumn;
                columnIndex <= startingColumn + (TotalBreakingColumns - 1);
                columnIndex++) breakingColums.Add(GetEntireColumnForRescue(columnIndex));

            var breakingRows = new List<List<Block>>();

            for (var rowIndex = startingRow; rowIndex <= startingRow + (TotalBreakingRows - 1); rowIndex++)
                breakingRows.Add(GetEntireRowForRescue(rowIndex));
            StartCoroutine(BreakAllCompletedLines(breakingRows, breakingColums, 0));
        }

        #region bomb mode

        if ((GameController.gameMode == GameMode.BLAST || GameController.gameMode == GameMode.CHALLENGE) &&
            GamePlayUI.Instance.currentGameOverReson == GameOverReason.BOMB_COUNTER_ZERO)
        {
            var bombBlocks = blockGrid.FindAll(o => o.isBomb);
            foreach (var block in bombBlocks)
            {
                if (block.bombCounter <= 1) block.SetCounter(block.bombCounter + 4);
                block.DecreaseCounter();
            }
        }

        #endregion

        #region time mode

        if (GameController.gameMode == GameMode.TIMED || GameController.gameMode == GameMode.CHALLENGE)
        {
            if (GamePlayUI.Instance.currentGameOverReson == GameOverReason.TIME_OVER) timeSlider.AddSeconds(30);
            timeSlider.ResumeTimer();
        }

        #endregion
    }

    /// <summary>
    ///     Ises the free rescue available.
    /// </summary>
    /// <returns><c>true</c>, if free rescue available was ised, <c>false</c> otherwise.</returns>
    public bool isFreeRescueAvailable()
    {
        if (TotalFreeRescueDone < MaxAllowedVideoWatchRescue || MaxAllowedVideoWatchRescue < 0) return true;
        return false;
    }

    /// <summary>
    ///     Raises the rescue done event.
    /// </summary>
    /// <param name="isFreeRescue">If set to <c>true</c> is free rescue.</param>
    public void OnRescueDone(bool isFreeRescue)
    {
        if (isFreeRescue)
            TotalFreeRescueDone += 1;
        else
            TotalRescueDone += 1;
        //CloseRescuePopup ();
        Invoke("ExecuteRescue", 0.5F);
    }

    /// <summary>
    ///     Raises the rescue discarded event.
    /// </summary>
    public void OnRescueDiscarded()
    {
        //CloseRescuePopup ();
        Debug.Log("GameOver Called..");
        OnGameOver();
    }

    /// <summary>
    /// Closes the rescue popup.
    /// </summary>
    // void CloseRescuePopup()
    // {
    // 	GameObject currentWindow = StackManager.Instance.WindowStack.Peek ();
    // 	if (currentWindow != null) {
    // 		if (currentWindow.OnWindowRemove () == false) {
    // 			Destroy (currentWindow);
    // 		}
    // 		StackManager.Instance.WindowStack.Pop ();
    // 	}
    // }

    /// <summary>
    ///     Raises the game over event.
    /// </summary>
    public void OnGameOver()
    {
        var gameOverScreen = StackManager.Instance.gameOverScreen;
        gameOverScreen.Activate();
        gameOverScreen.GetComponent<GameOver>().SetLevelScore(ScoreManager.Instance.Score, 10);
        GameProgressManager.Instance.ClearProgress();
        StackManager.Instance.DeactivateGamePlay();
    }

    #region powerup dandelion activation

    private IEnumerator HandleDandelionPowerup(Block dandelionPowerup)
    {
        var seedBlocks = new List<Block>();

        // give open blocks more priority over empty
        var availableBlocks = Instance.blockGrid.Where(b => !b.isFilled).ToList();
        availableBlocks.ToList().AddRange(Instance.blockGrid);

        while (seedBlocks.Count < 5)
        {
            var randomIndex = Random.Range(0, availableBlocks.Count());
            seedBlocks.Add(availableBlocks[randomIndex]);
        }

        var seedTweeners = seedBlocks.Aggregate(new List<Tweener>(), (tweeners, b) =>
        {
            b.isDandelionSeed = true;
            var newSeedBlockIcon = Instantiate(BlockShapeSpawner.Instance.powerupBlockIconDandelionPrefab,
                dandelionPowerup.transform.position, Quaternion.identity,
                b.blockImage.transform);
            tweeners.Add(newSeedBlockIcon.transform.DOMove(b.blockImage.transform.position, 0.4f));
            return tweeners;
        });

        yield return new WaitWhile(() => seedTweeners.Any(t => t != null && t.IsPlaying()));
    }

    #endregion

    #region powerup bomb prepping for detonation

    private void PrepDetonatingBombBlockPowerups(IEnumerable<Block> clearingBlocks)
    {
        var analyzedBlocks = new List<Block>();
        var bombPowerups = new Stack<Block>(clearingBlocks.Where(block => block.isBombPowerup));

        while (bombPowerups.Any())
        {
            var bombPowerup = bombPowerups.Pop();

            if (analyzedBlocks.Contains(bombPowerup)) continue;

            analyzedBlocks.Add(bombPowerup);

            Debug.Log("Found bomb block. Prepping surrounding blocks. rowId=" + bombPowerup.rowID + " columnId=" +
                      bombPowerup.columnID);

            foreach (var surroundingBlock in SurroundingBlocks(bombPowerup))
            {
                if (analyzedBlocks.Contains(surroundingBlock)) continue;

                analyzedBlocks.Add(surroundingBlock);

                if (!surroundingBlock.isFilled)
                {
                    Debug.Log("Prepping this block for exploding. rowId=" + surroundingBlock.rowID + " columnId=" +
                              surroundingBlock.columnID);

                    surroundingBlock.ConvertToExplodingBlock();
                }

                if (surroundingBlock.isBombPowerup) bombPowerups.Push(surroundingBlock);
            }
        }
    }

    #endregion

    public IEnumerable<Block> SurroundingBlocks(Block centerBlock)
    {
        return SurroundingBlocksInRadius(centerBlock, 1, false);
    }

    public IEnumerable<Block> SurroundingBlocksInRadius(Block centerBlock, int radius, bool includeCenterBlock)
    {
        for (var row = centerBlock.rowID - radius; row <= centerBlock.rowID + radius; row++)
        for (var col = centerBlock.columnID - radius; col <= centerBlock.columnID + radius; col++)
        {
            var block = Instance.blockGrid.Find(b => b.rowID == row && b.columnID == col);
            if (block)
                if (centerBlock != block || includeCenterBlock)
                    yield return block;
        }
    }

    #region Bomb Mode Specific

    /// <summary>
    ///     Updates the block count.
    /// </summary>
    private void UpdateBlockCount()
    {
        var bombBlocks = blockGrid.FindAll(o => o.isBomb);
        foreach (var block in bombBlocks) block.DecreaseCounter();

        var doPlaceBomb = false;
        if (MoveCount <= 10)
        {
            if (MoveCount % 5 == 0) doPlaceBomb = true;
        }
        else if (MoveCount <= 30)
        {
            if ((MoveCount - 10) % 4 == 0) doPlaceBomb = true;
        }
        else if (MoveCount <= 48)
        {
            if ((MoveCount - 30) % 3 == 0) doPlaceBomb = true;
        }
        else
        {
            if ((MoveCount - 48) % 2 == 0) doPlaceBomb = true;
        }

        if (doPlaceBomb) PlaceAtRandomPlace();
    }

    /// <summary>
    ///     Places at random place.
    /// </summary>
    private void PlaceAtRandomPlace()
    {
        var BombPlacingRows = new List<int>();

        for (var rowIndex = 0; rowIndex < GameBoardGenerator.Instance.TotalRows; rowIndex++)
        {
            var canPlace = CheckRowForBombPlace(rowIndex);
            if (canPlace) BombPlacingRows.Add(rowIndex);
        }

        var RandomRowForPlacingBomb = 0;
        if (BombPlacingRows.Count > 0)
            RandomRowForPlacingBomb = BombPlacingRows[Random.Range(0, BombPlacingRows.Count)];
        else
            RandomRowForPlacingBomb = Random.Range(0, GameBoardGenerator.Instance.TotalRows);

        PlaceBombAtRow(RandomRowForPlacingBomb);
    }

    /// <summary>
    ///     Places the bomb at row.
    /// </summary>
    /// <param name="rowIndex">Row index.</param>
    private void PlaceBombAtRow(int rowIndex)
    {
        var emptyBlockForThisRow = blockGrid.FindAll(o => o.rowID == rowIndex && o.isFilled == false);
        var randomBlock = emptyBlockForThisRow[Random.Range(0, emptyBlockForThisRow.Count)];

        if (randomBlock != null) randomBlock.ConvertToBomb();
    }

    /// <summary>
    ///     Checks the row for bomb place.
    /// </summary>
    /// <returns><c>true</c>, if row for bomb place was checked, <c>false</c> otherwise.</returns>
    /// <param name="rowIndex">Row index.</param>
    private bool CheckRowForBombPlace(int rowIndex)
    {
        var block = blockGrid.Find(o => o.rowID == rowIndex && o.isFilled);
        var totalEmptyBlocks = blockGrid.FindAll(o => o.rowID == rowIndex && o.isFilled == false).Count;

        if (block != null && totalEmptyBlocks >= 2)
            return true;
        return false;
    }

    #endregion

    #region show help if mode opened first time

    /// <summary>
    ///     Checks for help.
    /// </summary>
    public void CheckForHelp()
    {
        var isHelpShown = false;
        if (GameController.gameMode == GameMode.BLAST || GameController.gameMode == GameMode.ADVANCE ||
            GameController.gameMode == GameMode.TIMED)
        {
            isHelpShown = PlayerPrefs.GetInt("isHelpShown_" + GameController.gameMode, 0) == 0 ? false : true;
            if (!isHelpShown)
            {
                var inGameHelp = gameObject.AddComponent<InGameHelp>();
                inGameHelp.StartHelp();
            }
            else
            {
                ShowBasicHelp();
            }
        }
        else
        {
            ShowBasicHelp();
        }
    }

    /// <summary>
    ///     Raises the help popup closed event.
    /// </summary>
    public void OnHelpPopupClosed()
    {
        if (GameController.gameMode == GameMode.TIMED) timeSlider.ResumeTimer();
        ShowBasicHelp();
    }

    /// <summary>
    ///     Shows the basic help.
    /// </summary>
    private void ShowBasicHelp()
    {
        var isBasicHelpShown = PlayerPrefs.GetInt("isBasicHelpShown", 0) == 0 ? false : true;
        if (!isBasicHelpShown)
        {
            var inGameHelp = gameObject.GetComponent<InGameHelp>();
            if (inGameHelp == null) inGameHelp = gameObject.AddComponent<InGameHelp>();
            inGameHelp.ShowBasicHelp();
        }
    }

    #endregion
}