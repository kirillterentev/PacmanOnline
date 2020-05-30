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
			Header header = new Header();
			header.Type = MessageType.GameField;
			Serializer.SerializeWithLengthPrefix<Header>(Stream, header, PrefixStyle.Fixed32);
			Serializer.SerializeWithLengthPrefix<GameField>(Stream, server.mapManager.PacmanField.GetFieldProto(), PrefixStyle.Fixed32);
		
			header = new Header();
			header.Type = MessageType.PlayerInfo;

			foreach (var player in server.playerDict)
			{
				if (player.Key.Id == Id)
				{
					continue;
				}

				Serializer.SerializeWithLengthPrefix<Header>(Stream, header, PrefixStyle.Fixed32);
				Serializer.SerializeWithLengthPrefix<PlayerInfo>(Stream, player.Key, PrefixStyle.Fixed32);
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
							var player = Serializer.DeserializeWithLengthPrefix<PlayerInfo>(Stream, PrefixStyle.Fixed32);
							if (player != null)
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
									Id = player.Id;
									server.playerDict.Add(player, server.mapManager.GetFreePoint());
									Console.WriteLine($"Player {player.Nickname} connected!");

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
				Close();
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