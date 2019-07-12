using UnityEngine;

public class LinkEffectComponent : MonoBehaviour
{
  public Transform target;
  public bool initialized = false;

  public void Initialize(Unit unit)
  {
    this.target = GameObject.Find(SpawnerComponent.GetUnitObjectName(unit)).transform;
    this.initialized = true;
  }

  void Update()
  {
    if (!this.initialized) return;
    if (
      this.target == null ||
      Vector3.Distance(this.transform.position, target.position) <= 1f
    )
    {
      Destroy(this.gameObject);
      return;
    }
    this.transform.position += (target.position - this.transform.position) * Time.deltaTime * 5f;
  }
}
