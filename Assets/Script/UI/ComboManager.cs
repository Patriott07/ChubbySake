using TMPro;
using UnityEngine;

public class ComboSystem : MonoBehaviour
{
    [Header("Combo Settings")]
    [SerializeField] private float comboTimeLimit = 2.0f;
    [SerializeField] private float hitCooldown = 0.2f; // Jeda minimal biar gak spam (dalam detik)

    private int currentCombo = 0;
    private float lastCollisionExitTime = -100f;
    private float lastHitTime = -100f; // Menyimpan waktu hit terakhir
    private bool isTouchingEnemy = false;

    [Header("Combo UI")]
    [SerializeField] private GameObject comboUIOBJ; // (Sedikit ubah ke camelCase biar rapi)
    [SerializeField] private TextMeshProUGUI comboUIText;

    void Update()
    {
        // Mengecek apakah combo putus karena waktu habis
        if (!isTouchingEnemy && currentCombo > 0 && Time.time - lastCollisionExitTime > comboTimeLimit)
        {
            currentCombo = 0;
            Debug.Log("Combo dropped! Time's up.");
        }

        // Update Text UI
        comboUIText.text = $"Combo : {currentCombo} X ";

        // Optimasi: Hanya panggil SetActive jika state-nya memang perlu diubah 
        // (menghindari pemanggilan fungsi berat setiap frame)
        if (currentCombo >= 2 && !comboUIOBJ.activeSelf)
            comboUIOBJ.SetActive(true);
        else if (currentCombo < 2 && comboUIOBJ.activeSelf)
            comboUIOBJ.SetActive(false);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            isTouchingEnemy = true;

            // CEGAH SPAM: Jika waktu hit sekarang terlalu dekat dengan hit sebelumnya, abaikan!
            if (Time.time - lastHitTime < hitCooldown) return;

            // Catat waktu hit yang sah
            lastHitTime = Time.time;

            if (lastCollisionExitTime > 0 && Time.time - lastCollisionExitTime <= comboTimeLimit)
            {
                currentCombo++;
                Debug.Log("Combo Hit! Current Combo: " + currentCombo);
            }
            else
            {
                currentCombo = 1;
                Debug.Log("First Hit! Combo starts at: " + currentCombo);
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            isTouchingEnemy = false;
            lastCollisionExitTime = Time.time;
        }
    }
}
