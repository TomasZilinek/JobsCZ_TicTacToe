using JobsCZ_piskvorky.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobsCZ_piskvorky
{
    public static class PositionEvaluationManual
    {
        private static BoardPosition iterationStartPosition;
        private static CellStateEnum playerState = CellStateEnum.Empty;
        private static CellStateEnum positionState = CellStateEnum.Empty;
        private static CellStateEnum prevPlayerState = CellStateEnum.Empty;
        private static CellStateEnum prevPositionState = CellStateEnum.Empty;
        private static int consecutive = 0;
        private static int openEnds = 0;

        public static long VictoryScoreThreshold = int.MaxValue;

        public static long EvaluatePositionManual(Board board)
        {
            // MyTimer.ResumeStopWatch("EvaluationFunction");
            long score = 0;

            List<(int, int, CellStateEnum)> connectedSets = GetAllConnectedSets(board);

            foreach ((int, int, CellStateEnum) connectedSet in connectedSets)
                score += GetScoreFromConnectedSet(connectedSet.Item1, connectedSet.Item2, connectedSet.Item3);

            // MyTimer.StopStopWatch("EvaluationFunction");
            return score;
        }

        private static long GetScoreFromConnectedSet(int consecutiveCount, int openEndsCount, CellStateEnum playerState)
        {
            if (openEndsCount == 0 && consecutiveCount < 5)
                return 0;

            // separate branches for openEndsCount 1 and 2

            long result;

            if (consecutiveCount > 0 && consecutiveCount < 4)
                result = (long)Math.Pow(10, consecutiveCount) * openEndsCount;
            else if (consecutiveCount == 4)
            {
                if (openEndsCount == 2)
                    result = VictoryScoreThreshold - 100000;
                else
                    result = 1000000;
            }
            else  // if (consecutiveCount == 5)
                result = VictoryScoreThreshold;

            return playerState == CellStateEnum.PlayerCross ? result : -result;
        }

        public static List<(int, int, CellStateEnum)> GetAllConnectedSets(Board board)
        {
            List<(int, int, CellStateEnum)> result = new List<(int, int, CellStateEnum)>();
                
            result.AddRange(GetConnectedSetsStraight(board));
            result.AddRange(GetConnectedSetsDiagonal(board));

            return result;
        }

        private static void AddConnectedSetsIteration(List<(int, int, CellStateEnum)> lst, Board board, BoardPosition position)
        {
            // MyTimer.ResumeStopWatch("Iteration");
            positionState = board.StateAtPosition(position);

            if (positionState != CellStateEnum.Empty)
                playerState = positionState;

            if (positionState != CellStateEnum.Empty)  // a player symbol
            {
                if (position == iterationStartPosition)  // player symbol at the start position
                {
                    consecutive++;
                }
                else  // player symbol in a middle position
                {
                    // two equal player symbols
                    if (prevPositionState == positionState || prevPositionState == CellStateEnum.Empty)
                        consecutive++;
                    else  // opposite player symbols
                    {
                        lst.Add((consecutive, openEnds, prevPlayerState));
                        openEnds = 0;
                        consecutive = 1;
                    }
                }
            }
            else  // empty
            {
                if (consecutive > 0)  // consecution ending empty space
                {
                    lst.Add((consecutive, openEnds + 1, playerState));
                    consecutive = 0;
                }

                openEnds = 1;
            }

            prevPositionState = positionState;
            prevPlayerState = playerState;

            // MyTimer.StopStopWatch("Iteration");
        }

        private static void ResetIterationParameters()
        {
            consecutive = openEnds = 0;
            playerState = prevPlayerState = prevPositionState = CellStateEnum.Empty;
        }

        private static List<(int, int, CellStateEnum)> GetConnectedSetsStraight(Board board)
        {
            // MyTimer.ResumeStopWatch("SetsStraight");

            List<(int, int, CellStateEnum)> result = new List<(int, int, CellStateEnum)>();

            consecutive = 0;
            openEnds = 0;

            sbyte from = board.GetBoundaryPlusOneOrDefault(board.MinYTaken, true, false);
            sbyte to = board.GetBoundaryPlusOneOrDefault(board.MaxYTaken, false, false);
            sbyte fromInner = board.GetBoundaryPlusOneOrDefault(board.MinXTaken, true, true);
            sbyte toInner = board.GetBoundaryPlusOneOrDefault(board.MaxXTaken, false, true);

            for (sbyte y = from; y <= to; y++)
            {
                iterationStartPosition = new BoardPosition(0, y);

                for (sbyte x = fromInner; x <= toInner; x++)
                    AddConnectedSetsIteration(result, board, new BoardPosition(x, y));

                if (consecutive > 0)
                    result.Add((consecutive, openEnds, playerState));

                ResetIterationParameters();
            }

            from = board.GetBoundaryPlusOneOrDefault(board.MinXTaken, true, true);
            to = board.GetBoundaryPlusOneOrDefault(board.MaxXTaken, false, true);
            fromInner = board.GetBoundaryPlusOneOrDefault(board.MinYTaken, true, false);
            toInner = board.GetBoundaryPlusOneOrDefault(board.MaxYTaken, false, false);

            for (sbyte x = from; x <= to; x++)
            {
                iterationStartPosition = new BoardPosition(x, 0);

                for (sbyte y = fromInner; y <= toInner; y++)
                    AddConnectedSetsIteration(result, board, new BoardPosition(x, y));

                if (consecutive > 0)
                    result.Add((consecutive, openEnds, playerState));

                ResetIterationParameters();
            }

            // MyTimer.StopStopWatch("SetsStraight");
            return result;
        }

        private static List<(int, int, CellStateEnum)> GetConnectedSetsDiagonal(Board board)
        {
            // MyTimer.ResumeStopWatch("SetsDiagonal");
            List<(int, int, CellStateEnum)> result = new List<(int, int, CellStateEnum)>();
            BoardPosition startPosition;
            
            for (sbyte y = board.MinYTaken; y <= board.MaxYTaken; y++)
            {
                startPosition = new BoardPosition(board.MinXTaken, y);

                if (board.IsValidPosition(startPosition.AddVectorGet(-1, -1)))
                    startPosition.AddVector(-1, -1);

                AddConnectedSetsSpecificDiagonal(result, board, startPosition, true);
            }

            for (sbyte x = (sbyte)(board.MinXTaken + 1); x <= board.MaxXTaken; x++)
            {
                startPosition = new BoardPosition(x, board.MinYTaken);

                if (board.IsValidPosition(startPosition.AddVectorGet(-1, -1)))
                    startPosition.AddVector(-1, -1);

                AddConnectedSetsSpecificDiagonal(result, board, startPosition, true);
            }
            
            for (sbyte y = board.MinYTaken; y <= board.MaxYTaken; y++)
            {
                startPosition = new BoardPosition(board.MaxXTaken, y);

                if (board.IsValidPosition(startPosition.AddVectorGet(1, -1)))
                    startPosition.AddVector(1, -1);

                AddConnectedSetsSpecificDiagonal(result, board, startPosition, false);
            }
            
            for (sbyte x = board.MinXTaken; x <= board.MaxXTaken - 1; x++)
            {
                startPosition = new BoardPosition(x, board.MinYTaken);

                if (board.IsValidPosition(startPosition.AddVectorGet(1, -1)))
                    startPosition.AddVector(1, -1);

                AddConnectedSetsSpecificDiagonal(result, board, startPosition, false);
            }
            
            // MyTimer.StopStopWatch("SetsDiagonal");
            return result;
        }

        private static void AddConnectedSetsSpecificDiagonal(List<(int, int, CellStateEnum)> lst, Board board,
                                                             BoardPosition startPosition, bool diagonalRight)
        {
            BoardPosition position = startPosition;

            sbyte maxYBoundPlus = board.GetBoundaryPlusOneOrDefault(board.MaxYTaken, false, false);
            sbyte minXBoundPlus = board.GetBoundaryPlusOneOrDefault(board.MinXTaken, true, true);
            sbyte maxXBoundPlus = board.GetBoundaryPlusOneOrDefault(board.MaxXTaken, false, true);

            iterationStartPosition = startPosition;

            while (position.Y <= maxYBoundPlus && position.X >= minXBoundPlus && position.X <= maxXBoundPlus)
            {
                AddConnectedSetsIteration(lst, board, position);
                position.AddVector(diagonalRight ? 1 : -1, 1);
            }

            if (consecutive > 0)
                lst.Add((consecutive, openEnds, playerState));

            ResetIterationParameters();
        }
    }
}
