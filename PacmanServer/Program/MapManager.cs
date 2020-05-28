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
			var field = pacmanField.GetField();
			int xSize = field.GetLength(0);
			int ySize = field.GetLength(1);

			bool cellIsClose = false;
			Coord coord = new Coord();
			Random rnd = new Random();
			do
			{
				coord.X = rnd.Next(0, xSize);
				coord.Y = rnd.Next(0, ySize);
				cellIsClose = field[coord.X, coord.Y];


			} while (!cellIsClose);

			return coord;
		}
	}
}
