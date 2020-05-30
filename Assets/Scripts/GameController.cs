using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using UnityEngine;

public class GameController : MonoBehaviour
{
	[SerializeField]
	private GameObject planeQuad;
	[SerializeField]
	private GameObject highQuad;

	private Client client;
	private GameData gameData;
	private PlayerInfo playerInfo;
	private Queue<Action> queueTask = new Queue<Action>();

	protected internal Dictionary<PlayerInfo, Coord> playerDict = new Dictionary<PlayerInfo, Coord>();
	protected internal Dictionary<string, Transform> playerObj = new Dictionary<string, Transform>();

	private void Start()
	{
		gameData = new GameData();
	}

	public void ConnectToServer(string name, string color)
	{
		playerInfo = new PlayerInfo();
		playerInfo.Nickname = name;
		playerInfo.Color = color;
		playerInfo.ID = Guid.NewGuid().ToString();
		playerInfo.Status = Status.Connected;

		client = new Client(this, playerInfo);

		playerDict.Add(playerInfo, new Coord());
		gameData.MyPlayer = playerInfo;
	}

	public void DrawGameField(GameField gameField, bool fromOuterThread = false)
	{
		if (fromOuterThread)
		{
			queueTask.Enqueue(() => DrawGameField(gameField));
			return;
		}

		gameData.PacmanField.SetFieldProto(gameField);
		gameData.PacmanField.WriteFromGameField();

		var field = gameData.PacmanField.GetField();
		var parent = new GameObject("PacmanField").transform;

		for (int j = 0; j < field.GetLength(1); j++)
		{
			var calculatedLength = field.GetLength(0);
			for (int i = 0; i < calculatedLength; i++)
			{
				var cellPrefab = field[i, j] ? highQuad : planeQuad;
				var go =  Instantiate(cellPrefab, 
									new Vector3(i, cellPrefab.transform.localScale.y / 2f - 0.1f, j), 
									Quaternion.identity, parent);
			}
		}
	}

	public PlayerInfo GetMyPlayer()
	{
		return gameData.MyPlayer;
	}

	public void SetPlayerPosition(string id, Coord newCoord, bool fromOuterThread = false)
	{
		if (fromOuterThread)
		{
			queueTask.Enqueue(() => SetPlayerPosition(id, newCoord));
			return;
		}

		playerObj[id].position = new Vector3(newCoord.X, 0.5f, newCoord.Y);
	}

	public void AddPlayer(PlayerInfo player, bool fromOuterThread = false)
	{
		if (fromOuterThread)
		{
			queueTask.Enqueue(() => AddPlayer(player));
			return;
		}

		if (playerDict.Keys.FirstOrDefault(item => item.ID == player.ID) != null)
		{
			return;
		}

		playerDict.Add(player, new Coord());

		var playerObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		Color color = Color.white;
		switch (player.Color)
		{
			case "Red": color = Color.red; break;
			case "Green": color = Color.green; break;
			case "Yellow": color = Color.yellow; break;
			case "Black": color = Color.black; break;
		}

		playerObj.GetComponent<Renderer>().material.color = color;
		this.playerObj.Add(player.ID, playerObj.transform);
		this.playerObj[player.ID].position = new Vector3(0, -10, 0);
	}

	public void RemovePlayer(PlayerInfo player, bool fromOuterThread = false)
	{
		if (fromOuterThread)
		{
			queueTask.Enqueue(() => RemovePlayer(player));
			return;
		}

		if (playerDict.Keys.FirstOrDefault(item => item.ID == player.ID) == null)
		{
			return;
		}
		Destroy(playerObj[player.ID].gameObject);
	}

	public void CreateMyPlayer()
	{
		if (this.playerObj.ContainsKey(playerInfo.ID))
		{
			return;
		}

		var playerObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		playerObj.name = $"MyPlayer({playerInfo.Nickname})";
		Color color = Color.white;
		switch (playerInfo.Color)
		{
			case "Red": color = Color.red; break;
			case "Green": color = Color.green; break;
			case "Yellow": color = Color.yellow; break;
			case "Black": color = Color.black; break;
		}
		playerObj.GetComponent<Renderer>().material.color = color;
		this.playerObj.Add(playerInfo.ID, playerObj.transform);
		this.playerObj[playerInfo.ID].position = new Vector3(0, -10, 0);
	}

	private void FixedUpdate()
	{
		if (queueTask.Count > 0)
		{
			for (int i = 0; i < queueTask.Count; i++)
			{
				queueTask.Dequeue().Invoke();
			}
		}

		if (client == null || !client.isConnected)
		{
			return;
		}

		InputHandler();
	}

	private void InputHandler()
	{
		Vector2 moveDir = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
		if (moveDir != Vector2.zero)
		{
			var header = new Header();
			header.type = MessageType.Coord;
			var newCoord = new Coord();

			if (moveDir.x > 0)
			{
				newCoord.X = 1;
			}
			else if (moveDir.x < 0)
			{
				newCoord.X = -1;
			}
			else
			{
				newCoord.X = 0;
			}

			if (moveDir.y > 0)
			{
				newCoord.Y = 1;
			}
			else if (moveDir.y < 0)
			{
				newCoord.Y = -1;
			}
			else
			{
				newCoord.Y = 0;
			}

			Serializer.SerializeWithLengthPrefix(client.stream, header, PrefixStyle.Fixed32);
			Serializer.SerializeWithLengthPrefix(client.stream, newCoord, PrefixStyle.Fixed32);
		}
	}

	private void OnDestroy()
	{
		client.Disconnect();
		client = null;
	}
}
