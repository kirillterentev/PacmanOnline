﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace PacmanServer
{
	class ServerObject
	{
		TcpListener tcpListener;
		List<ClientObject> clients = new List<ClientObject>();

		protected internal void AddConnection(ClientObject clientObject)
		{
			clients.Add(clientObject);
		}

		protected internal void RemoveConnection(string id)
		{
			ClientObject client = clients.FirstOrDefault(c => c.Id == id);
			if (client != null)
				clients.Remove(client);
		}

		protected internal void Listen()
		{
			try
			{
				tcpListener = new TcpListener(IPAddress.Any, 8888);
				tcpListener.Start();
				Console.WriteLine("Сервер запущен. Ожидание подключений...");

				while (true)
				{
					TcpClient tcpClient = tcpListener.AcceptTcpClient();
					ClientObject clientObject = new ClientObject(tcpClient, this);
					Thread clientThread = new Thread(new ThreadStart(clientObject.Process.Invoke));
					clientThread.Start();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Disconnect();
			}
		}

		protected internal void BroadcastMessage(byte[] message, string id)
		{
			byte[] data = message;

			for (int i = 0; i < clients.Count; i++)
			{
				if (clients[i].Id != id)
				{
					clients[i].Stream.Write(data, 0, data.Length);
				}
			}
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
