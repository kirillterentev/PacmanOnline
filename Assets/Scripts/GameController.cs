using UnityEngine;

public class GameController : MonoBehaviour
{
	[SerializeField]
	private GameObject planeQuad;
	[SerializeField]
	private GameObject highQuad;

	private Client client;

	private PacmanField pacmanField;
	public PacmanField PacmanField
	{
		get
		{
			if (pacmanField == null)
			{
				pacmanField = new PacmanField();
			}

			return pacmanField;
		}
	}

	private void Start()
	{
		client = new Client(this);
	}

	private void OnDestroy()
	{
		client.Disconnect();
		client = null;
	}

	public void DrawGameField()
	{
		var field = pacmanField.GetField();
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
}
