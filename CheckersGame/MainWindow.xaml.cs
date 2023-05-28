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
using System.Numerics;
using System.Runtime.CompilerServices;


namespace CheckersGame
{
    public enum Piece
    {
        Empty,
        White,
        Black,
        WhiteKing,
        BlackKing
    }

    public enum Player
    {
        Empty,
        White,
        Black
    }

    public class Move
    {
        public int FromRow { get; set; }
        public int FromCol { get; set; }
        public int ToRow { get; set; }
        public int ToCol { get; set; }

        public Move(int fromRow, int fromCol, int toRow, int toCol)
        {
            FromRow = fromRow;
            FromCol = fromCol;
            ToRow = toRow;
            ToCol = toCol;
        }
    }

    public static class PieceExtensions
    {
        public static Player GetPlayer(this Piece piece)
        {
            if (piece == Piece.White || piece == Piece.WhiteKing)
                return Player.White;
            else if (piece == Piece.Black || piece == Piece.BlackKing)
                return Player.Black;

            // Возвращаем значение по умолчанию (например, Player.None), если передан недопустимый тип фигуры.
            return Player.Empty;
        }

        public static int[] GetMoveDirections(this Piece piece)
        {
            if (piece == Piece.White)
                return new int[] { 1 };
            else if (piece == Piece.Black)
                return new int[] { -1 };
            else if (piece == Piece.WhiteKing || piece == Piece.BlackKing)
                return new int[] { 1, -1 };

            // Возвращаем значение по умолчанию (например, пустой массив), если передан недопустимый тип фигуры.
            return new int[0];
        }
    }



    public partial class MainWindow : Window
    {
        private CheckersGameLogic gameLogic;
        private Button selectedButton;

        public MainWindow()
        {
            InitializeComponent();
            gameLogic = new CheckersGameLogic();
            UpdateUI();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int row = Grid.GetRow(button);
            int col = Grid.GetColumn(button);

            
            if (selectedButton == null)
            {
                // Кнопка еще не выбрана
                if (gameLogic.IsValidPiece(row, col))
                {
                    // Выберите кнопку, если она содержит правильный фрагмент
                    selectedButton = button;
                    button.IsEnabled = false;
                    button.Opacity = 0.5;
                }
            }
            else
            {
                // Переместите выбранную фигуру в новую позицию
                int selectedRow = Grid.GetRow(selectedButton);
                int selectedCol = Grid.GetColumn(selectedButton);

                if (gameLogic.IsValidMove(selectedRow, selectedCol, row, col))
                {
                    gameLogic.MovePiece(selectedRow, selectedCol, row, col);
                    UpdateUI();
                }

                // Очистить выбор
                selectedButton.IsEnabled = true;
                selectedButton.Opacity = 1.0;
                selectedButton = null;
            }
        }

        private void UpdateUI()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Button button = GetButtonAtPosition(row, col);
                    gameLogic.GetPieceAtPosition(row, col, out Piece piece);
                    UpdateButtonContent(button, piece);
                }
            }

            UpdateTurnIndicator();
        }

        private void UpdateTurnIndicator()
        {
            Player currentPlayer = gameLogic.CurrentPlayer;

            if (gameLogic.IsGameOver(out Player winningPlayer))
            {
                turnIndicatorTextBlock.Text =  winningPlayer == Player.White ? "White Wins!" : "Black Wins!" ;
                restartButton.Visibility = Visibility.Visible;

                return;
            }
            restartButton.Visibility = Visibility.Collapsed;

            switch (currentPlayer)
            {
                case Player.White:
                    turnIndicatorTextBlock.Text = "White's Move";
                    break;
                case Player.Black:
                    turnIndicatorTextBlock.Text = "Black's Move";
                    break;
                default:
                    turnIndicatorTextBlock.Text = string.Empty;
                    break;
            }
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            // Перезапуск игры 
            gameLogic.ResetGame(); // Сброс состояния игры
            UpdateUI(); // Обновление пользовательского интерфейса
            restartButton.Visibility = Visibility.Collapsed; // Снова скрыть кнопку перезапуска
        }

        private void UpdateButtonContent(Button button, Piece piece)
        {
            switch (piece)
            {
                case Piece.White:
                    button.Content = GetImageFromPath("/Sprites/w.png");
                    break;
                case Piece.Black:
                    button.Content = GetImageFromPath("/Sprites/b.png");
                    break;
                case Piece.WhiteKing:
                    button.Content = GetImageFromPath("/Sprites/wk.png");
                    break;
                case Piece.BlackKing:
                    button.Content = GetImageFromPath("/Sprites/bk.png");
                    break;
                case Piece.Empty:
                    button.Content = null;
                    break;
            }
        }

        //Для картинок 
        private Image GetImageFromPath(string imagePath)
        {
            Image image = new Image();
            image.Source = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));
            image.Stretch = Stretch.Fill;
            return image;
        }

        private Button GetButtonAtPosition(int row, int col)
        {
            foreach (var child in gameBoardGrid.Children)
            {
                if (child is Button button && Grid.GetRow(button) == row && Grid.GetColumn(button) == col)
                    return button;
            }

            return new Button();
        }

    }
}