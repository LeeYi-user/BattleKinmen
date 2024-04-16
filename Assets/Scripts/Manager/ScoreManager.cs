using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ScoreManager : NetworkBehaviour
{
    [SerializeField] private GameObject scoreUIPrefab;
    [SerializeField] private GameObject scoreboardScreen;
    [SerializeField] private GameObject scoreboardContainer;

    private Dictionary<ulong, ScoreUI> scoreboard = new Dictionary<ulong, ScoreUI>();

    private void Update()
    {
        if (Cursor.lockState == CursorLockMode.None || !GameManager.Instance.gamingScreen.activeSelf || !GameManager.Instance.gameStart || RelayManager.disconnecting)
        {
            scoreboardScreen.SetActive(false);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            scoreboardScreen.SetActive(true);
        }

        if (Input.GetKeyUp(KeyCode.Tab))
        {
            scoreboardScreen.SetActive(false);
        }

        ListPlayers();
    }

    private void ListPlayers()
    {
        foreach (Player player in LobbyManager.Instance.players.Values)
        {
            bool flag = false;

            foreach (Transform child in scoreboardContainer.transform)
            {
                if (player.NetworkObjectId == child.GetComponent<ScoreUI>().id)
                {
                    flag = true;
                    child.GetComponent<ScoreUI>().nameText.text = player.playerName.name.Value.ToString();
                    child.GetComponent<ScoreUI>().classText.text = MenuManager.classes[player.playerClass.Value];
                    child.GetComponent<ScoreUI>().scoreText.text = player.playerScore.Value.ToString();
                    break;
                }
            }

            if (!flag)
            {
                GameObject scoreUI = Instantiate(scoreUIPrefab, scoreboardContainer.transform.position, Quaternion.identity);
                scoreUI.transform.SetParent(scoreboardContainer.transform, false);
                scoreUI.GetComponent<ScoreUI>().id = player.NetworkObjectId;
                scoreUI.GetComponent<ScoreUI>().nameText.text = player.playerName.name.Value.ToString();
                scoreUI.GetComponent<ScoreUI>().classText.text = MenuManager.classes[player.playerClass.Value];
                scoreUI.GetComponent<ScoreUI>().scoreText.text = player.playerScore.Value.ToString();
            }
        }

        foreach (Transform child in scoreboardContainer.transform)
        {
            bool flag = false;

            foreach (Player player in LobbyManager.Instance.players.Values)
            {
                if (child.GetComponent<ScoreUI>().id == player.NetworkObjectId)
                {
                    flag = true;
                    break;
                }
            }

            if (!flag)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
