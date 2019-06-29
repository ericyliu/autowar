using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{

  public GameComponent gameComponent;
  public GameObject cameraRig;
  public Slider cameraSlider;
  public Text goldText;
  public Text upgradeText;
  public Button buyWorkerButton;
  public Button upgradeButton;
  public Button nukeButton;
  public Transform shopPanel;
  public float cameraMaxX = 170f;

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

  void Start()
  {
    cameraSlider.onValueChanged.AddListener(delegate { OnSliderChange(); });
  }

  void Update()
  {
    if (this.gameComponent.game != null)
    {
      this.UpdateMiscUI();
      this.UpdateShopUI();
    }
  }

  void UpdateMiscUI()
  {
    var player = this.gameComponent.player;
    this.goldText.text = player.gold + "g +" + (player.workers * 10) + "g/s";
    this.upgradeText.text = "Upgrade Level: " + player.upgrade;
    this.buyWorkerButton.interactable = player.gold >= Game.GetBuyWorkerCost(player);
    this.SetButtonText(this.buyWorkerButton, "Buy Worker (" + Game.GetBuyWorkerCost(player) + "g)");
    this.upgradeButton.interactable = player.gold >= Game.GetUpgradeCost(player);
    this.SetButtonText(this.upgradeButton, "Upgrade (" + Game.GetUpgradeCost(player) + "g)");
    if (player.nukes <= 0) nukeButton.gameObject.SetActive(false);
    this.SetButtonText(this.nukeButton, "Nuke (" + player.nukes + ")");
  }

  void UpdateShopUI()
  {
    var unitsToSpawn = this.gameComponent.player.unitsToSpawn;
    for (int i = 0; i < unitsToSpawn.Count; i++)
    {
      Transform buttonTransform = this.shopPanel.Find("Unit " + i);
      if (buttonTransform != null)
      {
        SetButtonText(buttonTransform.GetComponent<Button>(), unitsToSpawn[i].ToString());
        (buttonTransform as RectTransform).sizeDelta = new Vector2(160, 50);
      }
    }
  }

  void SetButtonText(Button button, string text)
  {
    button.gameObject.GetComponentInChildren<Text>().text = text;
  }
}
