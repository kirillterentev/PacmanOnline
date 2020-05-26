using System;
using System.Net.Sockets;
using System.Threading;
using ProtoBuf;
using UnityEngine;

public class Client 
{
	private const string host = "127.0.0.1";
	private const int port = 8888;

	private TcpClient client;
	private GameField gameField;
	private NetworkStream stream;
	private GameController gameController;
	private Thread workingThread;

	public Client(GameController controller, PlayerInfo player)
	{
		gameController = controller;
		Connect();

		try
		{
			Serializer.SerializeWithLengthPrefix(stream, player, PrefixStyle.Fixed32);
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			throw;
		}

		workingThread = new Thread(new ThreadStart(ReceiveGameField));
		workingThread.Start();

		while (true)
		{
			if (!workingThread.IsAlive)
			{
				break;
			}
		}

		gameController.DrawGameField(gameField);
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

	private void ReceiveGameField()
	{
		while (true)
		{
			try
			{
				GameField info;
				info = Serializer.DeserializeWithLengthPrefix<GameField>(stream, PrefixStyle.Fixed32);

				if (info != null)
				{
					gameField = info;
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
		workingThread.Abort();

		if (stream != null)
		{
			stream.Close();
		}

		if (client != null)
		{
			client.Close();
		}
	}

}

