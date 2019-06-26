using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class Server
{
  static bool localhost = true;
  List<PlayerHandler> playerHandlers = new List<PlayerHandler>();
  Game game;

  static void Main()
  {
    // Server.localhost = false;
    (new Server()).Start();
  }

  public void Start()
  {
    this.game = new Game();
    Console.WriteLine("Starting the server");
    new Thread(() => this.PublishSteps()).Start();
    new Thread(() => this.ReceiveConnection()).Start();
  }

  void ReceiveConnection()
  {
    IPHostEntry host = Dns.GetHostEntry(Server.localhost ? "localhost" : Dns.GetHostName());
    IPAddress ipAddress = host.AddressList[0];
    IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);
    try
    {
      Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
      listener.Bind(localEndPoint);
      listener.Listen(10);
      Console.WriteLine("Waiting for a connection...");
      var playerHandler = new PlayerHandler(listener.Accept());
      this.playerHandlers.Add(playerHandler);
      Console.WriteLine("Connection Received");
      new Thread(() => this.ListenForActions(playerHandler)).Start();
    }
    catch (Exception e)
    {
      Console.WriteLine(e);
      this.playerHandlers.ForEach(playerHandler =>
      {
        playerHandler.handler.Shutdown(SocketShutdown.Both);
        playerHandler.handler.Close();
      });
    }
  }

  void ListenForActions(PlayerHandler playerHandler)
  {
    while (playerHandler != null)
    {
      byte[] bytes = new byte[2];
      playerHandler.handler.Receive(bytes);
      this.game.actions.Add(new GameAction(bytes, this.game));
    }
  }

  void PublishSteps()
  {
    while (true)
    {
      GameStep nextStep = this.game.Step();
      this.playerHandlers.ForEach(playerHandler =>
      {
        if (playerHandler != null) PublishToPlayer(playerHandler, nextStep);
      });
      Thread.Sleep(60);
    }
  }

  void PublishToPlayer(PlayerHandler playerHandler, GameStep nextStep)
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
}

public class PlayerHandler
{
  public bool isNew = true;
  public Socket handler;

  public PlayerHandler(Socket handler)
  {
    this.handler = handler;
  }
}
