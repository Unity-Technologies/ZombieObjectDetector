using UnityEngine;

/// <summary>
/// Attachs itself to an event but doesn't remove itself when destroyed.
/// </summary>
public class EventListener : MonoBehaviour
{
    Texture2D textureAsset;

    void Start()
    {
        StaticEvents.OnDoAThing += TheThingToDo;
    }

    private void TheThingToDo()
    {
        textureAsset = new Texture2D(1024, 1024, TextureFormat.RGBA32, false);
        Debug.Log("Created Texture: " + textureAsset.ToString());
    }

}
