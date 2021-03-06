using System;
using UnityEngine;
using UnityEngine.UI;
#if HBDOTween
using DG.Tweening;

#endif

public class Block : MonoBehaviour, IComparable
{
    public GameObject autoMoveHighlight;
    public int blockID = -1;

    //Block image instance.
    /*[HideInInspector] */
    public Image blockImage;

    //Bomb blast counter, will keep reducing with each move.
    public int bombCounter;
    public GameObject bombExplosionPrefab;

    public int colorId = -1;

    //Column Index of block.
    public int columnID;

    // status if block is an avalanche powerup
    public bool isAvalanchePowerup;

    //Determines whether this block is normal or bomb.
    public bool isBomb;

    //Status whether block is a bomb powerup block.
    public bool isBombPowerup;

    // status of whether a block is a color coder powerup    
    public bool isColorCoderPowerup;

    //Status whether block is marked to produce seed blocks when cleared
    public bool isDandelionPowerup;

    // Status whether this block will sprout blocks at end of round
    public bool isDandelionSeed;

    //Status whether block is marked for double points
    public bool isDoublePoints;

    //Status whether block is on the edge of the board
    public bool isEdge;

    // Status of whether this block was in the blast radius of a bomb powerup
    public bool isExploding;

    //Status whether block is empty or filled.
    public bool isFilled;

    // status whether block is a frenzy powerup
    public bool isFrenzyPowerup;

    // status whether block is a lag powerup
    public bool isLagPowerup;

    // status if block is an omnicolor block
    public bool isOmnicolorBlock;

    // status if block is a quake powerup
    public bool isQuakePowerup;

    // when cleared starts a sticks galore powerup
    public bool isSticksGalorePowerup;

    // status whether block is a storm powerup
    public bool isStormPowerup;

    public int moveID;

    // used to store previous block image during game play
    public Sprite prevBlockImageSprite;

    public GameObject colorCoderActivationPrefab;
    public GameObject floodActivationPrefab;
    public GameObject quakeActivationPrefab;
    public GameObject dandelionActivationPrefab;
    public GameObject frenzyParticlesPrefab;
    public GameObject lagParticlesPrefab;
    public GameObject avalancheParticlesPrefab;

    //Row Index of block.
    public int rowID;
    private Text txtCounter;


    public int CompareTo(object obj)
    {
        if (obj == null) return 1;

        var b = obj as Block;

        if (b != null)
            return (rowID + columnID).CompareTo(b.rowID + b.columnID);

        throw new ArgumentException("Object is not a Block");
    }

    /// <summary>
    ///     Raises the enable event.
    /// </summary>
    private void OnEnable()
    {
        //Counter will be used on Blast and challenge mode only.
        if (GameController.gameMode == GameMode.BLAST || GameController.gameMode == GameMode.CHALLENGE)
            txtCounter = transform.GetChild(0).GetChild(0).GetComponent<Text>();
    }

    /// <summary>
    ///     Sets the highlight image.
    /// </summary>
    /// <param name="sprite">Sprite.</param>
    public void SetHighlightImage(ShapeInfo shapeInfo)
    {
        if (blockImage.sprite)
            prevBlockImageSprite = blockImage.sprite;
        blockImage.sprite = shapeInfo.blockImage;
        blockImage.color = new Color(1, 1, 1, 0.5F);
        autoMoveHighlight.SetActive(true);

        var powerupInfo = BlockShapeSpawner.Instance.FindPowerupById(shapeInfo.ShapeID);
        if (powerupInfo != null) Instantiate(powerupInfo.powerupBlockIcon, blockImage.transform, false);
    }

    /// <summary>
    ///     Stops the highlighting.
    /// </summary>
    public void StopHighlighting()
    {
        autoMoveHighlight.SetActive(false);
        if (prevBlockImageSprite && isFilled)
        {
            blockImage.sprite = prevBlockImageSprite;
            prevBlockImageSprite = null;
            blockImage.color = new Color(1, 1, 1, 1);
        }
        else
        {
            blockImage.sprite = null;
            blockImage.color = new Color(1, 1, 1, 0);
        }
    }

    /// <summary>
    ///     Sets the block image.
    /// </summary>
    /// <param name="sprite">Sprite.</param>
    /// <param name="_blockID">Block I.</param>
    /// <param name="moveCount"></param>
    public void SetBlockImage(Sprite sprite, int _blockID, int moveCount)
    {
        blockImage.sprite = sprite;
        blockImage.color = new Color(1, 1, 1, 1);
        colorId = sprite.name.TryParseInt();
        blockID = _blockID;
        moveID = moveCount;
        isFilled = true;
        isExploding = false;
    }

    /// <summary>
    ///     Converts to filled block.
    /// </summary>
    /// <param name="_blockID">Block I.</param>
    public void ConvertToFilledBlock(int _blockID)
    {
        if (blockImage.sprite == null || blockImage.sprite.name == "empty-counter")
            blockImage.sprite = BlockShapeSpawner.Instance.NextColorSprite();
        blockImage.color = new Color(1, 1, 1, 1);
        blockID = _blockID;
        isFilled = true;
        isExploding = false;
    }

    public void ClearExtraChildren()
    {
        // remove extra game objects added to the block
        foreach (Transform t in blockImage.transform)
        {
            // ignore self and legacy text counter
            if (blockImage.transform == t) continue;
            if (t.name == "Text-Counter") continue;
            if (txtCounter != null && t == txtCounter.transform) continue;
            if (t.name.Contains("DandelionSeed")) continue; // only seed block spawn should clear this

            Destroy(t.gameObject);
        }
    }

    /// <summary>
    ///     Clears the block.
    /// </summary>
    /// <param name="animate"></param>
    public Sequence ClearBlock(bool animate)
    {
        var emptyBlockImage = transform.GetComponent<Image>();
        var clearActionCallback = new TweenCallback(() =>
        {
            blockImage.color = new Color(1, 1, 1, 0);
            blockImage.transform.localScale = Vector3.one;
            blockImage.sprite = null;
            ClearExtraChildren();

            RemovePowerup();

            if (isOmnicolorBlock) RemovePowerupIcon();

            // reset all fields
            moveID = -1;
            blockID = -1;
            isFilled = false;
            isBomb = false;
            isExploding = false;
            prevBlockImageSprite = null;

            if (GameController.gameMode == GameMode.BLAST || GameController.gameMode == GameMode.CHALLENGE)
                RemoveCounter();
        });

        if (!animate)
        {
            emptyBlockImage.color = new Color(1, 1, 1, 1f);
            blockImage.color = new Color(1, 1, 1, 0);
            blockImage.transform.localScale = Vector3.one;
            blockImage.sprite = null;
            clearActionCallback.Invoke();
            return null;
        }

        var clearSequence = DOTween.Sequence();

        emptyBlockImage.color = new Color(1, 1, 1, 0);
        clearSequence.Join(blockImage.transform.DOScale(Vector3.zero, 0.4F));
        clearSequence.Join(blockImage.DOFade(0, 0.4F));
        clearSequence.Join(emptyBlockImage.DOFade(1f, 0.4F));

        clearSequence.AppendCallback(clearActionCallback);

        return clearSequence;
    }

    public void Copy(Block b)
    {
        // copy most fields
        moveID = b.moveID;
        blockID = b.blockID;
        isBombPowerup = b.isBombPowerup;
        isBomb = b.isBomb;
        isDandelionSeed = b.isDandelionSeed;
        isDandelionPowerup = b.isDandelionPowerup;
        isDoublePoints = b.isDoublePoints;
        isExploding = b.isExploding;
        isSticksGalorePowerup = b.isSticksGalorePowerup;
        isColorCoderPowerup = b.isColorCoderPowerup;
        isLagPowerup = b.isLagPowerup;
        isStormPowerup = b.isStormPowerup;
        isQuakePowerup = b.isQuakePowerup;
        isAvalanchePowerup = b.isAvalanchePowerup;
        isFrenzyPowerup = b.isFrenzyPowerup;
        isOmnicolorBlock = b.isOmnicolorBlock;

        blockImage.sprite = b.blockImage.sprite;
        blockImage.color = b.blockImage.color;
        foreach (Transform child in b.blockImage.transform)
            if (child.gameObject.activeInHierarchy)
                Instantiate(child, blockImage.transform, false);
    }

    public void ConvertToBomb()
    {
        var powerupInfo = BlockShapeSpawner.Instance.FindPowerupById((int) ShapeInfo.Powerups.Bomb);
        Instantiate(powerupInfo.powerupBlockIcon, blockImage.transform, false);
        isBombPowerup = true;
    }

    public void ConvertToDandelion()
    {
        var powerupInfo = BlockShapeSpawner.Instance.FindPowerupById((int) ShapeInfo.Powerups.Dandelion);
        Instantiate(powerupInfo.powerupBlockIcon, blockImage.transform, false);
        isDandelionPowerup = true;
    }

    public void ConvertToExplosion()
    {
        Instantiate(bombExplosionPrefab, transform);
    }

    public void ConvertToExplodingBlock()
    {
        isExploding = true;
    }

    public Tweener ConvertToSeedSproutBlock()
    {
        ClearExtraChildren();
        ConvertToFilledBlock(0);
        return transform.DOPunchScale(new Vector3(1.05f, 1.05f, 1.05f), 0.8f, 1, 0.1f);
    }

    public void ConvertToColorCoder()
    {
        var powerupInfo = BlockShapeSpawner.Instance.FindPowerupById((int) ShapeInfo.Powerups.ColorCoder);
        Instantiate(powerupInfo.powerupBlockIcon, blockImage.transform, false);
        isColorCoderPowerup = true;
    }

    public void ActivateColorCoder()
    {
        Instantiate(colorCoderActivationPrefab, transform);
    }

    public void ActivateFloodParticles()
    {
        Instantiate(floodActivationPrefab, transform);
    }

    public void ActivateQuakeParticles()
    {
        var width = transform.GetComponent<RectTransform>().sizeDelta.x;
        var leftParticles = Instantiate(quakeActivationPrefab, transform);
        var rightParticles = Instantiate(quakeActivationPrefab, transform);
        leftParticles.transform.localPosition -= leftParticles.transform.localPosition - new Vector3(width * 1.33f, 0, 0);
        rightParticles.transform.localPosition += rightParticles.transform.localPosition - new Vector3(width * 1.33f, 0, 0);
    }

    public void ActivateDandelionParticles()
    {
        Instantiate(dandelionActivationPrefab, transform);
    }

    public void ConvertToSticksGalore()
    {
        var powerupInfo = BlockShapeSpawner.Instance.FindPowerupById((int) ShapeInfo.Powerups.SticksGalore);
        Instantiate(powerupInfo.powerupBlockIcon, blockImage.transform, false);
        isSticksGalorePowerup = true;
    }

    public void ConvertToLagBlock()
    {
        var powerupInfo = BlockShapeSpawner.Instance.FindPowerupById((int) ShapeInfo.Powerups.Lag);
        Instantiate(powerupInfo.powerupBlockIcon, blockImage.transform, false);
        isLagPowerup = true;
    }

    public void ConvertToDoublerBlock()
    {
        isDoublePoints = true;
        var powerupInfo = BlockShapeSpawner.Instance.FindPowerupById((int) ShapeInfo.Powerups.Doubler);
        Instantiate(powerupInfo.powerupBlockIcon, blockImage.transform, false);
    }

    public void ConvertToStormBlock()
    {
        isStormPowerup = true;
        var powerupInfo = BlockShapeSpawner.Instance.FindPowerupById((int) ShapeInfo.Powerups.Storm);
        Instantiate(powerupInfo.powerupBlockIcon, blockImage.transform, false);
    }

    public void ConvertToQuakeBlock()
    {
        isQuakePowerup = true;
        var powerupInfo = BlockShapeSpawner.Instance.FindPowerupById((int) ShapeInfo.Powerups.Quake);
        Instantiate(powerupInfo.powerupBlockIcon, blockImage.transform, false);
    }

    public void ConvertToAvalancheBlock()
    {
        isAvalanchePowerup = true;
        var powerupInfo = BlockShapeSpawner.Instance.FindPowerupById((int) ShapeInfo.Powerups.Avalanche);
        Instantiate(powerupInfo.powerupBlockIcon, blockImage.transform, false);
    }

    public void ConvertToOmnicolorBlock()
    {
        blockID = 0;
        isFilled = true;
        isExploding = false;
        isOmnicolorBlock = true;
        Instantiate(GamePlay.Instance.blockOmnicolorPrefab, blockImage.transform, false);
    }

    public Tweener ConvertToDandelionSeed(Block dandelionPowerup)
    {
        isDandelionSeed = true;
        var newSeedBlockIcon = Instantiate(GamePlay.Instance.blockDandelionSeedPrefab,
            dandelionPowerup.transform.position, Quaternion.identity, blockImage.transform);
        return newSeedBlockIcon.transform.DOMove(blockImage.transform.position, 0.4f);
    }

    public void ConvertToFrenzyBlock()
    {
        isFrenzyPowerup = true;
        var powerupInfo = BlockShapeSpawner.Instance.FindPowerupById((int) ShapeInfo.Powerups.Frenzy);
        Instantiate(powerupInfo.powerupBlockIcon, blockImage.transform, false);
    }

    public void ConvertToFrenziedBlock()
    {
        ConvertToFilledBlock(0);
        ConvertToDoublerBlock();
    }

    public bool IsTouching(Block nextBlock)
    {
        return rowID == nextBlock.rowID &&
               Mathf.Abs(columnID - nextBlock.columnID) <= 1 ||
               columnID == nextBlock.columnID &&
               Mathf.Abs(rowID - nextBlock.rowID) <= 1;
    }

    public void RemovePowerup()
    {
        if (blockID < 1000) return;

        RemovePowerupIcon();

        blockID = 0;
        isBombPowerup = false;
        isDandelionPowerup = false;
        isDoublePoints = false;
        isSticksGalorePowerup = false;
        isColorCoderPowerup = false;
        isLagPowerup = false;
        isStormPowerup = false;
        isQuakePowerup = false;
        isAvalanchePowerup = false;
        isFrenzyPowerup = false;
        isOmnicolorBlock = false;
    }

    public void RemovePowerupIcon()
    {
        foreach (Transform t in blockImage.transform)
        {
            if (t == blockImage.transform) continue;
            if (t.name.Contains("DandelionSeed")) continue; // only seed sprout should clear this
            Destroy(t.gameObject);
        }
    }

    public void ClearDandelionSeedIcon()
    {
        isDandelionSeed = false;
        foreach (Transform t in blockImage.transform)
            if (t != blockImage.transform && t.name.Contains("DandelionSeed"))
                Destroy(t.gameObject);
    }

    #region bomb mode specific

    /// <summary>
    ///     Converts to bomb.
    /// </summary>
    /// <param name="counterValue">Counter value.</param>
    public void ConvertToBomb(int counterValue = 9)
    {
        blockImage.sprite = GamePlay.Instance.BombSprite;
        blockImage.color = new Color(1, 1, 1, 1);
        isFilled = true;
        isBomb = true;
        SetCounter(counterValue);
    }

    /// <summary>
    ///     Sets the counter.
    /// </summary>
    /// <param name="counterValue">Counter value.</param>
    public void SetCounter(int counterValue = 9)
    {
        txtCounter.gameObject.SetActive(true);
        txtCounter.text = counterValue.ToString();
        bombCounter = counterValue;
    }

    /// <summary>
    ///     Decreases the counter.
    /// </summary>
    public void DecreaseCounter()
    {
        bombCounter -= 1;
        txtCounter.text = bombCounter.ToString();

        if (bombCounter == 0) GamePlay.Instance.OnBombCounterOver();
    }

    /// <summary>
    ///     Removes the counter.
    /// </summary>
    private void RemoveCounter()
    {
        txtCounter.text = "";
        txtCounter.gameObject.SetActive(false);
        bombCounter = 0;
        isBomb = false;
    }

    #endregion

    public void ActivateFrenzyParticles()
    {
        Instantiate(frenzyParticlesPrefab, transform);
    }

    public void ActivateLagParticles()
    {
        Instantiate(lagParticlesPrefab, transform);
    }
    
    public void ActivateAvalancheParticles()
    {
        var halfCanvasHeight = transform.parent.GetComponent<RectTransform>().sizeDelta.y;
        var avalancheParticles = Instantiate(avalancheParticlesPrefab, GamePlay.Instance.transform);
        avalancheParticles.transform.localPosition = new Vector3(0, halfCanvasHeight * 1.2f, 0);
    }
}