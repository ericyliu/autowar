using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class Server
{
  public static bool localhost = false;
  public static int STEP_TIME = 50;

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

  public int AddPlayer(string ip)
  {
    var player = new PlayerHandler(this.nextPlayerId++, ip);
    var openGame = this.gameWebObjects.Find(g => g.playerHandlers.Count < 2);
    if (openGame != null)
    {
      openGame.playerHandlers.Add(player);
      return openGame.playerHandlers.Count - 1;
    }
    var game = new GameWebObject(this.nextGameId++);
    game.playerHandlers.Add(player);
    gameWebObjects.Add(game);
    return 0;
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
      var socket = listener.Accept();
      Console.WriteLine("Incoming Connection from " + socket.RemoteEndPoint.ToString());
      var game = this.gameWebObjects.Find(g => g.PlayerConnect(socket));
      if (game == null)
      {
        Console.WriteLine("Connection failed, player not in game");
        socket.Close();
      }
    }
  }
}

public class PlayerHandler
{
  public int id;
  public string ip;
  public bool isNew = true;
  public Socket handler;
  public bool alive = true;

  public PlayerHandler(int id, string ip)
  {
    this.id = id;
    this.ip = ip;
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

  public GameWebObject(int id)
  {
    Console.WriteLine("Creating game");
    this.id = id;
  }

  public bool PlayerConnect(Socket socket)
  {
    var player = this.playerHandlers.Find(p => p.ip == socket.RemoteEndPoint.ToString());
    if (player == null) return false;
    Console.WriteLine("Connection successful");
    player.handler = socket;
    new Thread(() => this.ListenForActions(player)).Start();
    if (this.playerHandlers.Count == 2 && this.playerHandlers.TrueForAll(p => p.handler != null)) this.Start();
    return true;
  }

  void Start()
  {
    Console.WriteLine("Starting game");
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
        playerHandler.handler.Receive(bytes);
        this.game.actions.Add(new GameAction(bytes, this.game));
      }
    }
    catch (Exception e)
    {
      if (Server.DidPlayerDisconnect(playerHandler, e)) return;
      Console.WriteLine(e);
      playerHandler.handler.Close();
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