using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace Snake_Game
{
    public class GameState
    {
        public int Rows { get; }
        public int Columns { get; }
        public GridValue[,] Grid { get; }
        public Direction Dir { get; private set; }
        public int Score { get; private set; }
        public System.Windows.Window MainWindow { get; set; }
        public bool GameOver { get; private set; }

        private readonly LinkedList<Direction> dirChanges = new LinkedList<Direction>();
        private readonly LinkedList<Position> snakePositions = new LinkedList<Position>();
        private readonly Random random = new Random();
        bool crownDropped = false;
        public bool crownGrabbed = false;
        public bool easyCompleted = false;
        public bool normalCompleted = false;

        public GameState(int rows, int cols)
        {
            Rows = rows; Columns = cols;
            Grid = new GridValue[rows, cols];
            Dir = Direction.Right;

            AddSnake();
            AddFood();

        }
        private void AddSnake()
        {
            int r = Rows / 2;
            for (int i = 1; i <= 3; i++)
            {
                Grid[r, i] = GridValue.Snake;
                snakePositions.AddFirst(new Position(r, i));
            }
        }

        private IEnumerable<Position> EmptyPositions()
        {
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {
                    if (Grid[r, c] == GridValue.Empty)
                        yield return new Position(r, c);
                }
            }
        }
        // Finds all of the empty positions using the EmptyPositions
        // method and finds a random value from that enumerable to 
        // pick to have the food.
        private void AddFood()
        {
            List<Position> empty = new List<Position>(EmptyPositions());

            if (empty.Count == 0)
                return;
            Position pos = empty[random.Next(empty.Count)];
            Grid[pos.Row, pos.Col] = GridValue.Food;

        }

        private void AddCrown()
        {
            List<Position> empty = new List<Position>(EmptyPositions());

            if (empty.Count == 0)
                return;
            Position pos = empty[random.Next(empty.Count)];
            Grid[pos.Row, pos.Col] = GridValue.Crown;

        }

        private void AddBomb()
        {
            List<Position> empty = new List<Position>(EmptyPositions());

            if (empty.Count == 0)
                return;
            Position pos = empty[random.Next(empty.Count)];
            Grid[pos.Row, pos.Col] = GridValue.Bomb;

        }

        public Position HeadPosition()
        {
            return snakePositions.First.Value;
        }
        public Position TailPosition()
        {
            return snakePositions.Last.Value;
        }

        public IEnumerable<Position> SnakePositions()
        {
            return snakePositions;
        }

        private void AddHead(Position pos)
        {
            snakePositions.AddFirst(pos);
            Grid[pos.Row, pos.Col] = GridValue.Snake;
        }

        private void RemoveTail()
        {
            Position tail = snakePositions.Last.Value;
            Grid[tail.Row, tail.Col] = GridValue.Empty;
            snakePositions.RemoveLast();
        }

        private Direction GetLastDirection()
        {
            if (dirChanges.Count == 0)
                return Dir;
            return dirChanges.Last.Value;
        }

        private bool CanChangeDirection(Direction newDir)
        {
            if (dirChanges.Count >= 2)
                return false;

            Direction lastDir = GetLastDirection();
            return (newDir != lastDir && newDir != lastDir.Opposite());

        }

        public void ChangeDirection(Direction dir)
        {
            // can the change be made, if so, add it to the buffer
            if (CanChangeDirection(dir))
                dirChanges.AddLast(dir);
        }

        private bool OutsideGrid(Position pos)
        {
            return pos.Row < 0 || pos.Row >= Rows || pos.Col < 0 || pos.Col >= Columns;
        }

        private GridValue WillHit(Position pos)
        {
            if (OutsideGrid(pos))
                return GridValue.Outside;

            if (pos == TailPosition())
                return GridValue.Empty;

            return Grid[pos.Row, pos.Col];
        }

        public void Move()
        {
            int crownDropPercentage = 5;
            int bombDropPercentage = 35;
            int randomValueBetween0And99 = 0;
            if (dirChanges.Count > 0)
            {
                Dir = dirChanges.First.Value;
                dirChanges.RemoveFirst();
            }

            Position newHeadPos = HeadPosition().Translate(Dir);
            GridValue hit = WillHit(newHeadPos);

            if (hit == GridValue.Outside || hit == GridValue.Snake)
                GameOver = true;
            else if (hit == GridValue.Empty)
            {
                RemoveTail();
                AddHead(newHeadPos);
            }
            else if (hit == GridValue.Food)
            {
                AddHead(newHeadPos);
                Score++;
                if (Score >= 5)
                {
                    // If the score is above a certain value,
                    // then we will have a 5% chance to spawn a crown
                    // every time a food is eaten.
                    randomValueBetween0And99 = random.Next(100);
                    if (randomValueBetween0And99 < crownDropPercentage && crownDropped == false)
                        AddCrown();
                    if (randomValueBetween0And99 < bombDropPercentage)
                        AddBomb();
                }
                if (Score == 10)
                    AddFood();
                AddFood();
            }
            else if (hit == GridValue.Crown && easyCompleted == false)
            {
                easyCompleted = true;
                RemoveTail();
                AddHead(newHeadPos);
                crownGrabbed = true;
            }
            else if (hit == GridValue.Crown && easyCompleted == true)
            {
                RemoveTail();
                AddHead(newHeadPos);
                crownGrabbed = true;
            }
            else if (hit == GridValue.Bomb)
            {
                RemoveTail();
                AddHead(newHeadPos);
                GameOver = true;
            }
        }
    }
}
