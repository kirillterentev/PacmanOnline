using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

	protected internal List<PlayerInfo> playersList = new List<PlayerInfo>();

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

		client = new Client(this, playerInfo);

		playersList.Add(playerInfo);
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

	public void SetMyPlayerPosition(Coord newCoord)
	{
		Debug.Log($"My new pos : {newCoord.X};{newCoord.Y}");
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
	}

	private void OnDestroy()
	{
		client.Disconnect();
		client = null;
	}
}
