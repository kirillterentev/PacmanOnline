using System;
using System.Net.Sockets;
using System.Threading;
using ProtoBuf;

public class Client 
{
	private const string host = "127.0.0.1";
	private const int port = 8888;

	private TcpClient client;
	private GameController gameController;
	private Thread readThread;

	protected internal NetworkStream stream;
	protected internal bool isConnected = false;

	public Client(GameController controller, PlayerInfo player)
	{
		gameController = controller;
	
		Connect();

		controller.CreateMyPlayer();
		try
		{
			var header = new Header();
			header.type = MessageType.PlayerInfo;
			Serializer.SerializeWithLengthPrefix(stream, header, PrefixStyle.Fixed32);
			Serializer.SerializeWithLengthPrefix(stream, player, PrefixStyle.Fixed32);
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			throw;
		}

		readThread = new Thread(new ThreadStart(GetMessage));
		readThread.Start();

		isConnected = true;
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
			throw;
		}
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
						var info = Serializer.DeserializeWithLengthPrefix<GameField>(stream, PrefixStyle.Fixed32);
						if (info != null)
						{
							gameController.DrawGameField(info, true);
						}
						break;

					case MessageType.PlayerInfo:
						var player = Serializer.DeserializeWithLengthPrefix<PlayerInfo>(stream, PrefixStyle.Fixed32);
						if (player != null)
						{
							if (player.ID == gameController.GetMyPlayer().ID)
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
						var moveInfo = Serializer.DeserializeWithLengthPrefix<MoveInfo>(stream, PrefixStyle.Fixed32);
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
				throw;
			}
		}
	}

	public void Disconnect()
	{
		if (stream != null)
		{
			var header = new Header();
			var player = gameController.GetMyPlayer();
			player.Status = Status.Disconnected;
			header.type = MessageType.PlayerInfo;
			Serializer.SerializeWithLengthPrefix(stream, header, PrefixStyle.Fixed32);
			Serializer.SerializeWithLengthPrefix(stream, player, PrefixStyle.Fixed32);

			stream.Close();
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

