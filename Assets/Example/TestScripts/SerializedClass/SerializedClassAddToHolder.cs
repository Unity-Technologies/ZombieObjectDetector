using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerializedClassAddToHolder : MonoBehaviour
{

    [SerializeField]
    SerializedClass[] serializedClassList = new SerializedClass[2];

    private void Start()
    {
        foreach (var serializedClass in serializedClassList)
        {
            SerializedClassHolder.serializedClassList.Add(serializedClass);
        }
    }
}
