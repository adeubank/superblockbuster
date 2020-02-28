using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;
#if HBDOTween
using DG.Tweening;

#endif

// This script has main logic to run entire gameplay.
public class GamePlay : Singleton<GamePlay>, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler
{
    private readonly List<Block> _activeQuakePowerups = new List<Block>();

    private bool _autoMoveLocked;
    private bool _holdingNewBlocks;
    private bool _isDraggingPlayableShape;
    private bool _isFrenzyPowerupRunning;

    private bool _powerupActivationAlreadyRunning;
    private List<PowerupActivation> _powerupsActivated;
    private int _frenzyPowerupsActivated;
    private int _spawnAvalancheBlocks;
    private int _spawnStormBlocks;
    private int _sticksGaloreRounds;

    [SerializeField] private GameObject autoMoveHighlightImage;
    [SerializeField] public GameObject blockDandelionSeedPrefab;
    [HideInInspector] public List<Block> blockGrid;

    public AudioClip blockNotPlacedSound;
    [SerializeField] public GameObject blockOmnicolorPrefab;

    public AudioClip blockPlaceSound;
    public AudioClip blockSelectSound;

    public Sprite BombSprite;
    [SerializeField] private AudioClip comboSound;
    [HideInInspector] public int currentRound = 1;

    private ShapeInfo currentShape;

    public List<Block> highlightingBlocks;
    private Transform hittingBlock;

    public bool isHelpOnScreen;

    // Line break sounds.
    [SerializeField] private List<AudioClip> lineClearSounds;

    [Tooltip("Max no. of times rescue can be used in 1 game. -1 is infinite")] [SerializeField]
    private int MaxAllowedRescuePerGame;

    [Tooltip("Max no. of times rescue can be used in 1 game using watch video. -1 is infinite")] [SerializeField]
    private int MaxAllowedVideoWatchRescue;

    [HideInInspector] public int MoveCount;
    [SerializeField] private GameObject outOfMovePrefab;

    [SerializeField] private AudioClip outOfMoveSound;

    [SerializeField] private GameObject shapeSelectionArrow;

    public Timer timeSlider;

    [HideInInspector] public int TotalFreeRescueDone;

    [HideInInspector] public int TotalRescueDone;
    [HideInInspector] public Text txtCurrentRound;
    public GameObject stormParticlesPrefab;
    [HideInInspector] public int comboMultiplier = 0;


    #region IBeginDragHandler implementation

    /// <summary>
    ///     Raises the begin drag event.
    /// </summary>
    /// <param name="eventData">Event data.</param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (HoldingNewBlocks()) return;

        if (_isDraggingPlayableShape && currentShape != null)
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
        if (HoldingNewBlocks()) return;

        if (_isDraggingPlayableShape && currentShape != null)
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
                    CanPlaceDraggedShape(hittingBlock);
                }
            }
            else
            {
                hittingBlock = null;
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
        _isDraggingPlayableShape = false;
        if (HoldingNewBlocks()) return;

        if (eventData.pointerCurrentRaycast.gameObject == null) return;

        var clickedObject = eventData.pointerCurrentRaycast.gameObject.transform;
        if (clickedObject.GetComponent<ShapeInfo>() == null) return;
        if (clickedObject.transform.childCount <= 0) return;

        _isDraggingPlayableShape = true;
        currentShape = clickedObject.GetComponent<ShapeInfo>();
        var pos = Camera.main.ScreenToWorldPoint(eventData.position);
        Transform transform1;
        (transform1 = currentShape.transform).localScale = BlockShapeSpawner.Instance.ShapePickupLocalScale();
        transform1.localPosition = new Vector3(pos.x, pos.y, 0);
        AudioManager.Instance.PlaySound(blockSelectSound);
    }

    #endregion

    #region IPointerUpHandler implementation

    /// <summary>
    ///     Raises the pointer up event.
    /// </summary>
    /// <param name="eventData">Event data.</param>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!InputManager.Instance.canInput(0.1f)) return;
        if (HoldingNewBlocks()) return;

        if (currentShape == null)
        {
            ResetCurrentShape();
            StartCoroutine(SetAutoMove());
            return;
        }

        if (_isDraggingPlayableShape && eventData.dragging)
        {
            if (highlightingBlocks.Count > 0)
            {
                StartCoroutine(nameof(PlaceBlockCheckBoardStatus));
            }
            else
            {
                ResetCurrentShape();
                StartCoroutine(SetAutoMove());
            }

            return;
        }

        if (eventData.pointerCurrentRaycast.gameObject == null)
        {
            ResetCurrentShape();
            StartCoroutine(SetAutoMove());
            return;
        }

        // track current highlighted blocks
        // since the state may need to be reset later
        var currentHighlightedBlocks = highlightingBlocks.ToList();

        // check if tapping a shape
        var clickedShape = eventData.pointerCurrentRaycast.gameObject.GetComponent<ShapeInfo>();
        if (clickedShape != null)
        {
            ResetCurrentShape();
            currentShape = clickedShape;
            currentShape.transform.localScale = BlockShapeSpawner.Instance.ShapeContainerLocalScale();

            if (currentShape == clickedShape && BlocksForAutoMove().Any(b => CanPlaceShape(b.transform, currentShape))) HighlightBlocksForAutoMove(currentShape);
            return;
        }

        // check if we have a bullseye
        var clickedBlocked = eventData.pointerCurrentRaycast.gameObject.GetComponent<Block>();
        if (clickedBlocked != null && highlightingBlocks.Count > 0 && highlightingBlocks.Contains(clickedBlocked))
        {
            StartCoroutine(nameof(PlaceBlockCheckBoardStatus));
            return;
        }

        // missed blocks, find the closest block, then see if we can highlight it with current shape
        if (currentShape != null)
        {
            var worldPosition = Camera.main.ScreenToWorldPoint(eventData.position);
            var raycastHit2Ds = new List<RaycastHit2D>(Physics2D.CircleCastAll(worldPosition, 0.1f, Vector2.zero));
            // sort by closest to the tap
            raycastHit2Ds.Sort((raycastHit2D, otherRaycastHit2D) => Vector2.Distance(worldPosition, raycastHit2D.point).CompareTo(Vector2.Distance(worldPosition, otherRaycastHit2D.point)));
            var nearbyBlocks = raycastHit2Ds.Select(hit2D => hit2D.collider.gameObject.GetComponent<Block>()).Where(b => b != null).ToList();
            var didTapHighlightedBlock = nearbyBlocks.Any(nearbyBlock =>
            {
                if (highlightingBlocks.Contains(nearbyBlock))
                {
                    StartCoroutine(nameof(PlaceBlockCheckBoardStatus));
                    return true;
                }

                return false; // no idea where the tap is
            });
            if (didTapHighlightedBlock) return; // no need to reset, assuming shape was placed

            var canPlaceShapeHere = nearbyBlocks.Any(nearbyBlock =>
            {
                // if not, see if we can play it directly here
                if (CanPlaceShape(nearbyBlock.transform, currentShape)) return true;

                // check playable blocks using the max row and columns to check all permutations of placing this shape
                var maxRow = currentShape.ShapeBlocks.Max(b => b.rowID);
                var maxColumn = currentShape.ShapeBlocks.Max(b => b.columnID);

                var playableBlocks = blockGrid.FindAll(b =>
                {
                    return b.rowID >= nearbyBlock.rowID - maxRow && b.rowID <= nearbyBlock.rowID + maxRow &&
                           b.columnID >= nearbyBlock.columnID - maxColumn && b.columnID <= nearbyBlock.columnID + maxColumn;
                }).ToList();

                playableBlocks.Sort((b, oB) => Vector2.Distance(worldPosition, b.transform.position).CompareTo(Vector2.Distance(worldPosition, oB.transform.position)));
                if (playableBlocks.Any(b => CanPlaceShape(b.transform, currentShape))) return true;


                // check surrounding blocks
                var surroundingBlocks = SurroundingBlocks(nearbyBlock).ToList();
                surroundingBlocks.Sort((b, oB) => Vector2.Distance(worldPosition, b.transform.position).CompareTo(Vector2.Distance(worldPosition, oB.transform.position)));
                if (surroundingBlocks.Any(b => CanPlaceShape(b.transform, currentShape))) return true;


                return false; // no idea where the tap is
            });
            if (canPlaceShapeHere) return; // no need to reset, assuming shape was placed
        }

        if (currentShape != null)
        {
            // made it to the end without tapping something meaningful, keep the same highlight
            highlightingBlocks = currentHighlightedBlocks;
            SetHighLightImage(currentShape);
            HighlightBlocksForAutoMove(currentShape);
        }

        StartCoroutine(SetAutoMove());
    }

    #endregion

    private void ResetCurrentShape()
    {
        if (_isDraggingPlayableShape && currentShape != null)
        {
            currentShape.transform.DOLocalMove(Vector3.zero, 0.5F);
            currentShape.transform.DOScale(BlockShapeSpawner.Instance.ShapeContainerLocalScale(), 0.5F);
            currentShape = null;
            AudioManager.Instance.PlaySound(blockNotPlacedSound);
        }
    }


    private void Start()
    {
        InitAds();

        AnalyticsEvent.GameStart();

        //Generate board from GameBoardGenerator Script Component.
        GetComponent<GameBoardGenerator>().GenerateBoard();
        highlightingBlocks = new List<Block>();
        _powerupsActivated = new List<PowerupActivation>();

        #region time mode

        // Timer will start with TIME and CHALLENGE mode.
        if (GameController.gameMode == GameMode.TIMED || GameController.gameMode == GameMode.CHALLENGE)
            timeSlider.gameObject.SetActive(true);

        #endregion

        if (!IsBasicHelpShown())
            ShowBasicHelp();
        else
            BlockShapeSpawner.Instance.FillShapeContainer();
    }

    private void InitAds()
    {
        if (GameController.GamesPlayed() < 1) return;

        AdController.Instance.RequestInterstitial();

        AdController.Instance.ShowBanner();
    }

    private void OnDestroy()
    {
        AdController.Instance.HideBanner();
        DOTween.CompleteAll();
    }

    public IEnumerator SetAutoMove()
    {
        if (_autoMoveLocked) yield break;
        if (currentShape != null) yield break;

        _autoMoveLocked = true;

        yield return new WaitWhile(() =>
        {
            if (HoldingNewBlocks()) return true;

            return false;
        });

        var playableShapes = BlockShapeSpawner.Instance.GetPlayableShapes();
        ShapeInfo nextPlayableShape = null;
        // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
        // find first block where we have a playable shape
        BlocksForAutoMove().Any(block =>
        {
            foreach (var info in playableShapes)
                if (CanPlaceShape(block.transform, info))
                {
                    nextPlayableShape = info;
                    return true;
                }

            return false;
        });

        if (nextPlayableShape != null)
        {
            currentShape = nextPlayableShape;
            HighlightBlocksForAutoMove(nextPlayableShape);
        }

        _autoMoveLocked = false;
    }

    private IEnumerable<Block> BlocksForAutoMove()
    {
        return blockGrid.Where(b => !b.isFilled).AsEnumerable().Reverse();
    }

    public void HighlightBlocksForAutoMove(ShapeInfo shapeInfo)
    {
        // reset the auto move highlight on shape inventory
        BlockShapeSpawner.Instance.GetPlayableShapes().ForEach(info =>
        {
            info.ShapeBlocks.ForEach(shapeBlock =>
            {
                foreach (Transform child in shapeBlock.block)
                    if (child.name.StartsWith(autoMoveHighlightImage.name))
                        Destroy(child.gameObject);
            });
        });

        shapeSelectionArrow.SetActive(true);
        shapeSelectionArrow.transform.SetParent(currentShape.transform.parent, false);
        shapeSelectionArrow.transform.localPosition = new Vector3(0, 105);
        shapeInfo.ShapeBlocks.ForEach(shapeBlock => { Instantiate(autoMoveHighlightImage, shapeBlock.block); });
    }

    /// <summary>
    ///     Determines whether this instance can place shape the specified currentHittingBlock.
    /// </summary>
    public bool CanPlaceDraggedShape(Transform currentHittingBlock)
    {
        return CanPlaceShape(currentHittingBlock, currentShape);
    }

    /// <summary>
    ///     Determines whether this instance can place shape the specified currentHittingBlock.
    /// </summary>
    public bool CanPlaceShape(Transform currentHittingBlock, ShapeInfo shapeInfo)
    {
        var currentCell = currentHittingBlock.GetComponent<Block>();
        var currentRowId = currentCell.rowID;
        var currentColumnId = currentCell.columnID;

        StopHighlighting();

        var canPlaceShape = shapeInfo.ShapeBlocks.All(c =>
        {
            var checkingCell = blockGrid.Find(o =>
                o.rowID == currentRowId + c.rowID + shapeInfo.startOffsetX &&
                o.columnID == currentColumnId + (c.columnID - shapeInfo.startOffsetY));

            if (checkingCell == null || checkingCell != null && !shapeInfo.IsBandageShape() && checkingCell.isFilled)
            {
                highlightingBlocks.Clear();
                return false;
            }

            if (!highlightingBlocks.Contains(checkingCell)) highlightingBlocks.Add(checkingCell);


            return true;
        });

        if (canPlaceShape) SetHighLightImage(shapeInfo);

        return canPlaceShape;
    }

    private void HoldNewBlocks(bool b)
    {
        if (b)
            InputManager.Instance.DisableTouch();
        else
            InputManager.Instance.EnableTouch();
        _holdingNewBlocks = b;
    }

    /// <summary>
    ///     Sets the high light image.
    /// </summary>
    /// <param name="shapeInfo"></param>
    private void SetHighLightImage(ShapeInfo shapeInfo)
    {
        foreach (var c in highlightingBlocks)
        {
            c.gameObject.tag = "Block/Highlighted";
            c.SetHighlightImage(shapeInfo);
        }
    }

    /// <summary>
    ///     Stops the highlighting.
    /// </summary>
    private void StopHighlighting()
    {
        if (highlightingBlocks != null && highlightingBlocks.Count > 0)
            foreach (var c in highlightingBlocks)
            {
                c.gameObject.tag = "Untagged";
                c.StopHighlighting();
                // clear any previous highlights
                c.ClearExtraChildren();
            }

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
            {
                c.StopHighlighting();
                c.SetBlockImage(currentShape.blockImage, currentShape.ShapeID, MoveCount);
            }
    }


    /// <summary>
    ///     Checks the board status.
    /// </summary>
    public IEnumerator PlaceBlockCheckBoardStatus()
    {
        timeSlider.PauseTimer();
        HoldNewBlocks(true);
        MoveCount += 1;

        shapeSelectionArrow.SetActive(false);
        shapeSelectionArrow.transform.SetParent(gameObject.transform);
        SetImageToPlacingBlocks();
        AudioManager.Instance.PlaySound(blockPlaceSound);

        if (currentShape.IsPowerup())
            yield return currentShape.GetComponent<ShapeInfo>().PerformPowerup(highlightingBlocks);

        Destroy(currentShape.gameObject);
        currentShape = null;

        var placingShapeBlockCount = highlightingBlocks.Count;
        var firstHighlightedBlock = highlightingBlocks.First();
        var touchingSameColor = blockGrid
            .FindAll(o => o.colorId == firstHighlightedBlock.colorId && o.blockID != firstHighlightedBlock.blockID).Any(
                o =>
                {
                    return highlightingBlocks.Any(hb =>
                    {
                        var touching = hb.IsTouching(o);
                        return touching;
                    });
                });

        if (touchingSameColor) placingShapeBlockCount *= 2;

        highlightingBlocks.Clear();

        yield return StartScoring(placingShapeBlockCount);

        yield return AddShapesAndUpdateRound(placingShapeBlockCount);

        if (GameController.gameMode == GameMode.BLAST || GameController.gameMode == GameMode.CHALLENGE)
            UpdateBlockCount();

        HoldNewBlocks(false);
        yield return SetAutoMove();
        timeSlider.ResumeTimer();
    }

    private IEnumerator PrepPowerupsBeforeClearing()
    {
        var clearedLineBlocks = GetFilledRows().Concat(GetFilledColumns()).SelectMany(line => line).ToList();

        // find any bomb blocks about to be detonated
        var bombPowerupMoveIds = clearedLineBlocks.Where(b => b.isBombPowerup).Select(b => b.moveID).Distinct();
        var bombPowerups = blockGrid.FindAll(b => bombPowerupMoveIds.Contains(b.moveID));
        if (bombPowerups.Any())
        {
            PrepDetonatingBombBlockPowerups(bombPowerups);
            clearedLineBlocks = GetFilledRows().Concat(GetFilledColumns()).SelectMany(line => line).ToList();
        }

        // find any quake blocks activated
        var quakePowerupMoveIds = clearedLineBlocks.Where(b => b.isQuakePowerup).Select(b => b.moveID).Distinct();
        var quakePowerups = blockGrid.FindAll(b => quakePowerupMoveIds.Contains(b.moveID));
        _activeQuakePowerups.AddRange(quakePowerups);

        // find any color coder blocks about to be detonated
        var colorCoderPowerupMoveIds =
            clearedLineBlocks.Where(b => b.isColorCoderPowerup).Select(b => b.moveID).Distinct();
        var colorCoderPowerups = blockGrid.FindAll(b => colorCoderPowerupMoveIds.Contains(b.moveID));
        if (colorCoderPowerups.Any()) yield return ActivateClearedColorCoderBlocks(colorCoderPowerups);
    }

    private IEnumerator ActivateAvalanchePowerup()
    {
        var blocksToConvertToOmnicolor = new List<Block>();
        for (; _spawnAvalancheBlocks > 0; _spawnAvalancheBlocks--)
        {
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
        var avalancheSequence = DOTween.Sequence();
        foreach (var b in blocksToConvertToOmnicolor)
        {
            avalancheSequence.AppendCallback(() => b.ConvertToOmnicolorBlock());
            avalancheSequence.AppendInterval(0.04f);
        }

        yield return avalancheSequence.WaitForCompletion();
    }

    public IEnumerator ShowPowerupActivationSprite(int powerupId, int moveId,
        Block powerupBlock,
        bool removePowerup)
    {
        var powerupBlockSpawn = BlockShapeSpawner.Instance.FindPowerupById(powerupId);

        if (powerupBlockSpawn == null || powerupBlockSpawn.powerupActivationSprite == null) yield break;

        if (!_powerupActivationAlreadyRunning)
            _powerupActivationAlreadyRunning = true;
        else
            // wait your turn
            yield return new WaitWhile(() => _powerupActivationAlreadyRunning);

        _powerupActivationAlreadyRunning = true;


        var powerupActivationSprite = Instantiate(powerupBlockSpawn.powerupActivationSprite,
            powerupBlock.transform.position, Quaternion.identity,
            gameObject.transform);
        var powerupActivationSpriteCanvas = powerupActivationSprite.AddComponent(typeof(Canvas)) as Canvas;
        powerupActivationSpriteCanvas.overrideSorting = true;
        powerupActivationSpriteCanvas.sortingOrder = 999;

        var powerupActivationSequence = DOTween.Sequence();
        powerupActivationSprite.transform.localScale = Vector3.zero;
        powerupActivationSequence.Append(powerupActivationSprite.transform.DOScale(Vector3.one, 0.4f));
        powerupActivationSequence.Append(
            powerupActivationSprite.transform.DOPunchScale(Vector3.one * 0.05f, 0.2f, 1, 0.1f));
        powerupActivationSequence.AppendCallback(() => _powerupActivationAlreadyRunning = false); // unlock before jump
        powerupActivationSequence.AppendInterval(0.4f);
        powerupActivationSequence.Append(
            powerupActivationSprite.transform.DOLocalJump(Vector3.up * 1000f, 100f, 1, 0.8f));
        powerupActivationSequence.AppendCallback(() =>
        {
            if (removePowerup)
                blockGrid.Where(b => b.moveID == moveId).ToList().ForEach(b => { b.RemovePowerup(); });
            Destroy(powerupActivationSprite);
        });

        if (powerupBlockSpawn.powerupSfx != null) AudioManager.Instance.PlaySound(powerupBlockSpawn.powerupSfx);

        yield return powerupActivationSequence.WaitForCompletion();
    }

    private IEnumerator ActivateQuakePowerup()
    {
        if (!_activeQuakePowerups.Any()) yield break;

        var shakeComponent = gameObject.GetComponent<ShakeGameObject>();
        shakeComponent.shakeDuration += 1f; // start the shake

        var quakeTweeners = _activeQuakePowerups.Aggregate(
            new List<int>(), (columnsToShake, nextQuakePowerup) =>
            {
                // show the activation sprite
                ShouldActivatePowerup(new PowerupActivation(nextQuakePowerup), nextQuakePowerup);

                for (var col = nextQuakePowerup.columnID - 1; col <= nextQuakePowerup.columnID + 1; col++)
                    if (!columnsToShake.Contains(col) && col >= 0 && col < GameBoardGenerator.Instance.TotalColumns)
                        columnsToShake.Add(col);

                return columnsToShake;
            }).SelectMany(columnToShake =>
        {
            var column = GetEntireColumnForRescue(columnToShake);
            return ShakeColumnDown(column.ToList());
        }).ToList();

        _activeQuakePowerups.Clear();

        foreach (var quakeTweener in quakeTweeners) yield return quakeTweener.WaitForCompletion();
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

                    // have it render on top of everything as it falls down
                    var nextBlockToFallCanvas = nextBlockToFall.blockImage.GetComponent(typeof(Canvas)) as Canvas;
                    nextBlockToFallCanvas.overrideSorting = true;
                    nextBlockToFallCanvas.sortingOrder = 3;

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
                            nextBlockToFallCanvas.overrideSorting = false;
                            nextBlockToFallCanvas.sortingOrder = 0;
                            nextBlockToFall.ClearBlock(false);
                            nextBlockToFall.transform.localPosition = origPos;
                            // since the dandelion seed has fallen, clear the old one
                            nextBlockToFall.ClearDandelionSeedIcon();
                        });

                    tweeners.Add(tweener);
                }
            }

            return tweeners;
        }).ToList();
    }

    private IEnumerator ActivateClearedColorCoderBlocks(List<Block> clearedColorCoderBlocks)
    {
        var analyzedBlocks = new List<Block>();
        var colorCoderTweeners = clearedColorCoderBlocks.SelectMany(colorCoderBlock =>
        {
            // show the activation sprite
            if (ShouldActivatePowerup(new PowerupActivation(colorCoderBlock), colorCoderBlock))
            {
                colorCoderBlock.ActivateColorCoder();
            }

            var tweeners = new List<Block>();
            var rowId = colorCoderBlock.rowID;
            var colId = colorCoderBlock.columnID;
            for (var index = 1;
                index < GameBoardGenerator.Instance.TotalRows ||
                index < GameBoardGenerator.Instance.TotalColumns;
                index++)
            {
                var thisTweeners = new List<Block>();
                var leftBlock = Instance.blockGrid.Find(b => b.rowID == rowId && b.columnID == colId - index);
                var rightBlock = Instance.blockGrid.Find(b => b.rowID == rowId && b.columnID == colId + index);
                var upBlock = Instance.blockGrid.Find(b => b.rowID == rowId + index && b.columnID == colId);
                var downBlock = Instance.blockGrid.Find(b => b.rowID == rowId - index && b.columnID == colId);
                if (leftBlock) thisTweeners.Add(leftBlock);

                if (rightBlock) thisTweeners.Add(rightBlock);

                if (upBlock) thisTweeners.Add(upBlock);

                if (downBlock) thisTweeners.Add(downBlock);

                thisTweeners.RemoveAll(t => analyzedBlocks.Contains(t));
                analyzedBlocks.AddRange(thisTweeners);
                tweeners.AddRange(thisTweeners);
            }

            return tweeners.Select(block =>
                {
                    var prevColor = block.blockImage.color;
                    var prevImageSprite = block.blockImage.sprite;

                    // transition block to the next color
                    return block.blockImage.DOFade(0.1f, 0.8f)
                        .OnStart(() => block.blockImage.sprite = colorCoderBlock.blockImage.sprite)
                        .OnComplete(() =>
                        {
                            if (block.isFilled)
                            {
                                block.colorId = colorCoderBlock.colorId;
                                block.blockImage.color = prevColor;
                                block.blockImage.sprite = colorCoderBlock.blockImage.sprite;
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
        var colorCoderSequence = DOTween.Sequence();
        colorCoderTweeners.ForEach(t => colorCoderSequence.Join(t));
        yield return colorCoderSequence.WaitForCompletion();
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

    public IEnumerator AddShapesAndUpdateRound(int placingShapeBlockCount)
    {
        // if blocks were filled, means end of round and some powerups are activated
        if (BlockShapeSpawner.Instance.FillShapeContainer())
        {
            var newRound = currentRound + 1;

            UpdateRound(newRound);

            yield return RoundClearPowerups();

            yield return StartScoring(placingShapeBlockCount);
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

            if (_sticksGaloreRounds >= 2)
            {
                BlockShapeSpawner.Instance.DeactivateSticksGalore();
                _sticksGaloreRounds = 0;
            }
        }

        #endregion

        #region dandelion seed sprout

        var dandelionSeeds = blockGrid.Where(b => b.isDandelionSeed).ToList();

        if (dandelionSeeds.Any())
        {
            var seedSproutSequence = DOTween.Sequence();
            foreach (var seedBlock in dandelionSeeds)
            {
                foreach (var surroundingBlock in SurroundingBlocksInRadius(seedBlock, 1, true))
                    if (!surroundingBlock.isFilled)
                        seedSproutSequence.Join(surroundingBlock.ConvertToSeedSproutBlock());

                seedBlock.ClearDandelionSeedIcon();
            }

            yield return seedSproutSequence.WaitForCompletion();
        }

        #endregion
    }

    private IEnumerator ActivateFrenzyPowerup()
    {
        if (_frenzyPowerupsActivated <= 0) yield break;

        const int frenzyOffscreenOffset = 10;
        const float blockTweenDuration = 0.1f;
        var emptyBottomBlocks = blockGrid.Where(b =>
        {
            // do not consider filled blocks that aren't exploding
            if (b.isFilled && !b.isExploding) return false;

            // fill rows from the bottom
            return b.rowID > GameBoardGenerator.Instance.TotalRows - (5 + _frenzyPowerupsActivated);
        }).ToList();
        var frenzySequence = DOTween.Sequence();
        var numberOfSequences = 0;
        var pseudoBlocks = new List<List<Block>>();

        while (emptyBottomBlocks.Any())
        {
            var nextBlock = emptyBottomBlocks.FirstOrDefault();
            if (!nextBlock) continue;

            emptyBottomBlocks.Remove(nextBlock);

            // find the first shape with less than 5 blocks (or if one empty block left) and be touching might the next block
            var nextBlockShape = pseudoBlocks.Where(blocks => blocks.Count <= 5 || emptyBottomBlocks.Count < 2)
                .FirstOrDefault(blocks => blocks.Any(b => b.IsTouching(nextBlock)));

            if (nextBlockShape != null)
                nextBlockShape.Add(nextBlock);
            else
                pseudoBlocks.Add(new List<Block> {nextBlock});
        }

        pseudoBlocks.ForEach(shape =>
        {
            var shapeSequence = DOTween.Sequence();
            shape.ForEach(b =>
            {
                var blockImageTransform = b.blockImage.transform;
                var startPosition = blockImageTransform.position;
                blockImageTransform.position = new Vector3(startPosition.x + frenzyOffscreenOffset, startPosition.y);
                Tween tween = b.blockImage.transform.DOMove(startPosition, blockTweenDuration);
                tween.SetDelay(numberOfSequences * blockTweenDuration / 4);
                shapeSequence.Insert(0, tween);
                b.ConvertToFrenziedBlock();
            });
            frenzySequence.Append(shapeSequence);
            numberOfSequences += 1;
        });

        yield return frenzySequence.WaitForCompletion();

        // unlock
        _frenzyPowerupsActivated = 0;
    }

    private void UpdateRound(int newRound)
    {
        currentRound = newRound;
        var strCurrentRound = currentRound.ToString();
        txtCurrentRound.SetText(strCurrentRound.PadLeft(Math.Min(strCurrentRound.Length + 1, 2), '0'));
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

            if (isFilledBlock(block))
                thisRow.Add(block);
            else
                return null;
        }

        return thisRow;
    }

    private bool isFilledBlock(Block block)
    {
        if (!block) return false;

        return block.isFilled || block.isExploding;
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
            if (isFilledBlock(block))
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

    private IEnumerator StartScoring(int placingShapeBlockCount)
    {
        comboMultiplier = 1;
        var breakingRows = GetFilledRows();
        var breakingColumns = GetFilledColumns();

        if (breakingRows.Count == 0 && breakingColumns.Count == 0)
        {
            ScoreManager.Instance.AddScore(10 * placingShapeBlockCount);
            yield break;
        }

        do
        {
            yield return PrepPowerupsBeforeClearing();

            // pick up any changes from prep powerups
            breakingRows = GetFilledRows();
            breakingColumns = GetFilledColumns();

            yield return BreakLines(placingShapeBlockCount, breakingRows, breakingColumns);

            // pick up any changes from after clear powerups
            breakingRows = GetFilledRows();
            breakingColumns = GetFilledColumns();

            IncrementComboMultiplier();
        } while (breakingRows.Count > 0 || breakingColumns.Count > 0);
    }

    private IEnumerator BreakLines(int placingShapeBlockCount, List<List<Block>> breakingRows, List<List<Block>> breakingColumns)
    {
        if (comboMultiplier > 2 && placingShapeBlockCount > 0) AudioManager.Instance.PlaySound(comboSound);

        var shouldActivatePowerups = placingShapeBlockCount > 0;
        var totalBreakingLines = breakingRows.Count + breakingColumns.Count;
        var totalBreakingRowBlocks =
            breakingRows.SelectMany(row => row.Select(b => b)).Sum(b => b.isDoublePoints ? 2 : 1);
        var totalBreakingColumnBlocks =
            breakingColumns.SelectMany(col => col.Select(b => b)).Sum(b => b.isDoublePoints ? 2 : 1);
        var totalBreakingBlocks = totalBreakingRowBlocks + totalBreakingColumnBlocks;

        // increase combo multiplier for every 2 lines broken
        var multiplierMultiLineBreak = totalBreakingLines / 2;
        
        // clearing row and column at same time multiplier
        var multiplierRowAndColumnBreak = breakingRows.Any() && breakingColumns.Any() ? 1 : 0;

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

        var multiplierSameColor = rowsWithSameColor + columnsWithSameColor;
        var multipliersTotal = multiplierSameColor + multiplierRowAndColumnBreak + multiplierMultiLineBreak;
        var newScore = 100 * totalBreakingBlocks * totalBreakingLines + placingShapeBlockCount * 100;

        // break the lines one at a time
        var soundsToPlay = new Queue<AudioClip>(lineClearSounds);
        var breakingLines = breakingRows.Concat(breakingColumns).ToList();
        breakingLines.Shuffle();
        var allLineBreaksSequence = DOTween.Sequence();
        var clearedMoveIds = breakingLines.SelectMany(line => line.Select(b => b.moveID)).Distinct().ToList();
        var clearedBlockIds = breakingLines.SelectMany(line => line.Select(b => b.blockID)).Distinct().ToList();
        clearedMoveIds.Sort();
        clearedBlockIds.Sort();

        foreach (var line in breakingLines)
        {
            allLineBreaksSequence.AppendCallback(() =>
            {
                if (soundsToPlay.Any())
                {
                    var soundToPlay = soundsToPlay.Dequeue();
                    if (placingShapeBlockCount > 0)
                        AudioManager.Instance.PlaySound(soundToPlay);
                }
                else
                {
                    if (placingShapeBlockCount > 0)
                        AudioManager.Instance.PlaySound(lineClearSounds.Last());
                }
            });
            allLineBreaksSequence.Join(BreakThisLine(line, shouldActivatePowerups));
        }

        if (placingShapeBlockCount > 0) ScoreManager.Instance.AddScore(newScore, (comboMultiplier + multipliersTotal));

        yield return allLineBreaksSequence.WaitForCompletion();

        // cleanup any powerups that were cleared ignoring blocks with the default move ID 0
        blockGrid.Where(b => b.moveID > 0 && clearedMoveIds.Contains(b.moveID)).ToList().ForEach(b => { b.RemovePowerup(); });

        #region clearing was exploding blocks

        // remove still exploding blocks and reset them
        var wasExplodingBlocks = blockGrid.Where(b => b.isExploding).ToList();
        if (wasExplodingBlocks.Any())
        {
            var blocksExploded = wasExplodingBlocks.Count(b => b.isFilled);
            var wasExplodingBlocksSequence = DOTween.Sequence();
            foreach (var wasExplodingBlock in wasExplodingBlocks)
                if (wasExplodingBlock.isFilled)
                {
                    PrepBlockForBreak(wasExplodingBlocksSequence, wasExplodingBlock);
                    wasExplodingBlocksSequence.Join(wasExplodingBlock.ClearBlock(true));
                }
                else
                {
                    wasExplodingBlock.isExploding = false;
                }

            if (blocksExploded > 0 && placingShapeBlockCount > 0)
                ScoreManager.Instance.AddScore(blocksExploded * 500);

            yield return wasExplodingBlocksSequence.WaitForCompletion();
        }

        #endregion

        yield return ActivateQuakePowerup();

        yield return ActivateFrenzyPowerup();

        yield return ActivateAvalanchePowerup();

        // do some cleanup, since there are still bugs
        foreach (var block in blockGrid)
        {
            block.blockImage.transform.localPosition = Vector3.zero;
            if (block.blockID == -1 && !block.isDandelionSeed)
            {
                block.ClearBlock(false);
                block.RemovePowerup();
                block.ClearDandelionSeedIcon();
            }
        }
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

        yield return BreakLines(placingShapeBlockCount, breakingRows, breakingColumns);
    }

    /// <summary>
    ///     Breaks the this line.
    /// </summary>
    /// <returns>The this line.</returns>
    /// <param name="breakingLine">Breaking line.</param>
    /// <param name="activatePowerups">Flag whether to call powerup code or not</param>
    private Sequence BreakThisLine(List<Block> breakingLine, bool activatePowerups)
    {
        var lineBreakSequence = DOTween.Sequence();

        foreach (var b in breakingLine)
        {
            if (activatePowerups)
                PrepBlockForBreak(lineBreakSequence, b);

            lineBreakSequence.Join(b.ClearBlock(true));
        }

        return lineBreakSequence;
    }

    private void PrepBlockForBreak(Sequence tweenSequence, Block block)
    {
        var maybeNewPowerup = new PowerupActivation(block);
        var shouldActivatePowerup = ShouldActivatePowerup(maybeNewPowerup, block);

        if (!shouldActivatePowerup) return;

        if (block.isDandelionPowerup)
        {
            block.isDandelionPowerup = false;
            tweenSequence.Join(HandleDandelionPowerup(block));
        }

        if (block.isBombPowerup)
            blockGrid.Where(other => other.moveID == block.moveID).ToList().ForEach(other =>
            {
                other.isBombPowerup = false;
                other.ConvertToExplosion();
            });

        if (block.isSticksGalorePowerup)
        {
            block.isSticksGalorePowerup = false;
            BlockShapeSpawner.Instance.isNextRoundSticksGaloreBlocks = true;
            BlockShapeSpawner.Instance.sticksGaloreColorId = block.colorId;
        }

        if (block.isLagPowerup)
        {
            block.isLagPowerup = false;
            timeSlider.ActivateLagPowerup();
        }

        if (block.isStormPowerup)
        {
            block.isStormPowerup = false;
            BlockShapeSpawner.Instance.ActivateStormFloodRound();
        }

        if (block.isFrenzyPowerup)
        {
            block.isFrenzyPowerup = false;
            _frenzyPowerupsActivated += 1;
        }

        if (block.isAvalanchePowerup) _spawnAvalancheBlocks += 1;
    }

    private bool ShouldActivatePowerup(PowerupActivation powerupActivation, Block powerupBlock)
    {
        if (powerupActivation.PowerupID == 0 || powerupBlock.moveID < 1) return false;
        if (!Enum.IsDefined(typeof(ShapeInfo.Powerups), powerupActivation.PowerupID)) return false;

        if (_powerupsActivated.Any(p => p.MoveID == powerupActivation.MoveID && p.PowerupID == powerupActivation.PowerupID))
            return false;

        IncrementComboMultiplier();
        _powerupsActivated.Add(powerupActivation);

        switch (powerupActivation.PowerupID)
        {
            case (int) ShapeInfo.Powerups.Dandelion:
                powerupBlock.ActivateDandelionParticles();
                break;
            case (int) ShapeInfo.Powerups.Quake:
                powerupBlock.ActivateQuakeParticles();
                break;
            case (int) ShapeInfo.Powerups.Storm:
                ActivateStormParticles();
                break;
            case (int) ShapeInfo.Powerups.Frenzy:
                powerupBlock.ActivateFrenzyParticles();
                break;
            case (int) ShapeInfo.Powerups.Lag:
                powerupBlock.ActivateLagParticles();
                break;
            case (int) ShapeInfo.Powerups.Avalanche:
                powerupBlock.ActivateAvalancheParticles();
                break;
        }

        // do not show for on-place activation powerups
        if ((int) ShapeInfo.Powerups.Doubler != powerupActivation.PowerupID &&
            (int) ShapeInfo.Powerups.Flood != powerupActivation.PowerupID &&
            (int) ShapeInfo.Powerups.Bandage != powerupActivation.PowerupID)
            StartCoroutine(ShowPowerupActivationSprite(powerupBlock.blockID, powerupBlock.moveID, powerupBlock, true));

        // use the activation animation as a time to cleanup the icons
        blockGrid.Where(b => b.moveID == powerupBlock.moveID).ToList().ForEach(b => b.RemovePowerupIcon());

        return true;
    }

    private void IncrementComboMultiplier()
    {
        comboMultiplier += 1;
    }

    private void ActivateStormParticles()
    {
        var halfCanvasWidth = transform.parent.GetComponent<RectTransform>().sizeDelta.x / 2;
        var gameBoardYOffset = GameBoardGenerator.Instance.BoardContent.transform.localPosition.y;
        var left = Instantiate(stormParticlesPrefab, Vector3.zero, Quaternion.LookRotation(Vector3.left), transform);
        var right = Instantiate(stormParticlesPrefab, Vector3.zero, Quaternion.LookRotation(Vector3.right), transform);
        left.transform.localPosition = new Vector2(-(halfCanvasWidth + 20), gameBoardYOffset);
        right.transform.localPosition = new Vector2(halfCanvasWidth + 20, gameBoardYOffset);
    }

    public void CheckIfOutOfMoves()
    {
        if (CanExistingBlocksPlaced(BlockShapeSpawner.Instance.GetPlayableShapes())) return;
        OnUnableToPlaceShape();
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

        if (placingBlockShape != null && placingBlockShape.ShapeBlocks != null && !placingBlockShape.IsBandageShape())
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
//        if (TotalRescueDone < MaxAllowedRescuePerGame || MaxAllowedRescuePerGame < 0)
//        {
//            GamePlayUI.Instance.ShowRescue(GameOverReason.OUT_OF_MOVES);
//        }
//        else
//        {
//            Debug.Log("GameOver Called..");
//            OnGameOver();
//        }


        StartCoroutine(StartOutOfMovesRescue());
    }

    public IEnumerator StartOutOfMovesRescue()
    {
        // hold play
        HoldNewBlocks(true);
        StopHighlighting();
        currentShape = null;
        timeSlider.PauseTimer();

        // notify users of out of moves
        GamePlayUI.Instance.currentGameOverReson = GameOverReason.OUT_OF_MOVES;
        var outOfMovesGameObject = Instantiate(outOfMovePrefab, transform.position + new Vector3(0, 1000, 0), Quaternion.identity, transform);
        yield return new WaitForFixedUpdate(); // let unity catch up
        var outOfMovesAlertSequence = DOTween.Sequence();
        outOfMovesAlertSequence.AppendCallback(() => { AudioManager.Instance.PlaySound(outOfMoveSound); });
        outOfMovesAlertSequence.Join(outOfMovesGameObject.transform.DOLocalMove(new Vector3(0, 254, 0), 0.8f));
        outOfMovesAlertSequence.AppendCallback(() =>
        {
            var minusScore = -1 * (float) ScoreManager.Instance.Score / 4;
            ScoreManager.Instance.AddScore((int) minusScore);
        });
        outOfMovesAlertSequence.AppendInterval(1.5f);
        outOfMovesAlertSequence.Append(outOfMovesGameObject.transform.DOLocalMove(new Vector3(0, 1000, 0), 0.2f));
        outOfMovesAlertSequence.AppendCallback(() => { Destroy(outOfMovesGameObject); });
        
        timeSlider.PauseTimer(); // just to be sure
        
        yield return new WaitForSeconds(0.8f);

        yield return ExecuteRescue();

        // let it sink in
        timeSlider.PauseTimer(); // just to be extra sure
        yield return new WaitUntil(() => DOTween.TotalPlayingTweens() == 0);

        // lets play
        HoldNewBlocks(false);
        yield return SetAutoMove();
        timeSlider.ResumeTimer();
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
    private IEnumerator ExecuteRescue()
    {
        HoldNewBlocks(true);

        if (GamePlayUI.Instance.currentGameOverReson == GameOverReason.OUT_OF_MOVES)
        {
            var TotalBreakingColumns = 4;
            var TotalBreakingRows = 4;

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

            breakingRows.Concat(breakingColums).SelectMany(b => b).ToList().ForEach(b => b.isFilled = true);
            yield return BreakAllCompletedLines(-1);
        }
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
        InputManager.Instance.DisableTouch();
        AnalyticsEvent.GameOver();
        GameController.IncrementGamesPlayed();

        var gameOverScreen = StackManager.Instance.gameOverScreen;
        gameOverScreen.Activate();
        gameOverScreen.GetComponent<GameOver>().SetLevelScore(ScoreManager.Instance.Score);
        GameProgressManager.Instance.ClearProgress();
        StackManager.Instance.DeactivateGamePlay();
    }

    #region powerup dandelion activation

    private Sequence HandleDandelionPowerup(Block dandelionPowerup)
    {
        var seedBlocks = new List<Block>();

        // give open blocks more priority over empty
        var availableBlocks = Instance.blockGrid.Where(b => !b.isFilled).ToList();
        availableBlocks.AddRange(Instance.blockGrid);

        while (seedBlocks.Count < 5)
        {
            var randomIndex = Random.Range(0, availableBlocks.Count);
            if (!seedBlocks.Contains(availableBlocks[randomIndex]))
                seedBlocks.Add(availableBlocks[randomIndex]);
        }

        var seedSequence = DOTween.Sequence();
        seedBlocks.ForEach(b => { seedSequence.Join(b.ConvertToDandelionSeed(dandelionPowerup)); });

        return seedSequence;
    }

    #endregion

    #region powerup bomb prepping for detonation

    private void PrepDetonatingBombBlockPowerups(IEnumerable<Block> bombBlocks)
    {
        var analyzedBlocks = new HashSet<Block>();
        var bombPowerups = new Stack<Block>(bombBlocks);

        while (bombPowerups.Any())
        {
            var bombPowerup = bombPowerups.Pop();

            foreach (var surroundingBlock in SurroundingBlocks(bombPowerup, true))
            {
                if (analyzedBlocks.Contains(surroundingBlock)) continue;

                analyzedBlocks.Add(surroundingBlock);
                surroundingBlock.ConvertToExplodingBlock();
                if (surroundingBlock.isBombPowerup) bombPowerups.Push(surroundingBlock);
            }
        }
    }

    #endregion

    public IEnumerable<Block> SurroundingBlocks(Block centerBlock, bool includeCenterBlock = false)
    {
        return SurroundingBlocksInRadius(centerBlock, 1, includeCenterBlock);
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

    private bool HoldingNewBlocks()
    {
        return _holdingNewBlocks;
    }

    private class PowerupActivation
    {
        public readonly int MoveID;
        public readonly int PowerupID;

        public PowerupActivation(Block block)
        {
            MoveID = block.moveID;
            PowerupID = block.blockID;
        }

        public override string ToString()
        {
            return base.ToString() + " MoveID=" + MoveID + " PowerupID=" + PowerupID;
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
    public bool IsBasicHelpShown()
    {
        return PlayerPrefs.GetInt("isBasicHelpShown", 0) != 0;
    }

    /// <summary>
    ///     Shows the basic help.
    /// </summary>
    public void ShowBasicHelp()
    {
        var inGameHelp = gameObject.GetComponent<InGameHelp>();
        if (inGameHelp == null) inGameHelp = gameObject.AddComponent<InGameHelp>();
        inGameHelp.ShowBasicHelp();
    }

    /// <summary>
    ///     Stop the basic help.
    /// </summary>
    public void StopBasicHelp()
    {
        var inGameHelp = gameObject.GetComponent<InGameHelp>();
        if (inGameHelp != null)
        {
            Debug.LogWarning("Did not find an InGameHelp game object");
            inGameHelp.StopHelp();
        }

        PowerupController.Instance.LoadSavedPurchasedPowerups();
        PowerupController.Instance.LoadSavedEquippedPowerups();
        BlockShapeSpawner.Instance.FillShapeContainer();
    }

    #endregion
}