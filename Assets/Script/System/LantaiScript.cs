using UnityEngine;
using data.structs;
public class LantaiScript : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        GasingStat statObj = collision.gameObject.GetComponentInParent<GasingStat>();
        if (statObj != null) statObj.TerimaDamagePart(PartType.Head, 9999999999, null);
    }
}
