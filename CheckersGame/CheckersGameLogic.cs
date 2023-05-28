using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace CheckersGame
{
    public class CheckersGameLogic
    {
        private Piece[,] board;
        private Player currentPlayer;

        public Player CurrentPlayer => currentPlayer;
        public Player WinningPlayer { get; private set; }

        public CheckersGameLogic()
        {
            board = new Piece[8, 8];
            currentPlayer = Player.White;
            WinningPlayer = Player.Empty;
            InitializeBoard();
        }

        public bool IsGameOver(out Player winningPlayer)
        {
            int WhiteCount = 0;
            int blackCount = 0;

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Piece checkersPiece = board[row, col];

                    if (checkersPiece == Piece.White || checkersPiece == Piece.WhiteKing)
                        WhiteCount++;
                    else if (checkersPiece == Piece.Black || checkersPiece == Piece.BlackKing)
                        blackCount++;
                }
            }

            if (WhiteCount == 0)
            {
                winningPlayer = Player.Black;
                return true;
            }
            else if (blackCount == 0)
            {
                winningPlayer = Player.White;
                return true;
            }

            winningPlayer = Player.Empty;
            return false;
        }

        private void InitializeBoard()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (row < 3 && (row + col) % 2 != 0)
                        board[row, col] = Piece.Black;
                    else if (row > 4 && (row + col) % 2 != 0)
                        board[row, col] = Piece.White;
                    else
                        board[row, col] = Piece.Empty;
                }
            }
        }

        public bool IsValidPiece(int row, int col)
        {
            Piece piece = board[row, col];

            if (piece == Piece.White || piece == Piece.Black || piece == Piece.WhiteKing || piece == Piece.BlackKing)
                return true;

            return false;
        }
        public bool IsValidMove(int fromRow, int fromCol, int toRow, int toCol)
        {
            Piece piece = board[fromRow, fromCol];

            // Проверка, находится ли ход в границах доски.
            if (toRow < 0 || toRow >= 8 || toCol < 0 || toCol >= 8)
                return false;

            // Проверк, не пуст ли пункт назначения
            if (board[toRow, toCol] != Piece.Empty)
                return false;

            // Проверка, настал ли ход текущего игрока.
            if ((piece == Piece.White || piece == Piece.WhiteKing) && currentPlayer != Player.White)
                return false;

            if ((piece == Piece.Black || piece == Piece.BlackKing) && currentPlayer != Player.Black)
                return false;

            // Проверка, является ли ход диагональным
            int rowDiff = toRow - fromRow;
            int colDiff = toCol - fromCol;

            if (Math.Abs(rowDiff) != Math.Abs(colDiff))
                return false;

            // Проверка, является ли это обычной шашкой.
            if (piece == Piece.White || piece == Piece.Black)
            {
                // Проверка, возможен ли захват 
                bool hasCapture = HasCaptureOption(currentPlayer);
                if (hasCapture && !IsCaptureMove(fromRow, fromCol, toRow, toCol, currentPlayer))
                    return false;

                // Проверка, является ли это действительным движением
                int moveDirection = piece == Piece.White ? -1 : 1;
                if (rowDiff == moveDirection && Math.Abs(colDiff) == 1)
                    return true;

                return hasCapture; // Возвращает true только в том случае, если захват возможен
            }


            // Проверка, является ли он королем
            if (piece == Piece.WhiteKing || piece == Piece.BlackKing)
            {
                // Проверка, если это правильный ход короля (диагональ с захватом или прыжком)
                int stepRow = rowDiff > 0 ? 1 : -1;
                int stepCol = colDiff > 0 ? 1 : -1;

                int currentRow = fromRow;
                int currentCol = fromCol;

                bool hasCapture = false;
                bool hasOpponentOnDiagonal = false; // Отследить соперника по диагонали за королями

                while (true)
                {
                    currentRow += stepRow;
                    currentCol += stepCol;

                    // Проверка, не выходит ли текущая позиция за пределы границ
                    if (currentRow < 0 || currentRow >= 8 || currentCol < 0 || currentCol >= 8)
                        break;

                    // Проверяем, есть ли фигура в текущей позиции
                    Piece currentPiece = board[currentRow, currentCol];

                    if (currentPiece == Piece.Empty)
                    {
                        // Если текущая ячейка пуста, это правильное перемещение
                        if (currentRow == toRow && currentCol == toCol)
                        {
                            if (hasCapture)
                            {
                                // Разрешить перемещение только в том случае, если захват уже был произведен
                                return true;
                            }
                            else
                            {
                                // Разрешить ход, если на диагонали нет противников
                                return !hasOpponentOnDiagonal;
                            }
                        }
                    }
                    else
                    {
                        // Если фигура есть, проверка, не является ли она фигурой противника.
                        if ((piece == Piece.WhiteKing && (currentPiece == Piece.Black || currentPiece == Piece.BlackKing)) ||
                            (piece == Piece.BlackKing && (currentPiece == Piece.White || currentPiece == Piece.WhiteKing)))
                        {
                            // Проверяем, пуста ли следующая ячейка, чтобы разрешить захват
                            int nextRow = currentRow + stepRow;
                            int nextCol = currentCol + stepCol;

                            if (nextRow < 0 || nextRow >= 8 || nextCol < 0 || nextCol >= 8)
                                break;

                            if (board[nextRow, nextCol] == Piece.Empty)
                            {
                                // Захват фигуры противника
                                board[currentRow, currentCol] = Piece.Empty;
                                hasCapture = true;
                                hasOpponentOnDiagonal = true; // Установить флаг, если противник находится на диагонали
                                continue; // Продолжить проверку на наличие дополнительных захватов
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            // Если есть препятствие или собственная фигура, это недействительный ход
                            break;
                        }
                    }
                }

                // Заблокировать ход для королей, если есть возможность захватить фигуру
                if (hasCapture)
                {
                    return false;
                }

                // Разрешить ход за королей, если на диагонали нет соперников
                return !hasOpponentOnDiagonal;
            }



            return false; // Неверный ход
        }




        private bool HasCaptureOption(Player player)
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Piece piece = board[row, col];

                    // Проверка, принадлежит ли фигура текущему игроку
                    if ((piece == Piece.White || piece == Piece.WhiteKing) && player == Player.White ||
                        (piece == Piece.Black || piece == Piece.BlackKing) && player == Player.Black)
                    {
                        // Проверка, возможен ли захват (поедание) для обычных шашек
                        if (piece == Piece.White || piece == Piece.Black)
                        {
                            // Проверка захвата перемещения влево
                            if (col >= 2 && row >= 2)
                            {
                                Piece leftPiece = board[row - 1, col - 1];
                                Piece behindPiece = board[row - 2, col - 2];

                                if (behindPiece == Piece.Empty && leftPiece != piece && leftPiece != Piece.Empty)
                                {
                                    return true;
                                }
                            }

                            // Проверить захват перемещения вправо
                            if (col <= 5 && row >= 2)
                            {
                                Piece rightPiece = board[row - 1, col + 1];
                                Piece behindPiece = board[row - 2, col + 2];

                                if (behindPiece == Piece.Empty && rightPiece != piece && rightPiece != Piece.Empty)
                                {
                                    return true;
                                }
                            }

                            // Проверка захвата перемещения в левый нижний угол
                            if (col >= 2 && row <= 5)
                            {
                                Piece bottomLeftPiece = board[row + 1, col - 1];
                                Piece behindPiece = board[row + 2, col - 2];

                                if (behindPiece == Piece.Empty && bottomLeftPiece != piece && bottomLeftPiece != Piece.Empty)
                                {
                                    return true;
                                }
                            }

                            // Проверьте перемещение захвата в правый нижний угол
                            if (col <= 5 && row <= 5)
                            {
                                Piece bottomRightPiece = board[row + 1, col + 1];
                                Piece behindPiece = board[row + 2, col + 2];

                                if (behindPiece == Piece.Empty && bottomRightPiece != piece && bottomRightPiece != Piece.Empty)
                                {
                                    return true;
                                }
                            }
                        }

                        // Проверьте, возможен ли захват для королей
                        if (piece == Piece.WhiteKing || piece == Piece.BlackKing)
                        {
                            // Проверяем захват перемещения в левый верхний угол
                            if (col >= 2 && row >= 2)
                            {
                                Piece topLeftPiece = board[row - 1, col - 1];
                                Piece behindPiece = board[row - 2, col - 2];

                                if (behindPiece == Piece.Empty && topLeftPiece != piece && topLeftPiece != Piece.Empty)
                                {
                                    return true;
                                }
                            }

                            // Проверка перемещения захвата в правый верхний угол
                            if (col <= 5 && row >= 2)
                            {
                                Piece topRightPiece = board[row - 1, col + 1];
                                Piece behindPiece = board[row - 2, col + 2];

                                if (behindPiece == Piece.Empty && topRightPiece != piece && topRightPiece != Piece.Empty)
                                {
                                    return true;
                                }
                            }

                            // Проверка захвата перемещения в левый нижний угол
                            if (col >= 2 && row <= 5)
                            {
                                Piece bottomLeftPiece = board[row + 1, col - 1];
                                Piece behindPiece = board[row + 2, col - 2];

                                if (behindPiece == Piece.Empty && bottomLeftPiece != piece && bottomLeftPiece != Piece.Empty)
                                {
                                    return true;
                                }
                            }

                            // Проверяем перемещение захвата в правый нижний угол
                            if (col <= 5 && row <= 5)
                            {
                                Piece bottomRightPiece = board[row + 1, col + 1];
                                Piece behindPiece = board[row + 2, col + 2];

                                if (behindPiece == Piece.Empty && bottomRightPiece != piece && bottomRightPiece != Piece.Empty)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }



        private bool IsValidCaptureMove(int row, int col, Player player)
        {
            Piece piece = board[row, col];

            int moveDirection = player == Player.White ? -1 : 1;

            // Проверка, возможен ли захват
            if (piece == Piece.White || piece == Piece.Black || piece == Piece.WhiteKing || piece == Piece.BlackKing)
            {
                if (Capture(row, col, row + 2 * moveDirection, col - 2, row + moveDirection, col - 1, player) ||
                   Capture(row, col, row + 2 * moveDirection, col + 2, row + moveDirection, col + 1, player) ||
                   Capture(row, col, row - 2 * moveDirection, col - 2, row - moveDirection, col - 1, player) ||
                   Capture(row, col, row - 2 * moveDirection, col + 2, row - moveDirection, col + 1, player))
                {
                    return true;
                }
            }

            // Проверка, является ли он королем
            if (piece == Piece.WhiteKing || piece == Piece.BlackKing )
            {
                if (Capture(row, col, row - 2, col - 2, row - 1, col - 1, player) ||
                    Capture(row, col, row - 2, col + 2, row - 1, col + 1, player) ||
                    Capture(row, col, row + 2, col - 2, row + 1, col - 1, player) ||
                    Capture(row, col, row + 2, col + 2, row + 1, col + 1, player))
                {
                    return true;
                }
            }

            return false;
        }

        private bool Capture(int fromRow, int fromCol, int row, int col, int captuWhiteRow, int captuWhiteCol, Player player)
        {
            if (row < 0 || row >= 8 || col < 0 || col >= 8)
                return false;

            Piece piece = board[fromRow, fromCol];
            Piece captuWhitePiece = board[captuWhiteRow, captuWhiteCol];

            if ((piece == Piece.White && (captuWhitePiece == Piece.Black || captuWhitePiece == Piece.BlackKing)) ||
                (piece == Piece.Black && (captuWhitePiece == Piece.White || captuWhitePiece == Piece.WhiteKing)))
            {
                if (board[row, col] == Piece.Empty)
                {
                    return true;
                }
            }

            return false;
        }


        private bool IsCaptureMove(int fromRow, int fromCol, int toRow, int toCol, Player player)
        {
            int rowDiff = Math.Abs(toRow - fromRow);
            int colDiff = Math.Abs(toCol - fromCol);

            if (rowDiff == 2 && colDiff == 2)
            {
                int captuWhiteRow = fromRow + ((toRow - fromRow) / 2);
                int captuWhiteCol = fromCol + ((toCol - fromCol) / 2);

                return IsValidCapture(fromRow, fromCol, captuWhiteRow, captuWhiteCol, toRow, toCol, player);
            }

            return false;
        }

        private bool IsValidCapture(int fromRow, int fromCol, int captureRow, int captureCol, int landingRow, int landingCol, Player player)
        {
            // Проверка, находится ли ход захвата в границах доски.
            if (landingRow < 0 || landingRow >= 8 || landingCol < 0 || landingCol >= 8)
                return false;

            // Проверка, пуста ли посадочная позиция
            if (board[landingRow, landingCol] != Piece.Empty)
                return false;

            // Проверка, нет ли на пути препятствий
            int stepRow = captureRow < fromRow ? -1 : 1;
            int stepCol = captureCol < fromCol ? -1 : 1;

            int currentRow = fromRow + stepRow;
            int currentCol = fromCol + stepCol;

            while (currentRow != captureRow && currentCol != captureCol)
            {
                if (board[currentRow, currentCol] != Piece.Empty)
                    return false; // Препятствие найдено, захват недействителен

                currentRow += stepRow;
                currentCol += stepCol;
            }

            // Проверка, принадлежит ли захваченная фигура противнику
            Piece opponentPiece = player == Player.White ? Piece.Black : Piece.White;
            Piece captuWhitePiece = board[captureRow, captureCol];
            if (captuWhitePiece != opponentPiece && captuWhitePiece != opponentPiece + 2)
                return false;

            return true;
        }
        private bool IsValidCaptureInRow(int kingRow, int kingCol, int captuWhiteRow, int captuWhiteCol, Player currentPlayer)
        {
            // Определите направление ряда
            int rowDirection = kingRow < captuWhiteRow ? 1 : -1;

            // Проверка, есть ли вражеская фигура в том же ряду, рядом с захваченной фигурой.
            int checkRow = captuWhiteRow + rowDirection;
            int checkCol = captuWhiteCol - 1; // Check the left neighbor
            if (IsValidPosition(checkRow, checkCol) && board[checkRow, checkCol].GetPlayer() != currentPlayer)
            {
                // Проверяем, есть ли пустая ячейка после левого соседа в том же ряду
                int targetRow = checkRow + rowDirection;
                int targetCol = checkCol - 1; // Проверка левого соседа
                if (IsValidPosition(targetRow, targetCol) && board[targetRow, targetCol] == Piece.Empty)
                    return true;
            }

            // Проверка правого соседа
            checkCol = captuWhiteCol + 1;
            if (IsValidPosition(checkRow, checkCol) && board[checkRow, checkCol].GetPlayer() != currentPlayer)
            {
                // Проверяем, есть ли пустая ячейка после правого соседа в том же ряду
                int targetRow = checkRow + rowDirection;
                int targetCol = checkCol + 1; // Проверка соседа правого соседа
                if (IsValidPosition(targetRow, targetCol) && board[targetRow, targetCol] == Piece.Empty)
                    return true;
            }

            return false;
        }

        private bool IsValidPosition(int row, int col)
        {
            return row >= 0 && row < board.GetLength(0) && col >= 0 && col < board.GetLength(1);
        }


        public void MovePiece(int fromRow, int fromCol, int toRow, int toCol)
        {
            Piece piece = board[fromRow, fromCol];

            board[fromRow, fromCol] = Piece.Empty;
            board[toRow, toCol] = piece;

            // Проверка, возможен ли захват 
            int rowDiff = Math.Abs(toRow - fromRow);
            int colDiff = Math.Abs(toCol - fromCol);
            bool capturingMove = rowDiff == 2 && colDiff == 2;

            if (capturingMove)
            {
                int captuWhiteRow = fromRow + ((toRow - fromRow) / 2);
                int captuWhiteCol = fromCol + ((toCol - fromCol) / 2);

                // Удалить захваченную фигуру
                board[captuWhiteRow, captuWhiteCol] = Piece.Empty;

                // Проверка, есть ли у фигуры, сделавшей захват, другие ходы для захвата.
                bool hasOtherCaptureMoves = IsValidCaptureMove(toRow, toCol, currentPlayer);

                // Если для захватывающей фигуры нет других ходов захвата, передайте ход сопернику
                if (!hasOtherCaptureMoves)
                    currentPlayer = currentPlayer == Player.White ? Player.Black : Player.White;
            }
            else
            {
                // Переключить текущего игрока, если это не ход захвата
                currentPlayer = currentPlayer == Player.White ? Player.Black : Player.White;
            }

            // Проверка, становится ли фигура королем
            if (piece == Piece.White && toRow == 0 && piece != Piece.WhiteKing)
                board[toRow, toCol] = Piece.WhiteKing;

            if (piece == Piece.Black && toRow == 7 && piece != Piece.BlackKing)
                board[toRow, toCol] = Piece.BlackKing;

            // Проверка, является ли перемещенная фигура королем и имеет ли она возможность захватить другую фигуру противника в том же ряду.
            if ((piece == Piece.WhiteKing || piece == Piece.BlackKing) && Math.Abs(toRow - fromRow) == 2)
            {
                int captuWhiteRow = fromRow + ((toRow - fromRow) / 2);
                int captuWhiteCol = fromCol + ((toCol - fromCol) / 2);

                // Удалить захваченную фигуру
                board[captuWhiteRow, captuWhiteCol] = Piece.Empty;

                // Проверка, есть ли в том же ряду другая вражеская фигура для захвата.
                bool hasOtherCaptureInRow = IsValidCaptureInRow(toRow, toCol, captuWhiteRow, captuWhiteCol, currentPlayer);

                if (hasOtherCaptureInRow)
                    currentPlayer = currentPlayer == Player.White ? Player.Black : Player.White;
            }
        }

        public void ResetGame()
        {
            // Сброс состояния игры к начальной конфигурации
            InitializeBoard();

        }


        public void GetPieceAtPosition(int row, int col, out Piece piece)
        {
            piece = board[row, col];
        }
    }
}
