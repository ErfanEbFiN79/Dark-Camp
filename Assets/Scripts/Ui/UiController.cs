using System;
using Photon.Pun;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UI;

public class UiController : MonoBehaviour
{
    public static UiController instance;

    #region Base Varibales

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

    [Header("Time")] 
    [SerializeField] private TMP_Text timeText;

    [Header("End Game")] 
    [SerializeField] private GameObject endGameScene;

    [Header("Option Menu")] 
    public GameObject optionScreen;

    #endregion

    #region Get Variables

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

    public TMP_Text TimeText
    {
        get => timeText;
        set => timeText = value;
    }

    #endregion

    #region Unity Methods

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            ShowHideOptionAction();
        }

        if (optionScreen.activeInHierarchy && Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    #endregion

    #region Buttons

    public void ShowHideOption() => ShowHideOptionAction();
    
    public void ReturnToMainMenu() => ReturnToMainMenuAction();

    public void QuitTheGame() => QuitTheGameAction();

    #endregion

    #region Action

    private void ShowHideOptionAction()
    {
        if (optionScreen.activeInHierarchy)
        {
            optionScreen.SetActive(false);
        }
        else
        {
            optionScreen.SetActive(true);
        }
    }
    
    private void ReturnToMainMenuAction()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
    }

    private void QuitTheGameAction()
    {
        Application.Quit();
    }

    #endregion

}
