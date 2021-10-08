using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CachedTransform : MonoBehaviour
{
    Transform _MyTransform;
    public Transform MyTransform
    {
        get
        {
            if (_MyTransform == null) _MyTransform = transform;
            return _MyTransform;
        }
    }
}

public class CachedRectTransform : MonoBehaviour
{
    RectTransform _MyTransform;
    public RectTransform MyTransform
    {
        get
        {
            if (_MyTransform == null) _MyTransform = GetComponent<RectTransform>();
            return _MyTransform;
        }
    }
}

public static class Utilities
{
    public static bool MouseOverUI()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current)
        {
            position = new Vector2(Input.mousePosition.x, Input.mousePosition.y)
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}
