﻿using System;
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
        public Point destinationPos;
        public List<Point> open;
        public List<Point> path;
        
        public Point mapDimensions;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x">Map width</param>
        /// <param name="y">Map height</param>
        public AStar(int x, int y)
        {
            open = new List<Point>();
            path = new List<Point>();
            mapDimensions = new Point(x, y);
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
                    inPath[i, j] = false;
                    parent[i, j] = new Point(-1, -1);
                }
            }
        }

        /// <summary>
        /// Check if coordinate is within the bounds of the map.
        /// </summary>
        /// <param name="current">Current position</param>
        /// <param name="i">Appendage to current position on X</param>
        /// <param name="j">Appendage to current position on Y</param>
        /// <returns></returns>
        public bool IsValid(Point current, int i=0, int j=0)
        {
            if (current.X + i < 0 || current.X + i >= mapDimensions.X
                || current.Y + j < 0 || current.Y+ j >= mapDimensions.Y)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Find path across map.
        /// </summary>
        /// <param name="map">Boolean map of open (true) and closed (false) areas.</param>
        /// <param name="to">Destination position.</param>
        /// <param name="from">Start position.</param>
        public void Build(bool[,] map, Point to, Point from)
        {
            // If the current position or destination do not register as on the map, exit.
            if (!IsValid(to) || !IsValid(from))
                return;

            gCost[from.X, from.Y] = 0;
            Point currentPosition = from;
            closed[from.X, from.Y] = true;

            // If the input map indicates a space is occupied, add it to the closed list.
            for (int i = 0; i < mapDimensions.X; i++)
                for (int j = 0; j < mapDimensions.Y; j++)
                    if (map[i, j])
                        closed[i, j] = true;

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

                            SetCost(currentPosition, to, i, j);
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

        /// <summary>
        /// Set costs and parent of input cell.
        /// </summary>
        /// <param name="currentPosition">Current position.</param>
        /// <param name="dest">Destination position.</param>
        /// <param name="x">Appendage to current position on X.</param>
        /// <param name="y">Appendage to current position on Y.</param>
        private void SetCost(Point currentPosition, Point dest, int x, int y)
        {
            int moveCost;
            // Change the cost if moving diagonally.
            if (x == 0 || y == 0)
                moveCost = 10;
            else
                moveCost = 14;

            if (gCost[currentPosition.X + x, currentPosition.Y + y] > gCost[currentPosition.X, currentPosition.Y] + moveCost)
            {
                parent[currentPosition.X + x, currentPosition.Y + y] = currentPosition;
                gCost[currentPosition.X + x, currentPosition.Y + y] = gCost[currentPosition.X, currentPosition.Y] + moveCost;
                fCost[currentPosition.X + x, currentPosition.Y + y] = gCost[currentPosition.X + x, currentPosition.Y + y] +
                    (Math.Abs(dest.X - (currentPosition.X + x)) + Math.Abs(dest.Y - (currentPosition.Y + y))) * 10; // Manhattan euristic.
            }
        }
    }
}
