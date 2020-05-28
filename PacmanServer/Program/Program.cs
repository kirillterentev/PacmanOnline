﻿using System;
using System.Threading;

namespace PacmanServer
{
	class Program
	{
		static ServerObject server;
		static Thread listenThread;
		static Thread gameThread;

		static void Main(string[] args)
		{
			try
			{
				server = new ServerObject();

				listenThread = new Thread(new ThreadStart(server.Listen));
				listenThread.Start();
			}
			catch (Exception ex)
			{
				server.Disconnect();
				Console.WriteLine(ex.Message);
			}
		}
	}
}
