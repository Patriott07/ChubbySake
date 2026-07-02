using UnityEngine;
using TMPro;
using System.Collections;
using DG.Tweening;
using data.structs;
public class RoundManager : MonoBehaviour
{
    [Header("Spawning & Target Objects")]
    [SerializeField] private Transform spawnPlayerLoc;
    [SerializeField] private Transform spawnEnemyLoc;
    [SerializeField] private GameObject playerObj;
    [SerializeField] private GameObject enemyObj;

    [Header("Match Rules")]
    [SerializeField] private int maxRounds = 3; // Maksimal ronde (Best of 3)
    private int roundCount = 0;
    public int playerScore = 0;
    public int enemyScore = 0;
    private bool isMatchOver = false;

    [Header("UI Status Displays")]
    [SerializeField] private TextMeshProUGUI roundText;       // Menampilkan "Round 1"
    [SerializeField] private TextMeshProUGUI countdownText;   // Menampilkan "READY", "SET", "GO!"
    [SerializeField] private CanvasGroup canvasGroupCountdownText;
    [SerializeField] private TextMeshProUGUI playerRoundScoreText; // Menampilkan skor player (Kiri)
    [SerializeField] private TextMeshProUGUI enemyRoundScoreText;  // Menampilkan skor musuh (Kanan)

    public static RoundManager instance;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // Memulai ronde pertama saat game baru dibuka
        StartCoroutine(StartNewRoundSequence());
    }

    // Dipanggil saat salah satu gasing kehabisan HP
    public void NewRound(string looseObj)
    {
        if (isMatchOver) return;

        // 1. Alokasi Skor & Reward berdasarkan tag pecundang (looseObj)
        if (looseObj == "Enemy")
        {
            playerScore++;
            playerObj.GetComponent<GasingStat>().ClaimRewards(5f, 3.5f);
            Debug.Log($"[ROUND] Player Menang Ronde! Skor saat ini: {playerScore} - {enemyScore}");
        }
        else if (looseObj == "Player")
        {
            enemyScore++;
            enemyObj.GetComponent<GasingStat>().ClaimRewards(5f, 3.5f);
            Debug.Log($"[ROUND] Enemy Menang Ronde! Skor saat ini: {playerScore} - {enemyScore}");
        }

        // Update teks skor di atas layar UI
        UpdateScoreUI();

        // 2. Cek apakah ada yang sudah mencapai target kemenangan (Misal: Dapat 2 poin di Best of 3)
        int targetWinPoints = Mathf.CeilToInt((float)maxRounds / 2f); // Hasilnya 2 kalau maxRounds = 3
        if (playerScore >= targetWinPoints || enemyScore >= targetWinPoints)
        {
            EndMatch();
            return;
        }

        // 3. Jika belum ada yang menang mutlak, lanjut ke ronde berikutnya dengan transisi aba-aba
        StartCoroutine(StartNewRoundSequence());
    }

    private IEnumerator StartNewRoundSequence()
    {
        roundCount++;

        // Update Teks Ronde Utama
        if (roundText != null) roundText.text = $"ROUND : {roundCount}";

        // 1. Teleportasi gasing ke titik spawn masing-masing & reset rotasi
        playerObj.transform.SetPositionAndRotation(spawnPlayerLoc.position, Quaternion.identity);
        enemyObj.transform.SetPositionAndRotation(spawnEnemyLoc.position, Quaternion.identity);

        // 2. Bekukan total pergerakan fisik (Velocity) agar diam di tempat saat aba-aba
        Rigidbody playerRb = playerObj.GetComponent<Rigidbody>();
        Rigidbody enemyRb = enemyObj.GetComponent<Rigidbody>();

        playerRb.linearVelocity = Vector3.zero;
        playerRb.angularVelocity = Vector3.zero;
        enemyRb.linearVelocity = Vector3.zero;
        enemyRb.angularVelocity = Vector3.zero;

        // 3. Reset Stat internal gasing (Darah, RPM, dll)
        playerObj.GetComponent<GasingStat>().ResetGasing();
        enemyObj.GetComponent<GasingStat>().ResetGasing();

        // 4. MATIKAN kontrol pergerakan sementara agar pemain/AI tidak bisa mencuri start
        GasingMovement playerMove = playerObj.GetComponent<GasingMovement>();
        GasingAI enemyAI = enemyObj.GetComponent<GasingAI>();

        if (playerMove != null) playerMove.enabled = false;
        if (enemyAI != null) enemyAI.enabled = false;

        // 5. SEKUENS TRANSISI ABA-ABA (COUNTDOWN)
        canvasGroupCountdownText.DOFade(1, 0.5f).SetUpdate(true);
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);

            countdownText.text = "READY";
            yield return new WaitForSeconds(1f);

            countdownText.text = "SET";
            yield return new WaitForSeconds(1f);

            countdownText.text = "GO!";
            yield return new WaitForSeconds(0.5f);
        }

        canvasGroupCountdownText.DOFade(0, 0.5f).SetUpdate(true);

        // 6. AKTIFKAN KEMBALI gasing untuk mulai bertarung
        if (playerMove != null) playerMove.enabled = true;
        if (enemyAI != null) enemyAI.enabled = true;

        // Sembunyikan teks countdown setelah beberapa saat gasing melesat
        if (countdownText != null)
        {
            yield return new WaitForSeconds(0.5f);
            countdownText.gameObject.SetActive(false);
        }
    }

    private void UpdateScoreUI()
    {
        if (playerRoundScoreText != null) playerRoundScoreText.text = playerScore.ToString();
        if (enemyRoundScoreText != null) enemyRoundScoreText.text = enemyScore.ToString();
    }

    private void EndMatch()
    {
        isMatchOver = true;
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = playerScore > enemyScore ? "YOU WIN!" : "YOU LOSE!";
        }

        if (playerObj.GetComponent<GasingMovement>() != null) playerObj.GetComponent<GasingMovement>().enabled = false;
        if (enemyObj.GetComponent<GasingMovement>() != null) enemyObj.GetComponent<GasingMovement>().enabled = false;
        if (enemyObj.GetComponent<GasingAI>() != null) enemyObj.GetComponent<GasingAI>().enabled = false;

        Debug.LogWarning("[MATCH OVER] Pertandingan selesai! Mengunci arena.");

        CalculateAndShowResult();
    }

    private void CalculateAndShowResult()
    {
        bool isWin = playerScore > enemyScore;
        float goldEarned = isWin ? 15f : 5f;
        float expEarned = isWin ? 25f : 10f;

        MatchResultUI resultUI = FindObjectOfType<MatchResultUI>();
        if (resultUI != null)
            resultUI.ShowResult(isWin, goldEarned, expEarned);
    }
}
