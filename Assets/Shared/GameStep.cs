using System;
using System.Collections.Generic;
using UnityEngine;

public class GameStep
{
  public static int BYTE_ARRAY_SIZE = 32;
  public int id = 0;
  public List<GameAction> actions = new List<GameAction>();

  public GameStep(int id, List<GameAction> actions)
  {
    this.id = id;
    this.actions = actions;
  }

  public GameStep(byte[] bytes, Game game)
  {
    string s = "";
    foreach (byte b in bytes) s += b + " ";
    this.id = BitConverter.ToInt32(bytes, 0);
    for (int i = 4; i < GameStep.BYTE_ARRAY_SIZE; i += GameAction.BYTE_ARRAY_SIZE)
    {
      var actionBytes = new byte[GameAction.BYTE_ARRAY_SIZE];
      for (int j = 0; j < GameAction.BYTE_ARRAY_SIZE; j++)
      {
        actionBytes[j] = bytes[i + j];
      }
      if (BitConverter.ToInt16(actionBytes, 0) == 0) break;
      this.actions.Add(new GameAction(actionBytes, game));
    }
  }

  public byte[] ToByteArray()
  {
    byte[] array = new byte[GameStep.BYTE_ARRAY_SIZE];
    BitConverter.GetBytes(this.id).CopyTo(array, 0);
    for (int i = 0; i < this.actions.Count; i++)
    {
      var payloadSize = GameAction.BYTE_ARRAY_SIZE;
      var payloadDestination = i * payloadSize + 4;
      if (payloadDestination + payloadSize >= GameStep.BYTE_ARRAY_SIZE) break;
      this.actions[i].ToByteArray().CopyTo(array, payloadDestination);
    }
    string s = "";
    foreach (byte b in array) s += b + " ";
    return array;
  }
}

public enum GameActionType
{
  Attack
}

public class GameAction
{
  public static int BYTE_ARRAY_SIZE = 2;
  public static GameAction CreateAttackAction(Player player)
  {
    return new GameAction(player, GameActionType.Attack);
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
    if (bytes[1] == 1) this.type = GameActionType.Attack;
    else throw new Exception("int to game action type fail, byte: " + bytes[1]);
  }

  public byte[] ToByteArray()
  {
    var bytes = new byte[2];
    bytes[0] = BitConverter.GetBytes(this.player.id)[0];
    if (this.type == GameActionType.Attack) bytes[1] = 1;
    return bytes;
  }

  public override string ToString()
  {
    var s = this.type.ToString() + " - ";
    if (this.type == GameActionType.Attack) s += "player: " + this.player.id;
    return s;
  }
}
