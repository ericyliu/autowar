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
    return !playerHandler.socket.Connected;
  }

  public static bool DidThreadAbort(Exception e)
  {
    return true;
  }

  public Socket listener;
  public List<GameWebObject> gameWebObjects = new List<GameWebObject>();
  int port = 11000;
  int nextGameId = 0;
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
    if (this.listener != null) this.listener.Close();
    this.threads.ForEach(thread => thread.Abort());
  }

  public void JoinGame(PlayerHandler playerHandler)
  {
    GameWebObject game;
    game = this.gameWebObjects.Find(g => g.playerHandlers.Count < 2);
    if (game == null) game = new GameWebObject(this.nextGameId++);
    game.playerHandlers.Add(playerHandler);
    gameWebObjects.Add(game);
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
      new PlayerHandler(socket, this);
    }
  }
}
