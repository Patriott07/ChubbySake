using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDUIHandler : MonoBehaviour
{
    [Header("Player References (RUSIDD)")]
    [SerializeField] private GasingStat playerStat;
    [SerializeField] private TextMeshProUGUI playerRpmText; // Menampilkan "140 RPM"
    [SerializeField] private Image playerHpBar;           // Image Filled Horizontal (Merah)
    [SerializeField] private Image playerAttackEnergyBar; // Image Filled Horizontal (Oranye)
    [SerializeField] private Image playerUltimateEnergyBar;// Image Filled Horizontal (Ungu)

    [Header("Enemy References (GEMINI0040)")]
    [SerializeField] private GasingStat enemyStat;
    [SerializeField] private Image enemyHpBar;
    [SerializeField] private Image enemyAttackEnergyBar;
    [SerializeField] private Image enemyUltimateEnergyBar;

    [Header("Match Info (Top Center)")]
    [SerializeField] private TextMeshProUGUI timerText;       // Menampilkan "00:14"
    [SerializeField] private TextMeshProUGUI playerRoundScoreText; // Skor ronde player (Kiri)
    [SerializeField] private TextMeshProUGUI enemyRoundScoreText;  // Skor ronde musuh (Kanan)

    [Header("Notification Log (Kanan Atas)")]
    [SerializeField] private TextMeshProUGUI logText;         // Menampilkan "Gemini0040 gain 400hp"
    [SerializeField] private GameObject logPanel;            // Indikator/Latar notifikasi jika ada

    void Start()
    {
        if (logPanel != null) logPanel.SetActive(false);
    }

    void Update()
    {
        UpdatePlayerHUD();
        UpdateEnemyHUD();
        UpdateMatchTimer();
    }

    private void UpdatePlayerHUD()
    {
        if (playerStat == null) return;

        // 1. Update Teks RPM di Kanan Bawah
        if (playerRpmText != null)
            playerRpmText.text = $"{(int)playerStat.currentRPM} RPM";

        // 2. Update 3 Bar Horizontal Player (Fill Amount: 0 sampai 1)
        if (playerHpBar != null)
            playerHpBar.fillAmount = playerStat.currentHp / playerStat.maxHp;

        if (playerAttackEnergyBar != null)
            playerAttackEnergyBar.fillAmount = playerStat.currentEnergyAttack / playerStat.maxEnergyAttack;

        if (playerUltimateEnergyBar != null)
            playerUltimateEnergyBar.fillAmount = playerStat.currentEnergyUltimate / playerStat.maxEnergyUltimate;
    }

    private void UpdateEnemyHUD()
    {
        if (enemyStat == null) return;

        // 3. Update 3 Bar Horizontal Musuh di Kanan Atas
        if (enemyHpBar != null)
            enemyHpBar.fillAmount = enemyStat.currentHp / enemyStat.maxHp;

        if (enemyAttackEnergyBar != null)
            enemyAttackEnergyBar.fillAmount = enemyStat.currentEnergyAttack / enemyStat.maxEnergyAttack;

        if (enemyUltimateEnergyBar != null)
            enemyUltimateEnergyBar.fillAmount = enemyStat.currentEnergyUltimate / enemyStat.maxEnergyUltimate;
    }

    private void UpdateMatchTimer()
    {
        // Mengambil data waktu dari TimeManager atau sistem buatanmu sendiri
        // Di bawah ini adalah contoh logika jika timer dihitung mundur/maju di game
        if (timerText != null)
        {
            // Misal: ambil dari Time.timeSinceLevelLoad atau variabel milik MatchManager
            float t = Time.timeSinceLevelLoad; 
            string minutes = ((int)t / 60).ToString("00");
            string seconds = (t % 60).ToString("00");
            
            timerText.text = $"{minutes}:{seconds}";
        }
    }

    // Fungsi Publik untuk menampilkan notifikasi log dinamis (misal saat potion diambil)
    public void ShowNotificationLog(string message)
    {
        if (logText != null)
        {
            logText.text = message;
            if (logPanel != null) logPanel.SetActive(true);
            
            // Opsional: Matikan log otomatis setelah 3 detik menggunakan Invoke
            CancelInvoke(nameof(HideNotificationLog));
            Invoke(nameof(HideNotificationLog), 3f);
        }
    }

    private void HideNotificationLog()
    {
        if (logPanel != null) logPanel.SetActive(false);
    }
}
