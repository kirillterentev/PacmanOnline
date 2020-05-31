using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ClientPacman
{
	public class GameController : MonoBehaviour
	{
		[Header("Префабы блоков для постройки карты")]
		[SerializeField]
		private GameObject planeQuad;
		[SerializeField]
		private GameObject highQuad;

		private Client client;
		private GameData gameData;
		private PlayerInfo playerInfo;
		private Queue<Action> queueTask;
		private Transform playersParent;

		protected internal Dictionary<PlayerInfo, Coord> PlayerDict;
		protected internal Dictionary<string, Transform> PlayerTransforms;

		private void Start()
		{
			gameData = new GameData();
			queueTask = new Queue<Action>();
			PlayerDict = new Dictionary<PlayerInfo, Coord>();
			PlayerTransforms = new Dictionary<string, Transform>();

			playersParent = new GameObject("Players").transform;
		}

		public void ConnectToServer(string name, string color)
		{
			playerInfo = new PlayerInfo();
			playerInfo.Nickname = name;
			playerInfo.Color = color;
			playerInfo.ID = Guid.NewGuid().ToString();
			playerInfo.Status = Status.Connected;

			client = new Client(this, playerInfo);
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
					var go = Instantiate(cellPrefab,
						new Vector3(i, cellPrefab.transform.localScale.y / 2f - 0.1f, j),
						Quaternion.identity, parent);
				}
			}
		}

		public void SetPlayerPosition(string id, Coord newCoord, bool fromOuterThread = false)
		{
			if (fromOuterThread)
			{
				queueTask.Enqueue(() => SetPlayerPosition(id, newCoord));
				return;
			}

			PlayerTransforms[id].position = new Vector3(newCoord.X, 0.5f, newCoord.Y);
		}

		public void AddPlayer(PlayerInfo player, bool fromOuterThread = false)
		{
			if (fromOuterThread)
			{
				queueTask.Enqueue(() => AddPlayer(player));
				return;
			}

			if (PlayerDict.Keys.FirstOrDefault(item => item.ID == player.ID) != null)
			{
				return;
			}

			PlayerDict.Add(player, new Coord());

			var playerObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			playerObj.name = $"Player({playerInfo.Nickname})";
			playerObj.transform.parent = playersParent;

			Color color = Color.white;
			switch (player.Color)
			{
				case "Red":
					color = Color.red;
					break;
				case "Blue":
					color = Color.blue;
					break;
				case "Yellow":
					color = Color.yellow;
					break;
				case "Black":
					color = Color.black;
					break;
			}

			playerObj.GetComponent<Renderer>().material.color = color;

			PlayerTransforms.Add(player.ID, playerObj.transform);
			PlayerTransforms[player.ID].position = new Vector3(-10, -10, -10);
		}

		public void RemovePlayer(PlayerInfo player, bool fromOuterThread = false)
		{
			if (fromOuterThread)
			{
				queueTask.Enqueue(() => RemovePlayer(player));
				return;
			}

			if (PlayerDict.Keys.FirstOrDefault(item => item.ID == player.ID) == null)
			{
				return;
			}

			Destroy(PlayerTransforms[player.ID].gameObject);
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

			InputHandler();
		}

		private void InputHandler()
		{
			if (client == null || !client.IsConnected)
			{
				return;
			}

			Vector2 moveDir = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
			if (moveDir != Vector2.zero)
			{
				var newCoord = new Coord();

				if (moveDir.x != 0)
				{
					newCoord.X = moveDir.x > 0 ? Mathf.CeilToInt(moveDir.x) : Mathf.FloorToInt(moveDir.x);
				}

				if (moveDir.y != 0)
				{
					newCoord.Y = moveDir.y > 0 ? Mathf.CeilToInt(moveDir.y) : Mathf.FloorToInt(moveDir.y);
				}

				client.WriteMessage<Coord>(MessageType.Coord, newCoord);
			}
		}

		private void OnDestroy()
		{
			client?.Disconnect();
		}
	}
}
