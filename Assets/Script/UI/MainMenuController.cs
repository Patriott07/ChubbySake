using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject quitConfirmationPanel;

    [Header("Audio & Control Settings (Dummy)")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle controlToggle;

    [Header("Transition Settings")]
    [SerializeField] private CanvasGroup fadePanel;

    public void OpenTutorial() => tutorialPanel.SetActive(true);
    
    public void OpenSettings() => settingsPanel.SetActive(true);

    public void CloseSettings() => settingsPanel.SetActive(false);

    public void ShowQuitConfirmation() => quitConfirmationPanel.SetActive(true);

    public void ConfirmQuit() => Application.Quit();

    public void CancelQuit() => quitConfirmationPanel.SetActive(false);

    public void GoToCredit(string sceneName)
    {
        fadePanel.gameObject.SetActive(true);
        fadePanel.alpha = 0;
        fadePanel.DOFade(1, 1f).OnComplete(() => SceneManager.LoadScene(sceneName));
    }

    public void GoToStore()
    {
        // Ganti dengan nama scene Store Anda
        SceneManager.LoadScene("StoreScene");
    }

    public void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
    }
}
