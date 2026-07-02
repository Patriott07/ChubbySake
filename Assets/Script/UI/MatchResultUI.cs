using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;
using data.structs;

public class MatchResultUI : MonoBehaviour
{
    [Header("Result Panel")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private CanvasGroup panelGroup;
    [SerializeField] private TextMeshProUGUI resultTitleText;
    [SerializeField] private TextMeshProUGUI resultSubText;

    [Header("Rewards Display")]
    [SerializeField] private TextMeshProUGUI goldRewardText;
    [SerializeField] private TextMeshProUGUI expRewardText;
    [SerializeField] private TextMeshProUGUI currentGoldText;
    [SerializeField] private TextMeshProUGUI currentExpText;
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Buttons")]
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button homeButton;
    [SerializeField] private Button storeButton;

    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "SampleScene";
    [SerializeField] private string homeSceneName = "MainMenu";
    [SerializeField] private string storeSceneName = "Store";

    [Header("Animation")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float entryDelay = 1.5f;

    void Awake()
    {
        if (resultPanel != null) resultPanel.SetActive(false);
    }

    void Start()
    {
        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(OnPlayAgain);
        if (homeButton != null)
            homeButton.onClick.AddListener(OnHome);
        if (storeButton != null)
            storeButton.onClick.AddListener(OnStore);
    }

    public void ShowResult(bool isWin, float goldEarned, float expEarned)
    {
        ProgressInGame progress = ProgressManager.Load();
        int oldLevel = progress.level;
        ProgressManager.ApplyMatchRewards(progress, goldEarned, expEarned);
        int newLevel = progress.level;

        if (resultPanel != null) resultPanel.SetActive(true);

        if (resultTitleText != null)
            resultTitleText.text = isWin ? "YOU WIN!" : "YOU LOSE!";

        if (resultSubText != null)
        {
            if (newLevel > oldLevel)
                resultSubText.text = $"<color=yellow>LEVEL UP! {oldLevel} → {newLevel}</color>";
            else
                resultSubText.text = isWin ? "Great battle!" : "Try again!";
        }

        if (goldRewardText != null)
            goldRewardText.text = $"+{goldEarned:F0}";
        if (expRewardText != null)
            expRewardText.text = $"+{expEarned:F0}";

        if (currentGoldText != null)
            currentGoldText.text = $"{progress.gold:F0}";
        if (currentExpText != null)
            currentExpText.text = $"{ProgressManager.ExpInCurrentLevel(progress.exp):F0} / {ProgressManager.ExpForNextLevel(progress.level):F0}";
        if (levelText != null)
            levelText.text = $"Level {progress.level}";

        if (panelGroup != null)
        {
            panelGroup.alpha = 0f;
            panelGroup.DOFade(1f, fadeDuration).SetUpdate(true).SetDelay(entryDelay);
        }
    }

    private void OnPlayAgain()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        SceneManager.LoadScene(gameSceneName);
    }

    private void OnHome()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        SceneManager.LoadScene(homeSceneName);
    }

    private void OnStore()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        SceneManager.LoadScene(storeSceneName);
    }
}
