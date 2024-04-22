using JobsCZ_piskvorky.Enums;
using System.Collections.Generic;

namespace JobsCZ_piskvorky
{
    public struct BoardPosition
    {
        public sbyte X;
        public sbyte Y;

        public BoardPosition(sbyte x, sbyte y)
        {
            X = x;
            Y = y;
        }

        public void AddVector(int x, int y)
        {
            X += (sbyte)x;
            Y += (sbyte)y;
        }

        public BoardPosition AddVectorGet(int x, int y)
        {
            BoardPosition result = new BoardPosition(X, Y);
            result.AddVector(x, y);

            return result;
        }

        public BoardPosition ShiftForSendingOnline()
        {
            return AddVectorGet(-Board.BOARD_SIZE_X / 2, -Board.BOARD_SIZE_Y / 2);
        }

        public BoardPosition ShiftFromReceivedOnline()
        {
            return AddVectorGet(Board.BOARD_SIZE_X / 2, Board.BOARD_SIZE_Y / 2);
        }

        public static bool operator ==(BoardPosition pos1, BoardPosition pos2)
        {
            return pos1.X == pos2.X && pos1.Y == pos2.Y;
        }

        public static bool operator !=(BoardPosition pos1, BoardPosition pos2)
        {
            return !(pos1 == pos2);
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !GetType().Equals(obj.GetType()))
                return false;
            else
            {
                BoardPosition p = (BoardPosition)obj;
                return (X == p.X) && (Y == p.Y);
            }
        }

        public override int GetHashCode()
        {
            return new { X, Y }.GetHashCode();
        }

        public override string ToString()
        {
            return $"{{X = {X}, Y = {Y}}}";
        }
    }

    public struct Cell
    {
        public CellStateEnum cellState;
        public BoardPosition position;
    }

    public class Board
    {
        public const int BOARD_SIZE_X = 58;
        public const int BOARD_SIZE_Y = 40;
        public Dictionary<BoardPosition, CellStateEnum> PositionsDict { get; private set; }
            = new Dictionary<BoardPosition, CellStateEnum>();

        private readonly (int, int)[] directions = new (int, int)[8] { (0, 1), (1, 1), (1, 0), (1, -1),
                                                                       (0, -1), (-1, -1), (-1, 0), (-1, 1) };

        public sbyte MinXTaken { get; private set; }
        public sbyte MaxXTaken { get; private set; }
        public sbyte MinYTaken { get; private set; }
        public sbyte MaxYTaken { get; private set; }

        public Board()
        {
            
        }

        public Board(Board board)
        {
            PositionsDict = new Dictionary<BoardPosition, CellStateEnum>(board.PositionsDict);

            UpdateMinAndMaxBoundsFromPosition(new BoardPosition(board.MinXTaken, board.MinYTaken));
            UpdateMinAndMaxBoundsFromPosition(new BoardPosition(board.MaxXTaken, board.MaxYTaken));
        }

        public CellStateEnum StateAtPosition(BoardPosition position)
        {
            PositionsDict.TryGetValue(position, out CellStateEnum result);

            return result;
        }

        public bool SetPositionState(BoardPosition position, CellStateEnum state)
        {
            if (!IsValidPosition(position))
                return false;

            UpdateMinAndMaxBoundsFromPosition(position);

            if (state == CellStateEnum.Empty)
                PositionsDict.Remove(position);
            else
                PositionsDict[position] = state;

            return true;
        }

        private void UpdateMinAndMaxBoundsFromPosition(BoardPosition position)
        {
            if (!IsValidPosition(position))
                return;

            if (position.X < MinXTaken)
                MinXTaken = position.X;
            else if (position.X > MaxXTaken)
                MaxXTaken = position.X;

            if (position.Y < MinYTaken)
                MinYTaken = position.Y;
            else if (position.Y > MaxYTaken)
                MaxYTaken = position.Y;
        }

        public void UpdateBoard(JsonPlayResponse playResponse)
        {
            PositionsDict = new Dictionary<BoardPosition, CellStateEnum>();

            foreach (JsonCoordinates coords in playResponse.coordinates)
            {
                CellStateEnum newCellState = coords.playerId == playResponse.playerCrossId ? CellStateEnum.PlayerCross :
                                                                                             CellStateEnum.PlayerCircle;
                BoardPosition position = new BoardPosition((sbyte)coords.x, (sbyte)coords.y).ShiftFromReceivedOnline();
                PositionsDict[position] = newCellState;

                UpdateMinAndMaxBoundsFromPosition(position);
            }
        }

        public HashSet<BoardPosition> GetFreeAdjacentCellPositions(int layerLevel = 1, bool copyBoard = true)
        {
            HashSet<BoardPosition> result = new HashSet<BoardPosition>();

            if (layerLevel == 0)
                return result;

            foreach (KeyValuePair<BoardPosition, CellStateEnum> pair in PositionsDict)
                result.UnionWith(GetAllFreeAdjacentPositionsOfCell(pair.Key));

            Board fromBoard = copyBoard ? new Board(this) : this;

            foreach (BoardPosition adjacentPosition in result)
                fromBoard.SetPositionState(adjacentPosition, CellStateEnum.PlayerCircle);

            result.UnionWith(fromBoard.GetFreeAdjacentCellPositions(layerLevel - 1, false));
            return result;
        }

        private HashSet<BoardPosition> GetAllFreeAdjacentPositionsOfCell(BoardPosition cellPosition)
        {
            HashSet<BoardPosition> result = new HashSet<BoardPosition>();

            foreach ((int, int) direction in directions)
            {
                BoardPosition boardPosition = new BoardPosition((sbyte)(cellPosition.X + direction.Item1),
                                                                (sbyte)(cellPosition.Y + direction.Item2));

                if (IsValidPosition(boardPosition) && !PositionsDict.ContainsKey(boardPosition))
                    result.Add(boardPosition);
            }

            return result;
        }

        public bool IsValidPosition(BoardPosition position)
        {
            bool result =  position.X < BOARD_SIZE_X && position.X >= 0 &&
                           position.Y < BOARD_SIZE_Y && position.Y >= 0;

            return result;
        }

        public bool IsWithinMinMaxBounds(BoardPosition position)
        {
            bool result = position.X <= MaxXTaken && position.X >= MinXTaken &&
                          position.Y <= MaxYTaken && position.Y >= MinYTaken;

            return result;
        }

        public sbyte GetBoundaryPlusOneOrDefault(sbyte value, bool isMin, bool isXAxis)
        {
            int axisBoardSize = isXAxis ? BOARD_SIZE_X : BOARD_SIZE_Y;
            int result = value + (isMin ? -1 : 1);

            if (isMin)
                return (sbyte)(result < 0 ? 0 : result);
            else
                return (sbyte)(result > axisBoardSize ? axisBoardSize - 1 : result);
        }

        public void SetMinAndMaxBorders()
        {
            MinXTaken = sbyte.MaxValue;
            MaxXTaken = sbyte.MinValue;
            MinYTaken = sbyte.MaxValue;
            MaxYTaken = sbyte.MinValue;

            if (PositionsDict.Count != 0)
            {
                foreach (BoardPosition position in PositionsDict.Keys)
                {
                    if (position.X < MinXTaken)
                        MinXTaken = position.X;
                    else if (position.X > MaxXTaken)
                        MaxXTaken = position.X;

                    if (position.Y < MinYTaken)
                        MinYTaken = position.Y;
                    else if (position.Y > MaxYTaken)
                        MaxYTaken = position.Y;
                }
            }
        }
    }
}
