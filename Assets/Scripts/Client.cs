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
	private NetworkStream stream;
	private GameController gameController;
	private Thread workingThread;

	public Client(GameController controller)
	{
		gameController = controller;
		Connect();
		workingThread = new Thread(new ThreadStart(ReceiveGameField));
		workingThread.Start();

		while (true)
		{
			if (!workingThread.IsAlive)
			{
				break;
			}
		}

		gameController.DrawGameField();
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
					gameController.PacmanField.SetFieldProto(info);
					gameController.PacmanField.WriteFromGameField();

					break;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}
}

