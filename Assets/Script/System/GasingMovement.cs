using UnityEngine;
using System.Collections;

public class GasingMovement : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Visualization")]
    public float speedRotateModel = 500f;

    [Header("Movement Otomatis")]
    public float autoSpeed = 5f;
    public float changeDirectionInterval = 2f;
    private Vector3 randomDirection;

    [Header("Kontrol Player (Support)")]
    public float playerForceMultiplier = 0.1f; // Rasio 1:10

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Memulai pergerakan acak gasing
        StartCoroutine(ChangeDirectionRoutine());
    }

    void Update()
    {
        // Tetap pastikan gasing berputar secara visual
        transform.Rotate(Vector3.up * speedRotateModel * Time.deltaTime);
    }

    void FixedUpdate()
    {
        // 1. Gerakan Otomatis Gasing
        Vector3 autoMovement = randomDirection * autoSpeed;
        rb.AddForce(new Vector3(autoMovement.x, 0f, autoMovement.z), ForceMode.Force);

        // 2. Dorongan Kecil dari Player (Input WASD / Arrow)
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 playerInput = new Vector3(moveHorizontal, 0f, moveVertical).normalized;
        
        if (playerInput.magnitude > 0.1f)
        {
            // Memberikan dorongan kecil (dikali 0.1)
            rb.AddForce(playerInput * autoSpeed * playerForceMultiplier, ForceMode.Impulse);
        }
    }

    IEnumerator ChangeDirectionRoutine()
    {
        while (true)
        {
            // Mengambil arah acak di bidang datar (X dan Z)
            float randomX = Random.Range(-1f, 1f);
            float randomZ = Random.Range(-1f, 1f);
            randomDirection = new Vector3(randomX, 0f, randomZ).normalized;

            // setiap dua detik bakal jalanin fungsi ini lg
            yield return new WaitForSeconds(changeDirectionInterval);
        }
    }
}