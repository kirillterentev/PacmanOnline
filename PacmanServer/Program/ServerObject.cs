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
		const int UpdatePeriod = 300;

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
					ClientObject clientObject = new ClientObject(tcpClient, this);
					Thread clientThread = new Thread(new ThreadStart(clientObject.InitMessage));
					clientThread.Start();
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
				MoveInfo info = new MoveInfo();
				foreach (var player in playerDict)
				{
					Console.WriteLine($"{player.Key.Nickname} : {player.Value.X};{player.Value.Y}");
					info.Id = player.Key.ID;
					info.NewCoord = player.Value;
					BroadcastMessage(MessageType.MoveInfo, info);
				}
			}
			catch (Exception e)
			{

			}
		}

		protected internal void BroadcastMessage<T>(MessageType type ,T message)
		{
			Header header = new Header();
			header.type = type;

			for (int i = 0; i < clients.Count; i++)
			{
				Serializer.SerializeWithLengthPrefix(clients[i].Stream, header, PrefixStyle.Fixed32);
				Serializer.SerializeWithLengthPrefix(clients[i].Stream, message, PrefixStyle.Fixed32);
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
