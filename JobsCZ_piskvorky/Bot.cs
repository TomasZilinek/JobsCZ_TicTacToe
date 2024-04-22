using JobsCZ_piskvorky.AI;
using JobsCZ_piskvorky.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JobsCZ_piskvorky
{
    public struct PlayerInfo
    {
        public PlayerIdentityEnum PlayerIdentity;
        public string UserNickName;
        public string UserId;
        public string UserToken;
    }

    public class Bot
    {
        private string gameToken = "";
        private string gameId = "";
        private string opponentUserId = "";
        private string rokerUserId = "deeae30a-1342-4ce8-a425-2c37970de97e";

        private Board board = new Board();
        private AIagent aiAgent = new MiniMaxAI();
        private ServerCommunicator serverCommunicator = new ServerCommunicator();
        private Dictionary<PlayerIdentityEnum, PlayerInfo> playersDict;

        private const int WAIT_MILLISECONDS = 1000;
        private readonly TimeSpan opponentMoveTimeLimit = TimeSpan.FromMinutes(5);

        private PlayerInfo playerInfo;
        private bool hasCrossSymbol;
        private int movesCount = 0;
        private DateTime lastTimeOpponentMoved;

        public Bot(PlayerIdentityEnum playerEnum)
        {
            playersDict = new Dictionary<PlayerIdentityEnum, PlayerInfo>();

            playersDict[PlayerIdentityEnum.Overmind] = new PlayerInfo()
            {
                PlayerIdentity = PlayerIdentityEnum.Overmind,
                UserNickName = "Overmind",
                UserId = "3d61d9fb-78d9-4f00-8d69-a7d2c0e013ff",
                UserToken = "d5864fb6-1e69-4bd4-b11d-92263e086812"
            };

            playersDict[PlayerIdentityEnum.DiscordAdOld] = new PlayerInfo()
            {
                PlayerIdentity = PlayerIdentityEnum.DiscordAdOld,
                UserNickName = "https://discord.gg/SYFVcBHd",
                UserId = "016bdb9f-8cfc-44b0-819b-922f60911698",
                UserToken = "5528f360-cb8d-43e3-ba6f-8b78eb16bc56"
            };

            playersDict[PlayerIdentityEnum.DiscordAdNew] = new PlayerInfo()
            {
                PlayerIdentity = PlayerIdentityEnum.DiscordAdNew,
                UserNickName = "PRIDAJ SA: discord.gg/SYFVcBHd",
                UserId = "5413e316-c881-44bb-aca4-f17fa1d5db67",
                UserToken = "c2b6476e-d0fc-4a8d-965d-d023553bc6ab"
            };

            playersDict[PlayerIdentityEnum.RokerInterceptor] = new PlayerInfo()
            {
                PlayerIdentity = PlayerIdentityEnum.RokerInterceptor,
                UserNickName = "Roker Interceptor",
                UserId = "e4df34a1-ba14-485f-803b-7953ddc6e391",
                UserToken = "c8a40d3a-4739-438d-a25f-d95a939f9108"
            };

            playerInfo = playersDict[playerEnum];
        }

        public void PlayAs(PlayerIdentityEnum playerEnum)
        {

        }

        public /*async Task<bool>*/ bool Run()
        {
            if (!Connect())
                return false;
            else
                Thread.Sleep(WAIT_MILLISECONDS);

            Console.WriteLine($"Connnected. gameToken = '{gameToken}'");
            Console.WriteLine($"gameId = '{gameId}'");

            board = new Board();

            JsonPlayResponse statusResponse;

            while (!CheckStatus(out statusResponse))
                Thread.Sleep(WAIT_MILLISECONDS);

            hasCrossSymbol = statusResponse.playerCrossId == playerInfo.UserId;
            opponentUserId = hasCrossSymbol ? statusResponse.playerCircleId : statusResponse.playerCrossId;
            lastTimeOpponentMoved = DateTime.Now;

            Console.WriteLine($"I have " + (hasCrossSymbol ? "Cross" : "Circle") + " symbol");

            if (statusResponse.actualPlayerId == playerInfo.UserId)
            {
                Console.WriteLine("Waiting for the opponent to connect");

                if (opponentUserId == null || opponentUserId == "")
                {
                    WaitForOpponentToConnect(out opponentUserId);
                    lastTimeOpponentMoved = DateTime.Now;
                }

                BoardPosition firstMovePosition = new BoardPosition(Board.BOARD_SIZE_X / 2, Board.BOARD_SIZE_Y / 2);
                firstMovePosition = firstMovePosition.ShiftForSendingOnline();

                if (!SendPlayRequest(firstMovePosition.X, firstMovePosition.Y, out JsonPlayResponse playResponse))
                    return false;

                if (!CoordinatesSavedOnServer(playResponse))
                    return false;

                if (opponentUserId == rokerUserId && playerInfo.PlayerIdentity == PlayerIdentityEnum.RokerInterceptor)
                    return true;

                movesCount++;
                Console.WriteLine("Moving first. Initial move = " + hasCrossSymbol);
            }
            else
            {
                if (opponentUserId == rokerUserId && playerInfo.PlayerIdentity == PlayerIdentityEnum.RokerInterceptor)
                    return true;

                Console.WriteLine("Moving second.");
            }

            Console.WriteLine($"Opponent user ID = '{opponentUserId}'");

            if (opponentUserId == rokerUserId)
                Console.WriteLine("roker again");

            return GameLoop();
        }

        private bool GameCompleted(JsonPlayResponse playResponse)
        {
            return playResponse != null && playResponse.statusCode == 226;
        }

        private bool CoordinatesSavedOnServer(JsonPlayResponse playResponse)
        {
            return playResponse != null && playResponse.statusCode == 201;
        }

        private bool BotMoving(JsonPlayResponse playResponse)
        {
            return playResponse != null && playResponse.actualPlayerId ==playerInfo.UserId;
        }

        private /*async Task<bool>*/ bool GameLoop()
        {
            while (true)
            {   
                Program.SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_AWAYMODE_REQUIRED);
                Console.WriteLine("\nWaiting for the enemy to move");

                // JsonPlayResponse enemyPlayedResponse = await serverCommunicator.GetEnemyMoveAsync(gameId);
                JsonPlayResponse enemyPlayedResponse = WaitForEnemyMove(out bool opponentWaitedTooLong);

                if (opponentWaitedTooLong)
                {
                    Console.WriteLine($"Opponent hasn't moved for {opponentMoveTimeLimit.TotalMinutes} minutes, disconnecting.\n");
                    return true;
                }

                lastTimeOpponentMoved = DateTime.Now;
                Thread.Sleep(WAIT_MILLISECONDS);

                if (enemyPlayedResponse == null)
                    return false;

                movesCount++;
                Console.WriteLine("Enemy moved");

                if (GameCompleted(enemyPlayedResponse))
                {
                    Console.WriteLine("My bot " + (enemyPlayedResponse.winnerId == playerInfo.UserId ? "won" : "lost") + "\n");
                    return true;
                }

                board.UpdateBoard(enemyPlayedResponse);

                BoardPosition bestMoveShifted, bestMoveUnshifted;

                try
                {
                    bestMoveShifted = GetNextMoveAndShifted(out bestMoveUnshifted);
                }
                catch
                {
                    Console.WriteLine("Error in GetNextMoveAndShifted. Probably sequence.Count = 0. Aborting.");
                    return false;
                }

                if (!SendPlayRequest(bestMoveShifted.X, bestMoveShifted.Y, out JsonPlayResponse myPlayResponse))
                    return false;

                if (!CoordinatesSavedOnServer(myPlayResponse))
                    return false;

                if (GameCompleted(myPlayResponse))
                {
                    Console.WriteLine("My bot " + (enemyPlayedResponse.winnerId == playerInfo.UserId ? "won" : "lost") + "\n");
                    return true;
                }

                movesCount++;
                board.UpdateBoard(myPlayResponse);
                Console.WriteLine($"My move: {bestMoveUnshifted}.\nMy move shifted: {bestMoveShifted}");
            }
        }

        private BoardPosition GetNextMoveAndShifted(out BoardPosition bestMoveUnshifted)
        {
            if (movesCount < 5)
            {
                List<BoardPosition> adjacentPositions = board.GetFreeAdjacentCellPositions().ToList();
                bestMoveUnshifted = adjacentPositions[new Random().Next(0, adjacentPositions.Count - 1)];
            }
            else
            {
                List<BoardPosition> positionsSequence = aiAgent.GetBestMove(board, hasCrossSymbol);

                if (positionsSequence.Count == 0)
                {
                    Console.WriteLine("Sequence.Count = 0. Aborting.");
                    throw new Exception();

                    MessageBox.Show("BestMoveSequence.Count = 0. Showing board to debug why");
                    UI.PlayAgainstAIForm form = new UI.PlayAgainstAIForm(aiAgent);
                    form.SetBoard(board);
                    Application.Run(form);
                }

                bestMoveUnshifted = positionsSequence[0];
            }
            
            return bestMoveUnshifted.ShiftForSendingOnline();
        }

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

        private bool SendPlayRequest(int x, int y, out JsonPlayResponse playResponse)
        {
            playResponse = null;
            JsonDeserializedObject response = serverCommunicator.SendPlayRequest(playerInfo.UserToken, gameToken, x, y);

            if (response is JsonBasicErrorResponse basicErrorResponse)
            {
                Console.WriteLine($"Can not play this move.\n{basicErrorResponse}\n");
                return false;
            }

            playResponse = response as JsonPlayResponse;

            return true;
        }

        private bool CheckStatus(out JsonPlayResponse playResponse)
        {
            playResponse = null;

            JsonDeserializedObject response = serverCommunicator.CheckStatus(playerInfo.UserToken, gameToken);

            if (response == null)
            {
                Console.WriteLine($"Could not check status. Response was null.\n");
                return false;
            }

            if (response is JsonBasicErrorResponse basicErrorResponse)
            {
                Console.WriteLine($"Could not check status.\n{basicErrorResponse}\n");
                return false;
            }

            if (response is JsonPlayResponse jsonPlayResponse)
                playResponse = jsonPlayResponse;
            else
                return false;

            return true;
        }

        private bool CheckLastStatus(out JsonPlayResponse playResponse)
        {
            playResponse = null;

            JsonDeserializedObject response = serverCommunicator.CheckLastStatus(playerInfo.UserToken, gameToken);

            if (response is JsonBasicErrorResponse basicErrorResponse)
            {
                Console.WriteLine($"Could not check last status.\n{basicErrorResponse}\n");
                return false;
            }

            playResponse = response as JsonPlayResponse;

            return true;
        }

        public void SendFeedback(string message)
        {
            serverCommunicator.SendFeedback(playerInfo.UserToken, message);
        }

        private JsonPlayResponse WaitForEnemyMove(out bool opponentWaitedTooLong)
        {
            opponentWaitedTooLong = false;

            while (true)
            {
                if (DateTime.Now - lastTimeOpponentMoved > opponentMoveTimeLimit)
                {
                    opponentWaitedTooLong = true;
                    return null;
                }

                JsonPlayResponse response = serverCommunicator.CheckStatus(playerInfo.UserToken, gameToken) as JsonPlayResponse;

                if (response != null && response.actualPlayerId ==playerInfo.UserId)
                    return response;

                Thread.Sleep(WAIT_MILLISECONDS);
            }
        }

        private JsonPlayResponse WaitForOpponentToConnect(out string _opponentUserId)
        {
            _opponentUserId = null;

            while (true)
            {
                JsonPlayResponse response = serverCommunicator.CheckStatus(playerInfo.UserToken, gameToken) as JsonPlayResponse;

                if (response != null)
                {
                    _opponentUserId = (hasCrossSymbol ? response.playerCircleId : response.playerCrossId);

                    if (_opponentUserId != null && _opponentUserId != "")
                        return response;
                }

                Thread.Sleep(WAIT_MILLISECONDS);
            }
        }

        public bool UserRegistration(string nickname, string email, out JsonUserResponse userResponse)
        {
            userResponse = null;
            JsonDeserializedObject response = serverCommunicator.UserRegistration(nickname, email);

            if (response is JsonBasicErrorResponse basicErrorResponse)
            {
                Console.WriteLine($"Could not sign up user.\n{basicErrorResponse}\n");
                return false;
            }

            userResponse = response as JsonUserResponse;

            return true;
        }
    }
}
