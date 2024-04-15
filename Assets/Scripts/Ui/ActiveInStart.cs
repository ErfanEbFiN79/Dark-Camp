using UnityEngine;

public class ActiveInStart : MonoBehaviour
{
    private void Update()
    {
        if (MatchManager.instance._stateOfGame == MatchManager.StateOfGame.Playing && !gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }
    }
}