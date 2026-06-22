using UnityEngine;

public class MovementGuide : MonoBehaviour
{
    [SerializeField] private Transform arrow;
    [SerializeField] private float radius = 1f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        Vector3 velocity = rb.linearVelocity;

        if (velocity.sqrMagnitude < 0.01f)
            return;

        velocity.y = 0f;

        Vector3 dir = velocity.normalized;

        float angle = Mathf.Atan2(dir.x, dir.z);

        Vector3 offset = new Vector3(
            Mathf.Sin(angle),
            0f,
            Mathf.Cos(angle)
        ) * radius;

        arrow.position = transform.position + offset;

        // 🔥 sekarang rotasi cuma horizontal
        arrow.rotation = Quaternion.LookRotation(dir, Vector3.up);
    }
}
