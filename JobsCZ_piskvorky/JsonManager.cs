using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JobsCZ_piskvorky
{
    public class JsonManager
    {
        public string Serialize(JsonDeserializedObject jsonDeserializedObject)
        {
            return JsonConvert.SerializeObject(jsonDeserializedObject);
        }

        public JsonDeserializedObject Deserialize(string json)
        {
            if (json.ToLower().Contains("not allowed"))
                return null;

            return GetJsonDeserializedObjectFromJObject(JObject.Parse(json));
        }

        public JsonDeserializedObject GetJsonDeserializedObjectFromJObject(IDictionary<string, JToken> jsonData)
        {
            JsonDeserializedObject result = null;

            if (jsonData.ContainsKey("playerCrossId"))
            {/*
                var a = jsonData["coordinates"];
                var aa = jsonData["coordinates"].Value<Array>();
                var b = a.Children()[0];
                */
                result = new JsonPlayResponse()
                {
                    statusCode = jsonData["statusCode"].Value<int>(),
                    playerCrossId = jsonData["playerCrossId"].Value<string>(),
                    playerCircleId = jsonData["playerCircleId"].Value<string>(),
                    actualPlayerId = jsonData["actualPlayerId"].Value<string>(),
                    winnerId = jsonData["winnerId"].Value<string>(),
                    coordinates = ParseCoordinates(jsonData["coordinates"] as JArray)
                };
            }
            else if (jsonData.ContainsKey("userId"))
            {
                result = new JsonUserResponse()
                {
                    statusCode = jsonData["statusCode"].Value<int>(),
                    userId = jsonData["userId"].Value<string>(),
                    userToken = jsonData["userToken"].Value<string>()
                };
            }
            else if (jsonData.ContainsKey("gameToken"))
            {
                result = new JsonConnectResponse()
                {
                    statusCode = jsonData["statusCode"].Value<int>(),
                    gameToken = jsonData["gameToken"].Value<string>(),
                    gameId = jsonData["gameId"].Value<string>()
                };
            }
            else if (jsonData.ContainsKey("errors"))
            {
                result = new JsonBasicErrorResponse()
                {
                    statusCode = jsonData["statusCode"].Value<int>(),
                    errors = ParseErrors(jsonData["errors"] as JObject)
                };
            }
            else
            {
                result = new JsonFeedbackResponse()
                {
                    statusCode = jsonData["statusCode"].Value<int>()
                };
            }

            return result;
        }

        private List<JsonCoordinates> ParseCoordinates(JArray arrayOfArrays)
        {
            List<JsonCoordinates> result = new List<JsonCoordinates>();

            foreach (JObject coordsJObject in arrayOfArrays)
            {
                JsonCoordinates coords = new JsonCoordinates()
                {
                    playerId = coordsJObject["playerId"].Value<string>(),
                    x = coordsJObject["x"].Value<int>(),
                    y = coordsJObject["y"].Value<int>()
                };

                result.Add(coords);
            }

            return result;
        }

        private Dictionary<string, string> ParseErrors(JObject errorsJObject)
        {
            if (errorsJObject == null || errorsJObject.Children().Count() == 0)
                return new Dictionary<string, string>();

            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (KeyValuePair<string, JToken> pair in errorsJObject)
            {
                result[pair.Key] = pair.Value.Value<string>();
            }

            return result;
        }

        public JsonBasicErrorResponse GetBasicErrorResponse(string json)
        {
            IDictionary<string, JToken> jsonData = JObject.Parse(json);

            JsonBasicErrorResponse basicErrorResponse = new JsonBasicErrorResponse()
            {
                statusCode = int.Parse(jsonData["statusCode"].Value<string>()),
                errors = new Dictionary<string, string>()
            };

            JObject errorsJObject = jsonData["errors"] as JObject;

            foreach (JProperty jProperty in errorsJObject.Children().Select(jt => jt as JProperty))
            {
                basicErrorResponse.errors.Add(jProperty.Name, (string)jProperty.Value);
            }

            return basicErrorResponse;
        }
    }
}
