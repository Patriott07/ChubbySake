using UnityEngine;
using System.Collections;
using NUnit.Framework;

public class GasingMovement : MonoBehaviour
{

    [SerializeField] private float shakeStrength, shakeDuration;
    private Rigidbody rb;

    [Header("Movement Otomatis")]
    public float autoSpeed = 5f;
    public float changeDirectionInterval = 2f;
    private Vector3 randomDirection;

    [Header("Kontrol Player (Support)")]
    public float playerForceMultiplier = 0.1f; // Rasio 1:10

    [SerializeField] private float knockbackForce = 10f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Memulai pergerakan acak gasing
        StartCoroutine(ChangeDirectionRoutine());
    }

    void Update()
    {
        // Tetap pastikan gasing berputar secara visual
        transform.Rotate(Vector3.up * 500 * Time.deltaTime);
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Vector3 enemyPosition = collision.transform.position;
            Vector3 myPosition = transform.position;

            Vector3 knockbackDirection = (myPosition - enemyPosition).normalized;
            GameEvents.CallShake.Invoke(shakeDuration, shakeStrength);
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
                
                // Menjalankan fungsi Coroutine untuk efek slow-mo
                StartCoroutine(SlowMotionEffect());
            }
            else
            {
                Debug.LogWarning("Objek ini tidak memiliki Rigidbody untuk knockback!");
            }
        }
    }

    // Fungsi Coroutine untuk mengatur durasi efek
    private IEnumerator SlowMotionEffect() {
        Time.timeScale = 0.1f;
        
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        yield return new WaitForSecondsRealtime(0.75f);
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
}