using System;
using System.Collections.Generic;
using ProtoBuf;

public enum MessageType
{
	Coord,
	PlayerInfo,
	GameField,
	MoveInfo
}

[Serializable]
[ProtoContract]
public class Header
{
	[ProtoMember(1)]
	public MessageType type;
}

[Serializable]
[ProtoContract]
public class MoveInfo
{
	[ProtoMember(1)]
	public string Id = "";

	[ProtoMember(2)]
	public Coord NewCoord;
}

[Serializable]
[ProtoContract]
public class PlayerInfo
{
	[ProtoMember(1)]
	public string Nickname = "";

	[ProtoMember(2)]
	public string Color = "";

	[ProtoMember(3)]
	public string ID = "me";
}

[Serializable]
[ProtoContract]
public class Coord
{
	[ProtoMember(1)]
	public int X = 0;

	[ProtoMember(2)]
	public int Y = 0;
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
