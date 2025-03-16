using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Chess
{
    internal class Chessboard
    {
        private int BoardSize;
        private int SquareSize;
        public Panel[,] squares;
        public Piece.PiecePanelPair[,] pieces;
        public Panel selectedPiece;
        public Point originalLocation;
        public int selectedRow;
        public int selectedCol;
        public int currentPlayer = Piece.White;
        public Color backColor1 = Color.RosyBrown;
        public Color backColor2 = Color.Wheat;
        public Color highlightColor = Color.DarkBlue;

        public string currentMoves = "";

        public int? enPassantTargetRow = null;
        public int? enPassantTargetCol = null;
        public List<Panel> validMoveMarkers = new List<Panel>();
        public Chessboard(int boardSize, int squareSize)
        {
            BoardSize = boardSize;
            SquareSize = squareSize;
            squares = new Panel[BoardSize, BoardSize];
            pieces = new Piece.PiecePanelPair[BoardSize, BoardSize];

            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    Panel square = new Panel
                    {
                        Size = new Size(SquareSize, SquareSize),
                        Location = new Point(col * SquareSize, row * SquareSize),
                        BackColor = (row + col) % 2 == 0 ? Color.LightYellow : Color.Brown
                    };
                    squares[row, col] = square;
                }
            }
            SetBackColor();
        }
        public void SwitchPlayer()
        {
            currentPlayer = (currentPlayer == Piece.White) ? Piece.Black : Piece.White;
        }
        public void FillBoard(string layout)
        {
            pieces = Tools.ParseFEN(layout);
        }
        public bool IsValidMove(int startRow, int startCol, int targetRow, int targetCol, int pieceType)
        {
            int rowDiff = targetRow - startRow;
            int colDiff = targetCol - startCol;
            int piece = pieceType & 7;


            if ((pieces[targetRow, targetCol].number & currentPlayer) != 0 || (pieces[targetRow, targetCol].number == Piece.King))
            {
                return false;
            }

            switch (piece)
            {
                case Piece.Pawn:
                    int direction = (pieceType & Piece.White) != 0 ? -1 : 1;

                    if (colDiff == 0)
                    {
                        if (rowDiff == direction && pieces[targetRow, targetCol].number == 0) return true;

                        if (rowDiff == 2 * direction && (startRow == 1 || startRow == 6) &&
                            pieces[targetRow, targetCol].number == 0 && pieces[startRow + direction, startCol].number == 0)
                        {
                            return true;
                        }
                    }  
                    else if (Math.Abs(colDiff) == 1 && rowDiff == direction)
                    {
                        if (pieces[targetRow, targetCol].panel != null) return true;

                        if (enPassantTargetRow.HasValue && enPassantTargetCol.HasValue &&
                            targetRow == enPassantTargetRow.Value && targetCol == enPassantTargetCol.Value &&
                            pieces[targetRow, targetCol].number == 0)
                        {
                            return true;
                        }
                    }
                    break;
                case Piece.Rook:
                    if (colDiff == 0 || rowDiff == 0)
                    {
                        if (IsPathClear(startRow, startCol, targetRow, targetCol, pieceType))
                            return true;
                    }
                    break;
                case Piece.Bishop:
                    if (Math.Abs(rowDiff) == Math.Abs(colDiff))
                    {
                        if (IsPathClear(startRow, startCol, targetRow, targetCol, pieceType))
                            return true;
                    }
                    break;
                case Piece.Knight:
                    if ((Math.Abs(rowDiff) == 2 && Math.Abs(colDiff) == 1) || (Math.Abs(rowDiff) == 1 && Math.Abs(colDiff) == 2)) return true;
                    break;
                case Piece.Queen:
                    if (Math.Abs(rowDiff) == Math.Abs(colDiff) || colDiff == 0 || rowDiff == 0)
                    {
                        if (IsPathClear(startRow, startCol, targetRow, targetCol, pieceType))
                            return true;
                    }
                    break;
                case Piece.King:
                    if (Math.Abs(rowDiff) <= 1 && Math.Abs(colDiff) <= 1) return true;
                    break;
            }
            return false;
        }

        public Chessboard Clone()
        {
            Chessboard newBoard = new Chessboard(Tools.BoardSize, Tools.SquareSize);

            for (int row = 0; row < Tools.BoardSize; row++)
            {
                for (int col = 0; col < Tools.BoardSize; col++)
                {
                    newBoard.pieces[row, col] = new Piece.PiecePanelPair
                    {
                        panel = this.pieces[row, col].panel,
                        number = this.pieces[row, col].number
                    };
                }
            }

            newBoard.currentPlayer = this.currentPlayer;
            return newBoard;
        }
        public void MakeMove(Move move)
        {
            pieces[move.TargetRow, move.TargetCol] = pieces[move.StartRow, move.StartCol];
            pieces[move.StartRow, move.StartCol] = new Piece.PiecePanelPair();
            SwitchPlayer();
        }
        public List<Move> GetAllPossibleMoves(int playerColor)
        {
            List<Move> moves = new List<Move>();
            for(int row = 0; row < BoardSize; row++)
            {
                for(int col = 0; col < BoardSize; col++)
                {
                    if ((pieces[row, col].number & playerColor) != 0)
                    {
                        for(int newRow = 0; newRow < BoardSize; newRow++)
                        {
                            for(int newCol = 0; newCol < BoardSize; newCol++)
                            {
                                if (IsValidMove(row, col, newRow, newCol, pieces[row, col].number))
                                {
                                    int? captured = pieces[newRow, newCol].number == 0 ? null : (int?)pieces[newRow, newCol].number;
                                    moves.Add(new Move(row, col, newRow, newCol, pieces[row, col].number, captured));
                                }

                            }
                        }
                    }
                }
            }

            return moves;
        }

        public List<Point> GetValidMovesForPiece(int row, int col)
        {
            List<Point> validMoves = new List<Point>();
            int pieceType = pieces[row, col].number;
            for (int newRow = 0; newRow < BoardSize; newRow++)
            {
                for (int newCol = 0; newCol < BoardSize; newCol++)
                {
                    if ((pieces[newRow, newCol].number & currentPlayer) != 0 || newRow == row && newCol == col || (pieces[newRow, newCol].number == Piece.King))
                    {
                        continue;
                    }
                    if (IsValidMove(row, col, newRow, newCol, pieceType))
                    {
                        validMoves.Add(new Point(newRow, newCol));
                    }
                }
            }
            return validMoves;
        }
        public bool IsPathClear(int startRow, int startCol, int targetRow, int targetCol, int pieceType)
        {
            int rowDiff = targetRow - startRow;
            int colDiff = targetCol - startCol;

            int rowStep = rowDiff == 0 ? 0 : (rowDiff > 0 ? 1 : -1);
            int colStep = colDiff == 0 ? 0 : (colDiff > 0 ? 1 : -1);

            int steps = Math.Max(Math.Abs(rowDiff), Math.Abs(colDiff));

            for (int i = 1; i < steps; i++)
            {
                int row = startRow + i * rowStep;
                int col = startCol + i * colStep;
                if (pieces[row, col].panel != null) return false;
            }

            return true;
        }
        public void SetBackColor()
        {
            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    squares[row, col].BackColor = (row + col) % 2 == 0 ? backColor1 : backColor2;
                }
            }
        }

        public bool IsGameOver()
        {
            if (IsCheckmate(currentPlayer))
            {
                Console.WriteLine($"Szach-mat! Gracz {(currentPlayer == Piece.White ? "Biały" : "Czarny")} przegrywa.");
                return true;
            }

            if (IsStalemate(currentPlayer))
            {
                Console.WriteLine("Pat! Gra kończy się remisem.");
                return true;
            }

            return false;
        }

        public bool IsCheck(int playerColor)
        {
            Point kingPosition = FindKing(playerColor);
            if (kingPosition.X == -1 || kingPosition.Y == -1) return false;

            return IsSquareAttacked(kingPosition.X, kingPosition.Y, -playerColor);
        }

        public bool IsCheckmate(int playerColor)
        {
            if (!IsCheck(playerColor)) return false; 

            List<Move> possibleMoves = GetAllPossibleMoves(playerColor);
            foreach (var move in possibleMoves)
            {
                Chessboard tempBoard = Clone();
                tempBoard.MakeMove(move);
                if (!tempBoard.IsCheck(playerColor)) return false; 
            }

            return true; 
        }

        public bool IsStalemate(int playerColor)
        {
            if (IsCheck(playerColor)) return false; 

            List<Move> possibleMoves = GetAllPossibleMoves(playerColor);
            return possibleMoves.Count == 0;
        }

        public bool IsSquareAttacked(int row, int col, int attackerColor)
        {
            foreach (var move in GetAllPossibleMoves(attackerColor))
            {
                if (move.TargetRow == row && move.TargetCol == col)
                {
                    Console.WriteLine("Król jest atakowany!!!");
                    return true; 
                }
            }
            return false;
        }

        public List<Point> GetValidMovesForKing(int row, int col)
        {
            List<Point> validMoves = new List<Point>();
            int[] dRow = { -1, -1, -1, 0, 0, 1, 1, 1 };
            int[] dCol = { -1, 0, 1, -1, 1, -1, 0, 1 };

            for (int i = 0; i < 8; i++)
            {
                int newRow = row + dRow[i];
                int newCol = col + dCol[i];

                if (newRow >= 0 && newRow < BoardSize && newCol >= 0 && newCol < BoardSize)
                {
                    if ((pieces[newRow, newCol].number & currentPlayer) == 0 && !IsSquareAttacked(newRow, newCol, -currentPlayer))
                    {
                        validMoves.Add(new Point(newRow, newCol));
                    }
                }
            }
            return validMoves;
        }

        private Point FindKing(int playerColor)
        {
            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    if ((pieces[row, col].number & playerColor) != 0 && (pieces[row, col].number & Piece.King) != 0)
                    {
                        return new Point(row, col);
                    }
                }
            }
            return new Point(-1, -1);
        }
        public int GetPieceAt(int row, int col)
        {
            if(row < 0 || row >= BoardSize || col < 0 || col >= BoardSize)
            {
                return 0;
            }
            return pieces[row, col].number;
        }

    }
}
