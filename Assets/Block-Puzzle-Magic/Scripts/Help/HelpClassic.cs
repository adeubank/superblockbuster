﻿using System.Collections.Generic;
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
    private Canvas firstShapeCanvas;
    [SerializeField] private Transform handImage;

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
        tapHandSequence?.Kill();
        firstShapeCanvas.sortingOrder = 0;
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
            firstHandSequence = DOTween.Sequence();
            firstHandSequence.Append(handImage.transform.DOMove(GamePlay.Instance.transform.Find("Game-Content").position, 1F).SetDelay(1));
            firstHandSequence.Append(handImage.transform.DOMove(firstPosition, 0.5F).SetDelay(1));
            firstHandSequence.SetLoops(-1, LoopType.Restart);
            InvokeRepeating("CheckForDragComplete", 0.3f, 0.3f);
        });
    }

    private void CheckForDragComplete()
    {
        var firstShape = BlockShapeSpawner.Instance.GetPlayableShapes()[0];
        if (firstShape == null)
        {
            Debug.LogWarning("Did not find a first shape somehow");
            GamePlay.Instance.StopBasicHelp();
            return;
        }

        if (firstShape.ShapeID == (int) ShapeInfo.Powerups.Doubler)
        {
            firstHandSequence?.Kill();
            handImage.gameObject.Deactivate();
            ShowTappableHelp();
            CancelInvoke("CheckForDragComplete");
        }
    }

    private void ShowTappableHelp()
    {
        GamePlay.Instance.highlightingBlocks.ForEach(b =>
        {
            var c = b.gameObject.GetComponent<Canvas>();
            c.overrideSorting = true;
            c.sortingOrder = 3;
            _highlightedBlocks.Add(c);
        });

        tapHandImage.gameObject.Activate();
        var unused = GamePlay.Instance.SetAutoMove();

        var middlePos = GamePlay.Instance.highlightingBlocks.Aggregate(Vector3.zero, (avgPos, b) => avgPos + b.gameObject.transform.position) / GamePlay.Instance.highlightingBlocks.Count;
        tapHandImage.position = (Vector2) middlePos - new Vector2(0, 0.6f);

        var secondHandTransform = tapHandImage.transform;
        var secondHandPosition = (Vector2) secondHandTransform.position;
        var secondFirstPos = secondHandPosition - new Vector2(0, 0.2f);
        var secondLastPos = secondHandPosition + new Vector2(0, 0.2f);
        tapHandSequence = DOTween.Sequence();
        tapHandSequence.Append(tapHandImage.transform.DOMove(secondFirstPos, 0.4F).SetDelay(0.4f));
        tapHandSequence.Append(tapHandImage.transform.DOMove(secondLastPos, 0.4F).SetDelay(0.4f));
        tapHandSequence.SetLoops(-1, LoopType.Yoyo);
        InvokeRepeating("CheckForTapHelpComplete", 0.1f, 0.1f);
    }

    private void CheckForTapHelpComplete()
    {
        if (BlockShapeSpawner.Instance.GetPlayableShapes().Count == 0) GamePlay.Instance.StopBasicHelp();
    }
}