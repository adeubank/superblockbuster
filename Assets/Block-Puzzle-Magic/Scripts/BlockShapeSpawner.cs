using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
#if HBDOTween
using DG.Tweening;

#endif


public class BlockShapeSpawner : Singleton<BlockShapeSpawner>
{
    private readonly int shapeBlockPoolCount = 1;
    [HideInInspector] public ShapeBlockList ActiveShapeBlockModule;

    [Tooltip(
        "Setting this true means placing a block will add new block instantly, false means new shape blocks will be added only once all three are placed on the board.")]
    public bool keepFilledAlways;

    [SerializeField] private ShapeBlockList shapeBlockList;

    [SerializeField] private ShapeBlockList shapeBlockList_Plus;

    private List<int> shapeBlockProbabilityPool;

    [SerializeField] private Sprite[] shapeColors;

    [SerializeField] private Transform[] ShapeContainers;

    /// <summary>
    ///     Awake this instance.
    /// </summary>
    private void Awake()
    {
        if (GameController.gameMode == GameMode.ADVANCE || GameController.gameMode == GameMode.CHALLENGE)
            ActiveShapeBlockModule = shapeBlockList_Plus;
        else
            ActiveShapeBlockModule = shapeBlockList;
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

        Invoke("SetupPreviousSessionShapes", 0.2F);
        Invoke("createShapeBlockProbabilityList", 0.5F);
        Invoke("FillShapeContainer", 0.5F);
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
        shapeBlockProbabilityPool = new List<int>();
        if (ActiveShapeBlockModule != null)
            foreach (var shapeBlock in ActiveShapeBlockModule.ShapeBlocks)
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
        var shapesFilled = false;

        ReorderShapes();

        if (!keepFilledAlways)
        {
            var emptyEnough = true;
            foreach (var shapeContainer in GetActiveShapeContainers())
                if (shapeContainer.childCount > 0)
                    emptyEnough = false;

            if (emptyEnough)
            {
                shapesFilled = true;
                foreach (var shapeContainer in GetActiveShapeContainers())
                    AddRandomShapeToContainer(shapeContainer);
            }
        }
        else
        {
            foreach (var shapeContainer in GetActiveShapeContainers())
                if (shapeContainer.childCount <= 0)
                {
                    shapesFilled = true;
                    AddRandomShapeToContainer(shapeContainer);
                }
        }

        Invoke("CheckOnBoardShapeStatus", 0.2F);

        return shapesFilled;
    }

    /// <summary>
    ///     Adds the random shape to container.
    /// </summary>
    /// <param name="shapeContainer">Shape container.</param>
    public void AddRandomShapeToContainer(Transform shapeContainer)
    {
        if (shapeBlockProbabilityPool == null || shapeBlockProbabilityPool.Count <= 0)
            createShapeBlockProbabilityList();

        var RandomShape = shapeBlockProbabilityPool[0];
        shapeBlockProbabilityPool.RemoveAt(0);

        var newShapeBlock = ActiveShapeBlockModule.ShapeBlocks.Find(o => o.BlockID == RandomShape).shapeBlock;
        var spawningShapeBlock = Instantiate(newShapeBlock);
        spawningShapeBlock.transform.SetParent(shapeContainer);
        spawningShapeBlock.transform.localScale = Vector3.one * 0.6F;
        spawningShapeBlock.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(800F, 0, 0);

        var randomColor = Random.Range(0, shapeColors.Length);
        var blockImages = spawningShapeBlock.GetComponentsInChildren<Image>();
        foreach (var blockImage in blockImages) blockImage.sprite = shapeColors[randomColor];


#if HBDOTween
        spawningShapeBlock.transform.DOLocalMove(Vector3.zero, 0.3F);
#endif
    }

    /// <summary>
    ///     Creates the shape with I.
    /// </summary>
    /// <param name="shapeContainer">Shape container.</param>
    /// <param name="shapeID">Shape I.</param>
    private void CreateShapeWithID(Transform shapeContainer, int shapeID)
    {
        var newShapeBlock = ActiveShapeBlockModule.ShapeBlocks.Find(o => o.BlockID == shapeID).shapeBlock;
        var spawningShapeBlock = Instantiate(newShapeBlock);
        spawningShapeBlock.transform.SetParent(shapeContainer);
        spawningShapeBlock.transform.localScale = Vector3.one * 0.6F;
        spawningShapeBlock.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(800F, 0, 0);
#if HBDOTween
        spawningShapeBlock.transform.DOLocalMove(Vector3.zero, 0.3F);
#endif
    }

    /// <summary>
    ///     Checks the on board shape status.
    /// </summary>
    public void CheckOnBoardShapeStatus()
    {
        var OnBoardBlockShapes = new List<ShapeInfo>();
        foreach (var shapeContainer in GetActiveShapeContainers())
            if (shapeContainer.childCount > 0)
                OnBoardBlockShapes.Add(shapeContainer.GetChild(0).GetComponent<ShapeInfo>());

        var canExistingBlocksPlaced = GamePlay.Instance.CanExistingBlocksPlaced(OnBoardBlockShapes);

        if (canExistingBlocksPlaced == false) GamePlay.Instance.OnUnableToPlaceShape();
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
        var gridLayout = gameObject.GetComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(120, 120);
        var inactivePanels = new List<GameObject>();

        foreach (Transform child in transform)
            if (!child.gameObject.activeInHierarchy)
                inactivePanels.Add(child.gameObject);

        foreach (var panel in inactivePanels) panel.Activate();

        // fill up the new containers
        var originalKeepFilledUp = keepFilledAlways;
        keepFilledAlways = true;
        FillShapeContainer();
        keepFilledAlways = originalKeepFilledUp;
    }

    public List<Transform> GetActiveShapeContainers()
    {
        return ShapeContainers.ToList().Where(o => o.gameObject.activeInHierarchy).ToList();
    }
}