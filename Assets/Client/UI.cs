using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
  public GameObject cameraRig;
  public Slider cameraSlider;
  public float cameraMaxX = 170f;
  // Start is called before the first frame update
  void Start()
  {
    cameraSlider.onValueChanged.AddListener(delegate { OnSliderChange(); });
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
