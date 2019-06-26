using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitType
{
  Base,
  Soldier
}

public class Unit
{
  public int id;
  public UnitType type;
  public Vector2 position;
  public Vector2 target;
  public float speed = 3f;
  public float size = 1.5f;
  public float height = 1f;
  public Game game;
  public Player player;

  public Unit(int id, UnitType type, Player player, Vector2 position)
  {
    this.id = id;
    this.type = type;
    this.player = player;
    this.position = position;
  }

  public void Act()
  {
    if (ShouldStop()) return;
    this.position = GetNextPosition(this.position);
  }

  Vector2 GetNextPosition(Vector2 position)
  {
    Vector2 move = (target - position).normalized;
    move = GetMoveWithCollision(move, position);
    return position + (move * this.speed / 10);
  }

  bool ShouldStop()
  {
    if (Vector2.Distance(this.position, this.target) <= this.size) return true;
    Vector2 futurePosition = this.position;
    for (int i = 0; i < 20; i++)
    {
      futurePosition = GetNextPosition(futurePosition);
    }
    if (Vector2.Distance(futurePosition, this.position) < this.size * 2f) return true;
    return false;
  }

  Vector3 GetMoveWithCollision(Vector2 move, Vector2 position)
  {
    // Dont Walk into Units
    List<Unit> collidedUnits = GetCollidedUnits();
    foreach (Unit unit in collidedUnits)
    {
      move = move + (position - unit.position).normalized;
    }
    // Go Left or Right
    if (collidedUnits.Count > 0)
    {
      if (Vector2.SignedAngle(move, this.target - position) < 0)
      {
        move = move + Vector2.Perpendicular(this.target - position).normalized;
      }
      else
      {
        move = move - Vector2.Perpendicular(this.target - position).normalized;
      }
    }
    return move.normalized;
  }

  List<Unit> GetCollidedUnits()
  {
    return this.game.units.FindAll(unit =>
      {
        if (unit == this) return false;
        return Vector2.Distance(this.position, unit.position) -
          (this.size / 2f) -
          (unit.size / 2f) < 0f;
      }
    );
  }
}
