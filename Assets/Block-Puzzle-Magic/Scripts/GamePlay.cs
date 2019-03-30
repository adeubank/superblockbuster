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
    private int _sticksGaloreRounds;
    [SerializeField] public GameObject blockDandelionSeedPrefab;
    [HideInInspector] public List<Block> blockGrid;

    public AudioClip blockNotPlacedSound;
    [SerializeField] public GameObject blockOmnicolorPrefab;

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
    private readonly List<Block> _activeQuakePowerups = new List<Block>();
    private int _spawnAvalancheBlocks;

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
        
        yield return BreakAllCompletedLines(placingShapeBlockCount);        
        
        yield return AddShapesAndUpdateRound();
   
        if (GameController.gameMode == GameMode.BLAST || GameController.gameMode == GameMode.CHALLENGE)
            UpdateBlockCount();
    }

    private IEnumerator PrepPowerupsBeforeClearing()
    {
        Debug.Log("Prepping any to be activated powerups");

        var clearedLineBlocks = GetFilledRows().Concat(GetFilledColumns()).SelectMany(line => line).ToList();

        // find any quake blocks activated
        _activeQuakePowerups.AddRange(clearedLineBlocks.Where(b => b.isQuakePowerup).ToList());

        #region powerup activation sprite

        clearedLineBlocks.Where(b => Enum.IsDefined(typeof(PowerupInfo.Powerups), b.blockID))
            .ToList()
            .ForEach((powerupBlock) =>
            {
                var powerupBlockSpawn = BlockShapeSpawner.Instance.FindPowerupById(powerupBlock.blockID);
                StartCoroutine(ShowPowerupActivationSprite(powerupBlockSpawn, powerupBlock));
            });

        #endregion

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

        #region avalanche block spawn

        _spawnAvalancheBlocks += clearedLineBlocks.Count(b => b.isAvalanchePowerup);

        #endregion
    }

    private IEnumerator ActivateAvalanchePowerup()
    {
        Debug.Log("Spawning avalanche blocks! spawnAvalancheBlocks=" + _spawnAvalancheBlocks);
        var blocksToConvertToOmnicolor = new List<Block>();
        for (; _spawnAvalancheBlocks > 0; _spawnAvalancheBlocks--)
        {
            Debug.Log("Avalanching these lines. b.rowID=" + (_spawnAvalancheBlocks - 1) + ", " +
                      "b.columnID=" + (GameBoardGenerator.Instance.TotalColumns - _spawnAvalancheBlocks) + ", " +
                      "b.rowID=" + (GameBoardGenerator.Instance.TotalRows - _spawnAvalancheBlocks) + ", " +
                      "b.columnID=" + (_spawnAvalancheBlocks - 1));
            var newBlocksToConvertToOmnicolor = blockGrid.Where(b =>
            {
                // top of the board
                return b.rowID == _spawnAvalancheBlocks - 1 ||
                       // right of the board
                       b.columnID == GameBoardGenerator.Instance.TotalColumns - _spawnAvalancheBlocks ||
                       // bottom of the board
                       b.rowID == GameBoardGenerator.Instance.TotalRows - _spawnAvalancheBlocks ||
                       // left of the board
                       b.columnID == _spawnAvalancheBlocks - 1;
            }).ToList();

            blocksToConvertToOmnicolor.AddRange(newBlocksToConvertToOmnicolor);
        }

        blocksToConvertToOmnicolor.Sort();

        foreach (var b in blocksToConvertToOmnicolor)
        {
            b.ConvertToOmnicolorBlock();
            yield return new WaitForSeconds(0.01f);
        }
    }

    public IEnumerator ShowPowerupActivationSprite(PowerupBlockSpawn powerupBlockSpawn, Block currentBlock)
    {
        if (!powerupBlockSpawn.powerupActivationSprite)
        {
            Debug.Log("No powerup activation sprite found. " + powerupBlockSpawn);
            yield break;
        }

        GameObject powerupActivationSprite = Instantiate(powerupBlockSpawn.powerupActivationSprite,
            currentBlock.transform.position, Quaternion.identity,
            GameBoardGenerator.Instance.BoardContent.transform);
        var powerupActivationSpriteCanvas = powerupActivationSprite.AddComponent(typeof(Canvas)) as Canvas;
        powerupActivationSpriteCanvas.overrideSorting = true;
        powerupActivationSpriteCanvas.sortingOrder = 999;

        Sequence sequence = DOTween.Sequence();
        powerupActivationSprite.transform.localScale = Vector3.zero;
        sequence.Append(powerupActivationSprite.transform.DOScale(Vector3.one, 0.4f));
        sequence.Append(powerupActivationSprite.transform.DOPunchScale(Vector3.one * 0.05f, 0.2f, 1, 0.1f));
        sequence.AppendInterval(0.4f);
        sequence.Append(powerupActivationSprite.transform.DOLocalJump(Vector3.up * 1000f, 100f, 1, 0.8f));
        sequence.AppendCallback(() => { Destroy(powerupActivationSprite); });
        yield return new WaitUntil(() => !sequence.IsActive() || sequence.IsComplete());
    }

    private IEnumerator ActivateQuakePowerup()
    {
        if (!_activeQuakePowerups.Any())
        {
            Debug.Log("No quake powerups to activate");
            yield break;
        }

        var shakeComponent = gameObject.GetComponent<ShakeGameObject>();
        shakeComponent.shakeDuration += 1.3f; // start the shake

        var allTweeners = _activeQuakePowerups.Aggregate(new List<int>(), (columnsToShake, nextQuakePowerup) =>
        {
            Debug.Log("Activating quake powerup. " + nextQuakePowerup);

            for (int col = nextQuakePowerup.columnID - 1; col <= nextQuakePowerup.columnID + 1; col++)
            {
                if (!columnsToShake.Contains(col) && col >= 0 && col < GameBoardGenerator.Instance.TotalColumns)
                {
                    columnsToShake.Add(col);
                }
            }

            return columnsToShake;
        }).SelectMany(columnToShake =>
        {
            var column = GetEntireColumnForRescue(columnToShake);
            return ShakeColumnDown(column.Where(b => !b.isQuakePowerup).ToList());
        }).ToList();
        
        _activeQuakePowerups.Clear();

        yield return new WaitWhile(() => allTweeners.Any(t => t.IsActive()));
    }

    private List<Tweener> ShakeColumnDown(List<Block> column)
    {
        var alreadyFalling = new List<Block>();
        column.Sort((a, b) => b.rowID.CompareTo(a.rowID));
        return column.Aggregate(new List<Tweener>(), (tweeners, nextBlockToFill) =>
        {
            if (nextBlockToFill && !nextBlockToFill.isFilled)
            {
                var nextBlockToFall = column.FirstOrDefault(b =>
                    b.columnID == nextBlockToFill.columnID && b.rowID < nextBlockToFill.rowID && b.isFilled &&
                    !alreadyFalling.Contains(b));
                if (nextBlockToFall)
                {
                    // track the block so we don't consider it again
                    alreadyFalling.Add(nextBlockToFall);
                    nextBlockToFall.isFilled = false; // mark it empty

                    // TODO spawn a empty cell to fill the hole as this block falls
                    var emptyCell = Instantiate(GameBoardGenerator.Instance.emptyBlockTemplate,
                        nextBlockToFall.transform.position, Quaternion.identity,
                        GameBoardGenerator.Instance.BoardContent.transform);
                    var emptyCellCanvas = emptyCell.AddComponent(typeof(Canvas)) as Canvas;
                    emptyCellCanvas.overrideSorting = true;
                    emptyCellCanvas.sortingOrder = 998;

                    // have it render on top of everything as it falls down
                    var nextBlockToFallCanvas = nextBlockToFall.gameObject.AddComponent(typeof(Canvas)) as Canvas;
                    nextBlockToFallCanvas.overrideSorting = true;
                    nextBlockToFallCanvas.sortingOrder = 999;

                    // track original position as we animate the block down and then move it up after it is cleared
                    var transform1 = nextBlockToFall.transform;
                    var origPos = transform1.localPosition;
                    transform1.localPosition = new Vector3(origPos.x, origPos.y, 999f);
                    var tweener = nextBlockToFall.transform.DOLocalMove(nextBlockToFill.transform.localPosition, 0.8f)
                        .OnComplete(() =>
                        {
                            // set the new blocks place
                            nextBlockToFill.isFilled = true;
                            nextBlockToFill.Copy(nextBlockToFall);

                            // clean up the falling block
                            nextBlockToFall.ClearBlock(false);
                            nextBlockToFall.transform.localPosition = origPos;
                            Destroy(emptyCell);
                            Destroy(nextBlockToFallCanvas);
                        });

                    tweeners.Add(tweener);
                }
            }

            return tweeners;
        }).ToList();
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
        // if blocks were filled, means end of round and some powerups are activated
        if (BlockShapeSpawner.Instance.FillShapeContainer())
        {
            var newRound = currentRound + 1;
            Debug.Log("Updating round from currentRound=" + currentRound + " newRound=" + newRound);
            UpdateRound(newRound);

            yield return RoundClearPowerups();

            yield return PrepPowerupsBeforeClearing();

            yield return BreakAllCompletedLines(1);

            var activeShapeContainers = BlockShapeSpawner.Instance.GetActiveShapeContainers();
            var playableShapes = activeShapeContainers.FindAll(t => t.childCount > 0)
                .Select(t => t.GetChild(0).GetComponent<ShapeInfo>()).ToList();
            BlockShapeSpawner.Instance.CheckOnBoardShapeStatus(playableShapes);
        }

        #region re-enable timer

        if (GameController.gameMode == GameMode.TIMED || GameController.gameMode == GameMode.CHALLENGE)
            timeSlider.ResumeTimer();

        #endregion
    }

    private IEnumerator RoundClearPowerups()
    {       
        #region sticks galore spawn

        // increment sticks galore round before spawning any shapes
        if (BlockShapeSpawner.Instance.isNextRoundSticksGaloreBlocks)
        {
            _sticksGaloreRounds++;

            Debug.Log("Spawning Stick Galore blocks! _sticksGaloreRounds=" + _sticksGaloreRounds);

            if (_sticksGaloreRounds >= 2)
            {
                BlockShapeSpawner.Instance.DeactivateSticksGalore();
                _sticksGaloreRounds = 0;
            }
        }

        #endregion

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

        yield return new WaitWhile(() => possibleTweens.Any(t => t.IsActive() && t.IsPlaying()));

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
    /// <param name="placingShapeBlockCount">Placing shape block count.</param>
    private IEnumerator BreakAllCompletedLines(int placingShapeBlockCount)
    {
        var breakingRows = GetFilledRows();
        var breakingColumns = GetFilledColumns();

        if (breakingRows.Count == 0 && breakingColumns.Count == 0)
        {
            Debug.Log("No breaking lines.");
            ScoreManager.Instance.AddScore(10 * placingShapeBlockCount);
            yield break;
        }
        
        yield return PrepPowerupsBeforeClearing();

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
            if (row.TrueForAll(b => b.colorId == firstColorId || b.isOmnicolorBlock)) return total + 1;
            return total;
        });
        var columnsWithSameColor = breakingColumns.Aggregate(0, (total, column) =>
        {
            var firstColorId = column.First().colorId;
            if (column.TrueForAll(b => b.colorId == firstColorId || b.isOmnicolorBlock)) return total + 1;
            return total;
        });

        var sameColorMultiplier = rowsWithSameColor + columnsWithSameColor;
        var multiplier = 1 + sameColorMultiplier + rowAndColumnBreakMultiplier;

        Debug.Log("Breaking lines! " +
                  "totalBreakingRowBlocks=" + totalBreakingRowBlocks +
                  " totalBreakingColumnBlocks=" + totalBreakingColumnBlocks +
                  " newScore=" + newScore +
                  " multiplier=" + multiplier +
                  " sameColorMultiplier=" + sameColorMultiplier +
                  " rowAndColumnBreakMultiplier=" + rowAndColumnBreakMultiplier);

        ScoreManager.Instance.AddScore(newScore * multiplier);

        if (breakingRows.Count > 0)
            foreach (var thisLine in breakingRows)
                StartCoroutine(BreakThisLine(thisLine));

        if (breakingColumns.Count > 0)
            foreach (var thisLine in breakingColumns)
                StartCoroutine(BreakThisLine(thisLine));

        yield return new WaitWhile(() => DOTween.TotalPlayingTweens() > 0);

        yield return ActivateQuakePowerup();
 
        yield return ActivateAvalanchePowerup();

        Debug.Log("Finished breaking lines.");

        #region clearing was exploding blocks

        // remove still exploding blocks and reset them
        foreach (var wasExplodingBlock in blockGrid.Where(b => b.isExploding))
        {
            Debug.Log("Removing the isExploding flag from block. " + wasExplodingBlock);
            wasExplodingBlock.isFilled = false;
            wasExplodingBlock.isExploding = false;
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
                b.isDandelionPowerup = false;
                Debug.Log("Cleared a dandelion powerup! Scattering seeds. " + b);
                yield return HandleDandelionPowerup(b);
            }

            if (b.isBandagePowerup)
            {
                b.isBandagePowerup = false;
                Debug.Log("Cleared a bandage powerup! Next round is bandage shapes. " + b);
                BlockShapeSpawner.Instance.isNextRoundBandageBlock = true;
            }

            if (b.isBombPowerup)
            {
                b.isBombPowerup = false;
                Debug.Log("Cleared a bomb powerup! Detonating this block! " + b);
                b.ConvertToExplosion();
            }

            if (b.isSticksGalorePowerup)
            {
                b.isSticksGalorePowerup = false;
                Debug.Log("Cleared a sticks galore powerup! Next round are stick shapes. " + b);
                BlockShapeSpawner.Instance.isNextRoundSticksGaloreBlocks = true;
                BlockShapeSpawner.Instance.sticksGaloreColorId = b.colorId;
            }

            if (b.isLagPowerup)
            {
                b.isLagPowerup = false;
                Debug.Log("Cleared a Lag powerup! Time is slower!  " + b);
                timeSlider.ActivateLagPowerup();
            }

            if (b.isStormPowerup)
            {
                b.isStormPowerup = false;
                Debug.Log("Cleared a Storm powerup! Randomly clearing 3 rows!  " + b);
                StartCoroutine(ActivateStormPowerup());
            }

            b.ClearBlock(true);

            yield return new WaitForEndOfFrame();
        }

        yield return new WaitUntil(() => DOTween.TotalPlayingTweens() == 0);
    }

    private IEnumerator ActivateStormPowerup()
    {
        var allRows = new List<List<Block>>();
        for (var index = 0; index < GameBoardGenerator.Instance.TotalRows; index++)
        {
            var row = GetEntireRowForRescue(index);
            allRows.Add(row);
        }

        var stormRows = new List<List<Block>>();
        while (stormRows.Count < 3)
        {
            var randomIndex = Random.Range(0, allRows.Count);
            var stormRow = allRows[randomIndex];
            stormRows.Add(stormRow);
            allRows.RemoveAt(randomIndex);
        }

        stormRows.SelectMany(row => row).Where(b => !b.isFilled).ToList();

        foreach (var row in stormRows)
        {
            foreach (var b in row)
            {
                b.ConvertToFilledBlock(0);
                yield return new WaitForSeconds(0.01f);
            }
        }

        yield return BreakAllCompletedLines(1);
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
            StartCoroutine(BreakAllCompletedLines(0));
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
            if (!seedBlocks.Contains(availableBlocks[randomIndex]))
                seedBlocks.Add(availableBlocks[randomIndex]);
        }

        var seedTweeners = seedBlocks.Select(b =>
        {
            Debug.Log("New seed block. " + b);
            return b.ConvertToDandelionSeed(dandelionPowerup);
        }).ToList();

        yield return new WaitWhile(() => seedTweeners.Any(t => t.IsPlaying()));
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