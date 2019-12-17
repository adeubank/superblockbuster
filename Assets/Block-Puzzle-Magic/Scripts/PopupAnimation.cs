using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     Popup animation.
/// </summary>
public class PopupAnimation : MonoBehaviour
{
    [SerializeField] private bool AnimateOnDestroy = true;
    [SerializeField] private bool AnimateOnLoad = true;
    private Animator BlackLayFadeAnimator;
    [SerializeField] private Image BlackLayImage;
    private Animator PopupAnimator;
    [SerializeField] private bool UseBackgroundFadeEffect = true;
    [SerializeField] private GameObject WindowContent;

    /// <summary>
    ///     Awake this instance.
    /// </summary>
    private void Awake()
    {
        if (WindowContent != null)
        {
            PopupAnimator = WindowContent.GetComponent<Animator>();

            if (BlackLayImage != null) BlackLayFadeAnimator = BlackLayImage.GetComponent<Animator>();
            if (BlackLayImage == null || BlackLayFadeAnimator == null) UseBackgroundFadeEffect = false;
        }

        if (PopupAnimator == null || WindowContent == null)
        {
            AnimateOnLoad = false;
            AnimateOnDestroy = false;
        }
    }

    /// <summary>
    ///     Raises the window added event.
    /// </summary>
    public void OnWindowAdded()
    {
        ShowStartAnimation();
    }

    /// <summary>
    ///     Shows the start animation.
    /// </summary>
    private void ShowStartAnimation()
    {
        if (AnimateOnLoad && WindowContent != null)
        {
            PopupAnimator.SetTrigger("Open");

            if (UseBackgroundFadeEffect) BlackLayFadeAnimator.SetTrigger("FadeIn");
        }
    }

    /// <summary>
    ///     Raises the window remove event.
    /// </summary>
    public void OnWindowRemove()
    {
        if (AnimateOnDestroy && WindowContent != null)
        {
            PopupAnimator.SetTrigger("Close");
            if (UseBackgroundFadeEffect) BlackLayFadeAnimator.SetTrigger("FadeOut");
            Invoke("OnRemoveTransitionComplete", 0.2F);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    ///     Raises the remove transition complete event.
    /// </summary>
    private void OnRemoveTransitionComplete()
    {
        gameObject.SetActive(false);
    }
}