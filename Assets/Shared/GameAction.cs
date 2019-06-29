using System;

public enum GameActionType
{
  Null,
  BuyWorker,
  BuyUnit,
  Upgrade,
  Nuke,
}

public class GameAction
{
  public static int BYTE_ARRAY_SIZE = 4;

  public static GameAction CreateBuyWorkerAction(Player player)
  {
    return new GameAction(player, GameActionType.BuyWorker);
  }

  public static GameAction CreateBuyUnitAction(Player player, int slot, UnitType type)
  {
    var ga = new GameAction(player, GameActionType.BuyUnit);
    ga.slot = slot;
    ga.unitType = type;
    return ga;
  }

  public static GameAction CreateUpgradeAction(Player player)
  {
    return new GameAction(player, GameActionType.Upgrade);
  }

  public static GameAction CreateNukeAction(Player player)
  {
    return new GameAction(player, GameActionType.Nuke);
  }

  public Player player;
  public GameActionType type;
  public int slot;
  public UnitType unitType;

  public GameAction(Player player, GameActionType type)
  {
    this.player = player;
    this.type = type;
  }

  public GameAction(byte[] bytes, Game game)
  {
    this.player = game.players.Find(player => player.id == bytes[0]);
    this.type = (GameActionType)bytes[1];
    this.slot = bytes[2];
    this.unitType = (UnitType)bytes[3];
  }

  public byte[] ToByteArray()
  {
    var bytes = new byte[GameAction.BYTE_ARRAY_SIZE];
    bytes[0] = BitConverter.GetBytes(this.player.id)[0];
    bytes[1] = (byte)this.type;
    bytes[2] = BitConverter.GetBytes(this.slot)[0];
    bytes[3] = (byte)this.unitType;
    return bytes;
  }
}