using System;
using System.Net.Sockets;
using ProtoBuf;

namespace PacmanServer
{
	class ClientObject
	{
		protected internal string Id { get; private set; }
		protected internal NetworkStream Stream { get; private set; }
		protected internal Coord LastInput;

		private TcpClient client;
		private ServerObject server;

		public ClientObject(TcpClient tcpClient, ServerObject serverObject)
		{
			client = tcpClient;
			server = serverObject;
			serverObject.AddConnection(this);
			Stream = client.GetStream();
		}

		protected internal void InitClient()
		{
			WriteMessage(MessageType.GameField, server.MapManager.PacmanField.GetFieldProto());

			foreach (var player in server.PlayerDict)
			{
				if (player.Key.Id == Id)
				{
					continue;
				}

				WriteMessage(MessageType.PlayerInfo, player.Key);
			}

			GetMessageStream();
		}

		protected internal void GetMessageStream()
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

					switch (header.Type)
					{
						case MessageType.PlayerInfo:
							var player = ReadMessage<PlayerInfo>();
							if (player != null)
							{
								if (player.Status == Status.Disconnected)
								{
									server.PlayerDict.Remove(player);

									if (server.PlayerDict.Count > 0)
									{
										server.BroadcastMessage<PlayerInfo>(MessageType.PlayerInfo, player);
									}

									Close();
									return;
								}

								if (player.Status == Status.Connected)
								{
									Id = player.Id;
									server.PlayerDict.Add(player, server.MapManager.GetFreePoint());
									Console.WriteLine($"Player {player.Nickname} connected!");

									if (server.PlayerDict.Count > 1)
									{
										server.BroadcastMessage<PlayerInfo>(MessageType.PlayerInfo, player);
									}
								}
							}
							break;

						case MessageType.Coord:
							var coord = ReadMessage<Coord>();
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
				Close();
			}
		}

		private T ReadMessage<T>()
		{
			try
			{
				T message = Serializer.DeserializeWithLengthPrefix<T>(Stream, PrefixStyle.Fixed32);
				if (message == null)
				{
					return default(T);
				}

				return message;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return default(T);
			}
		}

		protected internal void WriteMessage<T>(MessageType type, T message)
		{
			try
			{
				Header header = new Header();
				header.Type = type;
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

			if (client != null)
			{
				client.Close();
			}
		}
	}
}