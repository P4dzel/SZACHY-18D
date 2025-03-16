using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Chess
{
    public partial class Form1 : Form
    {
        private Chessboard Board = new Chessboard(Tools.BoardSize, Tools.SquareSize);
        private Label currentPlayerLabel;

        public Form1()
        {
            InitializeComponent();
            InitializeUI();
            InitializeBoard();
            InitializePieces();
        }

        private void InitializeUI()
        {
            currentPlayerLabel = new Label
            {
                Text = "Ruch: Gracz 1 (Białe)",
                Font = new Font("Arial", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, Tools.BoardSize * Tools.SquareSize + 10) 
            };

            Controls.Add(currentPlayerLabel);
        }

        private void InitializeBoard()
        {
            foreach (Panel square in Board.squares)
            {
                Controls.Add(square);
            }
        }

        private void InitializePieces(string layout = Piece.DefaultStartingPosition)
        {
            Board.FillBoard(layout);
            foreach (Piece.PiecePanelPair piece in Board.pieces)
            {
                if (piece.panel != null)
                {
                    Controls.Add(piece.panel);
                    piece.panel.MouseDown += Piece_MouseDown;
                    piece.panel.MouseMove += Piece_MouseMove;
                    piece.panel.MouseUp += Piece_MouseUp;
                    piece.panel.BringToFront();
                }
            }
        }

        private void Piece_MouseDown(object sender, MouseEventArgs e)
        {
            ClearHighlights();

            if (sender is Panel panel)
            {
                Board.selectedPiece = panel;
                Board.originalLocation = panel.Location;
                for (int row = 0; row < Tools.BoardSize; row++)
                {
                    for (int col = 0; col < Tools.BoardSize; col++)
                    {
                        if (Board.pieces[row, col].panel == panel)
                        {
                            int pieceType = Board.pieces[row, col].number;
                            if ((pieceType & Board.currentPlayer) == 0)
                            {
                                return;
                            }

                            Board.selectedRow = row;
                            Board.selectedCol = col;
                            List<Point> validMoves = Board.GetValidMovesForPiece(row, col);
                            HighlightValidMoves(row, col);

                            this.Refresh();
                            return;
                        }
                    }
                }
            }

            Board.selectedPiece = null;
        }

        private void Piece_MouseMove(object sender, MouseEventArgs e)
        {
            if (Board.selectedPiece != null && e.Button == MouseButtons.Left)
            {
                Board.selectedPiece.Location = new Point(Board.selectedPiece.Location.X + e.X - (Tools.SquareSize / 2),
                                                         Board.selectedPiece.Location.Y + e.Y - (Tools.SquareSize / 2));
            }
        }

        private void Piece_MouseUp(object sender, MouseEventArgs e)
        {
            if (Board.selectedPiece == null)
                return;

            int newCol = (int)Math.Round((float)Board.selectedPiece.Location.X / (float)Tools.SquareSize, 0, MidpointRounding.AwayFromZero);
            int newRow = (int)Math.Round((float)Board.selectedPiece.Location.Y / (float)Tools.SquareSize, 0, MidpointRounding.AwayFromZero);

            if (newRow >= 0 && newRow < Tools.BoardSize && newCol >= 0 && newCol < Tools.BoardSize)
            {
                int pieceType = Board.pieces[Board.selectedRow, Board.selectedCol].number;

                if (Board.IsValidMove(Board.selectedRow, Board.selectedCol, newRow, newCol, pieceType))
                {
                    MovePiece(newRow, newCol);
                }
                else
                {
                    Board.selectedPiece.Location = Board.originalLocation;
                }
            }

            ClearHighlights();
            Board.selectedPiece = null;
        }

        private void MovePiece(int newRow, int newCol)
        {
            if (Board.pieces[newRow, newCol].panel != null)
            {
                Controls.Remove(Board.pieces[newRow, newCol].panel);
                Board.pieces[newRow, newCol] = new Piece.PiecePanelPair();
            }

            Board.selectedPiece.Location = new Point(newCol * Tools.SquareSize + (Tools.Margin / 2),
                                                     newRow * Tools.SquareSize + (Tools.Margin / 2));

            Board.pieces[newRow, newCol] = Board.pieces[Board.selectedRow, Board.selectedCol];
            Board.pieces[Board.selectedRow, Board.selectedCol] = new Piece.PiecePanelPair();
            Board.SwitchPlayer();
            UpdatePlayerLabel();

            if (Board.IsGameOver())
            {
                MessageBox.Show($"Gra zakończona! {(Board.currentPlayer == Piece.White ? "Czarne" : "Białe")} wygrywają!");
            }
        }

        private void UpdatePlayerLabel()
        {
            currentPlayerLabel.Text = $"Ruch: {(Board.currentPlayer == Piece.White ? "Gracz 1 (Białe)" : "Gracz 2 (Czarne)")}";
        }


        private void HighlightValidMoves(int row, int col)
        {
            List<Point> validMoves = Board.GetValidMovesForPiece(row, col);

            foreach (Point move in validMoves)
            {
                if (Board.IsValidMove(row, col, move.X, move.Y, Board.pieces[row, col].number))
                {
                    Panel marker = new Panel
                    {
                        Size = new Size(Tools.SquareSize / 3, Tools.SquareSize / 3),
                        Location = new Point(move.Y * Tools.SquareSize + Tools.SquareSize / 3, move.X * Tools.SquareSize + Tools.SquareSize / 3),
                        BackColor = Color.Red,
                        BorderStyle = BorderStyle.FixedSingle
                    };

                    Controls.Add(marker);
                    marker.BringToFront();
                    Board.validMoveMarkers.Add(marker);
                }
            }
        }

        private void ClearHighlights()
        {
            foreach (Panel marker in Board.validMoveMarkers)
            {
                Controls.Remove(marker);
            }
            Board.validMoveMarkers.Clear();
        }
    }
}
