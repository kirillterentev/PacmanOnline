using System;
using System.Net.Sockets;
using System.Threading;
using ProtoBuf;

namespace PacmanServer
{
	class ClientObject
	{
		protected internal string Id { get; private set; }
		protected internal NetworkStream Stream { get; private set; }
		protected internal Coord LastInput;
		protected internal Thread readThread;

		TcpClient client;
		ServerObject server;
		PlayerInfo myPlayer;

		public ClientObject(TcpClient tcpClient, ServerObject serverObject)
		{
			Id = Guid.NewGuid().ToString();
			client = tcpClient;
			server = serverObject;
			serverObject.AddConnection(this);
			Stream = client.GetStream();

			readThread = new Thread(new ThreadStart(GetMessage));
			readThread.Start();
		}

		public void InitMessage()
		{
			//Отправляем игроку игровое поле
			Header header = new Header();
			header.type = MessageType.GameField;
			Serializer.SerializeWithLengthPrefix<Header>(Stream, header, PrefixStyle.Fixed32);
			Serializer.SerializeWithLengthPrefix<GameField>(Stream, server.mapManager.PacmanField.GetFieldProto(), PrefixStyle.Fixed32);
			Console.WriteLine("Send map");
			//Если в игре кто то уже есть, сообщаем игроку обо всех
			header.type = MessageType.PlayerInfo;
			foreach (var player in server.playerDict)
			{
				if (player.Key.ID == Id)
				{
					continue;
				}

				Serializer.SerializeWithLengthPrefix<Header>(Stream, header, PrefixStyle.Fixed32);
				Serializer.SerializeWithLengthPrefix<PlayerInfo>(Stream, player.Key, PrefixStyle.Fixed32);
			}
		}

		protected internal void GetMessage()
		{
			try
			{
				while (true)
				{
					var header = Serializer.DeserializeWithLengthPrefix<Header>(Stream, PrefixStyle.Fixed32);
					if (header == null)
					{
						continue;
					}

					switch (header.type)
					{
						case MessageType.PlayerInfo:
							var player = Serializer.DeserializeWithLengthPrefix<PlayerInfo>(Stream, PrefixStyle.Fixed32);
							if (player != null)
							{
								if (!server.playerDict.ContainsKey(player))
								{
									Id = player.ID;
									server.playerDict.Add(player, server.mapManager.GetFreePoint());
									myPlayer = player;
									if (server.playerDict.Count > 1)
									{
										server.BroadcastMessage<PlayerInfo>(MessageType.PlayerInfo, player);
									}
								}
							}
							break;

						case MessageType.Coord:
							var coord = Serializer.DeserializeWithLengthPrefix<Coord>(Stream, PrefixStyle.Fixed32);
							if (coord != null)
							{
								LastInput = coord;
							}
							break;
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				Console.ReadLine();
				Close();
				throw;
			}
		}

		protected internal void Close()
		{
			server.RemoveConnection(Id);
			if(myPlayer != null)
				server.playerDict.Remove(myPlayer);

			if (readThread != null)
			{
				readThread.Abort();
			}

			if (Stream != null)
			{
				Stream.Close();
			}

			if (client != null)
			{
				client.Close();
			}
		}
	}
}