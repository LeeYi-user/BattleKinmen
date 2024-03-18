using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class Shop : NetworkBehaviour
{
    public static Shop Instance;

    [HideInInspector] public NetworkVariable<int> teamCash = new NetworkVariable<int>(1000, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [HideInInspector] public ClientNetworkVariable<int> cashSpent = new ClientNetworkVariable<int>(0);
    [HideInInspector] public Dictionary<string, int> teamItems = new Dictionary<string, int>();

    public GameObject shopMenu;
    public GameObject[] categories;
    public GameObject[] areas;
    public ShopItem[] shopItems;
    public TextMeshProUGUI cashCounter;
    public Button backButton;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        int i = 0;

        foreach (GameObject category in categories)
        {
            int index = i;
            category.GetComponent<Button>().onClick.AddListener(() => { CategoryButtonClick(index); });
            i++;
        }

        int j = 0;

        foreach (ShopItem shopItem in shopItems)
        {
            int index = j;

            if (index >= 12)
            {
                teamItems[shopItem.name] = (int)shopItem.levelSlider.value;
            }

            shopItem.upgradeButton.onClick.AddListener(() => { UpgradeButtonClick(index, shopItem); });
            shopItem.levelSlider.onValueChanged.AddListener(delegate { LevelUpgraded(index, shopItem); });
            j++;
        }
    }

    private void Update()
    {
        if (MainSceneManager.Instance.start < 2 || MainSceneManager.Instance.gameover || MainSceneManager.disconnecting)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!shopMenu.activeSelf)
            {
                Cursor.lockState = CursorLockMode.None;
                MainSceneManager.Instance.gamingScreen.SetActive(false);
                shopMenu.SetActive(true);
            }
            else
            {
                BackButtonClick();
            }
        }

        cashCounter.text = "$ " + (teamCash.Value - cashSpent.Value).ToString();

        if (!IsHost)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            teamCash.Value = 100000;
        }
    }

    public void CategoryButtonClick(int index)
    {
        int i = 0;

        foreach (GameObject category in categories)
        {
            if (i == index)
            {
                category.GetComponent<Image>().color = new Color(166f / 255f, 112f / 255f, 78f / 255f, 1f);
            }
            else
            {
                category.GetComponent<Image>().color = new Color(166f / 255f, 112f / 255f, 78f / 255f, 0f);
            }

            i++;
        }

        int j = 0;

        foreach (GameObject area in areas)
        {
            if (j == index)
            {
                area.SetActive(true);
            }
            else
            {
                area.SetActive(false);
            }

            j++;
        }
    }

    public void UpgradeButtonClick(int index, ShopItem shopItem)
    {
        if (shopItem.levelSlider.value == shopItem.levelSlider.maxValue || shopItem.price > teamCash.Value - cashSpent.Value)
        {
            return;
        }

        cashSpent.Value += shopItem.price;
        shopItem.levelSlider.value++;

        if (shopItem.levelSlider.value < shopItem.levelSlider.maxValue)
        {
            if (index < 8)
            {
                shopItem.price += 200;
            }
            else
            {
                shopItem.price += 2000;
            }

            shopItem.priceText.text = shopItem.price.ToString() + " $";
        }
        else
        {
            shopItem.priceText.text = "MAX";
        }
    }

    public void LevelUpgraded(int index, ShopItem shopItem)
    {
        if (!IsHost)
        {
            NetworkManager.LocalClient.PlayerObject.GetComponent<Player>().playerItems[shopItem.name] = (int)shopItem.levelSlider.value;
        }
        
        LevelUpgraded_ServerRpc(NetworkManager.LocalClient.PlayerObject.NetworkObjectId, index, shopItem.name, (int)shopItem.levelSlider.value);
    }

    [ServerRpc(RequireOwnership = false)]
    public void LevelUpgraded_ServerRpc(ulong objectId, int index, string name, int level)
    {
        NetworkManager.SpawnManager.SpawnedObjects[objectId].GetComponent<Player>().playerItems[name] = level;

        if (index >= 12)
        {
            teamItems[name]++;
        }
    }

    public void BackButtonClick()
    {
        Cursor.lockState = CursorLockMode.Locked;
        MainSceneManager.Instance.gamingScreen.SetActive(true);
        shopMenu.SetActive(false);
    }
}
