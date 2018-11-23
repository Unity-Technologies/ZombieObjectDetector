using UnityEngine;

/// <summary>
/// Adds it to the dictionary with a unique key.
/// Fixed by remember to remove it OnDestroy
/// </summary>
public class ObjectAttachedToDictionary : MonoBehaviour
{

    private void Start()
    {
        StaticStringObjectDictionary.stringObjectDictionary.Add(
            StaticStringObjectDictionary.stringObjectDictionary.Count.ToString(), this);
    }

}
