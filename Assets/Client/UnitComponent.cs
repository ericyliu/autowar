using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitComponent : MonoBehaviour
{
  public Unit unit;

  void Update()
  {
    Vector3 direction = Vector3.zero;
    if (this.unit.attacking) direction = VectorUtil.Vector2To3(this.unit.attackTarget.position - this.unit.position, 0);
    else
    {
      Vector3 lastPosition = this.transform.position;
      this.transform.position = VectorUtil.Vector2To3(this.unit.position, this.unit.height);
      direction = (this.transform.position - lastPosition).normalized;
    }
    if (direction != Vector3.zero) this.RotateToDirection(direction);
  }

  void RotateToDirection(Vector3 direction)
  {
    this.transform.forward = Vector3.RotateTowards(this.transform.forward, direction, 50f * Time.deltaTime, 0f);
  }
}