using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Collections;

namespace BlockGame
{
    class GameField
    {
        //One means there is a block there, zero no block
        private int[,] field;
        private const int width = 10;
        private const int height = 22;
        //The point -1, -1 represent no block. This is possible in our version of tetris when the block creator has not yet created a block
        //for us to place 
        public Point[] humanPosition 
        {
            get; 
            set
            { 
                if(value.Length == 4)
                    humanPosition=value;
            }
        }
        public Point[] lastPlacedBlock { get; private set; }
        private bool locked;

        public GameField()
        {
            field = new int[height,width];
            humanPosition = new Point[4];
            lastPlacedBlock = new Point[4];
            locked = false;
            Clear();
        }

        public void Clear()
        {
            for (int i = 0; i < height; i++)
            {
                for (int k = 0; k < width; k++)
                {
                    field[i, k] = 0;
                }
            }
            ResetHumanPosition();
            
        }

        private void ResetHumanPosition()
        {
            lastPlacedBlock = humanPosition;
            for (int i = 0; i < humanPosition.Length; i++)
            {
                humanPosition[i] = new Point(-1, -1);
            }
            humanPosition[0].Y = 0;
            humanPosition[0].X = width / 2;
            locked = true;
        }

        private bool Coolision(Point[] points)
        {
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i].Y == height - 1 || field[points[i].X, points[i].Y + 1] == 1)
                    return true;
            }
            return false;
        }

        public void LockShape(Point[] points)
        {
            if (!locked)
            {
                humanPosition = points;
                locked = true;
            }
        }

        public void MakeMove(PlayerMove move)
        {
            throw new NotImplementedException();
        }

        public void MoveTimeStep()
        {
            if (Coolision(humanPosition))
            {
                for (int i = 0; i < humanPosition.Length; i++)
                {
                    field[humanPosition[i].X, humanPosition[i].Y] = 1;
                }
                ResetHumanPosition();
            }
            else
            {
                for (int i = 0; i < humanPosition.Length; i++)
                    humanPosition[i].Y += 1;
            }

            //ENDAST FÖR TEST
            for (int i = 0; i < height; i++)
            {
                for (int k = 0; k < width; k++)
                {
                    System.Diagnostics.Debug.Write(field[i, k]+" ");
                }
                System.Diagnostics.Debug.WriteLine("");
            }
        }

        public void RemoveRow(int row)
        {
            for (int i = row; 0 < i; i--)
            {
                for (int k = 0; k < width; k++)
                {
                    field[i, k] = field[i-1,k];
                }
            }
        }

        public List<int> GetRowsForRemoval()
        {
            List<int> rows = new List<int>();
            for (int i = 0; i < height; i++)
            {
                bool fullRow = true;
                for (int k = 0; k < width; k++)
                {
                    if (field[i, k] != 1)
                    {
                        fullRow = false;
                        break;
                    }
                }
                if (fullRow)
                    rows.Add(i);
            }
            return rows;
        }
    }
}
