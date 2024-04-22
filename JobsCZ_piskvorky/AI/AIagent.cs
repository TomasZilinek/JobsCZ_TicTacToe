using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobsCZ_piskvorky.AI
{
    public abstract class AIagent
    {
        /// <summary>
        /// Return best computed position
        /// </summary>
        /// <returns>(x, y)</returns>
        public abstract List<BoardPosition> GetBestMove(Board board, bool hasCrossSymbol, object form = null);
    }
}
