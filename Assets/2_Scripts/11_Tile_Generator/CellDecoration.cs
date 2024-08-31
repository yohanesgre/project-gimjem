using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

[Serializable]
public class CellDecoration
{
    [ReadOnly] public Vector2 position; // x and y coordinates
    public int cellSize;
    public Color color;
}
