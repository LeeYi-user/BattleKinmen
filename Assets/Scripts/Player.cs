using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class Player : NetworkBehaviour
{
    private Button startButton;
    private TextMeshProUGUI startButtonText;

    // Start is called before the first frame update
    void Start()
    {
        if (!IsOwner)
        {
            GetComponent<Player>().enabled = false;
            return;
        }

        startButton = GameObject.Find("Button").GetComponent<Button>();
        startButtonText = startButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        startButton.onClick.AddListener(StartButtonClick);

        if (!IsHost)
        {
            startButtonText.text = "<s>START</s>";
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void StartButtonClick()
    {
        if (!IsHost)
        {
            return;
        }

        StartGame_ClientRpc();
    }

    [ClientRpc]
    void StartGame_ClientRpc()
    {
        GameObject.Find("Panel").SetActive(false);
    }
}
