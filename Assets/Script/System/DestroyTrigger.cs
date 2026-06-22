using UnityEngine;

public class DestroyTrigger : MonoBehaviour
{
    public GameObject objectToDestroy;
    void OnDestroy()
    {
        if (objectToDestroy != null) Destroy(objectToDestroy);
    }
}