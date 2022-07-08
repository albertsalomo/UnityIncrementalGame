using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContentCursor : MonoBehaviour
{
    // Start is called before the first frame update
    public Texture2D cursorPointer;
    public Texture2D tapPointer;

    private void Start()
    {
        Cursor.SetCursor(tapPointer, Vector2.zero, CursorMode.ForceSoftware);
    }

    void OnMouseEnter()
    {
        //Cursor.visible = false;
        Cursor.SetCursor(cursorPointer, Vector2.zero, CursorMode.ForceSoftware);
    }

    void OnMouseExit()
    {
        Cursor.SetCursor(tapPointer, Vector2.zero, CursorMode.ForceSoftware);
    }
}
