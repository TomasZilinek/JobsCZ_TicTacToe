using JobsCZ_piskvorky.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobsCZ_piskvorky.AI
{
    public class MiniMaxAI : AIagent
    {
        private int maxDepth = 4;
        private int maxAdjacent = 1;

        private UI.PlayAgainstAIForm form;

        public override List<BoardPosition> GetBestMove(Board board, bool hasCrossSymbol, object _form = null)
        {
            form = _form as UI.PlayAgainstAIForm;
            // MyTimer.ResetAllStopwatches();

            // MyTimer.ResumeStopWatch("Overall");
            (List<BoardPosition>, long) result = MiniMaxAlphaBetaPruning(board, hasCrossSymbol, maxDepth, int.MinValue, int.MaxValue);

            // Console.WriteLine("score " + result.Item2);
            // MyTimer.StopStopWatch("Overall");

            return result.Item1;
        }

        private (List<BoardPosition>, long) MiniMaxAlphaBetaPruning(Board board, bool hasCrossSymbol, int depth, long alpha, long beta)
        {
            if (depth == 0)
            {
                long score = PositionEvaluationManual.EvaluatePositionManual(board);
                // Console.WriteLine($"minimax depth = {depth}, hasCrossSymbol = {hasCrossSymbol}, evaluating...");
                form?.SetBoardAndShowScore(board, score);

                return (new List<BoardPosition>(), score);
            }

            // Console.WriteLine($"minimax depth = {depth}, hasCrossSymbol = {hasCrossSymbol}");

            if (hasCrossSymbol)
            {
                (List<BoardPosition>, long) maxEval = (new List<BoardPosition>(), int.MinValue);

                List<BoardPosition> adjacentPositions = board.GetFreeAdjacentCellPositions(maxAdjacent).ToList();
                adjacentPositions = ChooseAdjacentPositionPortion(board, adjacentPositions, hasCrossSymbol);

                foreach (BoardPosition freeAdjacentPosition in adjacentPositions)
                {
                    board.SetPositionState(freeAdjacentPosition, CellStateEnum.PlayerCross);

                    long immediateEval = PositionEvaluationManual.EvaluatePositionManual(board);

                    if (immediateEval >= PositionEvaluationManual.VictoryScoreThreshold)
                    {
                        board.SetPositionState(freeAdjacentPosition, CellStateEnum.Empty);
                        board.SetMinAndMaxBorders();

                        return (new List<BoardPosition> { freeAdjacentPosition }.ToList(), immediateEval);
                    }

                    (List<BoardPosition>, long) eval = MiniMaxAlphaBetaPruning(board, !hasCrossSymbol, depth - 1, alpha, beta);

                    board.SetPositionState(freeAdjacentPosition, CellStateEnum.Empty);
                    board.SetMinAndMaxBorders();

                    if (eval.Item2 > maxEval.Item2)
                    {
                        maxEval = (eval.Item1.Prepend(freeAdjacentPosition).ToList(), eval.Item2);

                        if (eval.Item2 >= PositionEvaluationManual.VictoryScoreThreshold)
                            break;
                    }

                    if (eval.Item2 > alpha)
                        alpha = eval.Item2;

                    if (beta <= alpha)
                        break;
                }

                return maxEval;
            }
            else
            {
                (List<BoardPosition>, long) minEval = (new List<BoardPosition>(), int.MaxValue);

                List<BoardPosition> adjacentPositions = board.GetFreeAdjacentCellPositions(maxAdjacent).ToList();
                adjacentPositions = ChooseAdjacentPositionPortion(board, adjacentPositions, hasCrossSymbol);

                foreach (BoardPosition freeAdjacentPosition in adjacentPositions)
                {
                    board.SetPositionState(freeAdjacentPosition, CellStateEnum.PlayerCircle);

                    long immediateEval = PositionEvaluationManual.EvaluatePositionManual(board);

                    if (immediateEval <= -PositionEvaluationManual.VictoryScoreThreshold)
                    {
                        board.SetPositionState(freeAdjacentPosition, CellStateEnum.Empty);
                        board.SetMinAndMaxBorders();

                        return (new List<BoardPosition> { freeAdjacentPosition }.ToList(), immediateEval);
                    }

                    (List<BoardPosition>, long) eval = MiniMaxAlphaBetaPruning(board, !hasCrossSymbol, depth - 1, alpha, beta);

                    board.SetPositionState(freeAdjacentPosition, CellStateEnum.Empty);
                    board.SetMinAndMaxBorders();

                    if (eval.Item2 < minEval.Item2)
                    {
                        minEval = (eval.Item1.Prepend(freeAdjacentPosition).ToList(), eval.Item2);

                        if (eval.Item2 <= -PositionEvaluationManual.VictoryScoreThreshold)
                            break;
                    }

                    if (eval.Item2 < beta)
                        beta = eval.Item2;

                    if (beta <= alpha)
                        break;
                }

                return minEval;
            }
        }

        private List<BoardPosition> ChooseAdjacentPositionPortion(Board board, List<BoardPosition> input, bool hasCrossSymbol)
        {
            return input;

            List<(BoardPosition, long)> scores = new List<(BoardPosition, long)>();

            foreach (BoardPosition position in input)
            {
                board.SetPositionState(position, CellStateEnum.PlayerCircle);
                scores.Add((position, PositionEvaluationManual.EvaluatePositionManual(board)));
                board.SetPositionState(position, CellStateEnum.Empty);
            }

            board.SetMinAndMaxBorders();

            if (hasCrossSymbol)
                scores.Sort((x, y) => y.Item2.CompareTo(x.Item2));
            else
                scores.Sort((x, y) => x.Item2.CompareTo(y.Item2));

            List<BoardPosition> result = scores.Select(x => x.Item1).ToList();

            return result.GetRange(0, (int)(result.Count * 0.95f));
        }

        private List<BoardPosition> GetForcingThreatCounteringPositions(Board board, bool hasCrossSymbol)
        {
            List<BoardPosition> result = new List<BoardPosition>();
            BoardPosition startPosition;

            // horizontal
            sbyte from = board.GetBoundaryPlusOneOrDefault(board.MinYTaken, true, false);
            sbyte to = board.GetBoundaryPlusOneOrDefault(board.MaxYTaken, false, false);
            sbyte fromInner = board.GetBoundaryPlusOneOrDefault(board.MinXTaken, true, true);
            sbyte toInner = board.GetBoundaryPlusOneOrDefault(board.MaxXTaken, false, true);

            for (sbyte y = from; y <= to; y++)
            {
                for (sbyte x = fromInner; x <= toInner; x++)
                    AddForcingThreatCounterPositionsIteration(result, board, new BoardPosition(x, y), hasCrossSymbol);

                if (result.Count > 0)
                    return result;
            }

            // vertical
            from = board.GetBoundaryPlusOneOrDefault(board.MinXTaken, true, true);
            to = board.GetBoundaryPlusOneOrDefault(board.MaxXTaken, false, true);
            fromInner = board.GetBoundaryPlusOneOrDefault(board.MinYTaken, true, false);
            toInner = board.GetBoundaryPlusOneOrDefault(board.MaxYTaken, false, false);

            for (sbyte x = from; x <= to; x++)
            {
                for (sbyte y = fromInner; y <= toInner; y++)
                    AddForcingThreatCounterPositionsIteration(result, board, new BoardPosition(x, y), hasCrossSymbol);

                if (result.Count > 0)
                    return result;
            }
            

            // right diagonal from left vertical
            for (sbyte y = board.MinYTaken; y <= board.MaxYTaken; y++)
            {
                startPosition = new BoardPosition(board.MinXTaken, y);

                if (board.IsValidPosition(startPosition.AddVectorGet(-1, -1)))
                    startPosition.AddVector(-1, -1);

                AddForcingThreatCounterPositionsIteration(result, board, startPosition, hasCrossSymbol);

                if (result.Count > 0)
                    return result;
            }

            // right diagonal from upper horizontal
            for (sbyte x = (sbyte)(board.MinXTaken + 1); x <= board.MaxXTaken; x++)
            {
                startPosition = new BoardPosition(x, board.MinYTaken);

                if (board.IsValidPosition(startPosition.AddVectorGet(-1, -1)))
                    startPosition.AddVector(-1, -1);

                AddForcingThreatCounterPositionsIteration(result, board, startPosition, hasCrossSymbol);

                if (result.Count > 0)
                    return result;
            }

            // left diagonal from right vertical
            for (sbyte y = board.MinYTaken; y <= board.MaxYTaken; y++)
            {
                startPosition = new BoardPosition(board.MaxXTaken, y);

                if (board.IsValidPosition(startPosition.AddVectorGet(1, -1)))
                    startPosition.AddVector(1, -1);

                AddForcingThreatCounterPositionsIteration(result, board, startPosition, hasCrossSymbol);

                if (result.Count > 0)
                    return result;
            }

            // left diagonal from upper hprizontal
            for (sbyte x = board.MinXTaken; x <= board.MaxXTaken - 1; x++)
            {
                startPosition = new BoardPosition(x, board.MinYTaken);

                if (board.IsValidPosition(startPosition.AddVectorGet(1, -1)))
                    startPosition.AddVector(1, -1);

                AddForcingThreatCounterPositionsIteration(result, board, startPosition, hasCrossSymbol);

                if (result.Count > 0)
                    return result;
            }

            return result;
        }

        private void AddForcingThreatCounterPositionsIteration(List<BoardPosition> lst, Board board,
                                                               BoardPosition position, bool hasCrossSymbol)
        {

        }
    }
}
