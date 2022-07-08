using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapCursor : MonoBehaviour
{
    // Start is called before the first frame update
    public Texture2D cursorPointer;

    private void Start()
    {
        Cursor.SetCursor(cursorPointer, Vector2.zero, CursorMode.ForceSoftware);
    }
}
