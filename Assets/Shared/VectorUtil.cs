using UnityEngine;

public class VectorUtil
{
  public static Vector3 Vector2To3(Vector2 vector2, float height = 0f)
  {
    return new Vector3(vector2.x, height, vector2.y);
  }

  public static Vector2 Vector3To2(Vector3 vector3)
  {
    return new Vector2(vector3.x, vector3.z);
  }
}