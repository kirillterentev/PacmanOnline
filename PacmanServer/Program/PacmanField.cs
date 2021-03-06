﻿using System;
using System.Collections.Generic;
using System.IO;

namespace PacmanServer
{
    class PacmanField
    {
        private bool[,] field;
        private GameField fieldProto;
		private string[] splitStr;

		public void ReadFieldFromFile()
        {
            string[] lines = File.ReadAllLines(".\\pacman_field.txt");
            int coordX = 0;
            int coordY = 0;

            ParseLine(lines[0], ref coordX, ref coordY);

            field = new bool[coordX, coordY];

            Coord coordProto = new Coord();
	        coordProto.X = coordX;
	        coordProto.Y = coordY;

            fieldProto = new GameField();
	        fieldProto.Size = coordProto;
			fieldProto.Cells = new List<Coord>();

			int linesCount = lines.Length;
			for (int i = 1; i < linesCount; i++)
            {
                ParseLine(lines[i], ref coordX, ref coordY);
                field[coordX, coordY] = true;

                Coord cell = new Coord();
	            cell.X = coordX;
	            cell.Y = coordY;
                fieldProto.Cells.Add(cell);
            }
        }

        private void ParseLine(string line, ref int x, ref int y)
        {
            splitStr = line.Split(';');
            x = Convert.ToInt32(splitStr[0]);
            y = Convert.ToInt32(splitStr[1]);
        }

        public bool[,] GetField()
        {
            return field;
        }

        public GameField GetFieldProto()
        {
            return fieldProto;
        }
    }
}
