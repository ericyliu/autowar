using System;
using System.Collections.Generic;

public class GameStep
{
  public static int BYTE_ARRAY_SIZE = 24;
  public int id = 0;
  public List<GameAction> actions = new List<GameAction>();

  public static byte GameActionToByte(GameAction action)
  {
    byte b = 0;
    if (action == GameAction.Attack) b++;
    return b;
  }

  public static GameAction ByteToGameAction(byte b)
  {
    if (b == 1) return GameAction.Attack;
    throw new Exception("int to game action fail, byte: " + b);
  }

  public GameStep(int id, List<GameAction> actions)
  {
    this.id = id;
    this.actions = actions;
  }

  public GameStep(byte[] bytes)
  {
    this.id = BitConverter.ToInt32(bytes, 0);
    for (int i = 3; i < GameStep.BYTE_ARRAY_SIZE; i++)
    {
      if (bytes[i] == 1) this.actions.Add(GameAction.Attack);
    }
  }

  public byte[] ToByteArray()
  {
    byte[] array = new byte[GameStep.BYTE_ARRAY_SIZE];
    BitConverter.GetBytes(this.id).CopyTo(array, 0);
    for (int i = 0; i < actions.Count; i++)
    {
      byte actionByte = GameStep.GameActionToByte(actions[i]);
      array[i + 4] = actionByte;
    }
    return array;
  }
}

public enum GameAction
{
  Attack
}
