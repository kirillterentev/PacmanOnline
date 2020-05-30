using System;
using System.Threading;

namespace PacmanServer
{
	class Program
	{
		private static ServerObject server;
		private static Thread listenThread;

		static void Main(string[] args)
		{
			try
			{
				server = new ServerObject();
				server.InitServer();

				listenThread = new Thread(new ThreadStart(server.Listen));
				listenThread.Start();
			}
			catch (Exception e)
			{
				server.Disconnect();
			}
		}
	}
}
