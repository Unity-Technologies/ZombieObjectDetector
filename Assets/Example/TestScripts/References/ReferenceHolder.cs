using System;
using UnityEngine;

public class ReferenceHolder : MonoBehaviour
{

    [NonSerialized]
    public ObjectAttachedToReference objectReference;

    private void Start()
    {
        ReferenceHolderHolder.holderReference = this;
    }
}
