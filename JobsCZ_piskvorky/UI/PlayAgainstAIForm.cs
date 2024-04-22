using JobsCZ_piskvorky.AI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JobsCZ_piskvorky.UI
{
    public partial class PlayAgainstAIForm : Form
    {
        private AIagent aiAgent;
        private Board board = new Board();
        private Pen pen = new Pen(Color.Black);
        private int padding = 20;
        private float cellSize;
        private List<BoardPosition> positionsSequence;

        public PlayAgainstAIForm(AIagent _aiAgent)
        {
            InitializeComponent();
            DoubleBuffered = true;
            WindowState = FormWindowState.Maximized;
            
            aiAgent = _aiAgent;
        }

        private void PlayAgainstAIForm_Paint(object sender, PaintEventArgs e)
        {
            float lineLengthX = cellSize * Board.BOARD_SIZE_X;
            float lineLengthY = cellSize * Board.BOARD_SIZE_Y;

            // vertical
            for (int x = 0; x < Board.BOARD_SIZE_X + 1; x++)
            {
                e.Graphics.DrawLine(pen, padding + x * cellSize,
                                         padding,
                                         padding + x * cellSize,
                                         padding + lineLengthY);
            }

            // horizontal
            for (int y = 0; y < Board.BOARD_SIZE_Y + 1; y++)
            {
                e.Graphics.DrawLine(pen, padding,
                                         padding + y * cellSize,
                                         padding + lineLengthX,
                                         padding + y * cellSize);
            }

            foreach (KeyValuePair<BoardPosition, Enums.CellStateEnum> pair in board.PositionsDict)
                DrawStateToCell(e, pair.Key, pair.Value, Color.Black);

            if (positionsSequence != null)
            {
                Enums.CellStateEnum currentState = Enums.CellStateEnum.PlayerCircle;

                Color[] colors = new Color[] { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Purple };
                int i = 0;

                foreach (BoardPosition position in positionsSequence)
                {
                    DrawStateToCell(e, position, currentState, colors[i++]);

                    currentState = currentState == Enums.CellStateEnum.PlayerCircle ? Enums.CellStateEnum.PlayerCross :
                                                                                      Enums.CellStateEnum.PlayerCircle;
                }
            }
        }

        private void DrawStateToCell(PaintEventArgs e, BoardPosition position, Enums.CellStateEnum state, Color color)
        {
            float symbolPadding = cellSize / 6f;

            float cellPosX = padding + position.X * cellSize;
            float cellPosY = padding + position.Y * cellSize;

            Color previousColor = pen.Color;
            pen.Color = color;

            if (state == Enums.CellStateEnum.PlayerCross)
            {
                e.Graphics.DrawLine(pen, cellPosX + symbolPadding,
                                         cellPosY + symbolPadding,
                                         cellPosX + cellSize - symbolPadding,
                                         cellPosY + cellSize - symbolPadding);

                e.Graphics.DrawLine(pen, cellPosX + cellSize - symbolPadding,
                                         cellPosY + symbolPadding,
                                         cellPosX + symbolPadding,
                                         cellPosY + cellSize - symbolPadding);
            }
            else
            {
                e.Graphics.DrawEllipse(pen, cellPosX + symbolPadding,
                                            cellPosY + symbolPadding,
                                            cellSize - symbolPadding * 2,
                                            cellSize - symbolPadding * 2);
            }

            pen.Color = previousColor;
        }

        private void PlayAgainstAIForm_Resize(object sender, EventArgs e)
        {
            ComputeCellSize();
            Invalidate();
        }

        private void PlayAgainstAIForm_MouseClick(object sender, MouseEventArgs e)
        {
            BoardPosition boardPosition = GetClickedBoardPosition(e.X, e.Y);

            if (drawingCheckBox.Checked)
            {
                Enums.CellStateEnum buttonState;

                if (e.Button == MouseButtons.Left)
                    buttonState = Enums.CellStateEnum.PlayerCross;
                else if (e.Button == MouseButtons.Right)
                    buttonState = Enums.CellStateEnum.PlayerCircle;
                else
                    buttonState = Enums.CellStateEnum.Empty;

                board.SetPositionState(boardPosition, buttonState);
                Invalidate();

                return;
            }

            if (board.StateAtPosition(boardPosition) == Enums.CellStateEnum.Empty)
            {
                if (board.SetPositionState(boardPosition, Enums.CellStateEnum.PlayerCross))
                {
                    positionsSequence = aiAgent.GetBestMove(board, false, null);

                    string msg = MyTimer.StopWatchToString("Overall") + "\n" +
                                 MyTimer.StopWatchToString("EvaluationFunction") + "\n" +
                                 MyTimer.StopWatchToString("SetsStraight") + "\n" +
                                 MyTimer.StopWatchToString("SetsDiagonal") + "\n" +
                                 MyTimer.StopWatchToString("Iteration");
                    // MessageBox.Show(msg);

                    try
                    {
                        board.SetPositionState(positionsSequence[0], Enums.CellStateEnum.PlayerCircle);
                    }
                    catch { }

                    if (!rainbowCheckBox.Checked)
                        positionsSequence = null;  // stop debug
                    
                    Invalidate();
                }
            } 
        }

        private BoardPosition GetClickedBoardPosition(int x, int y)
        {
            return new BoardPosition((sbyte)((x - padding) / cellSize), (sbyte)((y - padding) / cellSize));
        }

        private void ComputeCellSize()
        {
            cellSize = (Math.Min(ClientSize.Width, ClientSize.Height) - 2 * padding) /
                            (float)(Math.Min(Board.BOARD_SIZE_X, Board.BOARD_SIZE_Y));
        }

        public void SetBoardAndShowScore(Board newBoard, long score)
        {
            Board prev = board;

            board = newBoard;
            Invalidate();

            Thread.Sleep(20);
            MessageBox.Show("Score: " + score);

            board = prev;
        }

        private void checkScoreButton_Click(object sender, EventArgs e)
        {
            Enums.CellStateEnum currentState = Enums.CellStateEnum.PlayerCircle;
            
            if (positionsSequence != null)
            {
                foreach (BoardPosition position in positionsSequence)
                {
                    board.SetPositionState(position, currentState);

                    currentState = currentState == Enums.CellStateEnum.PlayerCircle ? Enums.CellStateEnum.PlayerCross :
                                                                                          Enums.CellStateEnum.PlayerCircle;
                }
            }

            positionsSequence = null;
            Invalidate();

            long score = PositionEvaluationManual.EvaluatePositionManual(board);

            MessageBox.Show("Score = " + score);
        }

        private void sequenceButton_Click(object sender, EventArgs e)
        {
            positionsSequence = null;
            Invalidate();
        }

        public void SetBoard(Board newBoard)
        {
            board = newBoard;
        }

        private void restartButton_Click(object sender, EventArgs e)
        {
            board = new Board();
            Invalidate();
        }
    }
}
