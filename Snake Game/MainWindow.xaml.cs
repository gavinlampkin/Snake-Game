using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Snake_Game
{
    public partial class MainWindow : Window
    {
        private readonly Dictionary<GridValue, ImageSource> gridValueToImage = new()
        {
            { GridValue.Empty, Images.Empty },
            { GridValue.Snake, Images.Body },
            { GridValue.Food, Images.Food },
            { GridValue.Crown, Images.Crown },
            { GridValue.Gold_Body, Images.Gold_Body },
            { GridValue.Gold_Head, Images.Gold_Head },
            { GridValue.Bomb, Images.Bomb }
        };

        private readonly Dictionary<Direction, int> dirToRotate = new()
        {
            { Direction.Up, 0 },
            { Direction.Right, 90 },
            { Direction.Down, 180 },
            { Direction.Left, 270 }
        };

        private readonly int rows = 15, cols = 15;
        private int LevelNumber;
        private readonly Image[,] gridImages;
        private GameState gameState;
        private bool gameRunning;

        public MainWindow()
        {
            InitializeComponent();
            gridImages = SetupGrid();
            gameState = new GameState(rows, cols);
        }
        private async Task RunGame()
        {
            Draw();
            await ShowCountdown();
            Overlay.Visibility = Visibility.Hidden;
            await GameLoop();
            gameState = new GameState(rows, cols);
        }
        private async void Window_PreviewKeyDown_Easy(object sender, RoutedEventArgs e)
        {
            if (Easy_Before_Completed.Visibility == Visibility.Visible || Easy.Visibility == Visibility.Visible)
            {
                e.Handled = true;
                Easy_Before_Completed.Visibility = Visibility.Hidden;
                Easy.Visibility = Visibility.Hidden;
                Normal.Visibility = Visibility.Hidden;
            }
            if (!gameRunning)
            {
                gameRunning = true;
                LevelNumber = 150;
                await RunGame();
                gameRunning = false;
                if(gameState.crownGrabbed == true)
                {
                    Easy_Before_Completed.Visibility = Visibility.Hidden;
                    Easy.Visibility = Visibility.Visible;
                    Normal.Visibility = Visibility.Visible;
                }
            }
        }

        private async void Window_PreviewKeyDown_Normal(object sender, RoutedEventArgs e)
        {
            if (Normal.Visibility == Visibility.Visible)
                e.Handled = true;
                Easy.Visibility = Visibility.Hidden;
                Normal.Visibility = Visibility.Hidden;
            if (!gameRunning)
            {
                gameRunning = true;
                LevelNumber = 100;
                await RunGame();
                gameRunning = false;
                Easy_Before_Completed.Visibility = Visibility.Hidden;
                Easy.Visibility = Visibility.Visible;
                Normal.Visibility = Visibility.Visible;
            }
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (gameState.GameOver)
                return;
            else
                switch (e.Key)
                {
                    case Key.Left:
                        gameState.ChangeDirection(Direction.Left);
                        break;
                    case Key.Right:
                        gameState.ChangeDirection(Direction.Right);
                        break;
                    case Key.Up:
                        gameState.ChangeDirection(Direction.Up);
                        break;
                    case Key.Down:
                        gameState.ChangeDirection(Direction.Down);
                        break;
                }

        }

        private async Task GameLoop()
        {
            while (!gameState.GameOver && gameState.crownGrabbed == false)
            {
                await Task.Delay(LevelNumber);
                gameState.Move();
                Draw();
                if (gameState.crownGrabbed == true)
                {
                    await DrawGoldSnake();
                    await ShowGameOver();
                }
                else if (gameState.GameOver)
                {
                    await DrawDeadSnake();
                    await ShowGameOver();
                }
            }
        }
        private Image[,] SetupGrid()
        {
            Image[,] images = new Image[rows, cols];
            GameGrid.Rows = rows;
            GameGrid.Columns = cols;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Image image = new Image
                    {
                        Source = Images.Empty,
                        RenderTransformOrigin = new Point(0.5,0.5)
                    };

                    images[r, c] = image;
                    GameGrid.Children.Add(image);
                }
            }
            return images;
        }
        private void Draw()
        {
            DrawGrid();
            DrawSnakeHead();
            ScoreText.Text = $"Score: {gameState.Score}";
        }
        private void DrawGrid()
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    GridValue gridVal = gameState.Grid[r, c];
                    gridImages[r, c].Source = gridValueToImage[gridVal];
                    gridImages[r, c].RenderTransform = Transform.Identity;
                }
            }
        }

        private void DrawSnakeHead()
        {
            Position headPos = gameState.HeadPosition();
            Image image = gridImages[headPos.Row, headPos.Col];
            image.Source = Images.Head;

            int rotation = dirToRotate[gameState.Dir];
            image.RenderTransform = new RotateTransform(rotation);  
        }

        private async Task DrawDeadSnake()
        {
            List<Position> positions = new List<Position>(gameState.SnakePositions());

            for(int i=0;i< positions.Count;i++)
            {
                Position pos = positions[i];
                ImageSource source = (i == 0) ? Images.DeadHead : Images.DeadBody;
                gridImages[pos.Row, pos.Col].Source = source;
                await Task.Delay(100);
            }
        }

        private async Task DrawGoldSnake()
        {
            List<Position> positions = new List<Position>(gameState.SnakePositions());

            for (int i = 0; i < positions.Count; i++)
            {
                Position pos = positions[i];
                ImageSource source = (i == 0) ? Images.Gold_Head : Images.Gold_Body;
                gridImages[pos.Row, pos.Col].Source = source;
                await Task.Delay(100);
            }
        }
        private async Task ShowCountdown()
        {
            for (int i = 3; i > 0; i--)
            {
                OverlayText.Text = i.ToString();
                await Task.Delay(1000);
            }
            OverlayText.Text = null;
        }

        private void Normal_MouseEnter(object sender, MouseEventArgs e)
        {
            Normal.Opacity = 0.75;
        }

        private void Normal_MouseLeave(object sender, MouseEventArgs e)
        {
            Normal.Opacity = 1;
        }

        private void Easy_MouseEnter(object sender, MouseEventArgs e)
        {
            Easy.Opacity = 0.75;
        }

        private void Easy_MouseLeave(object sender, MouseEventArgs e)
        {
            Easy.Opacity = 1;
        }

        public async Task ShowGameOver()
        {
            await Task.Delay(1000);
            if (gameState.easyCompleted == true)
            {
                Easy.Visibility = Visibility.Visible;
                Normal.Visibility = Visibility.Visible;
            }
            else
                Easy_Before_Completed.Visibility = Visibility.Visible;
            Overlay.Visibility = Visibility.Visible;
        }
    }
}
