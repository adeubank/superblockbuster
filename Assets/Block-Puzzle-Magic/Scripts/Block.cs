using System;
using UnityEngine;
using UnityEngine.UI;
#if HBDOTween
using DG.Tweening;

#endif

public class Block : MonoBehaviour, IComparable
{
    public int blockID = -1;

    //Block image instance.
    /*[HideInInspector] */
    public Image blockImage;

    //Bomb blast counter, will keep reducing with each move.
    [HideInInspector] public int bombCounter;

    [HideInInspector] public int colorId = -1;

    //Column Index of block.
    public int columnID;

    // status if block is an avalanche powerup
    [HideInInspector] public bool isAvalanchePowerup;

    //Determines whether this block is normal or bomb.
    [HideInInspector] public bool isBomb;

    //Status whether block is a bomb powerup block.
    [HideInInspector] public bool isBombPowerup;

    // status of whether a block is a color coder powerup    
    [HideInInspector] public bool isColorCoderPowerup;

    //Status whether block is marked to produce seed blocks when cleared
    [HideInInspector] public bool isDandelionPowerup;

    // Status whether this block will sprout blocks at end of round
    [HideInInspector] public bool isDandelionSeed;

    //Status whether block is marked for double points
    [HideInInspector] public bool isDoublePoints;

    //Status whether block is on the edge of the board
    [HideInInspector] public bool isEdge;

    // Status of whether this block was in the blast radius of a bomb powerup
    [HideInInspector] public bool isExploding;

    //Status whether block is empty or filled.
    public bool isFilled;

    // status whether block is a lag powerup
    [HideInInspector] public bool isLagPowerup;

    // status if block is an omnicolor block
    [HideInInspector] public bool isOmnicolorBlock;

    // status if block is a quake powerup
    [HideInInspector] public bool isQuakePowerup;

    // when cleared starts a sticks galore powerup
    [HideInInspector] public bool isSticksGalorePowerup;

    // status whether block is a storm powerup
    [HideInInspector] public bool isStormPowerup;
    
    // status whether block is a frenzy powerup
    [HideInInspector] public bool isFrenzyPowerup;

    public Sprite prevBlockImageSprite;

    //Row Index of block.
    public int rowID;
    private Text txtCounter;
    
    [HideInInspector] public int moveID;

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
    public void SetHighlightImage(Sprite sprite)
    {
        if (blockImage.sprite)
            prevBlockImageSprite = blockImage.sprite;
        blockImage.sprite = sprite;
        blockImage.color = new Color(1, 1, 1, 0.5F);
    }

    /// <summary>
    ///     Stops the highlighting.
    /// </summary>
    public void StopHighlighting()
    {
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
        blockID = _blockID;
        colorId = sprite.name.TryParseInt();
        moveID = moveCount;
        isFilled = true;
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
    }

    public void ClearExtraChildren()
    {
        // remove extra game objects added to the block
        foreach (Transform t in blockImage.transform)
        {
            // ignore self and legacy text counter
            if (blockImage.transform == t || t.name == "Text-Counter" ||
                txtCounter != null && t == txtCounter.transform)
                continue;

            Destroy(t.gameObject);
        }
    }

    /// <summary>
    ///     Clears the block.
    /// </summary>
    /// <param name="animate"></param>
    public void ClearBlock(bool animate)
    {
        ClearExtraChildren();

#if HBDOTween
        if (animate)
        {
            transform.GetComponent<Image>().color = new Color(1, 1, 1, 0);

            blockImage.transform.DOScale(Vector3.zero, 0.35F).OnComplete(() =>
            {
                blockImage.transform.localScale = Vector3.one;
                blockImage.sprite = null;
            });

            transform.GetComponent<Image>().DOFade(0.65f, 0.35F).SetDelay(0.3F);
            blockImage.DOFade(0, 0.3F);
        }
        else
        {
            transform.GetComponent<Image>().color = new Color(1, 1, 1, 0.65f);
            blockImage.color = new Color(1, 1, 1, 0);
            blockImage.transform.localScale = Vector3.one;
            blockImage.sprite = null;
        }
#endif

        // reset all fields
        blockID = -1;
        isFilled = false;
        isBombPowerup = false;
        isBomb = false;
        isDandelionSeed = false;
        isDandelionPowerup = false;
        isDoublePoints = false;
        isExploding = false;
        isSticksGalorePowerup = false;
        isColorCoderPowerup = false;
        prevBlockImageSprite = null;
        isLagPowerup = false;
        isStormPowerup = false;
        isQuakePowerup = false;
        isAvalanchePowerup = false;
        isFrenzyPowerup = false;

        if (GameController.gameMode == GameMode.BLAST || GameController.gameMode == GameMode.CHALLENGE) RemoveCounter();
    }

    public void Copy(Block b)
    {
        // copy most fields
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
        blockImage.sprite = b.blockImage.sprite;
        blockImage.color = b.blockImage.color;
        isAvalanchePowerup = b.isAvalanchePowerup;
        foreach (Transform child in b.blockImage.transform)
            if (child.gameObject.activeInHierarchy)
                Instantiate(child, blockImage.transform, false);
    }

    public void ConvertToBomb()
    {
        var powerupInfo = BlockShapeSpawner.Instance.FindPowerupById((int) PowerupInfo.Powerups.Bomb);
        Instantiate(powerupInfo.powerupBlockIcon, blockImage.transform, false);
        isBombPowerup = true;
    }

    public void ConvertToDandelion()
    {
        var powerupInfo = BlockShapeSpawner.Instance.FindPowerupById((int) PowerupInfo.Powerups.Dandelion);
        Instantiate(powerupInfo.powerupBlockIcon, blockImage.transform, false);
        isDandelionPowerup = true;
    }

    public void ConvertToExplosion()
    {
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.GetComponent<Renderer>().material.color = Color.red;
        sphere.transform.position = transform.position;
        sphere.transform.localScale = Vector3.zero;
        sphere.transform.DOScale(Vector3.one * 1.25f, 0.4f).OnComplete(() => { Destroy(sphere); });
    }

    public void ConvertToExplodingBlock()
    {
        isFilled = true;
        isExploding = true;
    }

    public Tweener ConvertToSeedSproutBlock()
    {
        ClearExtraChildren();
        ConvertToFilledBlock(0);
        return transform.DOPunchScale(new Vector3(1.05f, 1.05f, 1.05f), 1f, 1, 0.1f);
    }

    public void ConvertToColorCoder()
    {
        var powerupInfo = BlockShapeSpawner.Instance.FindPowerupById((int) PowerupInfo.Powerups.ColorCoder);
        Instantiate(powerupInfo.powerupBlockIcon, blockImage.transform, false);
        isColorCoderPowerup = true;
    }

    public void ConvertToSticksGalore()
    {
        var powerupInfo = BlockShapeSpawner.Instance.FindPowerupById((int) PowerupInfo.Powerups.SticksGalore);
        Instantiate(powerupInfo.powerupBlockIcon, blockImage.transform, false);
        isSticksGalorePowerup = true;
    }

    public void ConvertToLagBlock()
    {
        var powerupInfo = BlockShapeSpawner.Instance.FindPowerupById((int) PowerupInfo.Powerups.Lag);
        Instantiate(powerupInfo.powerupBlockIcon, blockImage.transform, false);
        isLagPowerup = true;
    }

    public void ConvertToDoublerBlock()
    {
        isDoublePoints = true;
        var powerupInfo = BlockShapeSpawner.Instance.FindPowerupById((int) PowerupInfo.Powerups.Doubler);
        Instantiate(powerupInfo.powerupBlockIcon, blockImage.transform, false);
    }

    public void ConvertToStormBlock()
    {
        isStormPowerup = true;
        var powerupInfo = BlockShapeSpawner.Instance.FindPowerupById((int) PowerupInfo.Powerups.Storm);
        Instantiate(powerupInfo.powerupBlockIcon, blockImage.transform, false);
    }

    public void ConvertToQuakeBlock()
    {
        isQuakePowerup = true;
        var powerupInfo = BlockShapeSpawner.Instance.FindPowerupById((int) PowerupInfo.Powerups.Quake);
        Instantiate(powerupInfo.powerupBlockIcon, blockImage.transform, false);
    }

    public void ConvertToAvalancheBlock()
    {
        isAvalanchePowerup = true;
        var powerupInfo = BlockShapeSpawner.Instance.FindPowerupById((int) PowerupInfo.Powerups.Avalanche);
        Instantiate(powerupInfo.powerupBlockIcon, blockImage.transform, false);
    }

    public void ConvertToOmnicolorBlock()
    {
        isFilled = true;
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

    public int CompareTo(object obj)
    {
        if (obj == null) return 1;

        Block b = obj as Block;

        if (b != null)
            return (rowID + columnID).CompareTo(b.rowID + b.columnID);

        throw new ArgumentException("Object is not a Block");
    }

    public void ConvertToFrenzyBlock()
    {
        isFrenzyPowerup = true;
        var powerupInfo = BlockShapeSpawner.Instance.FindPowerupById((int) PowerupInfo.Powerups.Frenzy);
        Instantiate(powerupInfo.powerupBlockIcon, blockImage.transform, false);
    }

    public void convertToFrenziedBlock()
    {
        ConvertToFilledBlock(0);
        ConvertToDoublerBlock();
    }

    public bool IsTouching(Block nextBlock)
    {
        return  rowID == nextBlock.rowID &&
                Mathf.Abs(columnID - nextBlock.columnID) <= 1 ||
                columnID == nextBlock.columnID &&
                Mathf.Abs(rowID - nextBlock.rowID) <= 1;
    }
}