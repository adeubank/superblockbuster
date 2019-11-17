using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
#if HBDOTween
using DG.Tweening;

#endif


public class BlockShapeSpawner : Singleton<BlockShapeSpawner>
{
    private readonly int shapeBlockPoolCount = 1;

    [HideInInspector] public List<ShapeBlockSpawn> ActiveShapeBlocks;

    // first step in tutorial
    public int firstHelpShapeId = 8;

    [HideInInspector] public bool isNextRoundSticksGaloreBlocks;

    [Tooltip(
        "Setting this true means placing a block will add new block instantly, false means new shape blocks will be added only once all three are placed on the board.")]
    public bool keepFilledAlways;

    [SerializeField] private PowerupList powerupList;

    // second step in tutorial
    public int secondHelpShapeId = 14;

    [SerializeField] private ShapeBlockList shapeBlockList;

    [SerializeField] private ShapeBlockList shapeBlockList_Plus;

    private List<int> shapeBlockProbabilityPool;

    [SerializeField] private Sprite[] shapeColors;

    [SerializeField] private Transform[] ShapeContainers;
    [HideInInspector] public int sticksGaloreColorId = -1;

    // third step in tutorial
    public int thirdHelpPowerupId = (int) ShapeInfo.Powerups.Doubler;

    // third step in tutorial
    public int thirdHelpShapeId = 4;

    /// <summary>
    ///     Awake this instance.
    /// </summary>
    private void Awake()
    {
        SetActiveShapeBlocks();
    }

    private void SetActiveShapeBlocks()
    {
        if (shapeBlockList != null)
        {
            ActiveShapeBlocks = new List<ShapeBlockSpawn>(shapeBlockList.ShapeBlocks.Count + PowerupController.Instance.equippedPowerupIds.Count);
            ActiveShapeBlocks.AddRange(shapeBlockList.ShapeBlocks);
            ActiveShapeBlocks.AddRange(powerupList.powerupBlockSpawns.FindAll(p => PowerupController.Instance.equippedPowerupIds.Contains(p.BlockID)));    
        }
    }

    /// <summary>
    ///     Start this instance.
    /// </summary>
    private void Start()
    {
        #region blast mode

        if (GameController.gameMode == GameMode.BLAST || GameController.gameMode == GameMode.CHALLENGE)
            keepFilledAlways = true;

        #endregion

        SetBlockShapeToSix();
//        Invoke(nameof(SetupPreviousSessionShapes), 0.2F);
        Invoke(nameof(createShapeBlockProbabilityList), 0.5F);
    }

    /// <summary>
    ///     Setups the previous session shapes.
    /// </summary>
    private void SetupPreviousSessionShapes()
    {
        if (GameBoardGenerator.Instance.previousSessionData != null)
        {
            var shapes = GameBoardGenerator.Instance.previousSessionData.shapeInfo.Split(',').Select(int.Parse)
                .ToList();

            var shapeIndex = 0;
            foreach (var shapeID in shapes)
            {
                if (shapeID >= 0) CreateShapeWithID(ShapeContainers[shapeIndex], shapeID);
                shapeIndex += 1;
            }
        }
    }

    /// <summary>
    ///     Creates the shape block probability list.
    /// </summary>
    private void createShapeBlockProbabilityList()
    {
        Debug.Log("Creating shape block probability list");
        shapeBlockProbabilityPool = new List<int>();
        foreach (var shapeBlock in ActiveShapeBlocks)
            AddShapeInProbabilityPool(shapeBlock.BlockID, shapeBlock.spawnProbability);
        shapeBlockProbabilityPool.Shuffle();
    }

    /// <summary>
    ///     Adds the shape in probability pool.
    /// </summary>
    /// <param name="blockID">Block I.</param>
    /// <param name="probability">Probability.</param>
    private void AddShapeInProbabilityPool(int blockID, int probability)
    {
        var probabiltyTimesToAdd = shapeBlockPoolCount * probability;

        for (var index = 0; index < probabiltyTimesToAdd; index++) shapeBlockProbabilityPool.Add(blockID);
    }

    /// <summary>
    ///     Fills the shape container and returns true if shapes were placed.
    /// </summary>
    public bool FillShapeContainer()
    {
        if (GamePlay.Instance.isHelpOnScreen) return false;

        var shapesFilled = false;

        ReorderShapes();

        var activeShapeContainers = GetActiveShapeContainers();
        var playableShapes = GetPlayableShapes();

        if (!keepFilledAlways)
        {
            var emptyEnough = true;
            foreach (var shapeContainer in activeShapeContainers)
                if (shapeContainer.childCount > 0)
                    emptyEnough = false;

            if (activeShapeContainers.Count > 3 && activeShapeContainers.Count(shape => shape.childCount > 0) < 3)
                emptyEnough = true;

            if (emptyEnough)
                foreach (var shapeContainer in activeShapeContainers)
                    if (shapeContainer.childCount <= 0)
                    {
                        shapesFilled = true;
                        playableShapes.Add(AddRandomShapeToContainer(shapeContainer));
                    }
        }
        else
        {
            foreach (var shapeContainer in activeShapeContainers)
                if (shapeContainer.childCount <= 0)
                {
                    shapesFilled = true;
                    playableShapes.Add(AddRandomShapeToContainer(shapeContainer));
                }
        }

        GamePlay.Instance.CheckIfOutOfMoves();
        StartCoroutine(GamePlay.Instance.SetAutoMove());

        return shapesFilled;
    }

    public void FillShapesForFirstStepHelp()
    {
        var activeShapeContainers = GetActiveShapeContainers();

        // first shape info, draggable help
        var firstShapePrefab = ActiveShapeBlocks.First(b => b.BlockID == firstHelpShapeId).shapeBlock;
        var firstShapeBlock = Instantiate(firstShapePrefab, activeShapeContainers[0], true);
        PrepShapeForPlay(firstShapeBlock);
    }

    public void FillShapesForSecondStepHelp()
    {
        var activeShapeContainers = GetActiveShapeContainers();

        // 2nds shape info, tappable help
        var secondShapePrefab = ActiveShapeBlocks.First(b => b.BlockID == secondHelpShapeId).shapeBlock;
        var secondShapeBlock = Instantiate(secondShapePrefab, activeShapeContainers[0], true);
        PrepShapeForPlay(secondShapeBlock);
    }

    public void FillShapesForThirdStepHelp()
    {
        var activeShapeContainers = GetActiveShapeContainers();
        // first powerup info
        var powerupShapePrefab = ActiveShapeBlocks.First(b => b.BlockID == thirdHelpShapeId).shapeBlock;
        var powerupShapeBlock = Instantiate(powerupShapePrefab, activeShapeContainers[0], true);
        var powerupShapeInfo = powerupShapeBlock.GetComponent<ShapeInfo>();
        powerupShapeInfo.ShapeID = thirdHelpPowerupId;
        PrepShapeForPlay(powerupShapeBlock);
        // so that it spawns for first game
        PowerupController.Instance.AddEquippedPowerupId(thirdHelpPowerupId);
        SetActiveShapeBlocks();
    }

    public List<ShapeInfo> GetPlayableShapes()
    {
        var activeShapeContainers = GetActiveShapeContainers();
        return activeShapeContainers.FindAll(t => t.childCount > 0)
            .Select(t => t.GetChild(0).GetComponent<ShapeInfo>()).ToList();
    }

    /// <summary>
    ///     Adds the random shape to container.
    /// </summary>
    /// <param name="shapeContainer">Shape container.</param>
    public ShapeInfo AddRandomShapeToContainer(Transform shapeContainer)
    {
        var spawningShapeBlock = NextShapeBlock(shapeContainer);
        PrepShapeForPlay(spawningShapeBlock);
        return spawningShapeBlock.GetComponent<ShapeInfo>();
    }

    public void PrepShapeForPlay(GameObject shapeBlock)
    {
        var spawningShapeInfo = shapeBlock.GetComponent<ShapeInfo>();
        var spawningPowerupInfo = FindPowerupById(spawningShapeInfo.ShapeID);
        var spawningRectTransform = shapeBlock.GetComponent<RectTransform>();
        shapeBlock.transform.localScale = ShapeContainerLocalScale();
        spawningRectTransform.anchoredPosition3D = new Vector3(800F, 0, 0);
        spawningRectTransform.sizeDelta = ShapeSizeDelta();
        spawningShapeInfo.CreateBlockList();

        if (spawningShapeInfo.IsPowerup() && spawningPowerupInfo != null)
            spawningShapeInfo.ConvertToPowerup(spawningPowerupInfo);

        var colorSprite = NextColorSprite();
        var blockImages = shapeBlock.GetComponentsInChildren<Image>().Where(img => img.sprite != null);
        foreach (var blockImage in blockImages)
        {
            // just make sure that the Image parses to Int correctly so we only set block colors
            var currentColorId = blockImage.sprite.name.TryParseInt(-1);
            if (currentColorId > 0)
                blockImage.sprite = colorSprite;
        }

#if HBDOTween
        shapeBlock.transform.DOLocalMove(Vector3.zero, 0.3F);
#endif
    }

    public GameObject NextShapeBlock(Transform shapeContainer)
    {
        // only sticks on sticks galore
        if (isNextRoundSticksGaloreBlocks)
        {
            var normalBlocks = SticksGaloreBlocks().ToArray();
            var normalBlock = normalBlocks[Random.Range(0, normalBlocks.Length)].shapeBlock;
            return Instantiate(normalBlock, shapeContainer, true);
        }

        if (shapeBlockProbabilityPool == null || shapeBlockProbabilityPool.Count <= 0)
            createShapeBlockProbabilityList();

        var randomShape = shapeBlockProbabilityPool[0];
        shapeBlockProbabilityPool.RemoveAt(0);
        var nextShapeBlock = ActiveShapeBlocks.First(b => b.BlockID == randomShape).shapeBlock;
        var nextShapeInfo = nextShapeBlock.GetComponent<ShapeInfo>();

        // return a normal shape with a powerup ID
        if (Enum.IsDefined(typeof(ShapeInfo.Powerups), randomShape))
        {
            var nextNormalShape = Instantiate(NextNormalShape(), shapeContainer, true);
            nextNormalShape.GetComponent<ShapeInfo>().ShapeID = nextShapeInfo.ShapeID;
            return nextNormalShape;
        }

        return Instantiate(nextShapeBlock, shapeContainer, true);
        ;
    }

    public Sprite NextColorSprite()
    {
        if (isNextRoundSticksGaloreBlocks && sticksGaloreColorId > 0)
            return shapeColors.First(sprite => sprite.name.TryParseInt(-1) == sticksGaloreColorId);

        // just return a random color
        return shapeColors[Random.Range(0, shapeColors.Length)];
    }

    public IEnumerable<ShapeBlockSpawn> NormalShapes()
    {
        return ActiveShapeBlocks.Where(s => s.BlockID < 100);
    }

    public IEnumerable<ShapeBlockSpawn> SticksGaloreBlocks()
    {
        return ActiveShapeBlocks.Where(s => s.BlockID == 10 || s.BlockID == 11);
    }

    private GameObject NextNormalShape()
    {
        var normalBlocks = NormalShapes().Where(s => s.BlockID > 1).ToArray();
        return normalBlocks[Random.Range(0, normalBlocks.Length)].shapeBlock;
    }

    /// <summary>
    ///     Creates the shape with I.
    /// </summary>
    /// <param name="shapeContainer">Shape container.</param>
    /// <param name="shapeID">Shape I.</param>
    private void CreateShapeWithID(Transform shapeContainer, int shapeID)
    {
        var newShapeBlock = ActiveShapeBlocks.Find(o => o.BlockID == shapeID).shapeBlock;
        var spawningShapeBlock = Instantiate(newShapeBlock);
        var spawningRectTransform = spawningShapeBlock.GetComponent<RectTransform>();
        spawningShapeBlock.transform.SetParent(shapeContainer);
        spawningShapeBlock.transform.localScale = ShapeContainerLocalScale();
        spawningRectTransform.anchoredPosition3D = new Vector3(800F, 0, 0);
        spawningRectTransform.sizeDelta = ShapeSizeDelta();
#if HBDOTween
        spawningShapeBlock.transform.DOLocalMove(Vector3.zero, 0.3F);
#endif
    }

    private Vector2 ShapeSizeDelta()
    {
        return new Vector2(333f, 333f);
    }

    public Vector3 ShapeContainerLocalScale()
    {
        return Vector3.one * 0.6f;
    }

    public Vector3 ShapePickupLocalScale()
    {
        return Vector3.one * 1.2f;
    }

    /// <summary>
    ///     Reorders the shapes.
    /// </summary>
    private void ReorderShapes()
    {
        var EmptyShapes = new List<Transform>();

        foreach (var shapeContainer in GetActiveShapeContainers())
            if (shapeContainer.childCount == 0)
            {
                EmptyShapes.Add(shapeContainer);
            }
            else
            {
                if (EmptyShapes.Count > 0)
                {
                    var emptyContainer = EmptyShapes[0];
                    shapeContainer.GetChild(0).SetParent(emptyContainer);
                    EmptyShapes.RemoveAt(0);
#if HBDOTween
                    emptyContainer.GetChild(0).DOLocalMove(Vector3.zero, 0.3F);
#endif
                    EmptyShapes.Add(shapeContainer);
                }
            }
    }

    /// <summary>
    ///     Gets all on board shape names.
    /// </summary>
    /// <returns>The all on board shape names.</returns>
    public string GetAllOnBoardShapeNames()
    {
        var shapeNames = "";
        foreach (var shapeContainer in GetActiveShapeContainers())
            if (shapeContainer.childCount > 0)
                shapeNames = shapeNames + shapeContainer.GetChild(0).GetComponent<ShapeInfo>().ShapeID + ",";
            else
                shapeNames = shapeNames + "-1,";

        shapeNames = shapeNames.Remove(shapeNames.Length - 1);
        return shapeNames;
    }

    public void SetBlockShapeToSix()
    {
        var inactivePanels = new List<GameObject>();

        foreach (Transform child in transform)
            if (!child.gameObject.activeInHierarchy)
                inactivePanels.Add(child.gameObject);

        foreach (var panel in inactivePanels) panel.Activate();
    }

    public List<Transform> GetActiveShapeContainers()
    {
        return ShapeContainers.ToList().Where(o => o.gameObject.activeInHierarchy).ToList();
    }

    public void DeactivateSticksGalore()
    {
        isNextRoundSticksGaloreBlocks = false;
        sticksGaloreColorId = -1;
    }

    public PowerupBlockSpawn FindPowerupById(int id)
    {
        return powerupList.powerupBlockSpawns.FirstOrDefault(powerup => powerup.BlockID == id);
    }
}