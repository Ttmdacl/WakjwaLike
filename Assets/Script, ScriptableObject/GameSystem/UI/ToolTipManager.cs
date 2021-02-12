using UnityEngine;

public enum PivotType
{
    Mastery,
    Item
}

public class ToolTipManager : MonoBehaviour
{
    public static ToolTipManager current;
    public ToolTip toolTip;
    public RectTransform rectTransform;
    public Inventory Inventory;

    private void Awake()
    {
        current = this;
    }

    public static void Show(string header, string description, string comment, Vector3 position, PivotType pivotType)
    {
        current.toolTip.gameObject.SetActive(true);
        current.toolTip.SetText(header, description, comment);
        current.rectTransform.pivot = (pivotType == PivotType.Mastery) ? new Vector2(0.5f, -0.8f) : new Vector2(1.2f, 0.5f);
        current.toolTip.transform.position = position;
    }

    public static void Hide()
    {
        current.toolTip.gameObject.SetActive(false);
    }
}
