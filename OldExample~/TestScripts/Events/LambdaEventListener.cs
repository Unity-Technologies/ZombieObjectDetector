using UnityEngine;

/// <summary>
/// Attachs itself to an event but doesn't remove itself when destroyed.
/// Lambda's can show up differently as no predefined function associated.
/// </summary>
public class LambdaEventListener : MonoBehaviour
{

    Texture2D textureAsset;

    void Start()
    {
        StaticEvents.OnDoAThingPlusX += (x) =>
        {

            textureAsset = new Texture2D(1024, 1024, TextureFormat.RGBA32, false);
            Debug.Log("Created Texture: " + textureAsset.ToString());
        };
        // if no refrence to a member variable of this class, will create a standalone class with
        // function (x) =>
        //textureAsset = new Texture2D(1024, 1024, TextureFormat.RGBA32, false);

    }
}
