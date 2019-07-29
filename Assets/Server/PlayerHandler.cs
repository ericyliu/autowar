using System;
using System.Net.Sockets;
using System.Threading;

public class PlayerHandler
{
  public Socket socket;
  public Server server;
  public int id;
  public GameWebObject gameWebObject;
  public bool alive = true;
  public bool ingame = false;
  public bool isNew = true;

  public PlayerHandler(Socket socket, Server server)
  {
    this.socket = socket;
    this.server = server;
    new Thread(() => this.ListenForActions()).Start();
  }

  void ListenForActions()
  {
    try
    {
      while (true)
      {
        if (ingame) this.HandleGameAction();
        else this.HandleAppAction();
      }
    }
    catch (Exception e)
    {
      if (Server.DidPlayerDisconnect(this, e)) return;
      Console.WriteLine(e);
      this.socket.Close();
    }
  }

  public override string ToString()
  {
    return this.id + "(" + (this.alive ? "alive" : "dead") + ")";
  }

  void HandleGameAction()
  {
    byte[] bytes = new byte[GameAction.BYTE_ARRAY_SIZE];
    this.socket.Receive(bytes);
    this.gameWebObject.game.actions.Add(new GameAction(bytes, this.gameWebObject.game));
  }

  void HandleAppAction()
  {
    var bytes = new byte[AppAction.BYTE_ARRAY_SIZE];
    this.socket.Receive(bytes);
    var appAction = new AppAction(bytes);
    switch (appAction.type)
    {
      case AppActionType.SignIn:
        this.id = appAction.playerId;
        this.server.JoinGame(this);
        break;
    }
  }
}
