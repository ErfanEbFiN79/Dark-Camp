using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Random = UnityEngine.Random;

public class MatchManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    
    // keep track data of match and player 
    // Ex: number of kill and when match end , who win and more
    
    #region Variables


    public enum EventCodes : byte
    {
        // what kind of event we want to send
        NewPlayer,
        ListPlayers,
        ChangeStat,
        NextMatch,
    }

    
    public static MatchManager instance;

    public List<PlayerInfo> allPlayers = new List<PlayerInfo>();
    private int index;

    private EventCodes _eventCodes;
    
    private List<LeaderboardManager> lBoardPlayers = new List<LeaderboardManager>();

    public enum StateOfGame
    {
        Waiting,
        Playing,
        Ending
    }
    
    [SerializeField] private float killsNeedToEnd;
    [SerializeField] private float waitAfterEnding;

    public StateOfGame _stateOfGame { get; private set; }
    public Transform mapCamPoint;
    
    
    // continue match variables
    [SerializeField] private bool _perpetual;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && _stateOfGame != StateOfGame.Ending)
        {
            if (UiController.instance.Leaderboard.activeInHierarchy)
            {
                UiController.instance.Leaderboard.SetActive(false);
            }
            else
            {
                ShowLeaderboard();
            }
            
        }

    }

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(0); 
        }
        else
        {
            NewPlayerSend(PhotonNetwork.NickName);

            _stateOfGame = StateOfGame.Playing;
        }
    }

    #endregion
    

    #region Event Handeler

    public void OnEvent(EventData photonEvent)
    {
        // when event send by other clients -> read the event
        if (photonEvent.Code < 200)
        {
            EventCodes theEvent = (EventCodes)photonEvent.Code;
            object[] data = (object[])photonEvent.CustomData;

            // Debug.Log("Received event : " + theEvent);
            
            switch (theEvent)
            {
                case EventCodes.NewPlayer:
                    NewPlayerReceive(data);
                    break;
                
                case EventCodes.ListPlayers:
                    ListPlayerReceive(data);
                    break;
                
                case EventCodes.ChangeStat:
                    ChangeStateReceive(data);
                    break;
                
                case EventCodes.NextMatch:
                    
                    NextMatchReceive();
                    
                    break;
            }
        }
    }

    public override void OnEnable()
    {
        // when object or scripts enable this function run
        
        // add us to the list
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        // when object or scripts disable this function run
        
        // delete from the list
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    #endregion

    #region Manage event

    public void NewPlayerSend(string username) => NewPlayerSendAction(username);

    public void NewPlayerReceive(object[] dataReceived) => NewPlayerReceiveAction(dataReceived);

    public void ListPlayersSend() => ListPlayersSendAction();

    public void ListPlayerReceive(object[] dataReceived) => ListPlayersReceiveAction(dataReceived);

    public void ChangeStatSend(int actorSending, int statToUpdate, int amountToChange) => ChangeStatSendAction(
        actorSending,
        statToUpdate,
        amountToChange
        );

    public void ChangeStateReceive(object[] dataReceived) => ChangeStateReceiveAction(dataReceived);

    public void NextMatchSend() => NextMatchSendAction();

    public void NextMatchReceive() => NextMatchReceiveAction();
    
    #endregion

    #region Manage event action

    private void NewPlayerSendAction(string username)
    {
        object[] package = new object[4];
        package[0] = username;
        package[1] = PhotonNetwork.LocalPlayer.ActorNumber;
        package[2] = 0;
        package[3] = 0;

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.NewPlayer,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true }
        );
    }   

    private void NewPlayerReceiveAction(object[] dataReceived)
    {
        PlayerInfo player = new PlayerInfo(
            (string)dataReceived[0],
            (int)dataReceived[1],
            (int)dataReceived[2], 
            (int)dataReceived[3]
            );
        
        allPlayers.Add(player);
        
        ListPlayersSend();
    }

    private void ListPlayersSendAction()
    {
        object[] package = new object[allPlayers.Count + 1];

        package[0] = _stateOfGame; 

        for (int i = 0; i < allPlayers.Count; i++)
        {
            object[] piece = new object[4];

            piece[0] = allPlayers[i].name;
            piece[1] = allPlayers[i].actor;
            piece[2] = allPlayers[i].kill;
            piece[3] = allPlayers[i].death;

            package[i + 1] = piece;
        }
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.ListPlayers,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
        );
    }
    
    private void ListPlayersReceiveAction(object[] dataReceived)
    {
        allPlayers.Clear();

        _stateOfGame = (StateOfGame)dataReceived[0];
        
        for (int i = 1; i < dataReceived.Length; i++)
        {
            object[] piece = (object[])dataReceived[i];

            PlayerInfo player = new PlayerInfo(
                (string)piece[0],
                (int)piece[1],
                (int)piece[2],
                (int)piece[3]
            );
            
            allPlayers.Add(player);

            if (PhotonNetwork.LocalPlayer.ActorNumber == player.actor)
            {
                index = i - 1;
            }
        }
        
        StateCheck();
    }

    private void ChangeStatSendAction(int actorSending, int statToUpdate, int amountToChange)
    {
        object[] package = new object[] { actorSending, statToUpdate, amountToChange };
        
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.ChangeStat,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
        );
    }

    private void ChangeStateReceiveAction(object[] dataReceived)
    {
        int actor = (int)dataReceived[0];
        int statType = (int)dataReceived[1];
        int amount = (int)dataReceived[2];

        for (int i = 0; i < allPlayers.Count; i++)
        {
            if (allPlayers[i].actor == actor)
            {
                switch (statType)
                {
                    case 0:
                        allPlayers[i].kill += amount;
                        break;
                    
                    case 1:
                        allPlayers[i].death += amount;
                        break;
                }

                if (i == index)
                {
                    UpdateStateDisplay();
                }

                if (UiController.instance.Leaderboard.activeInHierarchy)
                {
                    ShowLeaderboard();
                }
                
                break;
            }
        }
        
        ScoreCheck();
    }

    private void NextMatchSendAction()
    {
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.NextMatch,
            null, // we don't need send data when we start a new match 
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
        );
    }

    private void NextMatchReceiveAction()
    {
        _stateOfGame = StateOfGame.Playing;
        UiController.instance.EndGameScene.SetActive(false);
        UiController.instance.Leaderboard.SetActive(false);
        foreach (PlayerInfo player in allPlayers)
        {
            player.kill = 0;
            player.death = 0;
        }
        UpdateStateDisplay();
        
        SpawnPlayerNetwork.instance.SpawnP();
    }

    #endregion

    #region Ui

    private void UpdateStateDisplay()
    {
        if (allPlayers.Count > index)
        {
            UiController.instance.KillNumberText.text = allPlayers[index].kill.ToString();
            UiController.instance.DeathNumberText.text = allPlayers[index].death.ToString();
            
        }
        else
        {
            UiController.instance.KillNumberText.text = "0";
            UiController.instance.DeathNumberText.text = "0";
        }

    }

    private void ShowLeaderboard()
    {
        UiController.instance.Leaderboard.SetActive(true);

        foreach (var lp in lBoardPlayers)
        {
            Destroy(lp.gameObject);
        }
        lBoardPlayers.Clear();
        
        UiController.instance.leaderboardPlayerDisplay.gameObject.SetActive(false);

        var sorted = SortPlayers(allPlayers);
        foreach (PlayerInfo player in sorted)
        {
            LeaderboardManager newPlayerDisplay = Instantiate(
                UiController.instance.leaderboardPlayerDisplay,
                UiController.instance.leaderboardPlayerDisplay.transform.parent);
            
            newPlayerDisplay.SetDetails(player.name, player.kill, player.death);
            
            newPlayerDisplay.gameObject.SetActive(true);
            
            lBoardPlayers.Add(newPlayerDisplay);
            
        }
    }
    
    #endregion

    #region Helper

    private List<PlayerInfo> SortPlayers(List<PlayerInfo> players)
    {
        List<PlayerInfo> sortList = new List<PlayerInfo>();

        while (sortList.Count < players.Count)
        {
            int high = -1;
            PlayerInfo playerS = players[0];
            foreach (var info in players.Where(info => !sortList.Contains(info)).Where(info => info.kill > high))
            {
                playerS = info;
                high = info.kill;
            }
            
            sortList.Add(playerS);
        }
        
        return sortList;
    }
    
    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        // we use this be after end game all the player leave the game
        SceneManager.LoadScene(0);
    }

    private void ScoreCheck()
    {
        bool winnerFound = false;

        foreach (PlayerInfo player in allPlayers)
        {
            if (player.kill >= killsNeedToEnd && killsNeedToEnd > 0)
            {
                winnerFound = true;
                break;
            }
        }

        if (winnerFound)
        {
            if (PhotonNetwork.IsMasterClient && _stateOfGame != StateOfGame.Ending)
            {
                _stateOfGame = StateOfGame.Ending;
                ListPlayersSend();
            }
        }
    }

    private void StateCheck()
    {
        if (_stateOfGame == StateOfGame.Ending)
        {
            EndGame();
        }
    }

    private void EndGame()
    {
        _stateOfGame = StateOfGame.Ending;

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.DestroyAll();
        }
        
        UiController.instance.DeathScreen.SetActive(false);
        UiController.instance.EndGameScene.SetActive(true);
        ShowLeaderboard();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Camera.main.transform.position = mapCamPoint.position;
        Camera.main.transform.rotation = mapCamPoint.rotation;

        StartCoroutine(EndCo());
    }

    private IEnumerator EndCo()
    {
        UiController.instance.DeathScreen.SetActive(false);
        yield return new WaitForSeconds(waitAfterEnding);

        if (!_perpetual)
        {
            PhotonNetwork.AutomaticallySyncScene = false;
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (!GameLauncher.instance.changeMapBtwRounds)
                {
                    NextMatchSend();
                }
                else
                {
                    int newLevel = Random.Range(0, GameLauncher.instance.levelToPlay.Length);

                    if (GameLauncher.instance.levelToPlay[newLevel] == SceneManager.GetActiveScene().name)
                    {
                        NextMatchSend();
                    }
                    else
                    {
                        PhotonNetwork.LoadLevel(GameLauncher.instance.levelToPlay[newLevel]);
                    }
                }
    
            }
        }

    }

    #endregion
    
    
    
}

#region New Class


[System.Serializable]
public class PlayerInfo
{
    public string name;
    public int actor, kill, death;

    // constructor

    public PlayerInfo(string _name, int _actor, int _kill, int _death)
    {
        name = _name;
        actor = _actor;
        kill = _kill;
        death = _death;
    }
}

#endregion

    

