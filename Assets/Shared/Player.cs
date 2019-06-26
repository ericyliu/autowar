using System.Collections.Generic;
using UnityEngine;

public class Player
{
  public int id;
  public List<Vector2> spawns = new List<Vector2>();
  public Vector2 spawnTarget = new Vector2(15f, 13f);
  public Unit playerBase;
  public Game game;
  public Player enemy;

  public Player(
    int id,
    Game game,
    Vector2 baseLocation,
    List<Vector2> spawns,
    Vector2 spawnTarget
  )
  {
    this.id = id;
    this.game = game;
    this.playerBase = game.spawner.SpawnBase(this, baseLocation);
    this.spawns = spawns;
    this.spawnTarget = spawnTarget;
  }

}