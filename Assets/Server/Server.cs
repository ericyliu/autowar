using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class Server
{
  public static bool localhost = false;

  static void Main(string[] args)
  {
    if (args.Length > 0 && args[0] == "localhost") Server.localhost = true;
    (new Server()).Start();
  }

  public static bool DidPlayerDisconnect(PlayerHandler playerHandler, Exception e)
  {
    if (e.GetType().Equals(new ObjectDisposedException("").GetType())) return true;
    if (e.GetType().Equals(new SocketException().GetType())) return true;
    return !playerHandler.handler.Connected;
  }

  public static bool DidThreadAbort(Exception e)
  {
    return true;
  }

  public Socket listener;
  List<PlayerHandler> playerHandlers = new List<PlayerHandler>();
  public List<GameWebObject> gameWebObjects = new List<GameWebObject>();
  int port = 11000;
  int nextGameId = 0;
  int nextPlayerId = 0;
  public List<Thread> threads = new List<Thread>();

  public void Start()
  {
    this.threads.Add(new Thread(() => new HttpServer(this).Start()));
    this.threads.Add(new Thread(this.PruneGameWebObjects));
    this.threads.Add(new Thread(() => this.ReceiveConnection()));
    this.threads.ForEach(thread => thread.Start());
  }

  public void Stop()
  {
    if (this.listener != null)
    {
      this.listener.Close();
    }
    this.threads.ForEach(thread => thread.Abort());
  }

  void PruneGameWebObjects()
  {
    while (true)
    {
      this.gameWebObjects = this.gameWebObjects.FindAll(g =>
      {
        var isAlive = g.playerHandlers.FindAll(h => h.alive).Count > 0;
        if (!isAlive) g.thread.Abort();
        return isAlive;
      });
      Thread.Sleep(1000);
    }
  }

  void ReceiveConnection()
  {
    IPHostEntry host = Dns.GetHostEntry(Server.localhost ? "localhost" : Dns.GetHostName());
    IPAddress ipAddress = host.AddressList[0];
    IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);
    Console.WriteLine("Starting the server on port: " + port);
    this.listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
    this.listener.Bind(localEndPoint);
    this.listener.Listen(10);
    this.ReceiveConnections(this.listener);
  }

  void ReceiveConnections(Socket listener)
  {
    while (true)
    {
      var playerHandler = new PlayerHandler(this.nextPlayerId++, listener.Accept());
      new Thread(() => this.ListenForActions(playerHandler)).Start();
    }
  }

  void ListenForActions(PlayerHandler playerHandler)
  {
    Console.WriteLine("Connection Received");
    try
    {
      this.playerHandlers.Add(playerHandler);
      if (gameWebObjects.Count == 0) gameWebObjects.Add(new GameWebObject(this.nextGameId++, playerHandler));
      else gameWebObjects[0].playerHandlers.Add(playerHandler);
      var gameWebObject = this.gameWebObjects.Find(
        g => g.playerHandlers.IndexOf(playerHandler) > -1
      );
      while (playerHandler != null)
      {
        byte[] bytes = new byte[GameAction.BYTE_ARRAY_SIZE];
        playerHandler.handler.Receive(bytes);
        gameWebObject.game.actions.Add(new GameAction(bytes, gameWebObject.game));
      }
    }
    catch (Exception e)
    {
      if (Server.DidPlayerDisconnect(playerHandler, e)) return;
      Console.WriteLine(e);
      playerHandler.handler.Shutdown(SocketShutdown.Both);
      playerHandler.handler.Close();
    }
  }
}

public class PlayerHandler
{
  public int id;
  public bool isNew = true;
  public Socket handler;
  public bool alive = true;

  public PlayerHandler(int id, Socket handler)
  {
    this.id = id;
    this.handler = handler;
  }

  public override string ToString()
  {
    var address = ((Server.localhost ? this.handler.LocalEndPoint : this.handler.RemoteEndPoint) as IPEndPoint).Address;
    var port = ((Server.localhost ? this.handler.LocalEndPoint : this.handler.RemoteEndPoint) as IPEndPoint).Port;
    return this.id + "(" + (this.alive ? "alive" : "dead") + "): " + address + ":" + port;
  }
}

public class GameWebObject
{
  public int id;
  public Game game = new Game();
  public List<PlayerHandler> playerHandlers = new List<PlayerHandler>();
  public Thread thread;

  public GameWebObject(int id, PlayerHandler playerHandler)
  {
    Console.WriteLine("Starting game");
    this.id = id;
    this.playerHandlers.Add(playerHandler);
    this.thread = new Thread(this.PublishSteps);
    this.thread.Start();
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
        Thread.Sleep(50);
      }
      Console.WriteLine("Closing game");
    }
    catch (Exception e)
    {
      Console.WriteLine("Closing game");
      if (Server.DidThreadAbort(e)) return;
      Console.WriteLine(e);
    }
  }

  void PublishToPlayer(PlayerHandler playerHandler, GameStep nextStep)
  {
    try
    {
      if (playerHandler.isNew)
      {
        this.game.steps.ForEach(step =>
          playerHandler.handler.Send(step.ToByteArray())
        );
        playerHandler.isNew = false;
      }
      else
      {
        playerHandler.handler.Send(nextStep.ToByteArray());
      }
    }
    catch (Exception e)
    {
      playerHandler.alive = false;
      Console.WriteLine("player disconnected: " + playerHandler.id);
      if (Server.DidPlayerDisconnect(playerHandler, e)) return;
      Console.WriteLine(e.GetType());
      playerHandler.handler.Shutdown(SocketShutdown.Both);
      playerHandler.handler.Close();
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