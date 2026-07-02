using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class TutorialPage
{
    public string title;
    [TextArea] public string content;
    public Sprite image;
}

public class TutorialManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text contentText;
    [SerializeField] private Image displayImage;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;

    [Header("Dummy Data")]
    public List<TutorialPage> tutorialPages = new List<TutorialPage>();

    private int _currentPage = 0;

    void Start()
    {
        // Setup dummy data
        if (tutorialPages.Count == 0)
        {
            tutorialPages.Add(new TutorialPage { title = "Welcome", content = "Ini adalah tutorial dasar game.", image = null });
            tutorialPages.Add(new TutorialPage { title = "Movement", content = "Gunakan WASD untuk bergerak.", image = null });
        }

        UpdateUI();
        nextButton.onClick.AddListener(NextPage);
        prevButton.onClick.AddListener(PrevPage);
    }

    public void OpenTutorial()
    {
        _currentPage = 0;
        tutorialPanel.SetActive(true);
        UpdateUI();
    }

    public void CloseTutorial() => tutorialPanel.SetActive(false);

    private void NextPage()
    {
        if (_currentPage < tutorialPages.Count - 1)
        {
            _currentPage++;
            UpdateUI();
        }
    }

    private void PrevPage()
    {
        if (_currentPage > 0)
        {
            _currentPage--;
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (tutorialPages.Count == 0) return;
        titleText.text = tutorialPages[_currentPage].title;
        contentText.text = tutorialPages[_currentPage].content;
        displayImage.sprite = tutorialPages[_currentPage].image;
        
        prevButton.interactable = _currentPage > 0;
        nextButton.interactable = _currentPage < tutorialPages.Count - 1;
    }
}
