using System;
using System.Collections;
using System.Globalization;
using Photon.Pun;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnPlayerNetwork : MonoBehaviourPunCallbacks
{
    #region Variables

    public static SpawnPlayerNetwork instance;

    [Header("Setting")]
    [SerializeField] private GameObject playerPr;
    [SerializeField] private Transform[] places;
    [SerializeField] private TMP_Text textTimer;
    [SerializeField] private float respawnTime;
    [SerializeField] private float respawnTimeShow;
    private GameObject player;
    
    [Header("Effects")]
    [SerializeField] private GameObject effectDie;
    #endregion
    

    private void Awake()
    {
        instance = this;
        
    }

    private void Start()
    {
        respawnTimeShow = respawnTime;
        if (PhotonNetwork.IsConnected)
        {
            SpawnPlayer();
        }
    }

    private void Update()
    {
        if (UiController.instance.DeathScreen.activeInHierarchy)
        {
            textTimer.text = (respawnTimeShow -= Time.deltaTime).ToString("F1");
        }
        else
        {
            respawnTimeShow = respawnTime;
        }
    }


    public void SpawnP() => SpawnPlayer(); 
    
    private void SpawnPlayer()
    {
        player = PhotonNetwork.Instantiate(
            playerPr.name,
            places[Random.Range(0, places.Length)].position,
            places[Random.Range(0, places.Length)].rotation
        );
    }


    public void Die(string whoGiveDamage) => DieAction(whoGiveDamage);
    
    private void DieAction(string whoGiveDamage)
    {
        UiController.instance.DeathText.text = "You were killed by " + whoGiveDamage;
        
        MatchManager.instance.ChangeStatSend(PhotonNetwork.LocalPlayer.ActorNumber, 1,1);
        
        if (player != null)
        {
            if (MatchManager.instance._stateOfGame != MatchManager.StateOfGame.Ending)
            {
                StartCoroutine(DieCo());
            }

        }
    }

    public IEnumerator DieCo()
    {
        PhotonNetwork.Instantiate(
            effectDie.name,
            player.transform.position,
            Quaternion.identity
        );
        PhotonNetwork.Destroy(player);
        player = null;
        UiController.instance.DeathScreen.SetActive(true);
        yield return new WaitForSeconds(respawnTime);
        UiController.instance.DeathScreen.SetActive(false);

        if (MatchManager.instance._stateOfGame == MatchManager.StateOfGame.Playing && player == null)
        {
            SpawnPlayer();
        }

    }
    
}
