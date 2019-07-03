using UnityEngine;

public class EffectComponent : MonoBehaviour
{
  public void Remove()
  {
    Destroy(this.gameObject);
  }
}
