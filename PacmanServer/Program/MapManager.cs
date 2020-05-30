using System;

namespace PacmanServer
{
	class MapManager
	{
		ServerObject server;
		PacmanField pacmanField;
		public PacmanField PacmanField
		{
			get
			{
				if (pacmanField == null)
				{
					pacmanField = new PacmanField();
					pacmanField.ReadFieldFromFile();
				}

				return pacmanField;
			}
		}

		public MapManager(ServerObject serverObject)
		{
			server = serverObject;
		}

		public Coord GetFreePoint()
		{
			var field = PacmanField.GetField();
			int xSize = field.GetLength(0);
			int ySize = field.GetLength(1);

			bool cellIsClose = true;
			Coord coord = new Coord();
			Random rnd = new Random();
			do
			{
				coord.X = rnd.Next(0, xSize);
				coord.Y = rnd.Next(0, ySize);
				cellIsClose = field[coord.X, coord.Y];

			} while (cellIsClose);

			return coord;
		}

		public Coord CalculateNextPos(Coord startPos, Coord dir)
		{
			Coord coord = new Coord();
			var field = pacmanField.GetField();

			if (dir.Y != 0 && !field[startPos.X, startPos.Y + dir.Y])
			{
				coord.X = startPos.X;
				coord.Y = startPos.Y + dir.Y;
				return coord;
			}

			if (dir.X != 0 && !field[startPos.X + dir.X, startPos.Y])
			{
				coord.X = startPos.X + dir.X;
				coord.Y = startPos.Y;
				return coord;
			}

			return startPos;
		}
	}
}
