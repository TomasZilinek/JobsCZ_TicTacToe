using JobsCZ_piskvorky.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace JobsCZ_piskvorky
{
    public class ServerCommunicator
    {
        private HttpClient httpClient = new HttpClient();
        private JsonManager jsonManager = new JsonManager();

        private Dictionary<EndpointEnum, string> nameToEndpointUrl = new Dictionary<EndpointEnum, string>();

        private Uri UserEndpointUri, ConnectEndpointUri, PlayEndpointUri, CheckStatusEndpointtUri,
                    CheckLastStatusEndpointtUri, FeedbackEndpointUri;
        private Uri[] endpointUris;
        private DateTime lastTokenSentTime = DateTime.Today;
        private TimeSpan TIME_BETWEEN_TOKENS_SENT = TimeSpan.FromMilliseconds(200);

        private const string CONTENT_TYPE = "application/json";
        private const string SSE_URI_STRING = "https://mercure-server.jobs.cz/.well-known/mercure?topic=";

        public ServerCommunicator()
        {
            FillNameToEndpointUriDictionary();
            InitializeEndpointUris();
        }

        private void InitializeEndpointUris()
        {
            UserEndpointUri = new Uri(nameToEndpointUrl[EndpointEnum.User]);
            ConnectEndpointUri = new Uri(nameToEndpointUrl[EndpointEnum.Connect]);
            PlayEndpointUri = new Uri(nameToEndpointUrl[EndpointEnum.Play]);
            CheckStatusEndpointtUri = new Uri(nameToEndpointUrl[EndpointEnum.CheckStatus]);
            CheckLastStatusEndpointtUri = new Uri(nameToEndpointUrl[EndpointEnum.CheckLastStatus]);
            FeedbackEndpointUri = new Uri(nameToEndpointUrl[EndpointEnum.Feedback]);

            endpointUris = new Uri[]
            {
                UserEndpointUri, ConnectEndpointUri, PlayEndpointUri, CheckStatusEndpointtUri,
                CheckLastStatusEndpointtUri, FeedbackEndpointUri
            };
        }

        private void FillNameToEndpointUriDictionary()
        {
            for (EndpointEnum i = 0; i <= EndpointEnum.Feedback; i++)
            {
                string str = i.ToString();
                str = string.Concat(str[0].ToString().ToLower(), str.Substring(1));

                nameToEndpointUrl[i] = "https://piskvorky.jobs.cz/api/v1/" + str;
            }
        }

        private JsonDeserializedObject SendTokenToServer(EndpointEnum endpointEnum, JsonDeserializedObject jsonDeserializedObject)
        {
            JsonDeserializedObject result;

            int tooManyRequestsResponseCounter = 0;

            while (true)
            {
                if (tooManyRequestsResponseCounter > 0)
                {
                    int sleeping = 1000 + tooManyRequestsResponseCounter * 200;
                    Console.WriteLine($"(429) Too many requests. Averted. Sleeping for {sleeping} milliseconds");
                    
                    Thread.Sleep(sleeping);
                }

                if (DateTime.Now - lastTokenSentTime < TIME_BETWEEN_TOKENS_SENT)
                    Thread.Sleep(DateTime.Now - lastTokenSentTime);

                Uri endpointUri = endpointUris[(int)endpointEnum];
                string jsonRequest = jsonManager.Serialize(jsonDeserializedObject);
                StringContent payload = new StringContent(jsonRequest, Encoding.UTF8, CONTENT_TYPE);

                int lastSent = (int)(DateTime.Now - lastTokenSentTime).TotalMilliseconds;
                // Console.WriteLine($"Sending token, last token sent {lastSent} milliseconds back");

                lastTokenSentTime = DateTime.Now;
                string responseJson = httpClient.PostAsync(endpointUri, payload).Result.Content.ReadAsStringAsync().Result;

                result = jsonManager.Deserialize(responseJson);

                if (result == null || (result is JsonBasicErrorResponse errResponse && errResponse.statusCode == 429))
                    tooManyRequestsResponseCounter++;
                else if (result != null)
                    return result;
            }
        }

        public void SendFeedback(string userToken, string message)
        {
            JsonFeedbackRequest feedbackRequest = new JsonFeedbackRequest
            {
                userToken = userToken,
                message = message
            };

            JsonDeserializedObject response = SendTokenToServer(EndpointEnum.Feedback, feedbackRequest);

            if (response is JsonBasicErrorResponse basicErrorResponse)
                Console.WriteLine(basicErrorResponse);
            else
                Console.WriteLine("Feedback saved.");
        }

        public JsonDeserializedObject CreateOrJoinGame(string userToken, out string gameToken, out string gameId)
        {
            gameToken = gameId = "";

            JsonConnectRequest connectRequest = new JsonConnectRequest()
            {
                userToken = userToken
                //userToken = "abrakadabra skuska"
            };

            JsonDeserializedObject responseJsonObject = SendTokenToServer(EndpointEnum.Connect, connectRequest);

            if (responseJsonObject is JsonConnectResponse connectResponse)
            {
                gameToken = connectResponse.gameToken;
                gameId = connectResponse.gameId;
            }

            return responseJsonObject;
        }

        public JsonDeserializedObject SendPlayRequest(string userToken, string gameToken, int x, int y)
        {
            JsonPlayRequest request = new JsonPlayRequest()
            {
                userToken = userToken,
                gameToken = gameToken,
                positionX = x,
                positionY = y
            };

            return SendTokenToServer(EndpointEnum.Play, request);
        }

        public JsonDeserializedObject CheckStatus(string userToken, string gameToken)
        {
            JsonCheckLastStatusRequest request = new JsonCheckLastStatusRequest()
            {
                userToken = userToken,
                gameToken = gameToken
            };

            return SendTokenToServer(EndpointEnum.CheckStatus, request);
        }

        public JsonDeserializedObject CheckLastStatus(string userToken, string gameToken)
        {
            JsonCheckLastStatusRequest request = new JsonCheckLastStatusRequest()
            {
                userToken = userToken,
                gameToken = gameToken
            };

            return SendTokenToServer(EndpointEnum.CheckLastStatus, request);
        }

        public JsonDeserializedObject UserRegistration(string nickname, string email)
        {
            JsonUserRequest request = new JsonUserRequest()
            {
                nickname = nickname,
                email = email
            };

            return SendTokenToServer(EndpointEnum.User, request);
        }

        public async Task<JsonPlayResponse> GetEnemyMoveAsync(string gameId)
        {
            string uriString = SSE_URI_STRING + HttpUtility.UrlEncode("five-in-a-row/" + gameId); ;
            string json = await httpClient.GetStringAsync(uriString);

            return jsonManager.Deserialize(json) as JsonPlayResponse;
        }
    }
}
