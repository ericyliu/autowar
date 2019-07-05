using UnityEngine;

public class ProjectileComponent : MonoBehaviour
{
  public Projectile projectile;
  bool initialized = false;

  public void Initialize()
  {
    var forward = this.projectile.target.position - this.projectile.position;
    if (forward != Vector2.zero)
    {
      this.transform.forward = VectorUtil.Vector2To3(
        this.projectile.target.position - this.projectile.position,
        0f
      );
    }
    this.initialized = true;
  }

  void Update()
  {
    if (!this.initialized) return;
    this.transform.position = VectorUtil.Vector2To3(this.projectile.position, this.projectile.target.height);
  }
}