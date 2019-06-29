using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

public class HttpServer
{
  public Server server;
  public Dictionary<string, Func<string>> routes = new Dictionary<string, Func<string>>();
  HttpListener listener;

  public HttpServer(Server server)
  {
    this.server = server;
    this.routes.Add("/", this.GetServerState);
    this.routes.Add("/favicon.ico", () => "Not found");
  }

  public void Start()
  {
    this.listener = new HttpListener();
    // Add the prefixes.
    this.listener.Prefixes.Add(Server.localhost ? "http://localhost:8080/" : "http://34.74.40.215:8080/");
    this.listener.Start();
    Console.WriteLine("HTTP Server started");
    // Note: The GetContext method blocks while waiting for a request.
    try
    {
      while (true)
      {
        HttpListenerContext context = listener.GetContext();
        new Thread(() => this.HandleRequest(context)).Start();
      }
    }
    catch
    {
      this.listener.Close();
    }
  }

  void HandleRequest(HttpListenerContext context)
  {
    HttpListenerRequest request = context.Request;
    // Obtain a response object.
    HttpListenerResponse response = context.Response;
    // Construct a response.
    string responseString = this.routes[request.Url.LocalPath]();
    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
    // Get a response stream and write the response to it.
    response.ContentLength64 = buffer.Length;
    System.IO.Stream output = response.OutputStream;
    output.Write(buffer, 0, buffer.Length);
    // You must close the output stream.
    output.Close();
  }

  string GetServerState()
  {
    var state = "";
    state += "AutoWar Status Panel\n\n";
    this.server.gameWebObjects.ForEach(gwo => state += gwo.ToString() + "\n");
    return state;
  }
}