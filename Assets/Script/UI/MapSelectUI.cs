using UnityEngine;
using UnityEngine.UI;
using TMPro;
using data.structs;

public class MapSelectUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text enemyCountText;
    public TMP_Text difficultyText;
    public TMP_Text mapNameText;

    private int _enemyCount = 1;
    private int _difficulty = 0; // 0: Normal, 1: Hard, 2: Expert
    private int _mapIndex = 0;

    private readonly int[] enemyCounts = { 1, 2, 4 };
    private readonly string[] difficulties = { "Normal", "Hard", "Expert" };
    private readonly string[] mapNames = { "Bedroom", "Kitchen", "Living Room" };

    private void Start()
    {
        LoadSettings();
        UpdateUI();
    }

    public void ChangeEnemyCount(int delta)
    {
        int index = System.Array.IndexOf(enemyCounts, _enemyCount);
        index = (index + delta + enemyCounts.Length) % enemyCounts.Length;
        _enemyCount = enemyCounts[index];
        UpdateUI();
    }

    public void ChangeDifficulty(int delta)
    {
        _difficulty = (_difficulty + delta + difficulties.Length) % difficulties.Length;
        UpdateUI();
    }

    public void ChangeMap(int delta)
    {
        _mapIndex = (_mapIndex + delta + mapNames.Length) % mapNames.Length;
        UpdateUI();
    }

    private void UpdateUI()
    {
        enemyCountText.text = _enemyCount.ToString();
        difficultyText.text = difficulties[_difficulty];
        mapNameText.text = mapNames[_mapIndex];
    }

    public void SaveAndNext()
    {
        GameState state = SaveSystem.Load() ?? new GameState();
        state.battleSettings.enemyCount = _enemyCount;
        state.battleSettings.difficultyLevel = _difficulty;
        state.battleSettings.mapIndex = _mapIndex;
        
        SaveSystem.Save(state);
        Debug.Log("Battle settings saved!");
        // Load next scene or trigger battle
    }

    private void LoadSettings()
    {
        GameState state = SaveSystem.Load();
        if (state != null)
        {
            _enemyCount = state.battleSettings.enemyCount;
            _difficulty = state.battleSettings.difficultyLevel;
            _mapIndex = state.battleSettings.mapIndex;
        }
    }
}
