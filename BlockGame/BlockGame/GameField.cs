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
        //Origin is top left corner.
        public int[,] field { get; private set; }
        public Color[,] fieldColor { get; private set; }
        public const int width = 10;
        public const int height = 24;
        public const int invisibleRows = 4;
        public int score { private set; get; }
        //Initial game speed.
        public double gameSpeed { set; get; }
        public Color humanColor;
        //The point -1, -1 represent no block. This is possible in our version of tetris when the block creator has not yet created a block
        //for us to place 
        public Point[] humanPosition
        {
            get;
            private set;
        }
        public Vector2 pivotPoint
        {
            get
            {
                return _pivotPoint;
            }
        }
        private Vector2 _pivotPoint;
        public Point[] lastPlacedBlock { get; private set; }
        private bool locked;
        public bool gameOver {get; private set;}

        public GameField()
        {
            field = new int[width, height];
            fieldColor = new Color[width, height];
            humanPosition = new Point[4];
            lastPlacedBlock = new Point[4];
            Clear();
        }

        public void Clear()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    field[x, y] = 0;
                    fieldColor[x, y] = Color.Black;
                }
            }
            score = 0;
            gameSpeed = 1;
            gameOver = false;
            ResetHumanPosition();
        }

        private void ResetHumanPosition()
        {
            humanColor = Color.Red;
            humanColor.A = (byte) 100;
            lastPlacedBlock = humanPosition;
            for (int i = 0; i < humanPosition.Length; i++)
            {
                humanPosition[i] = new Point(-1, -1);
            }
            humanPosition[0].Y = 0;
            humanPosition[0].X = width / 2;
            _pivotPoint = new Vector2(humanPosition[0].X, humanPosition[0].Y);
            locked = false;
        }

        private bool Collision()
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

        public void LockShape(PoseType pose, Color color)
        {
            if (!locked)
            {
                humanColor = color;
                System.Diagnostics.Debug.WriteLine("LockShape "+pose);
                //Shape might be needed to be moved if at the corner
                Point[] shape = new Point[4];
                switch (pose)
                {
                    case PoseType.O:
                        shape[0] = new Point(humanPosition[0].X, humanPosition[0].Y);
                        shape[1] = new Point(humanPosition[0].X - 1, humanPosition[0].Y);
                        shape[2] = new Point(humanPosition[0].X, humanPosition[0].Y - 1);
                        shape[3] = new Point(humanPosition[0].X - 1, humanPosition[0].Y - 1);
                        _pivotPoint = new Vector2((float)(humanPosition[0].X - 0.5), (float)(humanPosition[0].Y - 0.5));
                        MoveToClosestAvailablePosition(shape);
                        break;
                    case PoseType.L:
                        shape[0] = new Point(humanPosition[0].X, humanPosition[0].Y + 1);
                        shape[1] = new Point(humanPosition[0].X, humanPosition[0].Y);
                        shape[2] = new Point(humanPosition[0].X, humanPosition[0].Y - 1);
                        shape[3] = new Point(humanPosition[0].X - 1, humanPosition[0].Y - 1);
                        _pivotPoint = new Vector2(humanPosition[0].X, humanPosition[0].Y);
                        MoveToClosestAvailablePosition(shape);
                        break;
                    case PoseType.J:
                        shape[0] = new Point(humanPosition[0].X, humanPosition[0].Y + 1);
                        shape[1] = new Point(humanPosition[0].X, humanPosition[0].Y);
                        shape[2] = new Point(humanPosition[0].X, humanPosition[0].Y - 1);
                        shape[3] = new Point(humanPosition[0].X + 1, humanPosition[0].Y - 1);
                        _pivotPoint = new Vector2(humanPosition[0].X, humanPosition[0].Y);
                        MoveToClosestAvailablePosition(shape);
                        break;
                    case PoseType.T:
                        shape[0] = new Point(humanPosition[0].X - 1, humanPosition[0].Y);
                        shape[1] = new Point(humanPosition[0].X, humanPosition[0].Y);
                        shape[2] = new Point(humanPosition[0].X + 1, humanPosition[0].Y);
                        shape[3] = new Point(humanPosition[0].X, humanPosition[0].Y + 1);
                        _pivotPoint = new Vector2(humanPosition[0].X, humanPosition[0].Y);
                        MoveToClosestAvailablePosition(shape);
                        break;
                    case PoseType.I:
                        shape[0] = new Point(humanPosition[0].X, humanPosition[0].Y - 2);
                        shape[1] = new Point(humanPosition[0].X, humanPosition[0].Y - 1);
                        shape[2] = new Point(humanPosition[0].X, humanPosition[0].Y);
                        shape[3] = new Point(humanPosition[0].X, humanPosition[0].Y + 1);
                        _pivotPoint = new Vector2((float)(humanPosition[0].X + 0.5), (float)(humanPosition[0].Y - 0.5));
                        MoveToClosestAvailablePosition(shape);
                        break;
                    case PoseType.S:
                        shape[0] = new Point(humanPosition[0].X, humanPosition[0].Y - 1);
                        shape[1] = new Point(humanPosition[0].X, humanPosition[0].Y);
                        shape[2] = new Point(humanPosition[0].X + 1, humanPosition[0].Y);
                        shape[3] = new Point(humanPosition[0].X + 1, humanPosition[0].Y + 1);
                        _pivotPoint = new Vector2(humanPosition[0].X + 1, humanPosition[0].Y);
                        MoveToClosestAvailablePosition(shape);
                        break;
                    case PoseType.Z:
                        shape[0] = new Point(humanPosition[0].X, humanPosition[0].Y);
                        shape[1] = new Point(humanPosition[0].X, humanPosition[0].Y + 1);
                        shape[2] = new Point(humanPosition[0].X + 1, humanPosition[0].Y - 1);
                        shape[3] = new Point(humanPosition[0].X + 1, humanPosition[0].Y);
                        _pivotPoint = new Vector2(humanPosition[0].X + 1, humanPosition[0].Y);
                        MoveToClosestAvailablePosition(shape);
                        break;
                    case PoseType.NO_POSE:
                    default:
                        break;
                }
                locked = true;
            }
        }

        //Finds a free position for a given shape by testing positions in the following pattern:
        /*
         * 8 6 7
         * 5 3 4
         * 2 0 1
         */
        private void MoveToClosestAvailablePosition(Point[] shape)
        {
            int[] xDisplacement = { 0, 1, -1 };
            bool blocked = false;
            Point[] testPoints = new Point[humanPosition.Length];
            Point displacement = new Point(0, 0);
            for (int i = 0; i < humanPosition.Length; i++)
                testPoints[i] = new Point();

            //Overestimation of the inequality, but we won't get out of this loop a lot anyway.
            for (int n = 0; n < 3 * height; n++)
            {
                displacement.X = xDisplacement[n % 3];
                displacement.Y = -n / 3;
                blocked = false;
                //Add displacement to test points.
                for (int i = 0; i < humanPosition.Length; i++)
                {
                    testPoints[i].X = shape[i].X + displacement.X;
                    testPoints[i].Y = shape[i].Y + displacement.Y;
                }
                //Check if any of the displaced test points are blocked.
                for (int i = 0; i < humanPosition.Length; i++)
                    if (IsOccupied(testPoints[i]))
                        blocked = true;
                //If none are blocked, return the current test points.
                if (!blocked)
                {
                    humanPosition = testPoints;
                    _pivotPoint.X += displacement.X;
                    _pivotPoint.Y += displacement.Y;
                    return;
                }
            }
            
            //If no valid position was found, let the shape stay as a unit block.
            System.Diagnostics.Debug.WriteLine("Found no suitable position for block at " + humanPosition[0] + ".");
        }

        //Returns true if the point is occupied or outside of the game field.
        private bool IsOccupied(Point p)
        {
            return (p.X < 0 || p.X >= width || p.Y < 0 || p.Y >= height || field[p.X, p.Y] == 1);
        }

        public void MakeMove(PlayerMove move)
        {
            switch(move)
            {
                case PlayerMove.GO_DOWN:
                    MoveHumanBlockDownwards();
                    break;
                case PlayerMove.GO_LEFT:
                    MoveHumanBlockSideways(-1);
                    break;
                case PlayerMove.GO_RIGHT:
                    MoveHumanBlockSideways(1);
                    break;
                case PlayerMove.ROTATE_LEFT:
                    if(pivotPoint.X > 0 && pivotPoint.X < width-1 && pivotPoint.Y < height-1 
                        && field[(int)pivotPoint.X,(int)pivotPoint.Y + 1] != 1)
                        Rotate(-1);
                    break;
                case PlayerMove.ROTATE_RIGHT:
                    if (pivotPoint.X > 0 && pivotPoint.X < width - 1 && pivotPoint.Y < height - 1
                        && field[(int)pivotPoint.X, (int)pivotPoint.Y + 1] != 1)
                        Rotate(1);
                    break;
                case PlayerMove.NO_MOVE:
                default:
                    break;
            }
        }

        private void MoveHumanBlockSideways(int direction)
        {
            Point[] tempMove = { humanPosition[0], humanPosition[1], humanPosition[2], humanPosition[3]};
            bool collision = false;
            for(int i = 0; i < tempMove.Length;i++)
            {
                if (tempMove[i].X < 0)
                    continue;
                tempMove[i].X +=direction;
                if (IsOccupied(tempMove[i]))
                {
                    collision = true;
                    break;
                }
            }

            if (!collision)
            {
                humanPosition = tempMove;
                _pivotPoint.X += direction;
            }
        }

        private void MoveHumanBlockDownwards()
        {
            while (!Collision())
            {
                _pivotPoint.Y = _pivotPoint.Y + 1;
                for (int i = 0; i < humanPosition.Length; i++)
                    humanPosition[i].Y += 1;
            }
        }

        //direction should be 1 for clockwise rotation -1 for counter-closkwise
        private void Rotate(int direction)
        {
            System.Diagnostics.Debug.WriteLine("Rotate");
            Point[] tempRotation = {humanPosition[0], humanPosition[1], humanPosition[2], humanPosition[3] };
            bool collision = false;
            for (int i = 0; i < humanPosition.Length; i++)
            {
                if (humanPosition[i].X < 0)
                    continue;
                Vector2 translationCoordinate = new Vector2(humanPosition[i].X - pivotPoint.X, humanPosition[i].Y - pivotPoint.Y);
                translationCoordinate.Y *= direction;
                System.Diagnostics.Debug.WriteLine(translationCoordinate.Y + " " + translationCoordinate.X);
                Vector2 rotatedCoordinate = new Vector2(-translationCoordinate.Y, translationCoordinate.X);

                rotatedCoordinate.Y *= direction;
                rotatedCoordinate.X += pivotPoint.X;
                rotatedCoordinate.Y += pivotPoint.Y;

                tempRotation[i].X = (int)Math.Round(rotatedCoordinate.X, MidpointRounding.AwayFromZero);
                tempRotation[i].Y = (int)Math.Round(rotatedCoordinate.Y, MidpointRounding.AwayFromZero);
                System.Diagnostics.Debug.WriteLine("X: " + " Y: " + tempRotation[i].Y);
                if (IsOccupied(tempRotation[i]))
                {
                    collision = true;
                    break;
                }
            }

            if (!collision)
            {
                humanPosition = tempRotation;
            }
        }

        //Return true if MoveTimeStep had to reset human position and we need a new block
        public bool MoveTimeStep()
        {
            if (gameOver)
            {
                return true;
            }
                
            bool hasResetHumanPosition = false;
            if (Collision())
            {
                if (locked)
                {
                    System.Diagnostics.Debug.WriteLine("Coolision! Awww yeah! So cool!");
                    for (int i = 0; i < humanPosition.Length; i++)
                    {
                        if (humanPosition[i].X < 0 || humanPosition[i].Y < 0)
                            break;
                        field[humanPosition[i].X, humanPosition[i].Y] = 1;
                        fieldColor[humanPosition[i].X, humanPosition[i].Y] = humanColor;

                    }
                    RemoveRows();
                }
                else
                {
                    CreateTrash();
                }
                //Check if game is over
                for (int i = 0; i < width; i++)
                {
                    for (int k = 0; k < invisibleRows; k++)
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
                _pivotPoint.Y = _pivotPoint.Y + 1;
                for (int i = 0; i < humanPosition.Length; i++)             
                    humanPosition[i].Y += 1;
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

        private void CreateTrash()
        {
            Random random = new Random();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (y < height - 1)
                    {
                        field[x, y] = field[x, y + 1];
                        fieldColor[x, y] = fieldColor[x, y + 1];
                    }
                    else
                    {
                        field[x, y] = 0;
                    }
                }
            }
            for (int i = 0; i < width - 2; i++)
            {
                int x = random.Next(0, width);
                field[x, height-1] = 1;
                fieldColor[x, height - 1] = Color.Black;
            }
        }

        private void RemoveRow(int row)
        {
            for (int i = 0; i < width; i++)
            {
                for (int k = row; 0 < k; k--)
                {

                    field[i, k] = field[i, k - 1];
                    fieldColor[i, k] = fieldColor[i, k - 1];
                }
            }
        }

        private void RemoveRows()
        {
            int rowsRemoved = 0;
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
                {
                    RemoveRow(i);
                    rowsRemoved++;
                }
            }
            if (rowsRemoved > 0)
            {
                int[] lineScore = { 100, 300, 500, 800 };
                score += (int) (lineScore[rowsRemoved - 1] * gameSpeed);
            }
        }
    }
}
