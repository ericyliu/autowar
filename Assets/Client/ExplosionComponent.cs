using UnityEngine;

public class ExplosionComponent : MonoBehaviour
{
  public void Remove()
  {
    Destroy(this.gameObject);
  }
}
