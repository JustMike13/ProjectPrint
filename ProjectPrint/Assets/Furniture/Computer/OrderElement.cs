using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderElement : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI NoOfItemsText;
    [SerializeField] public TextMeshProUGUI PriceText;
    [SerializeField] public Button PrintButton;

    private Image selectionBorder;
    private static Sprite roundedBorderSprite;
    private static Vector2 roundedBorderSpriteSize;

    public void SetSelected(bool selected)
    {
        if (selectionBorder == null)
        {
            CreateSelectionBorder();
        }

        if (selectionBorder != null)
        {
            selectionBorder.enabled = selected;
        }
    }

    private void CreateSelectionBorder()
    {
        RectTransform rootRect = (RectTransform)transform;
        Rect rootBounds = rootRect.rect;
        Bounds bounds = new Bounds(rootBounds.center, rootBounds.size);
        Graphic[] childGraphics = GetComponentsInChildren<Graphic>(true);
        foreach (Graphic graphic in childGraphics)
        {
            if (graphic.transform != transform)
            {
                bounds.Encapsulate(RectTransformUtility.CalculateRelativeRectTransformBounds(rootRect, graphic.rectTransform));
            }
        }

        const float outerPadding = 9f;
        GameObject borderObject = new GameObject("SelectionBorder", typeof(RectTransform), typeof(Image));
        RectTransform borderRect = (RectTransform)borderObject.transform;
        borderRect.SetParent(rootRect, false);
        borderRect.anchorMin = new Vector2(0.5f, 0.5f);
        borderRect.anchorMax = new Vector2(0.5f, 0.5f);
        borderRect.anchoredPosition = bounds.center;
        borderRect.sizeDelta = bounds.size + Vector3.one * outerPadding * 2f;
        borderRect.SetAsLastSibling();

        selectionBorder = borderObject.GetComponent<Image>();
        selectionBorder.sprite = GetRoundedBorderSprite(borderRect.sizeDelta);
        selectionBorder.type = Image.Type.Simple;
        selectionBorder.color = new Color(0.62f, 1f, 0.68f, 1f);
        selectionBorder.raycastTarget = false;
        selectionBorder.enabled = false;
    }

    private static Sprite GetRoundedBorderSprite(Vector2 size)
    {
        if (roundedBorderSprite != null && roundedBorderSpriteSize == size)
        {
            return roundedBorderSprite;
        }

        const float pixelsPerUnit = 2f;
        const float radiusInUnits = 15f;
        const float thicknessInUnits = 6f;
        int textureWidth = Mathf.Max(8, Mathf.CeilToInt(size.x * pixelsPerUnit));
        int textureHeight = Mathf.Max(8, Mathf.CeilToInt(size.y * pixelsPerUnit));
        Texture2D texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[textureWidth * textureHeight];
        Vector2 center = new Vector2((textureWidth - 1) * 0.5f, (textureHeight - 1) * 0.5f);
        Vector2 halfSize = center;
        float thickness = thicknessInUnits * pixelsPerUnit;
        float radius = Mathf.Min(radiusInUnits * pixelsPerUnit, Mathf.Min(halfSize.x, halfSize.y));
        Vector2 innerHalfSize = halfSize - Vector2.one * thickness;
        float innerRadius = radius - thickness;

        for (int y = 0; y < textureHeight; y++)
        {
            for (int x = 0; x < textureWidth; x++)
            {
                Vector2 point = new Vector2(x, y);
                float outerDistance = RoundedRectangleDistance(point - center, halfSize, radius);
                float innerDistance = RoundedRectangleDistance(point - center, innerHalfSize, innerRadius);
                float outerAlpha = Mathf.Clamp01(0.5f - outerDistance);
                float innerAlpha = Mathf.Clamp01(0.5f + innerDistance);
                pixels[y * textureWidth + x] = new Color(1f, 1f, 1f, outerAlpha * innerAlpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        roundedBorderSprite = Sprite.Create(texture, new Rect(0f, 0f, textureWidth, textureHeight),
            new Vector2(0.5f, 0.5f), pixelsPerUnit);
        roundedBorderSpriteSize = size;
        return roundedBorderSprite;
    }

    private static float RoundedRectangleDistance(Vector2 point, Vector2 halfSize, float radius)
    {
        Vector2 q = new Vector2(Mathf.Abs(point.x), Mathf.Abs(point.y)) - halfSize + Vector2.one * radius;
        Vector2 outside = new Vector2(Mathf.Max(q.x, 0f), Mathf.Max(q.y, 0f));
        return outside.magnitude + Mathf.Min(Mathf.Max(q.x, q.y), 0f) - radius;
    }
}
