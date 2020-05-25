using System;
using System.Diagnostics;
using System.Net.Sockets;
using ProtoBuf;

namespace PacmanServer
{
	class ClientObject
	{
		protected internal string Id { get; private set; }
		protected internal NetworkStream Stream { get; private set; }

		public Action Process;

		TcpClient client;
		ServerObject server;

		public ClientObject(TcpClient tcpClient, ServerObject serverObject)
		{
			Id = Guid.NewGuid().ToString();
			client = tcpClient;
			server = serverObject;
			serverObject.AddConnection(this);
			Stream = client.GetStream();

			Process = InitMessage;
		}

		public void InitMessage()
		{
			PacmanField pacmanField = new PacmanField();
			pacmanField.ReadFieldFromFile();
			Serializer.SerializeWithLengthPrefix<GameField>(Stream, pacmanField.GetFieldProto(), PrefixStyle.Fixed32);
			Process = EmptyMethod;
		}

		public void EmptyMethod(){}

		protected internal void Close()
		{
			if (Stream != null)
			{
				Stream.Close();
			}

			if (client != null)
			{
				client.Close();
			}
		}
	}
}
	//server.BroadcastMessage(message, this.Id);

	//while (true)
	//{
	//	try
	//	{
	//		var message = GetMessage();
	//		server.BroadcastMessage(message, this.Id);
	//	}
	//	catch
	//	{
	//		break;
	//	}
	//}

	//private byte[] GetMessage()
	//{
	//byte[] data = new byte[1024];
	//byte[] output;
	//int bytes = 0;
	//	do
	//{
	//	bytes = Stream.Read(data, 0, data.Length);
	//	output = new byte[bytes];
	//	for (int i = 0; i < bytes; i++)
	//	{
	//		output[i] = data[i];
	//	}
	//}
	//while (Stream.DataAvailable);

	//return output;
	//}

//try
//{
//}
//catch (Exception e)
//{
//Console.WriteLine(e.Message);
//}
//finally
//{
//server.RemoveConnection(this.Id);
//Close();
//}