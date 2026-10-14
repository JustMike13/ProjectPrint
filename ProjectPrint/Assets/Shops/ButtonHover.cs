using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, ISelectHandler, IDeselectHandler
{
    private Button btn;
    private Image selectionOverlay;
    private bool controllerSelection;

    void Awake()
    {
        btn = GetComponent<Button>();
        if (btn == null)
        {
            Debug.LogError("ButtonHover requires a Button component.", this);
            return;
        }

        Navigation navigation = btn.navigation;
        navigation.mode = Navigation.Mode.None;
        btn.navigation = navigation;

        RectTransform buttonRect = (RectTransform)transform;
        Graphic[] childGraphics = GetComponentsInChildren<Graphic>(true);
        Rect buttonBounds = buttonRect.rect;
        Bounds highlightBounds = new Bounds(buttonBounds.center, buttonBounds.size);
        foreach (Graphic graphic in childGraphics)
        {
            if (graphic.transform != transform)
            {
                highlightBounds.Encapsulate(RectTransformUtility.CalculateRelativeRectTransformBounds(buttonRect, graphic.rectTransform));
            }
        }

        GameObject overlay = new GameObject("SelectionHighlight", typeof(RectTransform), typeof(Image));
        RectTransform overlayRect = (RectTransform)overlay.transform;
        overlayRect.SetParent(transform, false);
        overlayRect.anchorMin = new Vector2(0.5f, 0.5f);
        overlayRect.anchorMax = new Vector2(0.5f, 0.5f);
        overlayRect.anchoredPosition = highlightBounds.center;
        overlayRect.sizeDelta = highlightBounds.size + new Vector3(16f, 16f, 0f);
        overlayRect.SetAsLastSibling();
        selectionOverlay = overlay.GetComponent<Image>();
        selectionOverlay.color = new Color(0.08f, 0.32f, 0.82f, 0.35f);
        selectionOverlay.raycastTarget = false;
        selectionOverlay.enabled = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SelectButton(false);
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (selectionOverlay != null && controllerSelection)
        {
            selectionOverlay.enabled = true;
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (selectionOverlay != null)
        {
            selectionOverlay.enabled = false;
        }
    }

    public void SelectButton(bool fromController = false)
    {
        SetControllerHighlight(fromController);
        if (btn != null)
        {
            btn.Select();
        }
    }

    public void SetControllerHighlight(bool visible)
    {
        controllerSelection = visible;
        if (selectionOverlay != null)
        {
            selectionOverlay.enabled = visible;
        }
    }

    public void ActivateButton()
    {
        if (btn != null && btn.interactable)
        {
            btn.onClick.Invoke();
        }
    }
}
