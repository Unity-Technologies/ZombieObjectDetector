using System;
using UnityEngine;

/// <summary>
/// Hacks an new object onto the array.
/// Fixed by remember to remove it OnDestroy, not easy with simple array :/
/// </summary>
public class ObjectAttachedToArray : MonoBehaviour
{

    private void Start()
    {
        Array.Resize(ref StaticArray.objectArray, StaticArray.objectArray.Length + 1);
        StaticArray.objectArray[StaticArray.objectArray.Length - 1] = this;
    }
}
