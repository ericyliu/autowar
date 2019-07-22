using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuyUnitButton
{
  public Button button;
  public UnitType type;

  public BuyUnitButton(Button button, UnitType type)
  {
    this.button = button;
    this.type = type;
  }
}

public class UI : MonoBehaviour
{

  public GameComponent gameComponent;
  public Client client;
  public GameObject cameraRig;
  public GameObject startMenu;
  public GameObject gameMenu;
  public Slider cameraSlider;
  public Slider lineOfWarSlider;
  public Text goldText;
  public Text upgradeText;
  public Button buyWorkerButton;
  public Button upgradeButton;
  public Button nukeButton;
  public Transform shopPanel;
  public Transform innerShopPanel;
  public GameObject buyUnitButtonPrefab;
  List<BuyUnitButton> buyUnitButtons = new List<BuyUnitButton>();
  public float cameraMaxX = 170f;

  public void ShowStartMenu()
  {
    this.gameMenu.SetActive(false);
    this.startMenu.SetActive(true);
  }

  public void JoinServer()
  {
    var idText = this.startMenu.GetComponentInChildren<InputField>().text;
    this.client.JoinServer(idText == "" ? 0 : int.Parse(idText));
  }

  public void ShowGameMenu()
  {
    this.gameMenu.SetActive(true);
    this.startMenu.SetActive(false);
  }

  public void LookAtBase(int id)
  {
    cameraSlider.value = id;
    this.OnSliderChange();
  }
  public void OnSliderChange()
  {
    cameraRig.transform.position = new Vector3(
      cameraSlider.value * this.cameraMaxX,
      cameraRig.transform.position.y,
      cameraRig.transform.position.z
    );
  }

  public void OpenUnitShopRow(int slot)
  {
    this.CloseShop();
    var row = this.innerShopPanel.GetChild(slot).Find("RowContainer");
    var buyableUnitList = row.Find("Units");
    UnitMeta.GetBuyableUnits(this.gameComponent.player.unitsToSpawn[slot]).ForEach(type =>
    {
      if (type == this.gameComponent.player.unitsToSpawn[slot]) return;
      var button = Instantiate(this.buyUnitButtonPrefab, buyableUnitList).GetComponent<Button>();
      button.onClick.AddListener(() => this.BuyUnit(slot, type));
      button.interactable = this.gameComponent.player.gold >= UnitMeta.GetBuyUnitCost(type);
      this.SetButtonText(button, type.ToString() + " (" + UnitMeta.GetBuyUnitCost(type) + ")");
      this.buyUnitButtons.Add(new BuyUnitButton(button, type));
    });
    row.gameObject.SetActive(true);
  }

  public void CloseShop()
  {
    for (int i = 0; i < this.innerShopPanel.childCount; i++)
    {
      var row = this.innerShopPanel
        .GetChild(i)
        .Find("RowContainer");
      row
        .gameObject
        .SetActive(false);
      var buyableUnitList = row.Find("Units");
      for (int j = 0; j < buyableUnitList.childCount; j++)
      {
        GameObject.Destroy(buyableUnitList.GetChild(j).gameObject);
      }
      this.buyUnitButtons.Clear();
    }
  }

  void Start()
  {
    cameraSlider.onValueChanged.AddListener(delegate { OnSliderChange(); });
    this.CloseShop();
  }

  void Update()
  {
    if (this.gameComponent.game != null)
    {
      this.UpdateMiscUI();
      this.UpdateShopUI();
      this.UpdateLineOfWarSlider();
    }
  }

  void UpdateMiscUI()
  {
    var player = this.gameComponent.player;
    this.goldText.text = player.gold + "g +" + (player.workers * 10) + "g/s";
    this.upgradeText.text = "Upgrade Level: " + player.upgrade;
    this.buyWorkerButton.interactable = player.gold >= Game.GetBuyWorkerCost(player);
    this.SetButtonText(this.buyWorkerButton, "Add Income (" + Game.GetBuyWorkerCost(player) + "g)");
    this.upgradeButton.interactable = player.gold >= Game.GetUpgradeCost(player);
    this.SetButtonText(this.upgradeButton, "Upgrade (" + Game.GetUpgradeCost(player) + "g)");
    if (player.nukes <= 0) nukeButton.gameObject.SetActive(false);
    this.SetButtonText(this.nukeButton, "Nuke (" + player.nukes + ")");
  }

  void UpdateShopUI()
  {
    var player = this.gameComponent.player;
    var unitsToSpawn = player.unitsToSpawn;
    for (int i = 0; i < Player.UNIT_SLOTS; i++)
    {
      Transform buttonTransform = this.shopPanel.GetChild(i);
      if (buttonTransform != null && unitsToSpawn[i] != UnitType.Null)
      {
        SetButtonText(buttonTransform.GetComponent<Button>(), unitsToSpawn[i].ToString());
        (buttonTransform as RectTransform).sizeDelta = new Vector2(160, 50);
      }
    }
    this.buyUnitButtons.ForEach(bub => bub.button.interactable = player.gold >= UnitMeta.GetBuyUnitCost(bub.type));
  }

  void BuyUnit(int slot, UnitType type)
  {
    this.CloseShop();
    gameComponent.BuyUnit(slot, type);
  }

  void UpdateLineOfWarSlider()
  {
    Unit furthestPlayer0 = null;
    Unit furthestPlayer1 = null;
    this.gameComponent.game.units.ForEach(unit =>
    {
      if (unit.player == null) return;
      if (unit.player.id == 0 &&
        (furthestPlayer0 == null ||
        furthestPlayer0.position.x < unit.position.x))
      {
        furthestPlayer0 = unit;
      }
      else if (unit.player.id == 1 &&
        (furthestPlayer1 == null ||
        furthestPlayer1.position.x > unit.position.x))
      {
        furthestPlayer1 = unit;
      }
    });
    if (furthestPlayer0 == null || furthestPlayer1 == null) return;
    this.lineOfWarSlider.value = (furthestPlayer0.position.x + furthestPlayer1.position.x) / 2f / (this.cameraMaxX + 30f);

  }

  void SetButtonText(Button button, string text)
  {
    button.gameObject.GetComponentInChildren<Text>().text = text;
  }
}
