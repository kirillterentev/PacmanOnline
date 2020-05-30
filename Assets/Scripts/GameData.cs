public class GameData
{
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

	public PlayerInfo MyPlayer;
}
