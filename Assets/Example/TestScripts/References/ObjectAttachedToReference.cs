using UnityEngine;

public class ObjectAttachedToReference : MonoBehaviour
{
    [SerializeField]
    ReferenceHolder m_ReferenceHolder;
    private void Start()
    {
        m_ReferenceHolder.objectReference = this;
    }
}
