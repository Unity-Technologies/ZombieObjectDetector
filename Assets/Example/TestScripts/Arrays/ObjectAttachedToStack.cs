using UnityEngine;

/// <summary>
/// Pushes it onto <seealso cref="StaticObjectStack"/>.
/// </summary>
public class ObjectAttachedToStack : MonoBehaviour
{

    private void Start()
    {
        StaticObjectStack.stackOfObjects.Push(this);
    }
}
