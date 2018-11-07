using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo
{
    public class CelestialGrid
    {
        protected GridPoint[,] points = null;

        public int Rows { get; private set; }

        public int Columns { get; private set; }

        public CelestialGrid(int rows, int columns)
        {
            points = new GridPoint[rows, columns];
            Rows = rows;
            Columns = columns;

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    double latitude = i * 10 - Rows / 2 * 10;
                    double longitude = j * 15;
                    points[i, j] = new GridPoint(longitude, latitude);
                    points[i, j].RowIndex = i;
                    points[i, j].ColumnIndex = j;
                }
            }
        }

        public GridPoint this[int row, int column]
        {
            get { return points[row, column]; }
            set { points[row, column] = value; }
        }

        public IEnumerable<GridPoint> Points
        {
            get
            {
                return points.Cast<GridPoint>();
            }
        }

        public IEnumerable<GridPoint> Column(int columnNumber)
        {
            return Enumerable.Range(0, points.GetLength(0))
                    .Select(x => points[x, columnNumber]);
        }

        public IEnumerable<GridPoint> Row(int rowNumber)
        {
            return Enumerable.Range(0, points.GetLength(1))
                    .Select(x => points[rowNumber, x]);
        }

        public Func<CrdsHorizontal, GridPoint> FromHorizontal { get; set; }
        public Func<GridPoint, CrdsHorizontal> ToHorizontal { get; set; }
    }

    public class GridPoint
    {
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }

        public double Longitude { get; set; }
        public double Latitude { get; set; }

        public GridPoint(double longitude, double latitude)
        {
            Longitude = longitude;
            Latitude = latitude;
        }
    }
}
