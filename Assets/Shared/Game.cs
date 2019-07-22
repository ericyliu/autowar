using System;
using System.Collections.Generic;
using UnityEngine;

public class Game
{
  public const int SPAWN_RATE = 250;
  public const int GOLD_RATE = 50;
  public const int WORKER_COST = 100;
  public const int UPGRADE_COST = 500;
  public const int MAX_UNIT_COUNT = 50;

  public static int GetBuyWorkerCost(Player player)
  {
    return Game.WORKER_COST * player.workers;
  }

  public static int GetUpgradeCost(Player player)
  {
    return Game.UPGRADE_COST * (player.upgrade + 1);
  }

  public List<Unit> units = new List<Unit>();
  public List<Projectile> projectiles = new List<Projectile>();
  public List<Player> players = new List<Player>();
  public List<GameStep> steps = new List<GameStep>();
  public List<GameAction> actions = new List<GameAction>();
  public Spawner spawner;
  public int turn = 0;

  public Game()
  {
    this.spawner = new Spawner(this);
    this.players.Add(new Player(
      0,
      this,
      new Vector2(6f, 12f),
      new List<Vector2>(){
        new Vector2(2f, 4f),
        new Vector2(2f, 20f),
      },
      new Vector2(16f, 12f)
    ));
    this.players.Add(new Player(
      1,
      this,
      new Vector2(194f, 12f),
      new List<Vector2>(){
        new Vector2(198f, 4f),
        new Vector2(198f, 20f),
      },
      new Vector2(184f, 12f)
    ));
    this.players[0].enemy = this.players[1];
    this.players[1].enemy = this.players[0];
  }

  public GameStep Step(GameStep step = null)
  {
    if (step != null && step.id != this.turn)
    {
      throw new Exception("Step count off\nawaiting:" + this.turn + ", incoming: " + step.id);
    }
    if (step == null)
    {
      step = new GameStep(this.turn, this.actions);
      this.steps.Add(step);
      this.actions = new List<GameAction>();
    }
    foreach (GameAction action in step.actions)
    {
      switch (action.type)
      {
        case GameActionType.BuyWorker:
          BuyWorker(action.player);
          break;
        case GameActionType.BuyUnit:
          BuyUnit(action);
          break;
        case GameActionType.Upgrade:
          Upgrade(action.player);
          break;
        case GameActionType.Nuke:
          Nuke(action.player);
          break;
      }
    }
    if (step.id % Game.SPAWN_RATE == 0) Spawn();
    if (step.id % Game.GOLD_RATE == 0) GiveGold();
    foreach (var projectile in this.projectiles) projectile.Act();
    foreach (var unit in this.units) unit.Act();
    this.CleanupUnits();
    this.CleanupProjectiles();
    this.turn++;
    return step;
  }

  public override string ToString()
  {
    return "Turn: " + this.turn;
  }

  void BuyWorker(Player player)
  {
    var cost = Game.GetBuyWorkerCost(player);
    if (player.gold < cost) return;
    player.gold -= cost;
    player.workers++;
  }

  void BuyUnit(GameAction action)
  {
    if (action.slot > Player.UNIT_SLOTS - 1) return;
    var player = action.player;
    var cost = UnitMeta.GetBuyUnitCost(action.unitType);
    if (player.gold < cost) return;
    player.gold -= cost;
    player.unitsToSpawn[action.slot] = action.unitType;
  }

  void Upgrade(Player player)
  {
    var cost = Game.GetUpgradeCost(player);
    if (player.gold < cost) return;
    player.gold -= cost;
    player.upgrade++;
  }

  void Nuke(Player player)
  {
    if (player.nukes <= 0) return;
    player.nukes--;
    this.units.ForEach(unit =>
    {
      if (unit.type != UnitType.Base) unit.health = 0;
      this.CleanupUnits();
    });
  }

  void GiveGold()
  {
    this.players.ForEach(player => player.gold += player.workers * 10);
  }

  void Spawn()
  {
    this.players.ForEach(player =>
    {
      if (this.units.FindAll(u => u.player.id == player.id).Count < Game.MAX_UNIT_COUNT)
      {
        player.spawns.ForEach(spawn =>
          Array.ForEach(player.unitsToSpawn, type =>
          {
            if (type != UnitType.Null)
            {
              Unit unit = this.spawner.SpawnUnit(player, spawn, type);
              unit.target = player.enemy.playerBase.position;
            }
          })
        );
      }
    });
  }

  void CleanupUnits()
  {
    var units = this.units;
    this.units = new List<Unit>();
    foreach (var unit in units) if (unit.health > 0) this.units.Add(unit);
  }

  void CleanupProjectiles()
  {
    var projectiles = this.projectiles;
    this.projectiles = new List<Projectile>();
    foreach (var projectile in projectiles) if (projectile.alive) this.projectiles.Add(projectile);
  }
}
