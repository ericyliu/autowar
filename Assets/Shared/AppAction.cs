using System;

public enum AppActionType
{
  Null,
  SignIn,
  GameJoined
}

public class AppAction
{
  public static int BYTE_ARRAY_SIZE = 5;

  public static AppAction CreateReadyAction(int playerId)
  {
    var action = new AppAction(AppActionType.SignIn);
    action.playerId = playerId;
    return action;
  }

  public static AppAction CreateGameJoined(int playerSlot)
  {
    var action = new AppAction(AppActionType.GameJoined);
    action.playerSlot = playerSlot;
    return action;
  }

  public AppActionType type;
  public int playerId;
  public int playerSlot;


  public AppAction(AppActionType type)
  {
    this.type = type;
  }

  public AppAction(byte[] bytes)
  {
    this.type = (AppActionType)bytes[1];
    switch (this.type)
    {
      case AppActionType.SignIn:
        this.playerId = BitConverter.ToInt32(bytes, 1);
        break;
      case AppActionType.GameJoined:
        this.playerSlot = BitConverter.ToInt32(bytes, 1);
        break;
    }
  }

  public byte[] ToByteArray()
  {
    var bytes = new byte[AppAction.BYTE_ARRAY_SIZE];
    bytes[0] = (byte)this.type;
    switch (this.type)
    {
      case AppActionType.SignIn:
        BitConverter.GetBytes(this.playerId).CopyTo(bytes, 1);
        break;
      case AppActionType.GameJoined:
        BitConverter.GetBytes(this.playerSlot).CopyTo(bytes, 1);
        break;
    }
    return bytes;
  }
}