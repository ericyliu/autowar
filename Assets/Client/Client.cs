using System;
using System.Collections;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Networking;

public enum ServerOption
{
  Self,
  Local,
  Hosted
}

public class Client : MonoBehaviour
{
  public GameComponent gameComponent;
  public UI ui;
  public ServerOption serverOption = ServerOption.Self;
  public Socket socket;
  public Game game;
  public Server server;
  public HttpClient httpClient = new HttpClient();

  public void JoinServer(int id)
  {
    if (id != 0 && id != 1) return;
    this.game = new Game();
    this.gameComponent.game = this.game;
    this.gameComponent.player = this.game.players[id];
    this.ui.ShowGameMenu();
    this.ui.LookAtBase(id);
    StartCoroutine(this.Connect());
  }

  public void Send(GameAction action)
  {
    if (this.socket == null) return;
    this.socket.Send(action.ToByteArray());
  }

  void Start()
  {
    if (this.serverOption == ServerOption.Self) this.StartServer();
    if (this.serverOption == ServerOption.Hosted) this.ui.ShowStartMenu();
    else this.JoinServer(this.gameComponent.id);
  }

  void OnApplicationQuit()
  {
    if (this.socket != null && this.socket.Connected)
    {
      this.socket.Shutdown(SocketShutdown.Both);
      this.socket.Close();
    }
    if (this.server != null) server.Stop();
  }

  void StartServer()
  {
    Debug.Log("Starting Server");
    Server.localhost = true;
    this.server = new Server();
    this.server.Start();
  }

  IEnumerator Connect()
  {
    yield return new WaitForSeconds(1);
    string hostString = this.serverOption == ServerOption.Hosted ? "34.74.40.215" : "localhost";
    UnityWebRequest www = UnityWebRequest.Get(hostString + ":8080/join-game");
    yield return www.SendWebRequest();
    byte[] bytes = new byte[1024];
    IPHostEntry host = Dns.GetHostEntry(hostString);
    IPAddress ipAddress = host.AddressList[0];
    IPEndPoint remoteEp = new IPEndPoint(ipAddress, 11000);

    this.socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

    try
    {
      Debug.Log("Connecting to " + (this.serverOption == ServerOption.Hosted ? "remote" : "local") + " server");
      this.socket.Connect(remoteEp);
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
      if (this.socket != null)
      {
        this.socket.Shutdown(SocketShutdown.Both);
        this.socket.Close();
      }
    }
  }

  IEnumerator ReceiveGameSteps()
  {
    while (this.socket != null)
    {
      byte[] bytes = new byte[GameStep.BYTE_ARRAY_SIZE];
      this.socket.Receive(bytes);
      if (BitConverter.ToInt32(bytes, 0) == 0)
      {
        Debug.Log("Server connection lost");
        this.socket.Close();
        yield break;
      }
      GameStep gameStep = new GameStep(bytes, this.game);
      gameComponent.steps.Add(gameStep);
      yield return new WaitForSeconds(.01f);
    }
  }
}