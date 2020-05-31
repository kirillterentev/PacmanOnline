using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace PacmanServer
{
	class ServerObject
	{
		private const int UpdatePeriod = 500;

		protected internal Dictionary<PlayerInfo, Coord> PlayerDict;
		protected internal MapManager MapManager;

		private TcpListener tcpListener;
		private List<ClientObject> clients;
		private Timer timer;

		protected internal void InitServer()
		{
			MapManager = new MapManager();
			PlayerDict = new Dictionary<PlayerInfo, Coord>();
			clients = new List<ClientObject>();
		}

		protected internal void Listen()
		{
			try
			{
				tcpListener = new TcpListener(IPAddress.Any, 8888);
				tcpListener.Start();

				Console.WriteLine("Сервер запущен");

				timer = new Timer(new TimerCallback(GameCicle), null, 0, UpdatePeriod);

				while (true)
				{
					TcpClient tcpClient = tcpListener.AcceptTcpClient();

					if (tcpClient != null)
					{
						ClientObject clientObject = new ClientObject(tcpClient, this);
						Thread clientThread = new Thread(new ThreadStart(clientObject.InitClient));
						clientThread.Start();
					}
				}
			}
			catch (Exception e)
			{
				Disconnect();
			}
		}

		protected internal void GameCicle(object args)
		{
			if (PlayerDict.Count < 1)
			{
				return;
			}

			try
			{
				foreach (var client in clients)
				{
					if (client.LastInput == null)
					{
						continue;
					}

					MovePlayer(client.Id, client.LastInput);
					client.LastInput = null;
				}

				MoveInfo info;
				foreach (var player in PlayerDict)
				{
					info = new MoveInfo();
					info.Id = player.Key.Id;
					info.NewCoord = player.Value;

					BroadcastMessage(MessageType.MoveInfo, info);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		protected internal void MovePlayer(string id, Coord dir)
		{
			var playerDictItem = PlayerDict.First(player => player.Key.Id == id).Key;
			var newPos = MapManager.CalculateNextPos(PlayerDict[playerDictItem], dir);
			PlayerDict[playerDictItem] = newPos;
		}

		protected internal void BroadcastMessage<T>(MessageType type ,T message)
		{
			try
			{
				for (int i = 0; i < clients.Count; i++)
				{
					clients[i].WriteMessage(type, message);
				}
			}
			catch (Exception e)
			{
			}
		}

		protected internal void AddConnection(ClientObject clientObject)
		{
			clients.Add(clientObject);
		}

		protected internal void RemoveConnection(string id)
		{
			ClientObject client = clients.FirstOrDefault(c => c.Id == id);
			if (client != null)
			{
				clients.Remove(client);
			}
		}

		protected internal void Disconnect()
		{
			timer?.Dispose();
			tcpListener?.Stop(); 

			for (int i = 0; i < clients.Count; i++)
			{
				clients[i].Close();
			}

			Environment.Exit(0);
		}
	}
}
