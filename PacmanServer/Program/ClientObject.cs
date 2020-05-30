using System;
using System.Net.Mime;
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
		}

		public void InitMessage()
		{
			//Отправляем игроку игровое поле
			Header header = new Header();
			header.type = MessageType.GameField;
			Serializer.SerializeWithLengthPrefix<Header>(Stream, header, PrefixStyle.Fixed32);
			Serializer.SerializeWithLengthPrefix<GameField>(Stream, server.mapManager.PacmanField.GetFieldProto(), PrefixStyle.Fixed32);
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

			

			GetMessage();
		}

		protected internal void GetMessage()
		{
			try
			{
				while (true)
				{
					if (!client.Connected || client.Available == 0)
					{
						continue;
					}

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
									if (player.Status == Status.Disconnected)
									{
										server.playerDict.Remove(player);
										if (server.playerDict.Count > 0)
										{
											server.BroadcastMessage<PlayerInfo>(MessageType.PlayerInfo, player);
										}
										Close();
										return;
									}

									if (player.Status == Status.Connected)
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
				Close();
			}
		}

		protected internal void WriteMessage<T>(MessageType type, T message)
		{
			try
			{
				Header header = new Header();
				header.type = type;

				Serializer.SerializeWithLengthPrefix(Stream, header, PrefixStyle.Fixed32);
				Serializer.SerializeWithLengthPrefix(Stream, message, PrefixStyle.Fixed32);
			}
			catch (Exception e)
			{
				Close();
			}
		}

		protected internal void Close()
		{
			if (Stream != null)
			{
				Stream.Close();
			}

			if (server != null)
			{
				server.RemoveConnection(Id);
			}

			if (myPlayer != null)
			{
				server.playerDict.Remove(myPlayer);
			}

			if (readThread != null)
			{
				readThread.Abort();
			}

			if (client != null)
			{
				client.Close();
			}
		}
	}
}