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
    /// Time in minutes before a berd gets DESTROYED!
    /// </summary>
    private static readonly float DESTRUCTIONTIME = 60.0f;
    [Serializable]
    private class ChatMessage{
        public string Sender;
        public string Body;
        public DateTime DeleteTime;
    }


    private TcpClient client;
    private StreamReader reader;
    private StreamWriter writer;

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

    #region Admin Vars
    bool EnableAdminInputs = false;
    [SerializeField] bool EnableInputs = true;
    bool EnableSpawns = true;
    bool EnableRemoval = true;
    #endregion
    // Start is called before the first frame update
    void Start(){
    }

    // Update is called once per frame
    void Update(){
        AdminInputs();
        ChatRead();
    }

    void AdminInputs()
    {
        if(Input.GetKeyDown(KeyCode.Q)) EnableAdminInputs = !EnableAdminInputs;
        if(!EnableAdminInputs) return;

        if(Input.GetKeyDown(KeyCode.D)) EnableInputs  = !EnableInputs;
        if(Input.GetKeyDown(KeyCode.S)) EnableSpawns  = !EnableSpawns;
        if(Input.GetKeyDown(KeyCode.R)) EnableRemoval = !EnableRemoval;

        if(Input.GetKeyDown(KeyCode.A)) StartCoroutine(SPAWNBERDARMY());
        if(Input.GetKeyDown(KeyCode.C)) StartCoroutine(ClearBerds());
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
        GameObject ReturningBerd = ActiveBerds.FirstOrDefault(b => b != null && b.name.ToLower() == name.ToLower());

        if(ReturningBerd != null || !EnableSpawns)
            return ReturningBerd;

        //Make a new berd based upon the list of all berds
        ReturningBerd = berds.FirstOrDefault(b => b != null && b.name.ToLower() == name.ToLower());

        if(ReturningBerd == null)
            return null;

        ReturningBerd = Instantiate(ReturningBerd,transform);
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

    public bool Chatconnect(string username = null, string channel = null, string oauth = null){
        if(username == null)
            return false;
        if(channel == null)
            return false;
        if(oauth == null)
            return false;
        //setup Twitch connection
        client = new TcpClient("irc.chat.twitch.tv", 6667);
        reader = new StreamReader(client.GetStream());
        writer = new StreamWriter(client.GetStream());

        writer.WriteLine($"PASS oauth:{oauth}");
        writer.WriteLine($"NICK {username}");
        writer.WriteLine($"USER {username} 8 * :{username}");
        writer.WriteLine($"JOIN #{channel}");
        // writer.WriteLine("CAP REQ :twitch.tv/tags"); // enables metadata
        writer.Flush();
        return true;
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
        ChatMessage chatMessage = ParseMessage(message);
        if(chatMessage == null)
            return;
        if(EnableInputs)
            HandleCommand(chatMessage);
        if(EnableRemoval)
            UpdateDestructionList(chatMessage);
    }

    ChatMessage ParseMessage(string raw){
        if (!raw.Contains("PRIVMSG"))
            return null;

        int nameEnd     = raw.IndexOf('!');
        string username = raw[1..nameEnd];

        if(!TrackedName(username))
            return null;

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
        Berd berd;

        if(BerdObject == null)
            return;
        if(!EnableInputs)
            return;

        berd = BerdObject.GetComponent<Berd>();
        if(!message.Body.StartsWith("!"))
            return;

        string[] parts = message.Body.Split(' ');
        string Command = parts[0];
        float Arg1 = 1;
        float Arg2 = 1;
        Debug.Log(parts.Length);
        if(parts.Length > 1)
            float.TryParse(parts[1],out Arg1);
        if(Arg1 == 0)
            Arg1 = 1;
        if(parts.Length > 2)
            float.TryParse(parts[2],out Arg2);
        Arg2 = Mathf.Abs(Arg2);
        switch (Command){
            case "!reset":
                berd.ResetBerd();
                break;
            case "!up":
            case "!down":
            case "!left":
            case "!right":
                bool Vertical = Command == "!up" || Command == "!down";
                Vector2 direction = Vertical ? Vector2.up : Vector2.right;
                if(Command == "!left" || Command == "!down")
                    direction *= -1;
                direction *= Arg1;
                berd.MoveDir(direction,Arg2);
                break;
            case "!scale":
                berd.Scale(Arg1,Arg2);
                break;
            case "!quack":
                berd.Quack();
                break;
            case "!wiggle":
                Debug.Log($"WIGGLE {message.Sender}!, amplitude{Arg1}");
                berd.Wiggle(Arg1,Arg2);
                break;
        }
    }

    private void UpdateDestructionList(ChatMessage Message){
        Destructionlist.RemoveAll(m => m != null && m.Sender.ToLower() == Message.Sender.ToLower());
        Destructionlist.Add(Message);
        _DeletionTracker ??= StartCoroutine(DeletingBerds());
    }

    IEnumerator DeletingBerds(){
        while(Destructionlist.Count != 0 && EnableRemoval){
            ChatMessage ToCheck = Destructionlist[0];
            Destructionlist.RemoveAt(0);
            yield return new WaitForSeconds((float)(ToCheck.DeleteTime - DateTime.Now).TotalSeconds);
            if(!EnableRemoval)
                break;
            if(Destructionlist.Count(m => m.Sender == ToCheck.Sender) > 0)
                continue;
            GameObject ToDestroy = GetBerd(ToCheck);
            ActiveBerds.Remove(ToDestroy);
            ToDestroy.GetComponent<Berd>().DespawnBerd();
        }
    }

    private IEnumerator SPAWNBERDARMY(){
       foreach(GameObject berd in berds){
            GetBerd(berd.name.ToLower());
            ChatMessage NewMessage = new(){
                Sender     = berd.name.ToLower(),
                Body       = "",
                DeleteTime = DateTime.Now.AddMinutes(DESTRUCTIONTIME)
            };
            if(EnableRemoval)
                UpdateDestructionList(NewMessage);
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f,0.3f));
        }
    }
    private IEnumerator ClearBerds(){
        foreach(GameObject berd in ActiveBerds){
            berd.GetComponent<Berd>().DespawnBerd();
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f,0.3f));
        }
        ActiveBerds.Clear();
        Destructionlist.Clear();
        StopAllCoroutines();
    }
}