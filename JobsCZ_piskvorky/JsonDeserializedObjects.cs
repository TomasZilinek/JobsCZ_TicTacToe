using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JobsCZ_piskvorky
{
    public abstract class JsonDeserializedObject
    {
        public override string ToString()
        {
            string result = GetType().Name + "\n";

            foreach (PropertyInfo info in GetType().GetProperties())
            {
                if (info.PropertyType == typeof(List<List<JsonCoordinates>>))
                {
                    result += "Coordinates:\n";

                    foreach (List<JsonCoordinates> coordinatesList in info.GetValue(this) as List<List<JsonCoordinates>>)
                    {
                        foreach (JsonCoordinates coordinates in coordinatesList)
                            result += coordinates.ToString();

                        result += "\n";
                    }
                }
                else if (info.PropertyType == typeof(Dictionary<string, string>))
                {
                    foreach (KeyValuePair<string, string> pair in info.GetValue(this) as Dictionary<string, string>)
                        result += $"{pair.Key}: {pair.Value}\n";
                }
                else
                    result += $"{info.Name} = {info.GetValue(this)}\n";
            }

            return result;
        }
    }

    public class JsonUserRequest : JsonDeserializedObject
    {
        public string nickname { get; set; }
        public string email { get; set; }
    }

    public class JsonUserResponse : JsonDeserializedObject
    {
        public int statusCode { get; set; }
        public string userId { get; set; }
        public string userToken { get; set; }
    }

    public class JsonBasicErrorResponse : JsonDeserializedObject
    {
        public int statusCode { get; set; }
        public Dictionary<string, string> errors { get; set; }
    }

    public class JsonConnectRequest : JsonDeserializedObject
    {
        public string userToken { get; set; }
    }

    public class JsonConnectResponse : JsonDeserializedObject
    {
        public int statusCode { get; set; }
        public string gameToken { get; set; }
        public string gameId { get; set; }
    }

    public class JsonPlayRequest : JsonDeserializedObject
    {
        public string userToken { get; set; }
        public string gameToken { get; set; }
        public int positionX { get; set; }
        public int positionY { get; set; }
    }

    public class JsonCoordinates : JsonDeserializedObject
    {
        public string playerId { get; set; }
        public int x { get; set; }
        public int y { get; set; }

        public override string ToString()
        {
            return $"Coords: playerId={playerId}, X={x}, Y={y}";
        }
    }

    public class JsonPlayResponse : JsonDeserializedObject
    {
        public int statusCode { get; set; }
        public string playerCrossId { get; set; }
        public string playerCircleId { get; set; }
        public string actualPlayerId { get; set; }
        public string winnerId { get; set; }
        public List<JsonCoordinates> coordinates { get; set; }
    }

    public class JsonCheckStatusRequest : JsonDeserializedObject
    {
        public string userToken { get; set; }
        public string gameToken { get; set; }
    }

    public class JsonCheckLastStatusRequest : JsonDeserializedObject
    {
        public string userToken { get; set; }
        public string gameToken { get; set; }
    }

    public class JsonFeedbackRequest : JsonDeserializedObject
    {
        public string userToken { get; set; }
        public string message { get; set; }
    }

    public class JsonFeedbackResponse : JsonDeserializedObject
    {
        public int statusCode { get; set; }
    }
}
