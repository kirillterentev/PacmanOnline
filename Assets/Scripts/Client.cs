using System;
using System.Net.Sockets;
using System.Threading;
using ProtoBuf;

namespace ClientPacman
{
	public class Client
	{
		private const string host = "127.0.0.1";
		private const int port = 8888;

		private Thread readThread;
		private TcpClient client;
		private PlayerInfo myPlayer;
		private GameController gameController;
		private NetworkStream stream;

		protected internal bool IsConnected;

		public Client(GameController controller, PlayerInfo player)
		{
			gameController = controller;
			myPlayer = player;

			Connect();

			controller.AddPlayer(player);

			WriteMessage<PlayerInfo>(MessageType.PlayerInfo, player);

			readThread = new Thread(new ThreadStart(GetMessage));
			readThread.Start();
		}

		private void Connect()
		{
			try
			{
				client = new TcpClient();
				client.Connect(host, port);
				stream = client.GetStream();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}

			IsConnected = true;
		}

		private void GetMessage()
		{
			while (true)
			{
				try
				{
					var mesType = Serializer.DeserializeWithLengthPrefix<Header>(stream, PrefixStyle.Fixed32);
					if (mesType == null) continue;

					switch (mesType.type)
					{
						case MessageType.GameField:
							var info = ReadMessage<GameField>();
							if (info != null)
							{
								gameController.DrawGameField(info, true);
							}
							break;

						case MessageType.PlayerInfo:
							var player = ReadMessage<PlayerInfo>();
							if (player != null)
							{
								if (player.ID == myPlayer.ID)
								{
									break;
								}

								if (player.Status == Status.Disconnected)
								{
									gameController.RemovePlayer(player, true);
								}

								if (player.Status == Status.Connected)
								{
									gameController.AddPlayer(player, true);
								}
							}
							break;

						case MessageType.MoveInfo:
							var moveInfo = ReadMessage<MoveInfo>();
							if (moveInfo != null)
							{
								gameController.SetPlayerPosition(moveInfo.Id, moveInfo.NewCoord, true);
							}
							break;
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			}
		}

		private T ReadMessage<T>()
		{
			try
			{
				T message = Serializer.DeserializeWithLengthPrefix<T>(stream, PrefixStyle.Fixed32);
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
				header.type = type;

				Serializer.SerializeWithLengthPrefix<Header>(stream, header, PrefixStyle.Fixed32);
				Serializer.SerializeWithLengthPrefix<T>(stream, message, PrefixStyle.Fixed32);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		protected internal void Disconnect()
		{
			IsConnected = false;

			gameController.PlayerDict.Remove(myPlayer);
			gameController.PlayerTransforms.Remove(myPlayer.ID);

			if (stream != null)
			{
				myPlayer.Status = Status.Disconnected;
				WriteMessage<PlayerInfo>(MessageType.PlayerInfo, myPlayer);

				stream.Close();
			}

			readThread?.Abort();

			client?.Close();
		}
	}
}

