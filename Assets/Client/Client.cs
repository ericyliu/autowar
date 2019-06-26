using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public enum ServerOption
{
  Self,
  Local,
  Hosted
}

public class Client : MonoBehaviour
{
  public GameComponent gameComponent;
  public ServerOption serverOption = ServerOption.Self;
  public Socket client;
  public Game game;

  public void Attack()
  {
    if (this.client == null) return;
    this.client.Send(GameAction.CreateAttackAction(gameComponent.player).ToByteArray());
  }

  void Start()
  {
    if (this.serverOption == ServerOption.Self)
    {
      Server.localhost = true;
      Server server = new Server();
      server.Start();
    }
    this.game = new Game();
    this.gameComponent.game = this.game;
    this.gameComponent.player = this.game.players[this.gameComponent.id];
    StartCoroutine(this.Connect());
  }

  void OnApplicationQuit()
  {
    if (this.client != null)
    {
      client.Shutdown(SocketShutdown.Both);
      client.Close();
    }
  }

  IEnumerator Connect()
  {
    yield return new WaitForSeconds(1);
    byte[] bytes = new byte[1024];
    string hostString = this.serverOption == ServerOption.Hosted ? "34.74.40.215" : "localhost";
    IPHostEntry host = Dns.GetHostEntry(hostString);
    IPAddress ipAddress = host.AddressList[0];
    IPEndPoint remoteEp = new IPEndPoint(ipAddress, 11000);

    client = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

    try
    {
      Debug.Log("Connecting to server...");
      client.Connect(remoteEp);
      Debug.Log("Successfully connected to server");
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
      byte[] bytes = new byte[GameStep.BYTE_ARRAY_SIZE];
      this.client.Receive(bytes);
      GameStep gameStep = new GameStep(bytes, this.game);
      gameComponent.steps.Add(gameStep);
      yield return new WaitForSeconds(.01f);
    }
  }
}