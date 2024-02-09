using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class CPHInline
{
    // This variable saves the conversation history for each user.
    private static Dictionary<string, List<Message>> userConversations = new Dictionary<string, List<Message>>();

    public bool Execute()
    {
        string apiKey = args["OPEN_API_KEY"].ToString();
        string model = args["model"].ToString();
        string content = args["behavior"].ToString();
        string temperature = args["temperature"].ToString();
        string maxHistory = args["maxHistory"].ToString();

        // User and input text.
        string user = args["user"].ToString();
        string messageInput = args["textInput"].ToString();
        int maxHistoryLength = int.Parse(args["maxHistory"].ToString()); // Neu hinzugefügt

        // Update or initialize the conversation history for the user
        if (!userConversations.ContainsKey(user))
        {
            userConversations[user] = new List<Message>();
        }

        // Add the new user input to the history
        userConversations[user].Add(new Message { role = "user", content = messageInput });

        // Limit the conversation history to the last maxHistoryLength messages
        if (userConversations[user].Count > maxHistoryLength)
        {
            var limitedHistory = new List<Message>();
            int start = userConversations[user].Count - maxHistoryLength;
            for (int i = start; i < userConversations[user].Count; i++)
            {
                limitedHistory.Add(userConversations[user][i]);
            }
            userConversations[user] = limitedHistory;
        }

        // Create an instance of talkGPTAPI with the API key
        TalkGPTAPI talkGPT = new TalkGPTAPI(apiKey);

        // Generate a reply with the TalkGPTAPI
        string response = talkGPT.GenerateResponse(user, userConversations, model, content, temperature);

        // Process the answer and add it to the conversation history
        Root root = JsonConvert.DeserializeObject<Root>(response);
        string myString = root.choices[0].message.content;
        userConversations[user].Add(new Message { role = "assistant", content = myString });

        CPH.LogInfo("GPT myString " + myString);

        string myStringCleaned0 = myString.Replace(System.Environment.NewLine, " ");
        string mystringCleaned1 = Regex.Replace(myStringCleaned0, @"\r\n?|\n", " ");
        string myStringCleaned2 = Regex.Replace(mystringCleaned1, @"[\r\n]+", " ");
        string unescapedString = Regex.Unescape(myStringCleaned2);
        string finalGPT = unescapedString.Trim();

        //CPH.SendMessage(finalGPT);
        CPH.SetGlobalVar("GPT", finalGPT, false);

        CPH.LogInfo("GPT " + finalGPT);
        CPH.LogDebug(response);

        return true;
    }
}

class TalkGPTAPI
{
    private string _apiKey;
    private string _endpoint = "https://api.openai.com/v1/chat/completions";

    public TalkGPTAPI(string apiKey)
    {
        _apiKey = apiKey;
    }

    public string GenerateResponse(string user, Dictionary<string, List<Message>> userConversations, string model, string behavior, string temperature)
    {
        // Build the message list from the conversation history
        List<object> messagesList = new List<object>();
        foreach (var msg in userConversations[user])
        {
            messagesList.Add(new { role = msg.role, content = msg.content });
        }

        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_endpoint);
        request.Headers.Add("Authorization", "Bearer " + _apiKey);
        request.ContentType = "application/json";
        request.Method = "POST";

        var requestBody = new
        {
            model = model,
            temperature = double.Parse(temperature),
            messages = messagesList.ToArray()
        };

        string jsonRequestBody = JsonConvert.SerializeObject(requestBody);
        byte[] bytes = Encoding.UTF8.GetBytes(jsonRequestBody);
        request.ContentLength = bytes.Length;

        using (Stream requestStream = request.GetRequestStream())
        {
            requestStream.Write(bytes, 0, bytes.Length);
        }

        // Get the response from the TalkGPT API
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        string responseBody;
        using (Stream responseStream = response.GetResponseStream())
        {
            StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
            responseBody = reader.ReadToEnd();
        }
        return responseBody;
    }
}

public class Message
{
    public string role { get; set; }
    public string content { get; set; }
}
public class Choice
{
    public Message message { get; set; }
    public int index { get; set; }
    public object logprobs { get; set; }
    public string finish_reason { get; set; }
}
public class Root
{
    public string id { get; set; }
    public string @object { get; set; }
    public int created { get; set; }
    public string model { get; set; }
    public List<Choice> choices { get; set; }
    public Usage usage { get; set; }
}
public class Usage
{
    public int prompt_tokens { get; set; }
    public int completion_tokens { get; set; }
    public int total_tokens { get; set; }
}
