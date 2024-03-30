using System;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;

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
    }

    
    public static MatchManager instance;

    public List<PlayerInfo> allPlayers = new List<PlayerInfo>();
    private int index;

    private EventCodes _eventCodes;
    
    private List<LeaderboardManager> lBoardPlayers = new List<LeaderboardManager>();

    #endregion

    #region Unity Methods

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
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
        object[] package = new object[allPlayers.Count];

        for (int i = 0; i < allPlayers.Count; i++)
        {
            object[] piece = new object[4];

            piece[0] = allPlayers[i].name;
            piece[1] = allPlayers[i].actor;
            piece[2] = allPlayers[i].kill;
            piece[3] = allPlayers[i].death;

            package[i] = piece;
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
        for (int i = 0; i < dataReceived.Length; i++)
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
                index = i;
            }
        }
        
        
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
                        Debug.Log($"Player {allPlayers[i].name} : Kills {allPlayers[i].kill}");
                        break;
                    
                    case 1:
                        allPlayers[i].death += amount;
                        Debug.Log($"Player {allPlayers[i].name} : Death {allPlayers[i].death}");
                        break;
                }

                if (i == index)
                {
                    UpdateStateDisplay();
                }
                
                break;
            }
        }
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

        foreach (LeaderboardManager lp in lBoardPlayers)
        {
            Destroy(lp.gameObject);
        }
        lBoardPlayers.Clear();
        
        UiController.instance.leaderboardPlayerDisplay.gameObject.SetActive(false);

        foreach (PlayerInfo player in allPlayers)
        {
            print("Work");
            LeaderboardManager newPlayerDisplay = Instantiate(
                UiController.instance.leaderboardPlayerDisplay,
                UiController.instance.leaderboardPlayerDisplay.transform.parent);
            
            newPlayerDisplay.SetDetails(player.name, player.kill, player.death);
            
            newPlayerDisplay.gameObject.SetActive(true);
            
            lBoardPlayers.Add(newPlayerDisplay);
            
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

    

