using JobsCZ_piskvorky.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobsCZ_piskvorky
{
    public class DdosBot
    {
        private string gameToken = "";
        private string gameId = "";
        private string opponentUserId = "";
        private string rokerUserId = "deeae30a-1342-4ce8-a425-2c37970de97e";

        private Board board = new Board();
        private ServerCommunicator serverCommunicator = new ServerCommunicator();
        private Dictionary<PlayerIdentityEnum, PlayerInfo> playersDict;

        private const int WAIT_MILLISECONDS = 1000;
        private readonly TimeSpan opponentMoveTimeLimit = TimeSpan.FromMinutes(5);

        private PlayerInfo playerInfo;
        private bool hasCrossSymbol;
        private int movesCount = 0;
        private DateTime lastTimeOpponentMoved;

        private bool Connect()
        {
            JsonDeserializedObject response = serverCommunicator.CreateOrJoinGame(playerInfo.UserToken, out gameToken, out gameId);

            if (response == null)
            {
                Console.WriteLine($"Could not create or join a game. response = null");
                return false;
            }

            if (response is JsonBasicErrorResponse basicErrorResponse)
            {
                Console.WriteLine($"Could not create or join a game.\n{basicErrorResponse}\n");
                return false;
            }

            return true;
        }
    }
}
