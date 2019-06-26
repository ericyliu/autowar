using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class Server
{
  public static bool localhost = false;

  public static bool DidPlayerDisconnect(Exception e)
  {
    if (e.GetType().Equals(new ObjectDisposedException("").GetType())) return true;
    if (e.GetType().Equals(new SocketException().GetType())) return true;
    return false;
  }

  public static bool DidThreadAbort(Exception e)
  {
    return true;
  }

  List<PlayerHandler> playerHandlers = new List<PlayerHandler>();
  List<GameWebObject> gameWebObjects = new List<GameWebObject>();
  int port = 11000;
  int nextId = 0;

  static void Main(string[] args)
  {
    if (args.Length > 0 && args[0] == "localhost") Server.localhost = true;
    (new Server()).Start();
  }

  public void Start()
  {
    new Thread(this.PruneGameWebObjects).Start();
    new Thread(() => this.ReceiveConnection()).Start();
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
    Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
    listener.Bind(localEndPoint);
    listener.Listen(10);
    this.ReceiveConnections(listener);
  }

  void ReceiveConnections(Socket listener)
  {
    while (true)
    {
      var playerHandler = new PlayerHandler(this.nextId++, listener.Accept());
      new Thread(() => this.ListenForActions(playerHandler)).Start();
    }
  }

  void ListenForActions(PlayerHandler playerHandler)
  {
    Console.WriteLine("Connection Received");
    try
    {
      this.playerHandlers.Add(playerHandler);
      if (gameWebObjects.Count == 0) gameWebObjects.Add(new GameWebObject(playerHandler));
      else gameWebObjects[0].playerHandlers.Add(playerHandler);
      var gameWebObject = this.gameWebObjects.Find(
        g => g.playerHandlers.IndexOf(playerHandler) > -1
      );
      while (playerHandler != null)
      {
        byte[] bytes = new byte[2];
        playerHandler.handler.Receive(bytes);
        gameWebObject.game.actions.Add(new GameAction(bytes, gameWebObject.game));
      }
    }
    catch (Exception e)
    {
      if (Server.DidPlayerDisconnect(e)) return;
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
}

public class GameWebObject
{
  public Game game = new Game();
  public List<PlayerHandler> playerHandlers = new List<PlayerHandler>();
  public Thread thread;

  public GameWebObject(PlayerHandler playerHandler)
  {
    Console.WriteLine("Starting game");
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
        Thread.Sleep(60);
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
      if (Server.DidPlayerDisconnect(e)) return;
      Console.WriteLine(e.GetType());
      playerHandler.handler.Shutdown(SocketShutdown.Both);
      playerHandler.handler.Close();
    }
  }
}