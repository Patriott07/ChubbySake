using Unity.VisualScripting;
using UnityEngine;

public class LineUI : MonoBehaviour
{
    void OnEnable()
    {
        GameEvents.OnDeleteLine += DestroyObj;
    }

    void OnDisable()
    {
        GameEvents.OnDeleteLine -= DestroyObj;
    }

    void DestroyObj()
    {
        Destroy(gameObject);
    }
}
