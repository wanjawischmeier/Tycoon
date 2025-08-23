using System.Globalization;
using TMPro;
using UnityEngine;

public static class Utility
{
    public static string ToDollars(this int price)
    {
        return "$" + string.Format(CultureInfo.InvariantCulture, "{0:N0}", price);
    }

    public static bool SetChildTextView(this GameObject parent, string childName, string text)
    {
        var textView = parent.transform.Find(childName);
        if (textView == null)
        {
            Debug.Log($"Unable to find text view: {childName}");
            return false;
        }

        var textComponent = textView.GetComponent<TextMeshProUGUI>();
        if (textComponent == null)
        {
            Debug.Log($"Supposed text view {childName} has no text component");
            return false;
        }

        textComponent.text = text;
        return true;
    }

    public static Transform FindPath(this Transform transform, string path)
    {
        string[] elementNames = path.Split('/');

        Transform currentElement = transform;
        foreach (var elementName in elementNames)
        {
            currentElement = currentElement.Find(elementName);
        }

        return currentElement;
    }
}
