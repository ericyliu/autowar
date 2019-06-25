using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Client : MonoBehaviour
{
  public GameComponent gameComponent;
  public Socket client;

  void Start()
  {
    Server server = new Server();
    server.Start();
    Game game = new Game();
    game.gameComponent = this.gameComponent;
    this.gameComponent.game = game;
    StartCoroutine(this.Connect());
  }

  IEnumerator Connect()
  {
    yield return new WaitForSeconds(1);
    byte[] bytes = new byte[1024];

    // IPHostEntry host = Dns.GetHostEntry(game.targetServer == TargetServer.Local ? "localhost" : "104.196.10.129");
    IPHostEntry host = Dns.GetHostEntry("localhost");
    IPAddress ipAddress = host.AddressList[0];
    IPEndPoint remoteEp = new IPEndPoint(ipAddress, 11000);

    client = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

    try
    {
      Debug.Log("Connecting to server...");
      client.Connect(remoteEp);
      Debug.Log("Successfully connected to server");
      // StartCoroutine(this.SendClientControlState());
      StartCoroutine(this.ReceiveGameSteps());
    }
    catch (Exception e)
    {
      Debug.Log("Exception: " + e.ToString());
      if (e.Message.IndexOf("No connection could be made because the target machine actively refused it.") > -1)
      {
        Debug.Log("Reconnecting in 1s");
        StartCoroutine(this.Connect());
      }
      if (client != null)
      {
        client.Shutdown(SocketShutdown.Both);
        client.Close();
      }
    }
  }

  IEnumerator ReceiveGameSteps()
  {
    while (this.client != null)
    {
      byte[] bytes = new byte[4];
      this.client.Receive(bytes);
      GameStep gameStep = new GameStep(bytes);
      gameComponent.steps.Add(gameStep);
      yield return new WaitForSeconds(.01f);
    }
  }
}