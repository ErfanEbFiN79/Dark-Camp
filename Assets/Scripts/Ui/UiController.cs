using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UiController : MonoBehaviour
{
    public static UiController instance;

    [Header("Text")]
    [SerializeField] private TMP_Text gT1;
    [SerializeField] private TMP_Text gT2;

    [Header("Slider Hand1")]
    public Slider hpSlider;

    [Header("Dead")] 
    [SerializeField] private GameObject deathScreen;
    [SerializeField] private TMP_Text deathText;

    [Header("Info")] 
    [SerializeField] private TMP_Text killNumberText;
    [SerializeField] private TMP_Text deathNumberText;

    [Header("Leaderboard")] 
    [SerializeField] private GameObject leaderboard;
    public LeaderboardManager leaderboardPlayerDisplay;

    [Header("End Game")] 
    [SerializeField] private GameObject endGameScene;
    
    
    public TMP_Text GT1 => gT1;
    public TMP_Text GT2 => gT2;

    public GameObject DeathScreen
    {
        get => deathScreen;
        set => deathScreen = value;
    }

    public TMP_Text DeathText
    {
        get => deathText;
        set => deathText = value;
    }

    public TMP_Text KillNumberText
    {
        get => killNumberText;
        set => killNumberText = value;
    }

    public TMP_Text DeathNumberText
    {
        get => deathNumberText;
        set => deathNumberText = value;
    }

    public GameObject LeaderboardPlayerDisplay
    {
        get => leaderboard;
        set => leaderboard = value;
    }

    public GameObject Leaderboard
    {
        get => leaderboard;
        set => leaderboard = value;
    }

    public GameObject EndGameScene
    {
        get => endGameScene;
        set => endGameScene = value;
    }

    private void Awake()
    {
        instance = this;
    }
}
