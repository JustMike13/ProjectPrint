using UnityEngine;

public static class VerifyOrderPositions
{
    public static string Run()
    {
        Transform computerUI = GameObject.Find("Computer").transform.Find("[ComputerUI]");
        ComputerScreen screen = computerUI.GetComponent<ComputerScreen>();
        if (screen == null)
        {
            throw new System.InvalidOperationException("ComputerScreen instance not found in the running scene.");
        }

        screen.OpenOrderManager();

        Transform ordersCanvas = GameObject.Find("Computer/[ComputerUI]/OrderManager").transform;
        RectTransform canvasRect = (RectTransform)ordersCanvas;
        OrderElement[] orders = ordersCanvas.GetComponentsInChildren<OrderElement>(true);

        string result = "OrdersCanvas sizeDelta=" + canvasRect.sizeDelta + "\n";
        foreach (OrderElement order in orders)
        {
            RectTransform rect = (RectTransform)order.transform;
            result += order.name
                + " localPos=" + rect.localPosition.ToString("F2")
                + " worldPos=" + rect.position.ToString("F2")
                + " text='" + order.NoOfItemsText.text + "'\n";
        }

        return result;
    }
}
