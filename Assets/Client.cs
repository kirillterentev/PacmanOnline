using System;
using System.Collections;
using System.Net.Sockets;
using ProtoBuf;
using UnityEngine;

public class Client : MonoBehaviour
{
	private string userName;
	private const string host = "127.0.0.1";
	private const int port = 8888;
	static TcpClient client;
	static NetworkStream stream;

	private void Start()
	{
		client = new TcpClient();
		try
		{
			client.Connect(host, port);
			stream = client.GetStream();

			Debug.Log("Подключено");

			StartCoroutine(ReceiveGameField());
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}
		finally
		{
			Disconnect();
		}
	}

	private IEnumerator ReceiveGameField()
	{
		while (true)
		{
			try
			{
				int bytes = 0;
				Debug.Log("Сообщение получено");

				//GameField gamefield = new GameField();
				GameField info;
				info = Serializer.DeserializeWithLengthPrefix<GameField>(stream, PrefixStyle.Fixed32);
				Debug.Log("Here " + info.Size.Y);
				//Debug.Log("Here " + info.Y);
				if (info != null)
				{
					break;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}

			yield return null;
		}
	}

	static void Disconnect()
	{
		if (stream != null)
			stream.Close();//отключение потока
		if (client != null)
			client.Close();//отключение клиента
	}
}

