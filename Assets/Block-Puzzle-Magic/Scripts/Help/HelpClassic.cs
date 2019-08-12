using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if HBDOTween
using DG.Tweening;

#endif


/// <summary>
///     Help classic.
/// </summary>
public class HelpClassic : MonoBehaviour
{
    private readonly List<Canvas> _highlightedBlocks = new List<Canvas>();
    private Sequence firstHandSequence;
    private Vector2 firstPosition = Vector2.zero;
    [SerializeField] private Transform handImage;

    private Vector2 secondPosition = Vector2.zero;
    [SerializeField] private Transform tapHandImage;
    private Sequence tapHandSequence;

    /// <summary>
    ///     Start this instance.
    /// </summary>
    private void Start()
    {
        Invoke("StartHelp", 1F);
    }

    private void OnDestroy()
    {
        _highlightedBlocks.ForEach(go =>
        {
            var c = go.GetComponent<Canvas>();
            c.overrideSorting = false;
            c.sortingOrder = 0;
        });
        firstHandSequence?.Kill();
        tapHandSequence?.Kill();
    }

    /// <summary>
    ///     Starts the help.
    /// </summary>
    private void StartHelp()
    {
        var firstShape = BlockShapeSpawner.Instance.transform.GetChild(0).gameObject;

        firstPosition = firstShape.transform.position;
        firstPosition -= new Vector2(-0.2F, 0.4F);
        handImage.gameObject.SetActive(true);
        handImage.transform.position = firstPosition;
        secondPosition = GamePlay.Instance.transform.Find("Game-Content").position;

        if (firstShape.transform.childCount > 0)
            firstShape.transform.GetChild(0).GetComponent<Canvas>().sortingOrder = 3;

        var unused = GamePlay.Instance.SetAutoMove();
        GamePlay.Instance.highlightingBlocks.ForEach(b =>
        {
            var c = b.gameObject.GetComponent<Canvas>();
            c.overrideSorting = true;
            c.sortingOrder = 3;
            _highlightedBlocks.Add(c);
        });

        var middlePos = GamePlay.Instance.highlightingBlocks.Aggregate(Vector3.zero, (avgPos, b) => avgPos + b.gameObject.transform.position) / GamePlay.Instance.highlightingBlocks.Count;
        tapHandImage.position = (Vector2) middlePos - new Vector2(0, 0.6f);

#if HBDOTween
        transform.GetComponent<CanvasGroup>().DOFade(1F, 0.5F).OnComplete(() => { AnimateInLoop(); });
#endif
    }

    /// <summary>
    ///     Animates the in loop.
    /// </summary>
    private void AnimateInLoop()
    {
#if HBDOTween
        handImage.transform.position = firstPosition;
        firstHandSequence = DOTween.Sequence();
        firstHandSequence.Append(handImage.transform.DOMove(secondPosition, 1F).SetDelay(1));
        firstHandSequence.Append(handImage.transform.DOMove(firstPosition, 0.5F).SetDelay(1));
        firstHandSequence.SetLoops(-1, LoopType.Restart);
#endif
        var secondHandTransform = tapHandImage.transform;
        var secondHandPosition = (Vector2) secondHandTransform.position;
        var secondFirstPos = secondHandPosition - new Vector2(0, 0.2f);
        var secondLastPos = secondHandPosition + new Vector2(0, 0.2f);
        tapHandSequence = DOTween.Sequence();
        tapHandSequence.Append(tapHandImage.transform.DOMove(secondFirstPos, 0.4F).SetDelay(0.4f));
        tapHandSequence.Append(tapHandImage.transform.DOMove(secondLastPos, 0.4F).SetDelay(0.4f));
        tapHandSequence.SetLoops(-1, LoopType.Yoyo);
    }
}