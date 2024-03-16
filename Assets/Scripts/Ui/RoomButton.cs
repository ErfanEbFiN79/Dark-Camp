using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;

public class RoomButton : MonoBehaviour
{
    #region Variables

    // private
    private RoomInfo info;

    
    // Public
    [SerializeField] private TMP_Text buttonText;

    #endregion

    #region For Calls

    public void SetButtonDetails(RoomInfo inputInfo) => SetButtonDetailsAction(inputInfo);

    #endregion

    #region Button

    public void JoinRoom() => JoinRoomAction();

    #endregion

    #region Actions

    private void SetButtonDetailsAction(RoomInfo inputInfo)
    {
        info = inputInfo;
        buttonText.text = inputInfo.Name;
    }

    private void JoinRoomAction()
    {
        print(info);
        GameLauncher.instance.JoinRoom(info);
    }

    #endregion
}