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
      // new Thread(new ThreadStart(this.ListenForActions)).Start();
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

  // void ListenForActions()
  // {
  //   while (true)
  //   {
  //     if (this.handler == null) break;
  //     byte[] bytes = new byte[1024];
  //     this.handler.Receive(bytes);
  //     List<Action> actions =
  //   }
  // }

  void PublishSteps()
  {
    while (true)
    {
      GameStep nextStep = this.game.Step(new GameStep(this.game.step));
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

  // void ListenForClientControlState()
  // {
  //   while (true)
  //   {
  //     if (this.handler == null)
  //     {
  //       break;
  //     }
  //     byte[] bytes = new byte[1024];
  //     this.handler.Receive(bytes);
  //     var clientControlState = new ClientControlState(bytes);
  //     character.HandleMovement(clientControlState);

  //   }
  // }

  // void PublishStepState()
  // {
  //   while (true)
  //   {
  //     if (this.handler == null)
  //     {
  //       break;
  //     }
  //     this.handler.Send(character.ToByteArray());
  //     Thread.Sleep(10);
  //   }
  // }
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

// public class ServerCharacter
// {
//   public Vector2 position = Vector2.zero;
//   Vector3 playerCameraUp = new Vector3(0.3f, 0.7f, 0.6f);
//   Vector3 terrainUp = new Vector3(0f, 1f, 0f);
//   DateTime lastUpdate;
//   float speed = 3f;

//   public ServerCharacter()
//   {
//     this.position = new Vector2(5f, 5f);
//   }

//   public ServerCharacter(byte[] raw)
//   {
//     this.position.x = BitConverter.ToInt16(raw, 0) / 100f;
//     this.position.y = BitConverter.ToInt16(raw, 2) / 100f;
//   }

//   public void HandleMovement(ClientControlState clientControlState)
//   {
//     if (this.lastUpdate == null)
//     {
//       this.lastUpdate = DateTime.Now;
//       return;
//     }
//     float timePassed = (DateTime.Now - this.lastUpdate).Milliseconds / 1000f;
//     Vector3 forward = Vector3.ProjectOnPlane(this.playerCameraUp, this.terrainUp).normalized * this.speed * timePassed;
//     Vector2 direction = Vector2.zero;
//     if (clientControlState.moveUp)
//     {
//       direction += new Vector2(forward.x, forward.z);
//     }
//     if (clientControlState.moveLeft)
//     {
//       direction += new Vector2(-forward.z, forward.x);
//     }
//     if (clientControlState.moveDown)
//     {
//       direction += new Vector2(-forward.x, -forward.z);
//     }
//     if (clientControlState.moveRight)
//     {
//       direction += new Vector2(forward.z, -forward.x);
//     }
//     this.position += direction;
//     this.lastUpdate = DateTime.Now;
//   }

//   public byte[] ToByteArray()
//   {
//     byte[] bytes = new byte[4];
//     Array.Copy(this.FloatToByte(this.position.x), 0, bytes, 0, 2);
//     Array.Copy(this.FloatToByte(this.position.y), 0, bytes, 2, 2);
//     return bytes;
//   }

//   public override string ToString()
//   {
//     return this.position.ToString();
//   }

//   byte[] FloatToByte(float f)
//   {
//     return BitConverter.GetBytes(Convert.ToInt16(f * 100f));
//   }

// }

// public class ClientControlState
// {
//   public bool moveLeft = false;
//   public bool moveRight = false;
//   public bool moveUp = false;
//   public bool moveDown = false;

//   public ClientControlState(bool moveLeft = false, bool moveRight = false, bool moveUp = false, bool moveDown = false)
//   {
//     this.moveLeft = moveLeft;
//     this.moveRight = moveRight;
//     this.moveUp = moveUp;
//     this.moveDown = moveDown;
//   }

//   public ClientControlState(byte[] raw)
//   {
//     this.moveLeft = (raw[0] & 1) == 1 ? true : false;
//     this.moveRight = ((raw[0] >> 1) & 1) == 1 ? true : false;
//     this.moveUp = ((raw[0] >> 2) & 1) == 1 ? true : false;
//     this.moveDown = ((raw[0] >> 3) & 1) == 1 ? true : false;
//   }

//   public byte[] toByteArray()
//   {
//     var array = new byte[1];
//     byte b = 0;
//     if (this.moveLeft) b++;
//     if (this.moveRight) b += 2;
//     if (this.moveUp) b += 4;
//     if (this.moveDown) b += 8;
//     array[0] = b;
//     return array;
//   }

//   public override string ToString()
//   {
//     return "[" +
//       this.moveLeft + "," +
//       this.moveRight + "," +
//       this.moveUp + "," +
//       this.moveDown +
//     "]";
//   }
// }