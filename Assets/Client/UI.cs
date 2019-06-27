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
      this.goldText.text = "(G) " + this.gameComponent.player.gold;
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
