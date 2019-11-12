using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if HBDOTween
using DG.Tweening;

#endif


/// <summary>
///     Help classic.
/// </summary>
public class HelpClassic : Singleton<HelpClassic>
{
    private readonly List<Canvas> _highlightedBlocks = new List<Canvas>();
    private Sequence dragHandHelpSequence;
    private Vector2 firstPosition = Vector2.zero;
    private Canvas firstShapeCanvas;
    [SerializeField] private Transform handImage;
    private GameObject powerupHelp;
    private Sequence tapHandHelpSequence;

    [SerializeField] private Transform tapHandImage;

    /// <summary>
    ///     Start this instance.
    /// </summary>
    private void Start()
    {
        StartHelp();
    }

    /// <summary>
    ///     Starts the help.
    /// </summary>
    private void StartHelp()
    {
        ShowDraggableHelp();
    }

    private void ShowDraggableHelp()
    {
        InputManager.Instance.DisableTouch();
        var firstShape = BlockShapeSpawner.Instance.transform.GetChild(0).gameObject;

        if (firstShape.transform.childCount > 0)
        {
            firstShapeCanvas = firstShape.transform.GetChild(0).GetComponent<Canvas>();
            firstShapeCanvas.sortingOrder = 3;
        }

        transform.GetComponent<CanvasGroup>().DOFade(1F, 0.5F).OnComplete(() =>
        {
            InputManager.Instance.EnableTouch();
            firstPosition = firstShape.transform.position;
            firstPosition -= new Vector2(-0.2F, 0.4F);
            handImage.gameObject.SetActive(true);
            handImage.transform.position = firstPosition;
            dragHandHelpSequence = DOTween.Sequence();
            dragHandHelpSequence.Append(handImage.transform.DOMove(GamePlay.Instance.transform.Find("Game-Content").position, 1F).SetDelay(1));
            dragHandHelpSequence.Append(handImage.transform.DOMove(firstPosition, 0.5F).SetDelay(1));
            dragHandHelpSequence.SetLoops(-1, LoopType.Restart);
            StartCoroutine(CheckForDragComplete());
        });
    }

    private void ResetHighlightedBlocks()
    {
        _highlightedBlocks.RemoveAll(go =>
        {
            var c = go.GetComponent<Canvas>();
            c.overrideSorting = false;
            c.sortingOrder = 0;
            return true;
        });
    }

    private IEnumerator CheckForDragComplete()
    {
        yield return new WaitUntil(ArePlayableShapesEmpty);
        ResetHighlightedBlocks();
        dragHandHelpSequence?.Kill();
        handImage.gameObject.Deactivate();
        StartCoroutine(ShowTappableHelp());
    }

    private void TrackHighlightedBlocks()
    {
        GamePlay.Instance.highlightingBlocks.ForEach(b =>
        {
            var c = b.gameObject.GetComponent<Canvas>();
            c.overrideSorting = true;
            c.sortingOrder = 3;
            _highlightedBlocks.Add(c);
        });
    }

    private IEnumerator ShowTappableHelp()
    {
        BlockShapeSpawner.Instance.FillShapesForSecondStepHelp();
        yield return new WaitUntil(() => !ArePlayableShapesEmpty());
        yield return GamePlay.Instance.SetAutoMove();
        ShowTapHandOverHighlightedBlocks();
        StartCoroutine(CheckForTapHelpComplete());
    }

    private IEnumerator CheckForTapHelpComplete()
    {
        yield return new WaitUntil(ArePlayableShapesEmpty);
        ResetHighlightedBlocks();
        tapHandHelpSequence?.Kill();
        tapHandImage.gameObject.Deactivate();
        StartCoroutine(ShowPowerupHelp());
    }

    public IEnumerator ShowPowerupHelp()
    {
        BlockShapeSpawner.Instance.FillShapesForThirdStepHelp();
        yield return new WaitUntil(() => !ArePlayableShapesEmpty());
        yield return GamePlay.Instance.SetAutoMove();
        ShowTapHandOverHighlightedBlocks();
        powerupHelp = StackManager.Instance.SpawnUIScreen("Help-Powerups");
        StartCoroutine(CheckForPowerupHelpPopupComplete());
    }

    private void ShowTapHandOverHighlightedBlocks()
    {
        TrackHighlightedBlocks();
        tapHandImage.gameObject.Activate();
        var middlePos = GamePlay.Instance.highlightingBlocks.Aggregate(Vector3.zero, (avgPos, b) => avgPos + b.gameObject.transform.position) / GamePlay.Instance.highlightingBlocks.Count;
        tapHandImage.position = (Vector2) middlePos - new Vector2(0, 0.6f);

        var secondHandTransform = tapHandImage.transform;
        var secondHandPosition = (Vector2) secondHandTransform.position;
        var secondFirstPos = secondHandPosition - new Vector2(0, 0.2f);
        var secondLastPos = secondHandPosition + new Vector2(0, 0.2f);
        tapHandHelpSequence = DOTween.Sequence();
        tapHandHelpSequence.Append(tapHandImage.transform.DOMove(secondFirstPos, 0.4F).SetDelay(0.4f));
        tapHandHelpSequence.Append(tapHandImage.transform.DOMove(secondLastPos, 0.4F).SetDelay(0.4f));
        tapHandHelpSequence.SetLoops(-1, LoopType.Yoyo);
    }

    public IEnumerator CheckForPowerupHelpPopupComplete()
    {
        yield return new WaitUntil(() => powerupHelp == null);
        yield return new WaitUntil(ArePlayableShapesEmpty);
        ResetHighlightedBlocks();
        tapHandHelpSequence?.Kill();
        tapHandImage.gameObject.Deactivate();
        GamePlay.Instance.StopBasicHelp();
    }

    private bool ArePlayableShapesEmpty()
    {
        return BlockShapeSpawner.Instance.GetPlayableShapes().Count == 0;
    }
}