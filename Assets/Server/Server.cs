using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class Server
{
  static bool isRealServer = false;
  PlayerHandler playerHandler;
  Game game;

  static void Main()
  {
    Server.isRealServer = true;
    (new Server()).Start();
  }

  public void Start()
  {
    this.game = new Game();
    Console.WriteLine("Starting the server");
    new Thread(new ThreadStart(this.PublishSteps)).Start();
    new Thread(new ThreadStart(this.ReceiveConnection)).Start();
  }

  void ReceiveConnection()
  {
    IPHostEntry host = Dns.GetHostEntry(Server.isRealServer ? Dns.GetHostName() : "localhost");
    IPAddress ipAddress = host.AddressList[0];
    IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);
    try
    {
      Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
      listener.Bind(localEndPoint);
      listener.Listen(10);
      Debug.Log("Waiting for connection");
      Console.WriteLine("Waiting for a connection...");
      this.playerHandler = new PlayerHandler(listener.Accept());
      Console.WriteLine("Connection Received");
      new Thread(new ThreadStart(this.ListenForActions)).Start();
    }
    catch (Exception e)
    {
      Console.WriteLine(e);
      if (this.playerHandler != null)
      {
        this.playerHandler.handler.Shutdown(SocketShutdown.Both);
        this.playerHandler.handler.Close();
      }
    }
  }

  void ListenForActions()
  {
    while (true)
    {
      if (this.playerHandler == null) break;
      byte[] bytes = new byte[1];
      this.playerHandler.handler.Receive(bytes);
      GameAction action = GameStep.ByteToGameAction(bytes[0]);
      this.game.actions.Add(action);
    }
  }

  void PublishSteps()
  {
    while (true)
    {
      GameStep nextStep = this.game.Step();
      if (this.playerHandler != null)
      {
        if (this.playerHandler.isNew)
        {
          foreach (GameStep step in this.game.steps)
          {
            this.playerHandler.handler.Send(step.ToByteArray());
          }
          this.playerHandler.isNew = false;
        }
        else
        {
          this.playerHandler.handler.Send(nextStep.ToByteArray());
        }
      }
      Thread.Sleep(60);
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
