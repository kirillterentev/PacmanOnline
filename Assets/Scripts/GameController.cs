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

	private void Start()
	{
		gameData = new GameData();
	}

	public void ConnectToServer(string name, string color)
	{
		playerInfo = new PlayerInfo();
		playerInfo.Nickname = name;
		playerInfo.Color = color;

		client = new Client(this, playerInfo);
	}

	public void DrawGameField(GameField gameField)
	{
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

	private void OnDestroy()
	{
		client.Disconnect();
		client = null;
	}
}
