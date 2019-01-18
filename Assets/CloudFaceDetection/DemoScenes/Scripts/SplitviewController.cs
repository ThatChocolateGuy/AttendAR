using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class SplitviewController : MonoBehaviour {

    [Tooltip("RectTransform of the top panel")]
    public RectTransform rect1;

    [Tooltip("RectTransform of the bottom panel")]
    public RectTransform rect2;

    [Tooltip("Min height of each of the panels")]
    public float minHeight;

    [Tooltip("Cursor texture applied when resizing")]
    public Texture2D cursorTexture;

    [Tooltip("Cursor mode for the cursorTexture")]
    public CursorMode cursorMode = CursorMode.Auto;

    [Tooltip("Hotspot of the cursorTexture")]
    public Vector2 hotSpot = Vector2.zero;

    private bool drag = false;

    void Start () {
	
	}
	
	void Update () {
	
	}

    // Called when mouse enters splitbar in order to set custom resize cursor
    public void OnMouseEnter()
    {
        if (cursorTexture)
        {
            Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
        }
    }

    // Called when mouse exits splitbar in order to reset cursor
    public void OnMouseExit()
    {
        if (cursorTexture && !drag)
        {
            Cursor.SetCursor(null, Vector2.zero, cursorMode);
        }
    }

    // Called when drag splitbar starts
    public void OnDragStart()
    {
        drag = true;
    }

    // Called when drag splitbar ends
    public void OnDragEnd()
    {
        drag = false;
        Cursor.SetCursor(null, Vector2.zero, cursorMode);
    }

    // Called on splitbar drag in order to resize panels
    public void OnDrag()
    {        
        float mousePos = Input.mousePosition.y;

        if(Mathf.Abs(rect1.InverseTransformPoint(0, mousePos, 0).y) < minHeight ||
           Mathf.Abs(rect2.InverseTransformPoint(0, mousePos, 0).y) < minHeight)
            return;

        Vector3 pos = transform.position;
        pos.y = mousePos;
        transform.position = pos;

        float localY = transform.localPosition.y;

        Vector2 size = rect1.sizeDelta;
        size.y = -localY;
        rect1.sizeDelta = size;

        Vector2 size2 = rect2.sizeDelta;
        size2.y = transform.localPosition.y;
        rect2.sizeDelta = size2;
    }
}
