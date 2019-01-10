﻿using System;
using System.Net;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using NarcityMedia;
using NarcityMedia.Net;
using NarcityMedia.Log;
using Newtonsoft.Json;
using System.Linq;

namespace dotnet_core_socket_server
{
    public class Program
    {
        private static bool exit = false;
        private static HTTPServer httpServer;
        const string NAVIGATE_MARKER = "navigate:";

        static void Main(string[] args)
        {
            int workerThreads, portThreads;
            ThreadPool.GetMaxThreads(out workerThreads, out portThreads);
            Logger.Log("The process Thread Pool has a mixaimum of " + workerThreads.ToString() + " worker threads", Logger.LogType.Info);

            Thread HTTP = new Thread(Program.DispatchHTTPServer);
            HTTP.Name = "HTTP Localhost Listener";
            HTTP.Start();

            WebSocketServer socketServer = new WebSocketServer();
            socketServer.OnConnect += OnSocketConnect;
            socketServer.OnMessage += OnSocketMessage;
            socketServer.OnDisconnect += OnSocketDisconnect;

            try
            {
                socketServer.Start(new IPEndPoint(IPAddress.Loopback, 13003));
            }
            catch (WebSocketServerException e)
            {
<<<<<<< HEAD
                cli.Greet();
                cli.StartListenAsync();
                Logger.Log("Socket connection accepted #" + SocketManager.Instance.ClientsCount, Logger.LogType.Success);
                if (!SocketManager.Instance.AddClient(cli))
                {
                    Logger.Log("The new client couldn't be added to the Socket Manager and will be disposed of", Logger.LogType.Error);
                    cli.SendControlFrame(new SocketControlFrame(SocketFrame.OPCodes.Close));
                    cli.Dispose();
                }
            } else {
                Logger.Log("Socket connection refused, couldn't parse headers", Logger.LogType.Error);
		Logger.Log(System.Text.Encoding.UTF8.GetString(cli.requestheaders), Logger.LogType.Error);
                cli.Dispose();
=======
                Logger.Log("An error occured when starting the WebSocket server - " + e.Message, Logger.LogType.Error);
>>>>>>> isolate-websocket-server
            }
        }

        private static void OnSocketConnect(object sender, WebSocketServerEventArgs args)
        {
<<<<<<< HEAD
            if (message.Plaintext.StartsWith(NAVIGATE_MARKER))
            {
                string newUrl = message.Plaintext.Substring(NAVIGATE_MARKER.Length);
                SocketManager.Instance.RemoveClient(client);
                client.currentUrl = newUrl;
                SocketManager.Instance.AddClient(client);

	        Logger.Log("Received text frame with content length: " + message.contentLength, Logger.LogType.Info);
            }
            else if (message.Plaintext == "1")
            {
                client.SendApplicationMessage((WebSocketMessage.ApplicationMessageCode)2);
            }
=======
            Console.WriteLine("Got new socket at : " + args.Cli.InitTime);
>>>>>>> isolate-websocket-server
        }

        private static void OnSocketDisconnect(object sender, WebSocketServerEventArgs args)
        {
            double socketDuration = Math.Round(DateTime.Now.Subtract(args.Cli.InitTime).TotalSeconds, 2);
            Logger.Log(String.Format("Socket is closing after {0} seconds", socketDuration), Logger.LogType.Info);
        }

        private static void OnSocketMessage(object sender, WebSocketServerEventArgs args)
        {
            if (args.DataFrame.Plaintext.StartsWith(NAVIGATE_MARKER))
            {
                string newUrl = args.DataFrame.Plaintext.Substring(NAVIGATE_MARKER.Length);
                args.Cli.currentUrl = newUrl;
            }
            else if (args.DataFrame.Plaintext == "1")
            {
                args.Cli.SendApplicationMessage((WebSocketMessage.ApplicationMessageCode) 2);
            }

            if (args.DataFrame.DataType == SocketDataFrame.DataFrameType.Text)
            {
                Logger.Log("Received message : " + args.DataFrame.Plaintext, Logger.LogType.Info);
            }
            else if (args.DataFrame.DataType == SocketDataFrame.DataFrameType.Binary)
            {
                Logger.Log("Received binary frame : " + BitConverter.ToString(args.DataFrame.GetBytes()), Logger.LogType.Info);
            }
        }

        private static void DispatchHTTPServer()
        {
            httpServer = new HTTPServer(new Uri("http://localhost:8887"));

            // httpServer.on404 = on404;
            // httpServer.on500 = on500;

            HTTPServer.EndpointCallback index = Index;
            HTTPServer.EndpointCallback hello = Hello;
            // HTTPServer.EndpointCallback stats = GetStats;
            HTTPServer.EndpointCallback sendNotificationToUser = SendNotificationToUser;
            // HTTPServer.EndpointCallback sendnotificationToEndpoint = SendNotificationToEndpoint;

            httpServer.Get("/", index);
            httpServer.Get("/hello", hello);
            // httpServer.Get("/stats", stats);
            httpServer.Get("/notifyuser", SendNotificationToUser);
            // httpServer.Get("/notifyendpoint", SendNotificationToEndpoint);

            // .Start() is blocking meaning the thread won't finish until the server stops listenning
            httpServer.Start();
        }

        private static void Index(HttpListenerRequest req, HttpListenerResponse res)
        {
            httpServer.SendResponse(res, HttpStatusCode.OK, "GET Index");
        }

        private static void Hello(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Log("HTTP Request to GET /hello", Logger.LogType.Success);
            httpServer.SendResponse(res, HttpStatusCode.OK, "GET /hello");
        }

        private static void SendNotificationToUser(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Log("HTTP Request to POST /sendNotificationToUser", Logger.LogType.Success);
            httpServer.SendResponse(res, HttpStatusCode.OK, "GET /notifyuser");
        }

    //     private static void SendNotificationToEndpoint(HttpListenerRequest req, HttpListenerResponse res)
    //     {
    //         Logger.Log("HTTP Request to POST /sendNotificationToEndpoint", Logger.LogType.Success);
    //         // Hardcoded for testing purposes
    //         Room endpointRoom = SocketManager.Instance.GetRoomByName("www.test.narcity.com/test");
    //         if (endpointRoom != null)
    //         {
    //             endpointRoom.Broadcast(NarcityMedia.WebSocketMessage.ApplicationMessageCode.FetchComments);
    //             httpServer.SendResponse(res, HttpStatusCode.OK, "GET /notifyendpoint");
    //         }
    //         else
    //         {
    //             httpServer.SendResponse(res, HttpStatusCode.NotFound, "Couldn't find any room with the specified endpoint");
    //         }
    //     }
        
    //     private static void on404(HttpListenerRequest req, HttpListenerResponse res)
    //     {
    //         Logger.Log(String.Format("HTTP 404 - Couldn't find endpoint {0}", req.Url.ToString()), Logger.LogType.Warning);
    //         httpServer.SendResponse(res, HttpStatusCode.NotFound, "Not Found");
    //     }

    //     private static void on500(HttpListenerRequest req, HttpListenerResponse res)
    //     {
    //         Logger.Log("HTTP 500 - Internal Server Error", Logger.LogType.Error);
    //         httpServer.SendResponse(res, HttpStatusCode.NotFound, "Internal Server Error");
    //     }

    //     private class Stats
    //     {
    //         public int RealtimeSessionsNumber;
    //         public int ActiveEndpointsNumber;
    //         public List<Room> rooms;

    //         public Stats(List<Room> rooms)
    //         {
    //             this.rooms = rooms.OrderByDescending(room => room.Clients.Count).ToList();
    //             this.ActiveEndpointsNumber = rooms.Count;
    //         }

    //         public Stats(List<Room> rooms, int realtimeSessionsNumber) : this(rooms)
    //         {
    //             this.RealtimeSessionsNumber = realtimeSessionsNumber;
    //         }
    //     }
    }
}
