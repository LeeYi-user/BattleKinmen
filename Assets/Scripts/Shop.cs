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

            shopItem.upgradeButton.onClick.AddListener(() => { UpgradeButtonClick(index, shopItem); });
            shopItem.levelSlider.onValueChanged.AddListener(delegate { LevelUpgraded(shopItem); });
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
            teamCash.Value += 50000;
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

    public void LevelUpgraded(ShopItem shopItem)
    {
        LevelUpgraded_ServerRpc(NetworkManager.LocalClient.PlayerObject.NetworkObjectId, shopItem.name, (int)shopItem.levelSlider.value);
    }

    [ServerRpc(RequireOwnership = false)]
    public void LevelUpgraded_ServerRpc(ulong objectId, string name, int level)
    {
        GameObject player = NetworkManager.SpawnManager.SpawnedObjects[objectId].gameObject;
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        PlayerGun playerGun = player.GetComponent<Player>().playerWeapon.transform.GetChild(0).GetComponent<PlayerGun>();
        PlayerKnife playerKnife = player.GetComponent<Player>().playerWeapon.transform.GetChild(1).GetComponent<PlayerKnife>();

        switch (name)
        {
            case "health": // 需要更改 maxHealth 和 currentHealth
                break;
            case "movingSpeed": // 需要更改 moveSpeed
                playerMovement.moveSpeed.Value += 5f / 4f;
                break;
            case "jumpHeight": // 需要更改 jumpForce
                playerMovement.jumpForce.Value += 2f / 4f;
                break;
            case "bulletproof": // 需要更改 bulletproof
                break;
            case "damage": // 需要更改 damage
                break;
            case "ammo": // 需要更改 maxAmmo
                playerGun.maxAmmo.Value += 2;
                break;
            case "attackSpeed": // 需要更改 fireRate 和 attackRate
                playerGun.fireRate.Value += 0.5f / 2.5f;
                playerKnife.attackRate.Value += 1.25f / 5f;
                break;
            case "reloadSpeed": // 需要更改 reloadTime
                playerGun.reloadTime.Value -= 0.1f;
                break;
            case "caber": // 需要額外做技能
                break;
            case "landmine": // 需要額外做技能
                break;
            case "heal": // 需要額外做技能
                break;
            case "nuke": // 需要額外做技能
                break;
            case "respawnSpeed": // 需要更改 respawnCooldown
                break;
            case "enemyDelay": // 需要更改 enemyDelay
                break;
            case "cashBonus": // 需要更改 cashBonus
                break;
            case "mapDefense": // 需要更改 maxDefense 和 currentDefense
                break;
            default:
                break;
        }
    }

    public void BackButtonClick()
    {
        Cursor.lockState = CursorLockMode.Locked;
        MainSceneManager.Instance.gamingScreen.SetActive(true);
        shopMenu.SetActive(false);
    }
}
