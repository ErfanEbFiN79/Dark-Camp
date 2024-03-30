using UnityEngine;
using TMPro;

public class LeaderboardManager : MonoBehaviour
{
    #region Variables
    
    [Header("Setting")]
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text killText;
    [SerializeField] private TMP_Text deathText;


    #endregion

    #region Work

    public void SetDetails(string namePlayer, int kill, int death) => SetDetailsAction(
        namePlayer,
        kill,
        death
        );

    #endregion

    #region Action

    private void SetDetailsAction(string namePlayer, int kill, int death)
    {
        playerNameText.text = namePlayer;
        killText.text = kill.ToString();
        deathText.text = death.ToString(); 
    }

    #endregion

    
}
