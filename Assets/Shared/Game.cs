using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Game
{
  public const int WORKER_COST = 100;
  public const int UPGRADE_COST = 500;

  public static int GetBuyWorkerCost(Player player)
  {
    return Game.WORKER_COST * player.workers;
  }

  public static int GetUpgradeCost(Player player)
  {
    return Game.UPGRADE_COST * (player.upgrade + 1);
  }

  public List<Unit> units = new List<Unit>();
  public List<Player> players = new List<Player>();
  public List<GameStep> steps = new List<GameStep>();
  public List<GameAction> actions = new List<GameAction>();
  public Spawner spawner;
  public int step = 0;

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
    if (step != null && step.id != this.step)
    {
      throw new Exception("Step count off\nawaiting:" + this.step + ", incoming: " + step.id);
    }
    if (step == null)
    {
      step = new GameStep(this.step, this.actions);
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
        case GameActionType.Upgrade:
          Upgrade(action.player);
          break;
        case GameActionType.Nuke:
          Nuke(action.player);
          break;
      }
    }
    if (step.id % 100 == 0) Spawn();
    if (step.id % 20 == 0) GiveGold();
    foreach (Unit unit in this.units) unit.Act();
    this.CleanupUnits();
    this.step++;
    return step;
  }

  void BuyWorker(Player player)
  {
    var cost = Game.GetBuyWorkerCost(player);
    if (player.gold < cost) return;
    player.gold -= cost;
    player.workers++;
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
      player.spawns.ForEach(spawn =>
        player.unitsToSpawn.ForEach(type =>
        {
          Unit unit = this.spawner.SpawnUnit(player, spawn, type);
          unit.target = player.enemy.playerBase.position;
        })
      )
    );
  }

  void CleanupUnits()
  {
    var units = this.units;
    this.units = new List<Unit>();
    foreach (Unit unit in units) if (unit.health > 0) this.units.Add(unit);
  }
}
