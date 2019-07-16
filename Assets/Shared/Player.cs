using System.Collections.Generic;
using UnityEngine;

public class Player
{
  public static int UNIT_SLOTS = 4;
  public int id;
  public List<Vector2> spawns = new List<Vector2>();
  public UnitType[] unitsToSpawn = new UnitType[Player.UNIT_SLOTS];
  public Vector2 spawnTarget = new Vector2(15f, 13f);
  public Unit playerBase;
  public Game game;
  public Player enemy;
  public int gold = 100;
  public int workers = 1;
  public int nukes = 2;
  public int upgrade = 0;

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
    this.playerBase = game.spawner.SpawnUnit(this, baseLocation, UnitType.Base);
    this.spawns = spawns;
    this.spawnTarget = spawnTarget;
    this.unitsToSpawn[0] = UnitType.Soldier;
    this.unitsToSpawn[1] = UnitType.Soldier;
    this.unitsToSpawn[2] = UnitType.Sniper;
  }
}