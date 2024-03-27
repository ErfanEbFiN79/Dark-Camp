using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnPlayerNetwork : MonoBehaviourPunCallbacks
{
    #region Variables

    public static SpawnPlayerNetwork instance;

    [Header("Setting")]
    [SerializeField] private GameObject playerPr;
    [SerializeField] private Transform[] places;
    [SerializeField] private float respawnTime;
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
        if (PhotonNetwork.IsConnected)
        {
            SpawnPlayer();
        }
    }

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
        
        if (player != null)
        {
            StartCoroutine(DieCo());
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
        UiController.instance.DeathScreen.SetActive(true);
        yield return new WaitForSeconds(respawnTime);
        UiController.instance.DeathScreen.SetActive(false);
        SpawnPlayer();
    }
    
}
