using UnityEngine;

/// <summary>
/// Classes based on <seealso cref="IgnoredTypeParam{T}"/> will get scanned.
/// All inherited member variables also get scanned.
/// </summary>
public class BasedOnIgnoredTypeParam : IgnoredTypeParam<BasedOnIgnoredTypeParam>
{

    public void Start()
    {
        BasicStaticObjectStack.stackOfObjects.Push(this);
        notIgnoredInDerivedClass = new TextureHolder();
        notIgnoredInDerivedClass.texture = new Texture2D(1024, 1024, TextureFormat.RGBA32, false);
    }
}

