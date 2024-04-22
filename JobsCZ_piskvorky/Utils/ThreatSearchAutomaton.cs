using JobsCZ_piskvorky.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobsCZ_piskvorky.Utils
{
    internal class AutomatonState
    {
        public AutomatonState(int id)
        {
            StateId = id;
        }

        public int StateId { get; private set; }
        public ThreatEnum ThreatEnum { get; private set; }
        private Dictionary<CellStateEnum, AutomatonState> transitions = new Dictionary<CellStateEnum, AutomatonState>();

        public void CreateTransition(CellStateEnum transitionSymbol, AutomatonState targetState)
        {
            transitions[transitionSymbol] = targetState;
        }

        public AutomatonState ApplyTransition(CellStateEnum transitionSymbol)
        {
            if (transitions.TryGetValue(transitionSymbol, out AutomatonState toState))
                return transitions[transitionSymbol];
            else
                return null;
        }
    }

    public class ThreatSearchAutomaton
    {
        private ConstantSizeQueue<BoardPosition> traversedPositions = new ConstantSizeQueue<BoardPosition>();
        private AutomatonState originState = null;
        private AutomatonState currentState = null;
        private AutomatonState endState = null;

        public ThreatSearchAutomaton()
        {
            traversedPositions.Size = 6;
        }

        private void CreateStatesAndTransitions()
        {

        }

        public void Clear()
        {
            traversedPositions = new ConstantSizeQueue<BoardPosition>();
            currentState = originState;
        }

        private void CreateTransition(AutomatonState fromState, AutomatonState toState, CellStateEnum transitionSymbol)
        {
            fromState.CreateTransition(transitionSymbol, toState);
        }

        public List<BoardPosition> ApplyTransitionAndGetThreatPositions(CellStateEnum transitionSymbol, BoardPosition position)
        {
            AutomatonState newState = currentState.ApplyTransition(transitionSymbol);

            if (newState != null)
            {
                traversedPositions.Enqueu(position);
                List<BoardPosition> result = new List<BoardPosition>();

                if (newState == endState)
                {
                    result = GetThreatPositions(currentState.ThreatEnum, traversedPositions);
                    currentState = originState;
                }
                else
                    currentState = newState;

                return result;
            }
            else
            {
                ReturnToOriginState();
                traversedPositions.Clear();

                return new List<BoardPosition>();
            }
        }

        private void ReturnToOriginState()
        {
            traversedPositions = new ConstantSizeQueue<BoardPosition>();
            currentState = originState;
        }

        private List<BoardPosition> GetThreatPositions(ThreatEnum threat, ConstantSizeQueue<BoardPosition> positions)
        {
            string threatString = threat.ToString();
            List<BoardPosition> result = new List<BoardPosition>();
            List<BoardPosition> positionsList = positions.ToList();
            positionsList = positionsList.GetRange(positionsList.Count - threatString.Count(), threatString.Count());

            for (int i = 0; i < threatString.Count(); i++)
                if (threatString[i] == 'P')
                    result.Add(positionsList[i]);

            return result;
        }
    }
}
