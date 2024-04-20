using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameLauncher : MonoBehaviourPunCallbacks
{
    #region Variables

    public static GameLauncher instance;
    
    [Header("Base Setting")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private TMP_Text loadingText;
    
    [Header("Error Setting")]
    [SerializeField] private GameObject errorScene;
    [SerializeField] private TMP_Text errorText;
    
    [Header("Set Name")]
    [SerializeField] private GameObject nameInputScene;
    [SerializeField] private TMP_InputField nameInput;    
    private static bool hasSetNick;
    
    [Header("Create Room Setting")]  
    [SerializeField] private TMP_InputField nameCreateRoom;
    [SerializeField] private GameObject roomScene;
    [SerializeField] private TMP_Text roomNameText;
    public string[] levelToPlay;
    public bool changeMapBtwRounds;
    [SerializeField] private GameObject startButton;
    
    [Header("Find Rooms")]
    [SerializeField] private GameObject roomBrowserScene;
    [SerializeField] private RoomButton theRoomButton;
    private List<RoomButton> allRoomButtons = new List<RoomButton>();
    
    [Header("Manage Room")]
    [SerializeField] private TMP_Text playerNameLabel;
    private List<TMP_Text> allPlayerNames = new List<TMP_Text>();
    
    #endregion

    #region Unity Methods

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        LoadingManager(true, "Loading...");
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    #endregion

    #region Photon CallBacks

    public override void OnConnectedToMaster()
    {
        // allow the photon network to be able to tell us witch scene we should be going to
        PhotonNetwork.AutomaticallySyncScene = true;
        LoadingManager(true,"Joining lobby lobby");
        PhotonNetwork.JoinLobby();
    }
    
    public override void OnJoinedLobby()
    {
        LoadingManager(false,"");
        
        if (!hasSetNick) 
        {
            LoadingManager(false,"");
            nameInputScene.SetActive(true);

            if (PlayerPrefs.HasKey("playerName"))
            {
                nameInput.text = PlayerPrefs.GetString("playerName");
            }
        }
        else
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("playerName");
        }
    }

    public override void OnJoinedRoom()
    {
        LoadingManager(false,"");
        roomScene.SetActive(true);
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
        ManageAllPlayerList();
        if (!PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(false);
        }
    }
    
    public override void OnLeftRoom()
    {
        LoadingManager(false,"");
        roomScene.SetActive(false);
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        ErrorManager($"Failed to create room : {message}\nError code : {returnCode.ToString()}");
    }
    
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomButton rb in allRoomButtons)
        {
            Destroy(rb.gameObject);
        }
        allRoomButtons.Clear();

        theRoomButton.gameObject.SetActive(false);
        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].PlayerCount != roomList[i].MaxPlayers && !roomList[i].RemovedFromList)
            {
                RoomButton newButton = Instantiate(theRoomButton, theRoomButton.transform.parent);
                newButton.SetButtonDetails(roomList[i]);
                newButton.gameObject.SetActive(true);
                
                allRoomButtons.Add(newButton);
            }
        }
    }
    
    
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        ErrorManager($"Failed to join room : {message}\nError code : {returnCode.ToString()}");
    }
    
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        TMP_Text newPlayerLabel = Instantiate(playerNameLabel, playerNameLabel.transform.parent);
        newPlayerLabel.text = newPlayer.NickName;
        newPlayerLabel.gameObject.SetActive(true);
        allPlayerNames.Add(newPlayerLabel);
    }   

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ManageAllPlayerList();
    }
    #endregion
    
    #region Managers

    private void LoadingManager(bool active, string text)
    {
        loadingScreen.SetActive(active);
        if (active)
        {
            loadingText.text = text;
        }
    }
    
    private void ErrorManager(string error)
    {
        LoadingManager(false,"");
        errorScene.SetActive(true);
        errorText.text = error; 
    }
    
    private void ManageAllPlayerList()
    {
        foreach (TMP_Text player in allPlayerNames)
        {
            Destroy(player.gameObject);
        }
        allPlayerNames.Clear();
        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            TMP_Text newPlayerLabel = Instantiate(playerNameLabel, playerNameLabel.transform.parent);
            newPlayerLabel.text = players[i].NickName;
            newPlayerLabel.gameObject.SetActive(true);
            allPlayerNames.Add(newPlayerLabel);
        }
    }
    
    #endregion

    #region Buttons

    public void SetNickName() => SetNickNameAction();

    public void CreateRoom() => CreateRoomAction();

    public void LeaveRoom() => LeaveRoomAction();
    
    public void OpenFindRoom() => OpenFindRoomAction();
    
    public void JoinRoom(RoomInfo roomInfo) => JoinRoomAction(roomInfo);
    
    public void StartGame() => StartGameAction();

    public void ExitGame() => ExitGameAction();

    #endregion

    #region Actions

    private void SetNickNameAction()
    {
        if (!string.IsNullOrEmpty(nameInput.text))
        {
            PhotonNetwork.NickName = nameInput.text;
            PlayerPrefs.SetString("playerName", nameInput.text);
            print($"Player Name: {PhotonNetwork.NickName}");
            hasSetNick = true;
        }
    }

    private void CreateRoomAction()
    {
        if (!string.IsNullOrEmpty(nameCreateRoom.text))
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = 8;
            PhotonNetwork.CreateRoom(nameCreateRoom.text, roomOptions);
            LoadingManager(true,"Creating room" );
        }
    }

    private void LeaveRoomAction()
    {
        PhotonNetwork.LeaveRoom();
        LoadingManager(true,$"by {PhotonNetwork.NickName}");
    }
    
    private void JoinRoomAction(RoomInfo roomInfo)
    {
        roomBrowserScene.SetActive(false);
        LoadingManager(true,"Joining Room");
        PhotonNetwork.JoinRoom(roomInfo.Name);
    }

    private void OpenFindRoomAction()
    {
        roomBrowserScene.SetActive(true);
    }

    private void StartGameAction()
    {
        int level = Random.Range(0, levelToPlay.Length); 
        if (IsLevelLoaded(levelToPlay[level]))
        {
            PhotonNetwork.LoadLevel(levelToPlay[level]);
        }
        else
        {
            LoadingManager(true,"Jumping at the world");
            StartCoroutine(LoadLevelAsync(level));
        }
    }

    private void ExitGameAction()
    {
        Application.Quit();
    }

    #endregion

    #region Helper
    private bool IsLevelLoaded(string levelName)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene loadedScene = SceneManager.GetSceneAt(i);
            if (loadedScene.name == levelName)
            {
                return true;
            }
        }
        return false;
    }
    
    IEnumerator LoadLevelAsync(int level)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelToPlay[level]);
        
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        LoadingManager(false,"");
    }

    #endregion
}
