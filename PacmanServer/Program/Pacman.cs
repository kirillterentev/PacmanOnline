using System;
using System.Collections.Generic;
using ProtoBuf;

namespace PacmanServer
{
	[Serializable]
	[ProtoContract]
	public class Joystick
	{
		[ProtoMember(1)]
		public int axisX;

		[ProtoMember(2)]
		public int axisY;
	}

	[Serializable]
	[ProtoContract]
	public class PlayerInfo
	{
		[ProtoMember(1)]
		public string Nickname;

		[ProtoMember(2)]
		public string Color;

		[ProtoMember(3)]
		public Coord Coord;
	}

	[Serializable]
	[ProtoContract]
	public class Coord
	{
		[ProtoMember(1)]
		public int X;

		[ProtoMember(2)]
		public int Y;
	}

	[Serializable]
	[ProtoContract]
	public class GameField
	{
		[ProtoMember(1)]
		public Coord Size;

		[ProtoMember(2)]
		public List<Coord> Cells;
	}

}

