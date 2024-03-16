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

    [HideInInspector] public List<NetworkVariable<int>> teamItems = new List<NetworkVariable<int>>();
    [HideInInspector] public NetworkVariable<int> respawnSpeedLevel = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [HideInInspector] public NetworkVariable<int> midSupplyLevel = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [HideInInspector] public NetworkVariable<int> ultCooldownLevel = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [HideInInspector] public NetworkVariable<int> mapDefenseLevel = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

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

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        teamItems.Add(respawnSpeedLevel);
        teamItems.Add(midSupplyLevel);
        teamItems.Add(ultCooldownLevel);
        teamItems.Add(mapDefenseLevel);
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
            shopItem.upgradeButton.onClick.AddListener(() => { UpgradeButtonClick(index, shopItem); });
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

        if (index >= 12)
        {
            UpgradeButtonClick_ServerRpc(index - 12);
        }

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

    [ServerRpc]
    public void UpgradeButtonClick_ServerRpc(int index)
    {
        teamItems[index].Value++;
    }

    public void BackButtonClick()
    {
        Cursor.lockState = CursorLockMode.Locked;
        MainSceneManager.Instance.gamingScreen.SetActive(true);
        shopMenu.SetActive(false);
    }
}
