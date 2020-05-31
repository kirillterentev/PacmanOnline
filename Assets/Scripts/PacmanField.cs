namespace ClientPacman
{
	public class PacmanField
	{
		private bool[,] field;
		private GameField fieldProto;

		public void WriteFromGameField()
		{
			field = new bool[fieldProto.Size.X, fieldProto.Size.Y];

			foreach (var cell in fieldProto.Cells)
			{
				field[cell.X, cell.Y] = true;
			}
		}

		public bool[,] GetField()
		{
			return field;
		}

		public void SetFieldProto(GameField gameField)
		{
			fieldProto = gameField;
		}
	}
}
