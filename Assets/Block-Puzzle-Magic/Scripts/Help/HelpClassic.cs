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
    private Vector2 firstPosition = Vector2.zero;

    [SerializeField] private Transform handImage;
    [SerializeField] private Transform secondHandImage;
    private Vector2 secondPosition = Vector2.zero;

    /// <summary>
    ///     Start this instance.
    /// </summary>
    private void Start()
    {
        Invoke("StartHelp", 1F);
    }

    private void OnDestroy()
    {
        _highlightedBlocks.ForEach(Destroy);
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
            var c = b.gameObject.AddComponent<Canvas>();
            c.overrideSorting = true;
            c.sortingOrder = 3;
            _highlightedBlocks.Add(c);
        });

        var middlePos = GamePlay.Instance.highlightingBlocks.Aggregate(Vector2.zero, (avgPos, b) => avgPos + (Vector2) b.gameObject.transform.position);
        secondHandImage = Instantiate(handImage, middlePos, Quaternion.Euler(-handImage.eulerAngles), transform);

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
        var handImgSequence = DOTween.Sequence();
        handImgSequence.Append(handImage.transform.DOMove(secondPosition, 1F).SetDelay(1));
        handImgSequence.Append(handImage.transform.DOMove(firstPosition, 0.5F).SetDelay(1));
        handImgSequence.SetLoops(-1, LoopType.Restart);

#endif
        var secondHandTransform = secondHandImage.transform;
        var secondHandPosition = secondHandTransform.position;
        var secondFirstPos = secondHandPosition + new Vector3(secondHandPosition.x, secondHandPosition.y - 5f);
        var secondLastPos = secondHandPosition + new Vector3(secondHandPosition.x, secondHandPosition.y);
        var secondHandImgSequence = DOTween.Sequence();
        secondHandImgSequence.Append(secondHandImage.transform.DOMove(secondFirstPos, 1F).SetDelay(1));
        secondHandImgSequence.Append(secondHandImage.transform.DOMove(secondLastPos, 0.5F).SetDelay(1));
        secondHandImgSequence.SetLoops(-1, LoopType.Restart);
    }
}