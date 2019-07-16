using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class HealthBarComponent : MonoBehaviour
{
  #region PRIVATE_VARIABLES
  private float positionCorrection = 0f;
  #endregion
  #region PUBLIC_REFERENCES
  public RectTransform targetCanvas;
  public RectTransform healthBarContainer;
  public RectTransform healthBar;
  public Transform objectToFollow;
  #endregion
  #region PUBLIC_METHODS
  public void SetHealthBarData(Transform targetTransform, RectTransform healthBarPanel, float up)
  {
    this.targetCanvas = healthBarPanel;
    healthBarContainer = GetComponent<RectTransform>();
    objectToFollow = targetTransform;
    RepositionHealthBar();
    healthBar.gameObject.SetActive(true);
    this.positionCorrection = up;
  }
  public void OnHealthChanged(float healthFill)
  {
    if (healthFill == 1f)
    {
      this.transform.localScale = Vector3.zero;
      return;
    }
    this.transform.localScale = Vector3.one;
    float filled = healthFill * 14f;
    healthBar.sizeDelta = new Vector2(filled, 0f);
  }
  #endregion
  #region UNITY_CALLBACKS
  void Update()
  {
    RepositionHealthBar();
  }
  #endregion
  #region PRIVATE_METHODS
  private void RepositionHealthBar()
  {
    Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(objectToFollow.position);
    Vector2 WorldObject_ScreenPosition = new Vector2(
      ((ViewportPosition.x * targetCanvas.sizeDelta.x) - (targetCanvas.sizeDelta.x * 0.5f)),
      ((ViewportPosition.y * targetCanvas.sizeDelta.y) - (targetCanvas.sizeDelta.y * 0.5f)) + this.positionCorrection
    );
    //now you can set the position of the ui element
    healthBarContainer.anchoredPosition = WorldObject_ScreenPosition;
  }
  #endregion
}