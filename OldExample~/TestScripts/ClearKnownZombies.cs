using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

public class ClearKnownZombies : MonoBehaviour {

    [SerializeField]
    StaticEvents staticEventsObject =null;

    private void Start()
    {
        BasicStaticObjectStack.stackOfObjects.Clear();
        StaticArray.objectArray = new ObjectAttachedToArray[0];
        StaticObjectList.objectList.Clear();
        StaticObjectStack.stackOfObjects.Clear();
        StaticStringObjectDictionary.stringObjectDictionary.Clear();

        SerializedClassHolder.serializedClassList.Clear();
        if (ReferenceHolderHolder.holderReference)
        {
            ReferenceHolderHolder.holderReference.objectReference = null;
        }
        ReferenceHolderHolder.holderReference = null;
        
        staticEventsObject.RemoveAllEventsHack();
        StaticEvents.DoAThing();
        StaticEvents.DoAThingPlusX(1);

        Debug.Log("Zombies Cleaned Up");
        Resources.UnloadUnusedAssets();
        SceneManager.LoadScene("Boot");
    }
}
