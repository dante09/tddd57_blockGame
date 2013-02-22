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
        private const int height = 24;
        //The point -1, -1 represent no block. This is possible in our version of tetris when the block creator has not yet created a block
        //for us to place 
        public Point[] humanPosition
        {
            get;
            private set;
        }
        Vector2 pivotPoint;
        public Point[] lastPlacedBlock { get; private set; }
        private bool locked;
        private bool gameOver;

        public GameField()
        {
            field = new int[width, height];
            humanPosition = new Point[4];
            lastPlacedBlock = new Point[4];
            Clear();
        }

        public void Clear()
        {
            for (int i = 0; i < width; i++)
            {
                for (int k = 0; k < height; k++)
                {
                    field[i, k] = 0;
                }
            }
            gameOver = false;
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
            pivotPoint = new Vector2(humanPosition[0].Y, humanPosition[0].X);
            locked = false;
        }

        private bool Coolision()
        {
            for (int i = 0; i < humanPosition.Length; i++)
            {
                if (humanPosition[i].X < 0 || humanPosition[i].Y < 0)
                    continue;
                if (humanPosition[i].Y == height - 1 ||humanPosition[i].Y+1>=height
                    || field[humanPosition[i].X, humanPosition[i].Y + 1] == 1)
                    return true;
            }
            return false;
        }

        public void LockShape(PoseType pose)
        {
            if (!locked)
            {
                System.Diagnostics.Debug.WriteLine("LockShape"+pose);
                //Shape might be needed to be moved if at the corner
                int x1,x2,x = x1 = x2 = humanPosition[0].X;
                if (x == 0)
                    x1 += 1;
                else if (x == width - 1)
                    x2 -= 1;
                Point[] shape = new Point[4];
                switch (pose)
                {
                    case PoseType.L:
                        shape[0] = new Point(x1, humanPosition[0].Y);
                        shape[1] = new Point(x1, humanPosition[0].Y - 1);
                        shape[2] = new Point(x1, humanPosition[0].Y-2);
                        shape[3] = new Point(x1-1, humanPosition[0].Y-2);
                        pivotPoint = new Vector2(x1, humanPosition[0].Y - 1);
                        humanPosition = shape;
                        break;
                    case PoseType.O:
                        shape[0] = new Point(x1, humanPosition[0].Y);
                        shape[1] = new Point(x1-1, humanPosition[0].Y);
                        shape[2] = new Point(x1, humanPosition[0].Y-1);
                        shape[3] = new Point(x1-1, humanPosition[0].Y-1);
                        pivotPoint = new Vector2((float)(x1 - 0.5), (float)(humanPosition[0].Y - 0.5));
                        humanPosition = shape;
                        break;
                    case PoseType.NO_POSE:
                    default:
                        break;
                }
                locked = true;
            }
        }

        public void MakeMove(PlayerMove move)
        {
            switch(move)
            {
                case PlayerMove.GO_DOWN:
                    break;
                case PlayerMove.GO_LEFT:
                    break;
                case PlayerMove.GO_RIGHT:
                    break;
                case PlayerMove.ROTATE_LEFT:
                    if(pivotPoint.X > 0 && pivotPoint.X < width-1 && pivotPoint.Y < height-1 
                        && field[(int)pivotPoint.Y+1,(int)pivotPoint.X] != 1)
                        Rotate(-1);
                    break;
                case PlayerMove.ROTATE_RIGHT:
                    if (pivotPoint.X > 0 && pivotPoint.X < width - 1 && pivotPoint.Y < height - 1
                        && field[(int)pivotPoint.Y + 1, (int)pivotPoint.X] != 1)
                        Rotate(1);
                    break;
                case PlayerMove.NO_MOVE:
                default:
                    break;
            }
            throw new NotImplementedException();
        }

        //direction should be 1 for clockwise rotation -1 for counter-closkwise
        private void Rotate(int direction)
        {
            System.Diagnostics.Debug.WriteLine("Rotate");
            for (int i = 0; i < humanPosition.Length ;i++ )
            {
                Vector2 translationCoordinate = new Vector2(humanPosition[i].X - pivotPoint.X, humanPosition[i].Y - pivotPoint.Y);
                translationCoordinate.Y *= direction;
                System.Diagnostics.Debug.WriteLine(translationCoordinate.Y + " " + translationCoordinate.X);
                Vector2 rotatedCoordinate = new Vector2((float)Math.Truncate((double)(-translationCoordinate.Y)),
                    (float)Math.Truncate((double)translationCoordinate.X));

                rotatedCoordinate.Y *= direction;
              // System.Diagnostics.Debug.WriteLine(rotatedCoordinate.Y + " " + rotatedCoordinate.X); 
                rotatedCoordinate.X += pivotPoint.X;
                rotatedCoordinate.Y += pivotPoint.Y;      

                humanPosition[i].X = (int)Math.Truncate(rotatedCoordinate.X);
                humanPosition[i].Y = (int)Math.Truncate(rotatedCoordinate.Y);
                //System.Diagnostics.Debug.WriteLine(humanPosition[i].X + " " + humanPosition[i].Y); 
            }
        }

        //Return true if MoveTimeStep had to reset human position and we need a new block
        public bool MoveTimeStep()
        {
            if (gameOver)
            {
                return false;
            }
                
            bool hasResetHumanPosition = false;
            if (Coolision())
            {
                System.Diagnostics.Debug.WriteLine("Coolision");
                for (int i = 0; i < humanPosition.Length; i++)
                {
                    if (humanPosition[i].X < 0 || humanPosition[i].Y < 0)
                        break;
                    field[humanPosition[i].X, humanPosition[i].Y] = 1;
                }
                RemoveRows();
                //Check if game is over
                for (int i = 0; i < width; i++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        if (field[i, k] == 1)
                        {
                            gameOver = true;
                            break;
                        }
                    }
                }
                ResetHumanPosition();
                hasResetHumanPosition = true;
            }
            else
            {
                for (int i = 0; i < humanPosition.Length; i++)
                {
                    pivotPoint.Y += 1;
                    humanPosition[i].Y += 1;
                }
            }

            //ENDAST FÖR TEST
            if (hasResetHumanPosition)
            {
                for (int i = 0; i < height; i++)
                {
                    for (int k = 0; k < width; k++)
                    {
                        System.Diagnostics.Debug.Write(field[k, i] + " ");
                    }
                    System.Diagnostics.Debug.WriteLine("");
                }
            }
            return hasResetHumanPosition;
        }

        private void RemoveRow(int row)
        {
            for (int i = 0; i < width; i++)
            {
                for (int k = row; 0 < k; k--)
                {

                    field[i, k] = field[i,k-1];
                }
            }
        }

        private void RemoveRows()
        {
            for (int i = 0; i < height; i++)
            {
                bool fullRow = true;
                for (int k = 0; k < width; k++)
                {
                    if (field[k, i] != 1)
                    {
                        fullRow = false;
                        break;
                    }
                }
                if (fullRow)
                    RemoveRow(i);
            }
        }
    }
}
