using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class BerdInterface : MonoBehaviour
{
    /// <summary>
    /// Time before a berd gets DESTROYED!
    /// </summary>
    private static readonly float DESTRUCTIONTIME = 60.0f;
    private static readonly float MINBERDSPEED = 1.0f;
    private static readonly float MAXBERDSPEED = 10.0f;
    [Serializable]
    private class ChatMessage{
        public string Sender;
        public string Body;
        public DateTime DeleteTime;
    }


    private TcpClient client;
    private StreamReader reader;
    private StreamWriter writer;

    private readonly string _username = "artwomble";
    private readonly string _oauth    = "TO INPUT"; // get from Twitch
    private readonly string _channel  = "artwomble";
    /// <summary>
    /// All possible berd prefabs
    /// </summary>
    [SerializeField] private List<GameObject> berds;
    /// <summary>
    /// Currently active berds
    /// </summary>
    [SerializeField] private List<GameObject> ActiveBerds; //Currently active berds
    /// <summary>
    /// List of messages with their senders, sendtime and deletetime
    /// </summary>
    [SerializeField] private List<ChatMessage> Destructionlist;
    private Coroutine _DeletionTracker = null;

    // Start is called before the first frame update
    void Start(){
        Chatconnect();
    }

    // Update is called once per frame
    void Update(){
        ChatRead();
    }

    private bool TrackedName(string name = "") => berds.Any(b => b != null && b.name.ToLower() == name.ToLower());
    private bool TrackedName(ChatMessage message ) => TrackedName(message.Sender);

    /// <summary>
    /// Get the berd associated with this name.
    /// Use Tracked name before this. Any name given to this function should be valid
    /// </summary>
    /// <param name="name">Lowercase name of the sender</param>
    /// <returns></returns>
    private GameObject GetBerd(string name = "" ){
        //See if the berd is contained within the active berds list.
        //If so return this berd
        GameObject ReturningBerd = ActiveBerds.FirstOrDefault(b => b != null && b.name.ToLower() == name.ToLower());

        if(ReturningBerd != null)
            return ReturningBerd;

        //Make a new berd based upon the list of all berds
        GameObject NewBerd = berds.FirstOrDefault(b => b != null && b.name == name);

        if(NewBerd == null)
            return null;

        ReturningBerd = Instantiate(NewBerd,transform);
        ReturningBerd.name = name;
        ActiveBerds.Add(ReturningBerd);
        return ReturningBerd;
    }
    /// <summary>
    /// Get the berd associated with this message.
    /// Use Tracked name before this. Any message given to this function should be valid
    /// </summary>
    /// <param name="message">Message that was sent</param>
    /// <returns></returns>
    private GameObject GetBerd(ChatMessage message = null ) => message != null ? GetBerd(message.Sender) : null;

    void Chatconnect(){
        //setup Twitch connection
        client = new TcpClient("irc.chat.twitch.tv", 6667);
        reader = new StreamReader(client.GetStream());
        writer = new StreamWriter(client.GetStream());


        writer.WriteLine($"PASS oauth:{_oauth}");
        writer.WriteLine($"NICK {_username}");
        writer.WriteLine($"USER {_username} 8 * :{_username}");
        writer.WriteLine($"JOIN #{_channel}");
        // writer.WriteLine("CAP REQ :twitch.tv/tags"); // enables metadata
        writer.Flush();
    }

    void ChatRead(){
        if(client == null)
            return;
        if(client.Available <= 0)
            return;

        string message = reader.ReadLine();

        if (message.StartsWith("PING")){
            writer.WriteLine("PONG :tmi.twitch.tv\r\n");
            writer.Flush();
            return;
        }
        Debug.Log(message);
        ChatMessage chatMessage = ParseMessage(message);
        if(chatMessage == null)
            return;
        HandleCommand(chatMessage);
        UpdateDestructionList(chatMessage);
    }

    ChatMessage ParseMessage(string raw){
        if (!raw.Contains("PRIVMSG"))
            return null;

        // Extract username
        int nameEnd     = raw.IndexOf('!');
        string username = raw[1..nameEnd];

        if(!TrackedName(username))
            return null;

        // Extract message
        int msgIndex = raw.IndexOf("PRIVMSG");
        int msgStart = raw.IndexOf(':', msgIndex);
        string message = raw[(msgStart + 1)..];

        ChatMessage NewMessage = new(){
            Sender     = username.ToLower(),
            Body       = message,
            DeleteTime = DateTime.Now.AddMinutes(DESTRUCTIONTIME)
        };
        return NewMessage;
    }

    private void HandleCommand(ChatMessage message){
        GameObject BerdObject = GetBerd(message);
        Berd berd = BerdObject.GetComponent<Berd>();
        if(!message.Body.StartsWith("!"))
            return;

        string[] parts = message.Body.Split(' ');

        if(BerdObject == null)
            return;

        switch (parts[0]){
            case "!left":
            case "!right":
                float distance = 1;
                float speed    = 1;
                if(parts.Length > 1)
                    float.TryParse(parts[1],out distance);
                if(parts.Length > 2)
                    float.TryParse(parts[2],out speed);
                speed = Mathf.Abs(speed);
                speed = Mathf.Clamp(speed,MINBERDSPEED,MAXBERDSPEED);
                berd.Move(parts[0] == "!left",distance,speed);
                break;
        }
    }

    private void UpdateDestructionList(ChatMessage Message){
        Destructionlist.RemoveAll(m => m != null && m.Sender.ToLower() == Message.Sender.ToLower());
        Destructionlist.Add(Message);
        _DeletionTracker ??= StartCoroutine(DeletingBerds());
    }

    IEnumerator DeletingBerds(){
        while(Destructionlist.Count != 0){
            ChatMessage ToCheck = Destructionlist[0];
            Destructionlist.RemoveAt(0);
            yield return new WaitForSeconds((float)(ToCheck.DeleteTime - DateTime.Now).TotalSeconds);
            if(Destructionlist.Count(m => m.Sender == ToCheck.Sender) > 0)
                continue;
            GameObject ToDestroy = ActiveBerds.First(m => m.name.ToLower() == ToCheck.Sender.ToLower());
            ActiveBerds.Remove(ToDestroy);
            Destroy(ToDestroy);
        }
    }
}