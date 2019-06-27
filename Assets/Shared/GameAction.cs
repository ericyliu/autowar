using System;

public enum GameActionType
{
  Null,
  BuyWorker,
  Upgrade,
  Nuke,
}

public class GameAction
{
  public static int BYTE_ARRAY_SIZE = 2;

  public static GameAction CreateBuyWorkerAction(Player player)
  {
    return new GameAction(player, GameActionType.BuyWorker);
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

  public GameAction(Player player, GameActionType type)
  {
    this.player = player;
    this.type = type;
  }

  public GameAction(byte[] bytes, Game game)
  {
    this.player = game.players.Find(player => player.id == bytes[0]);
    this.type = (GameActionType)bytes[1];
  }

  public byte[] ToByteArray()
  {
    var bytes = new byte[2];
    bytes[0] = BitConverter.GetBytes(this.player.id)[0];
    bytes[1] = (byte)this.type;
    return bytes;
  }
}