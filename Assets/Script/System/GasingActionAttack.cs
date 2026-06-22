using UnityEngine;
using System.Collections;
using DG.Tweening;

public class GasingActionAttack : MonoBehaviour
{
    [Header("Referensi")]
    private Rigidbody rb;
    private GasingStat stats;
    private GasingMovement movement;
    private Transform enemyTransform;

    [Header("Attack Settings")]
    [SerializeField] private float attackDashSpeed = 25f; // Kecepatan melesat ke musuh
    [SerializeField] private float dashDuration = 0.4f;   // Durasi melesat

    [Header("UI QTE Mini-Game")]
    [SerializeField] private GameObject panelQTE; // Parent UI Canvas untuk bundaran QTE
    private bool isQTEActive = false;
    private bool IsHoming = true;
    private float qteBonusDamage = 0f;
    public CanvasGroup canvasGroupBgBlack;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        stats = GetComponent<GasingStat>();
        movement = GetComponent<GasingMovement>();

        // Cari musuh di arena
        GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");
        if (enemy != null) enemyTransform = enemy.transform;
    }

    void Update()
    {
        // 1 & 2. Player tekan Space DAN Energi Attack sudah 100%
        if (Input.GetKeyDown(KeyCode.Space) && stats != null && stats.currentEnergyAttack >= stats.maxEnergyAttack && !isQTEActive)
        {
            StartCoroutine(StartActionAttackSequence());
        }
    }

    private IEnumerator StartActionAttackSequence()
    {
        Debug.Log("ACTION ATTACK");
        isQTEActive = true;
        qteBonusDamage = 0f;
        rb.linearVelocity = Vector3.zero;
        canvasGroupBgBlack.DOFade(0.7f, 0.4f).SetUpdate(true);

        // 6. Game berjalan sangat lambat
        Time.timeScale = 0.05f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        // Tampilkan panel tempat bundaran QTE bermunculan
        if (panelQTE != null) panelQTE.SetActive(true);

        // 5. Jalankan mini-game selama 5 detik (menggunakan Realtime karena Time.timeScale lambat)
        float qteTimer = 0f;
        while (qteTimer < 5f)
        {
            qteTimer += Time.unscaledDeltaTime;
            yield return null; 
        }

        canvasGroupBgBlack.DOFade(0, 0.4f).SetUpdate(true);

        // Matikan panel QTE setelah 5 detik selesai
        if (panelQTE != null) panelQTE.SetActive(false);

        // Kembalikan waktu game menjadi normal sebelum melesat
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        // 3 & 4. Lakukan gerakan melesat cepat ke musuh dengan efek kebal
        if (enemyTransform != null && enemyTransform.gameObject.activeInHierarchy)
        {
            stats.isInvincibleAttack = true; // Aktifkan kebal
            
            // Matikan kontrol pergerakan otomatis sementara agar tidak tabrakan arah
            if (movement != null) movement.enabled = false;

            // Hitung arah ke musuh
            Vector3 attackDir = (enemyTransform.position - transform.position).normalized;
            attackDir.y = 0;

            // Melesat menggunakan kecepatan tinggi ke arah musuh
            float currentDashTime = 0f;
            while (currentDashTime < dashDuration)
            {// Cek ulang posisi musuh di SETIAP FRAME agar arahnya terus berbelok mengunci target (Homing)
                if (enemyTransform != null && enemyTransform.gameObject.activeInHierarchy && IsHoming)
                {
                    Vector3 dynamicAttackDir = (enemyTransform.position - transform.position).normalized;
                    dynamicAttackDir.y = 0; // Tetap kunci sumbu Y agar tidak terbang/menembus cekungan wajan

                    // Tembakkan Rigidbody dengan arah yang selalu diperbarui
                    rb.linearVelocity = dynamicAttackDir * attackDashSpeed; 
                }

                currentDashTime += Time.deltaTime;
                yield return null;
                // rb.linearVelocity = attackDir * attackDashSpeed; // atau rb.velocity untuk Unity versi lama
                // currentDashTime += Time.deltaTime;
                // yield return null;
            }

            // Kembalikan kontrol pergerakan gasing dan matikan status kebal
            if (movement != null) movement.enabled = true;
            stats.isInvincibleAttack = false;

            // Reset energi kembali ke 0 setelah digunakan
            stats.currentEnergyAttack = 0f;
            stats.damageTambahanQTE = 0f;
        }

        isQTEActive = false;
    }

    // Fungsi yang akan dipanggil oleh bundaran UI QTE saat berhasil di-hover + hold
    public void TambahBonusDamage(float bonus)
    {
        qteBonusDamage += bonus;
        stats.damageTambahanQTE = qteBonusDamage;
        Debug.Log($"Bonus Damage Bertambah! Total: {qteBonusDamage}");
    }
}