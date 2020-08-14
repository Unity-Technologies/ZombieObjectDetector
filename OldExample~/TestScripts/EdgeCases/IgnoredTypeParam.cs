using UnityEngine;

/// <summary>
/// The Value of T can't be determined so it gets ignored when scanning all the types from reflection.
/// </summary>
/// <typeparam name="T"></typeparam>
public class IgnoredTypeParam<T> : MonoBehaviour where T : MonoBehaviour
{
    public TextureHolder notIgnoredInDerivedClass;
}

public struct TextureHolder
{
    public Texture2D texture;
}