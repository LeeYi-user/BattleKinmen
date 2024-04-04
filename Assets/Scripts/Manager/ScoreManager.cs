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

    private void Start()
    {
        StartCoroutine(ListPlayers());
    }

    private void Update()
    {
        if (Cursor.lockState == CursorLockMode.None || !GameManager.Instance.gamingScreen.activeSelf || GameManager.Instance.gameStart < 2 || RelayManager.disconnecting)
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

        foreach (KeyValuePair<ulong, ScoreUI> entry in scoreboard)
        {
            if (GameManager.Instance.players.ContainsKey(entry.Key))
            {
                Player player = GameManager.Instance.players[entry.Key].GetComponent<Player>();
                scoreboard[entry.Key].nameText.text = player.playerName.name.Value.ToString();
                scoreboard[entry.Key].classText.text = MenuManager.classes[player.playerClass.Value];
                scoreboard[entry.Key].scoreText.text = player.playerScore.Value.ToString();
            }
            else
            {
                Destroy(scoreboard[entry.Key]);
                scoreboard.Remove(entry.Key);
            }
        }
    }

    private IEnumerator ListPlayers()
    {
        yield return new WaitUntil(() => GameManager.Instance.gameStart > 0);

        foreach (Player player in GameManager.Instance.players.Values)
        {
            GameObject scoreUI = Instantiate(scoreUIPrefab, scoreboardContainer.transform.position, Quaternion.identity);
            scoreUI.transform.SetParent(scoreboardContainer.transform, false);
            scoreUI.GetComponent<ScoreUI>().id = player.NetworkObjectId;
            scoreUI.GetComponent<ScoreUI>().nameText.text = player.playerName.name.Value.ToString();
            scoreUI.GetComponent<ScoreUI>().classText.text = MenuManager.classes[player.playerClass.Value];
            scoreUI.GetComponent<ScoreUI>().scoreText.text = player.playerScore.Value.ToString();
            scoreboard[player.NetworkObjectId] = scoreUI.GetComponent<ScoreUI>();
        }
    }
}
