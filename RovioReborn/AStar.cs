using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace PredatorPreyAssignment
{
    class AStar
    {

        public bool[,] closed;
        public float[,] fCost;
        public float[,] gCost;
        public bool[,] inPath;

        public Point[,] parent;
        public List<Point> open;
        public List<Point> path;
        public Point destinationPos;

        public Point mapDimensions;

        public AStar(int x, int y)
        {

            mapDimensions = new Point(x, y);
            open = new List<Point>();
            path = new List<Point>();
            destinationPos = new Point(-1, -1);

            closed = new bool[x, y];
            fCost = new float[x, y];
            gCost = new float[x, y];
            inPath = new bool[x, y];
            parent = new Point[x, y];

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    fCost[i, j] = int.MaxValue;
                    gCost[i, j] = int.MaxValue;
                    closed[i, j] = false;
                    parent[i, j] = new Point(-1, -1);
                    inPath[i, j] = false;
                }
            }
        }

        public bool IsValid(Point current, int i=0, int j=0)
        {
            if (current.X + i < 0 || current.X + i >= mapDimensions.X
                || current.Y + j < 0 || current.Y+ j >= mapDimensions.Y)
                return false;
            else
                return true;
        }

        public void Build(bool[,] map, Point to, Point from)
        {

            if (!IsValid(to) || !IsValid(from))
                return;

            gCost[from.X, from.Y] = 0;
            Point currentPosition = from;
            closed[from.X, from.Y] = true;


            for (int i = 0; i < mapDimensions.X; i++)
            {
                for (int j = 0; j < mapDimensions.Y; j++)
                {
                    if (map[i, j])
                        closed[i, j] = true;
                }
            }
            while (!closed[to.X, to.Y])
            {
                
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    { 
                        if (IsValid(currentPosition, i, j) && !closed[currentPosition.X+i, currentPosition.Y+j])
                        {
                            if (!open.Contains(new Point(currentPosition.X + i, currentPosition.Y + j)))
                                open.Add(new Point(currentPosition.X + i, currentPosition.Y + j));


                            bool diagonal;
                            if (i == 0 || j == 0)
                                diagonal = false;
                            else
                                diagonal = true;

                            SetCost(currentPosition, to, i, j, diagonal);
                        }
                    }
                }

                float lowestCost = int.MaxValue;
                Point nextPosition = currentPosition;
                for (int i = 0; i < open.Count; i++)
                {
                    if (fCost[open[i].X, open[i].Y] < lowestCost)
                    {
                        lowestCost = fCost[open[i].X, open[i].Y];
                        nextPosition = new Point(open[i].X, open[i].Y);
                    }
                }
                if (currentPosition == nextPosition)
                    break;
                open.Remove(nextPosition);
                closed[nextPosition.X, nextPosition.Y] = true;
                currentPosition = nextPosition;

                bool done = false;
                Point nextClose = to;

                while (!done)
                {
                    if (nextClose.X == -1)
                        break;
                    if (nextClose == from)
                    {
                        destinationPos = to;
                        done = true;
                    }
                    inPath[nextClose.X, nextClose.Y] = true;
                    path.Add(new Point(nextClose.X, nextClose.Y));
                    nextClose = parent[nextClose.X, nextClose.Y];
                }
            }

            
        }

        private void SetCost(Point currentPosition, Point dest, int x, int y, bool diag)
        {
            int moveCost;
            if (!diag)
                moveCost = 10;
            else
                moveCost = 14;

            if (gCost[currentPosition.X + x, currentPosition.Y + y] > gCost[currentPosition.X, currentPosition.Y] + moveCost)
            {
                parent[currentPosition.X + x, currentPosition.Y + y] = currentPosition;
                gCost[currentPosition.X + x, currentPosition.Y + y] = gCost[currentPosition.X, currentPosition.Y] + moveCost;
                fCost[currentPosition.X + x, currentPosition.Y + y] = gCost[currentPosition.X + x, currentPosition.Y + y] +
                    (Math.Abs(dest.X - (currentPosition.X + x)) + Math.Abs(dest.Y - (currentPosition.Y + y))) * 10; // Heuristic.

            }
        }
    }
}
