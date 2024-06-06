using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class ShopManager : NetworkBehaviour
{
    public static ShopManager Instance;

    [SerializeField] private GameObject shopMenu;
    [SerializeField] private List<GameObject> categories;
    [SerializeField] private List<GameObject> areas;
    [SerializeField] private List<ShopItem> shopItems;
    [SerializeField] private TextMeshProUGUI cashCounter1;
    [SerializeField] private TextMeshProUGUI cashCounter2;
    [SerializeField] private Button backButton;

    private int cashSpent;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        Instance = null;

        if (!IsHost)
        {
            return;
        }

        NetworkManager.OnClientDisconnectCallback -= LevelReset;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsHost)
        {
            return;
        }

        NetworkManager.OnClientDisconnectCallback += LevelReset;
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

            shopItem.upgradeButton.onClick.AddListener(() => { UpgradeButtonClick(shopItem); });
            shopItem.levelSlider.onValueChanged.AddListener(delegate { LevelUpgraded(shopItem); });
            j++;
        }

        categories[2].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = MenuManager.classes[MenuManager.playerClass].Substring(0, 2);

        for (int k = 0; k < 3; k++)
        {
            if (k != MenuManager.playerClass)
            {
                areas.RemoveAt(k + 2 - (6 - areas.Count));
            }
        }

        if (GameManager.Instance.skillDisable)
        {
            categories[2].SetActive(false);
        }

        if (GameManager.Instance.teamDisable)
        {
            categories[3].SetActive(false);
        }
    }

    private void Update()
    {
        if (!GameManager.Instance.gamingScreen.activeSelf || !GameManager.Instance.gameStart || RelayManager.disconnecting)
        {
            if (shopMenu.activeSelf)
            {
                BackButtonClick();
            }

            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!shopMenu.activeSelf)
            {
                Cursor.lockState = CursorLockMode.None;
                shopMenu.SetActive(true);
            }
            else
            {
                BackButtonClick();
            }
        }

        cashCounter1.text = "$ " + (GameManager.Instance.teamCash.Value - cashSpent).ToString();
        cashCounter2.text = "$ " + (GameManager.Instance.teamCash.Value - cashSpent).ToString();
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

    public void UpgradeButtonClick(ShopItem shopItem)
    {
        if (shopItem.levelSlider.value == shopItem.levelSlider.maxValue || shopItem.price > GameManager.Instance.teamCash.Value - cashSpent)
        {
            return;
        }

        cashSpent += shopItem.price;
        shopItem.levelSlider.value++;

        if (shopItem.levelSlider.value < shopItem.levelSlider.maxValue)
        {
            shopItem.price += 200;
            shopItem.priceText.text = shopItem.price.ToString() + " $";
        }
        else
        {
            shopItem.priceText.text = "MAX";
        }
    }

    public void LevelUpgraded(ShopItem shopItem)
    {
        LevelUpgraded_ServerRpc(NetworkManager.LocalClientId, shopItem.name);
    }

    [ServerRpc(RequireOwnership = false)]
    public void LevelUpgraded_ServerRpc(ulong clientId, string name)
    {
        Player player = NetworkManager.ConnectedClients[clientId].PlayerObject.GetComponent<Player>();
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        PlayerGun playerGun = player.playerWeapon.transform.GetChild(0).GetComponent<PlayerGun>();
        PlayerKnife playerKnife = player.playerWeapon.transform.GetChild(1).GetComponent<PlayerKnife>();

        if (!LobbyManager.Instance.playersItems.ContainsKey(clientId))
        {
            LobbyManager.Instance.playersItems[clientId] = new List<string>();
        }

        switch (name)
        {
            case "health": // 需要更改 maxHealth 和 currentHealth
                player.maxHealth.Value += 25f;
                player.currentHealth.Value += 25f;
                break;
            case "movingSpeed": // 需要更改 moveSpeed
                playerMovement.moveSpeed.Value += 5f / 4f;
                break;
            case "jumpHeight": // 需要更改 jumpForce
                playerMovement.jumpForce.Value += 2f / 4f;
                break;
            case "bulletproof": // 需要更改 bulletproof
                player.bulletproof.Value += 0.1f;
                break;
            case "damage": // 需要更改 damage
                playerGun.damage.Value += 15f;
                playerKnife.damage.Value += 25f;
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
            case "grenadeCooldown": // 需要額外做技能
                player.playerWeapon.grenadeCooldown.Value -= 2f;
                break;
            case "grenadeRange": // 需要額外做技能
                player.playerWeapon.grenadeRange.Value += 0.25f;
                break;
            case "grenadeDamage": // 需要額外做技能
                player.playerWeapon.grenadeDamage.Value += 15f;
                break;
            case "grenadeDistance": // 需要額外做技能
                player.playerWeapon.grenadeDistance.Value += 100f;
                break;
            case "landmineCooldown": // 需要額外做技能
                player.playerWeapon.landmineCooldown.Value -= 2f;
                break;
            case "landmineRange": // 需要額外做技能
                player.playerWeapon.landmineRange.Value += 0.25f;
                break;
            case "landmineDamage": // 需要額外做技能
                player.playerWeapon.landmineDamage.Value += 15f;
                break;
            case "landmineLimit": // 需要額外做技能
                player.playerWeapon.landmineLimit.Value += 1f;
                break;
            case "healCooldown": // 需要額外做技能
                player.playerWeapon.healCooldown.Value -= 5f;
                break;
            case "healRange": // 需要額外做技能
                player.playerWeapon.healRange.Value += 1.5f;
                break;
            case "healAmount": // 需要額外做技能
                player.playerWeapon.healAmount.Value += 25f;
                break;
            case "healInv": // 需要額外做技能
                player.playerWeapon.healInv.Value += 0.5f;
                break;
            case "respawnSpeed": // 需要更改 respawnCooldown
                GameManager.Instance.Popup_ClientRpc("玩家生成時間 -" + (GameManager.Instance.respawnCooldown - GameManager.Instance.respawnCooldown * (1f / Mathf.Pow(2f, 1f / 3f))).ToString("0.00") + "s", Color.green);
                GameManager.Instance.respawnCooldown *= 1f / Mathf.Pow(2f, 1f / 3f);
                break;
            case "enemyDelay": // 需要更改 enemyDelay
                GameManager.Instance.Popup_ClientRpc("敵人生成速度 -10%", Color.green);
                GameManager.Instance.enemyDelay += 0.1f;
                break;
            case "cashBonus": // 需要更改 cashBonus
                GameManager.Instance.Popup_ClientRpc("資金收益 +10%", Color.green);
                GameManager.Instance.cashBonus += 0.1f;
                break;
            case "mapDefense": // 需要更改 maxDefense 和 currentDefense
                GameManager.Instance.currentDefense += 1;

                if (GameManager.Instance.maxDefense < GameManager.Instance.currentDefense)
                {
                    GameManager.Instance.maxDefense = GameManager.Instance.currentDefense;
                }

                GameManager.Instance.Popup_ClientRpc("防禦提升! (" + GameManager.Instance.currentDefense.ToString() + " / " + GameManager.Instance.maxDefense.ToString() + ")", Color.green);

                break;
            default:
                break;
        }

        LobbyManager.Instance.playersItems[clientId].Add(name);
    }

    public void LevelReset(ulong clientId)
    {
        if (!LobbyManager.Instance.playersItems.ContainsKey(clientId))
        {
            return;
        }

        foreach (string name in LobbyManager.Instance.playersItems[clientId])
        {
            switch (name)
            {
                case "respawnSpeed": // 需要更改 respawnCooldown
                    GameManager.Instance.Popup_ClientRpc("玩家生成時間 +" + (GameManager.Instance.respawnCooldown / (1f / Mathf.Pow(2f, 1f / 3f)) - GameManager.Instance.respawnCooldown).ToString("0.00") + "s", Color.red);
                    GameManager.Instance.respawnCooldown /= 1f / Mathf.Pow(2f, 1f / 3f);
                    break;
                case "enemyDelay": // 需要更改 enemyDelay
                    GameManager.Instance.Popup_ClientRpc("敵人生成速度 +10%", Color.red);
                    GameManager.Instance.enemyDelay -= 0.1f;
                    break;
                case "cashBonus": // 需要更改 cashBonus
                    GameManager.Instance.Popup_ClientRpc("資金收益 -10%", Color.red);
                    GameManager.Instance.cashBonus -= 0.1f;
                    break;
                case "mapDefense": // 需要更改 maxDefense 和 currentDefense
                    if (GameManager.Instance.currentDefense <= 1)
                    {
                        return;
                    }

                    GameManager.Instance.currentDefense -= 1;
                    GameManager.Instance.Popup_ClientRpc("防禦降低! (" + GameManager.Instance.currentDefense.ToString() + " / " + GameManager.Instance.maxDefense.ToString() + ")", Color.red);
                    break;
                default:
                    break;
            }
        }

        LobbyManager.Instance.playersItems.Remove(clientId);
    }

    public void BackButtonClick()
    {
        if (!GameManager.Instance.pauseScreen.activeSelf)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        shopMenu.SetActive(false);
    }
}
