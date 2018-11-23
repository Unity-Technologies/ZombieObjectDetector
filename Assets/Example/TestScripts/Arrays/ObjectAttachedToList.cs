using UnityEngine;

/// <summary>
/// Adds itself to the object list.
/// Fixed by remember to remove it OnDestroy
/// </summary>
public class ObjectAttachedToList : MonoBehaviour
{

    private void Start()
    {
        StaticObjectList.objectList.Add(this);
    }
}
