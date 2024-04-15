using UnityEngine;

public class BugFixerShow1 : MonoBehaviour
{
    private void Update()
    {
        if (MatchManager.instance._stateOfGame == MatchManager.StateOfGame.Ending)
        {
            gameObject.SetActive(false);
        }
    }


}
