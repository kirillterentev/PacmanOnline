using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ProtoBuf;

namespace PacmanServer
{
	class ServerObject
	{
		const int UpdatePeriod = 500;

		protected internal Dictionary<PlayerInfo, Coord> playerDict = new Dictionary<PlayerInfo, Coord>();
		protected internal MapManager mapManager;

		TcpListener tcpListener;
		List<ClientObject> clients = new List<ClientObject>();

		protected internal void AddConnection(ClientObject clientObject)
		{
			clients.Add(clientObject);
		}

		protected internal void Listen()
		{
			mapManager = new MapManager(this);

			try
			{
				tcpListener = new TcpListener(IPAddress.Any, 8888);
				tcpListener.Start();
				Console.WriteLine("Сервер запущен. Ожидание подключений...");

				TimerCallback timerCallback = new TimerCallback(GameCicle);
				Timer timer = new Timer(timerCallback, null, 0, UpdatePeriod);

				while (clients.Count < 4)
				{
					TcpClient tcpClient = tcpListener.AcceptTcpClient();
					if (tcpClient != null)
					{
						ClientObject clientObject = new ClientObject(tcpClient, this);
						Thread clientThread = new Thread(new ThreadStart(clientObject.InitMessage));
						clientThread.Start();
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Disconnect();
			}
		}

		protected internal void GameCicle(object arg)
		{
			if (playerDict.Count < 1)
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

				MoveInfo info = new MoveInfo();
				foreach (var player in playerDict)
				{
					info.Id = player.Key.ID;
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
			var playerDictItem = playerDict.First(player => player.Key.ID == id).Key;
			var newPos = mapManager.CalculateNextPos(playerDict[playerDictItem], dir);
			playerDict[playerDictItem] = newPos;
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

		protected internal void RemoveConnection(string id)
		{
			ClientObject client = clients.FirstOrDefault(c => c.Id == id);
			if (client != null)
				clients.Remove(client);
		}

		protected internal void Disconnect()
		{
			tcpListener.Stop(); 

			for (int i = 0; i < clients.Count; i++)
			{
				clients[i].Close();
			}

			Environment.Exit(0);
		}
	}
}
