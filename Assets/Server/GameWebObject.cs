using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

public class GameWebObject
{
  public int id;
  public Game game = new Game();
  public List<PlayerHandler> playerHandlers = new List<PlayerHandler>();
  public Thread thread;

  public GameWebObject(int id)
  {
    Console.WriteLine("Creating game");
    this.id = id;
  }

  void Start()
  {
    Console.WriteLine("Starting game");
    for (int i = 0; i < this.playerHandlers.Count; i++)
    {
      this.playerHandlers[i].socket.Send(AppAction.CreateGameJoined(i).ToByteArray());
      this.playerHandlers[i].ingame = true;
    }
    this.thread = new Thread(this.PublishSteps);
    this.thread.Start();
  }


  void ListenForActions(PlayerHandler playerHandler)
  {
    try
    {
      while (true)
      {
        byte[] bytes = new byte[GameAction.BYTE_ARRAY_SIZE];
        playerHandler.socket.Receive(bytes);
        this.game.actions.Add(new GameAction(bytes, this.game));
      }
    }
    catch (Exception e)
    {
      if (Server.DidPlayerDisconnect(playerHandler, e)) return;
      Console.WriteLine(e);
      playerHandler.socket.Close();
    }
  }

  void PublishSteps()
  {
    try
    {
      while (true)
      {
        var alivePlayerHandlers = this.playerHandlers.FindAll(p => p.alive);
        if (alivePlayerHandlers.Count == 0) break;
        GameStep nextStep = this.game.Step();
        alivePlayerHandlers.ForEach(playerHandler =>
          PublishToPlayer(playerHandler, nextStep)
        );
        Thread.Sleep(Server.STEP_TIME);
      }
      Console.WriteLine("Closing game");
    }
    catch (Exception e)
    {
      Console.WriteLine(e);
      Console.WriteLine("Closing game");
      if (Server.DidThreadAbort(e)) return;
    }
  }

  void PublishToPlayer(PlayerHandler playerHandler, GameStep nextStep)
  {
    try
    {
      if (playerHandler.isNew)
      {
        this.game.steps.ForEach(step =>
          playerHandler.socket.Send(step.ToByteArray())
        );
        playerHandler.isNew = false;
      }
      else
      {
        playerHandler.socket.Send(nextStep.ToByteArray());
      }
    }
    catch (Exception e)
    {
      playerHandler.alive = false;
      Console.WriteLine("player disconnected: " + playerHandler.id);
      if (Server.DidPlayerDisconnect(playerHandler, e)) return;
      Console.WriteLine(e.GetType());
      playerHandler.socket.Close();
    }
  }

  public override string ToString()
  {
    var str = "\nGame " + this.id + "\n-----------------------------\n";
    str += "Players\n";
    this.playerHandlers.ForEach(p => str += " - " + p.ToString() + "\n");
    str += "\nState\n";
    str += " - " + this.game.ToString() + "\n";
    return str;
  }
}