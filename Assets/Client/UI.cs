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
  public Text buyWorkerText;
  public Text upgradeButtonText;
  public GameObject nukeButton;
  public Text nukeText;
  public float cameraMaxX = 170f;
  // Start is called before the first frame update
  void Start()
  {
    cameraSlider.onValueChanged.AddListener(delegate { OnSliderChange(); });
  }

  void Update()
  {
    if (this.gameComponent.game != null)
    {
      var player = this.gameComponent.player;
      this.goldText.text = player.gold + "g +" + (player.workers * 10) + "g/s";
      this.upgradeText.text = "Upgrade Level: " + player.upgrade;
      this.buyWorkerText.text = "Buy Worker (" + Game.GetBuyWorkerCost(player) + "g)";
      this.upgradeButtonText.text = "Upgrade (" + Game.GetUpgradeCost(player) + "g)";
      this.nukeText.text = "Nuke (" + player.nukes + ")";
      if (player.nukes <= 0) nukeButton.SetActive(false);
    }
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
}
