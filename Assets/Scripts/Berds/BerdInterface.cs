using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BerdInterface : MonoBehaviour
{
    /// <summary>
    /// Time in minutes before a berd gets DESTROYED!
    /// </summary>
    private static readonly float DESTRUCTIONTIME = 60.0f;
    private static readonly int ARGCOUNT = 5;
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
    private string username = "";
    private bool EnableAdminInputs = false;
    private bool EnableInputs = true;
    private bool EnableSpawns = true;
    private bool EnableRemoval = true;
    static public bool EnableQuackCooldown{get;private set;} = true;
    #endregion
    public static BoundPos WALKBOUND{get;private set;}
    public static BoundPos DRAGBOUND{get;private set;}
    public static BoundPos SPAWNBOUND{get;private set;}
    public static BoundPos DESPAWNBOUND{get;private set;}
    [field:SerializeField] public BoundPos WalkBound{get;private set;}    = new(-7.8,8.5,-4.96f,-3.56f);
    [field:SerializeField] public BoundPos DragBound{get;private set;}    = new(-7.8,8.5,-4.96f,3.58f);
    [field:SerializeField] public BoundPos SpawnBound{get;private set;}   = new(-7.8,8.5,-4.96f,-3.56f);
    [field:SerializeField] public BoundPos DespawnBound{get;private set;} = new(-7.8,8.5,-4.96f,3.58f);

    void Awake()
    {
        WALKBOUND    = WalkBound;
        DRAGBOUND    = DragBound;
        SPAWNBOUND   = SpawnBound;
        DESPAWNBOUND = DespawnBound;
    }

    // Update is called once per frame
    void Update(){
        AdminInputs();
        GrabBerds();
        ChatRead();
    }

#if UNITY_EDITOR
    public void FindAllBerds(){
        string folderPath = "Assets/Prefabs/Berds";
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
        berds.Clear();
        foreach (string guid in guids){
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            if (assetPath.Contains("Berd Base"))
                continue;

            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (asset == null)
                continue;
            berds.Add(asset);

            if(!assetPath.Contains(" Variant"))
                continue;

            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            string result = AssetDatabase.RenameAsset(assetPath, fileName.Replace(" Variant", ""));

            if (!string.IsNullOrEmpty(result))
                Debug.LogError($"Rename failed: {result}");
        }
    }
#endif

    void AdminInputs(){
        if(Input.GetKeyDown(KeyCode.Q)) EnableAdminInputs = !EnableAdminInputs;
        if(!EnableAdminInputs) return;

        if(Input.GetKeyDown(KeyCode.D)) EnableInputs  = !EnableInputs;
        if(Input.GetKeyDown(KeyCode.S)) EnableSpawns  = !EnableSpawns;
        if(Input.GetKeyDown(KeyCode.R)) EnableRemoval = !EnableRemoval;
        if(Input.GetKeyDown(KeyCode.E)) EnableQuackCooldown = !EnableQuackCooldown;

        if(Input.GetKeyDown(KeyCode.A)) StartCoroutine(SPAWNBERDARMY());
        if(Input.GetKeyDown(KeyCode.C)) StartCoroutine(ClearBerds());
    }

    void GrabBerds(){
        if(!Input.GetMouseButtonDown(0))
            return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin,ray.direction);
        Transform HitObject = hit ? hit.collider.transform : null;
        if(HitObject == null || !HitObject.transform.TryGetComponent(out Berd shotBerd))
            return;
        shotBerd.StartDragging();
    }

    private bool TrackedName(string name = "") => berds.Any(b => b != null && string.Equals(b.name,name, StringComparison.OrdinalIgnoreCase));
    private bool TrackedName(ChatMessage message) => TrackedName(message.Sender);

    /// <summary>
    /// Get the berd associated with this name.
    /// Use Tracked name before this. Any name given to this function should be valid
    /// </summary>
    /// <param name="name">Lowercase name of the sender</param>
    /// <returns></returns>
    private GameObject GetBerd(string name = "", bool spawn = true){
        //See if the berd is contained within the active berds list.
        if(name.StartsWith('@'))
            name = name[1..];
        GameObject ReturningBerd = ActiveBerds.FirstOrDefault(b => b != null && string.Equals(b.name,name,StringComparison.OrdinalIgnoreCase));

        if(ReturningBerd != null || !EnableSpawns || !spawn)
            return ReturningBerd;

        //Make a new berd based upon the list of all berds
        ReturningBerd = berds.FirstOrDefault(b => b != null && string.Equals(b.name,name,StringComparison.OrdinalIgnoreCase));

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
    private GameObject GetBerd(ChatMessage message = null, bool spawn = true) => message != null ? GetBerd(message.Sender,spawn) : null;

    public bool Chatconnect(string username = null, string channel = null, string oauth = null){
        if(username == null)
            return false;
        if(channel == null)
            return false;
        if(oauth == null)
            return false;
        this.username = username.ToLower();

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
            Debug.Log(message);
            writer.WriteLine("PONG :tmi.twitch.tv\r\n");
            writer.Flush();
            return;
        }
        ChatMessage chatMessage = ParseMessage(message);
        if(chatMessage == null)
            return;
        if(chatMessage.Sender == username)
            HandleAdminCommand(chatMessage);
        if(EnableInputs)
            HandleCommand(chatMessage);
        if(EnableRemoval)
            UpdateDestructionList(chatMessage);
    }
    /// <summary>
    /// Parses a raw twitch message into the chatmessage format.
    /// </summary>
    /// <param name="RawMessage">Raw twitch message</param>
    /// <returns>Parsed message in chatmessage class</returns>
    ChatMessage ParseMessage(string RawMessage){
        if (!RawMessage.Contains("PRIVMSG"))
            return null;

        int nameEnd     = RawMessage.IndexOf('!');
        string username = RawMessage[1..nameEnd];

        if(!TrackedName(username) && string.Equals(username, this.username,StringComparison.OrdinalIgnoreCase))
            return null;

        int msgIndex = RawMessage.IndexOf("PRIVMSG");
        int msgStart = RawMessage.IndexOf(':', msgIndex);
        string message = RawMessage[(msgStart + 1)..];

        return new(){
            Sender     = username.ToLower(),
            Body       = message,
            DeleteTime = DateTime.Now.AddMinutes(DESTRUCTIONTIME)
        };
    }
    private void HandleAdminCommand(ChatMessage message){
        if(!message.Body.StartsWith("!"))
            return;

        string[] parts = message.Body.Split(' ');
        string Command = parts[0];

        switch (Command){
            case "!summon":
                GetBerd(parts[1]);
                break;
            case "!dismiss":
                GameObject berd = GetBerd(parts[1], false);
                if(berd == null)
                    return;
                Berd berdScript = berd.GetComponent<Berd>();
                berdScript.DespawnBerd();
                ActiveBerds.Remove(berd);
                break;
            case "!admin":
                if(parts.Length < 1)
                    return;
                ChatMessage phony = new (){
                    Sender = parts[0],
                    Body = ""
                };
                for(int i = 1; i < parts.Length; i++)
                    phony.Body.Concat(parts[i] + " ");
                phony.Body = phony.Body[..^1];
                HandleCommand(phony);
                break;
            default:
                break;
        }
    }
    private void HandleCommand(ChatMessage message){
        GameObject BerdObject = GetBerd(message);

        if(BerdObject == null)
            return;
        if(!EnableInputs)
            return;

        Berd berd = BerdObject.GetComponent<Berd>();
        if(berd.IsDragging)
            return;
        if(!message.Body.StartsWith("!"))
            return;

        string[] parts = message.Body.Split(' ');
        string Command = parts[0][1..].ToLower();
        float[] Fargs = new float[ARGCOUNT];
        for(int i = 0; i < ARGCOUNT; i++){
            if(i >= parts.Length-1 || !float.TryParse(parts[i+1], out Fargs[i]))
                Fargs[i] = 1;
        }


        switch (Command){
            case "reset": berd.ResetBerd(); break;
            case "scale" : berd.Scale(Fargs[0],Fargs[1]); break;
            case "quack" : berd.Quack(Fargs[0],Fargs[1]); break;
            case "wiggle": berd.Wiggle(Fargs[0],Fargs[1]); break;

            case "up": case "down": case "left": case "right":
                bool Vertical = Command == "up" || Command == "down";
                Vector2 direction = Vertical ? Vector2.up * 0.1f : Vector2.right;
                if(Command == "left" || Command == "down")
                    direction *= -1;
                direction *= Fargs[0] == 0 ? 1 : Fargs[0];
                berd.MoveDir(direction,Fargs[1]);
                break;

            //Handling of variable commands. Defined by the berds class themselves
            default:
                //all double arg commands. Like !follow [berd]
                if(parts.Length < 2)
                    break;
                GameObject Berd2Object = GetBerd(parts[1],false);
                if(Berd2Object == null)
                    break;
                if(berd.TryFollow(Berd2Object,Command,Fargs[2]) != -1)
                    break;
                break;

        }
    }

    private void UpdateDestructionList(ChatMessage Message){
        Destructionlist.RemoveAll(m => m != null && string.Equals(m.Sender, Message.Sender,StringComparison.OrdinalIgnoreCase));
        Destructionlist.Add(Message);
        _DeletionTracker ??= StartCoroutine(DeletingBerds());
    }

    IEnumerator DeletingBerds(){
        while(Destructionlist.Count != 0 && EnableRemoval){
            ChatMessage ToCheck = Destructionlist[0];
            Destructionlist.RemoveAt(0);
            yield return new WaitForSeconds((float)(ToCheck.DeleteTime - DateTime.Now).TotalSeconds);
            if(!EnableRemoval)
                yield break;
            if(Destructionlist.Count(m => m.Sender == ToCheck.Sender) > 0)
                continue;
            GameObject ToDestroy = GetBerd(ToCheck, false);
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
    void OnDrawGizmos(){
        DrawBoundPosGizmo(WalkBound,Color.yellow);
        DrawBoundPosGizmo(DragBound,Color.blue);
        DrawBoundPosGizmo(SpawnBound,Color.green);
        DrawBoundPosGizmo(DespawnBound,Color.red);
    }
    void DrawBoundPosGizmo(BoundPos bounds, Color color){
        Gizmos.color = color;
        Gizmos.DrawLine(bounds.UPLEFT   ,bounds.DOWNLEFT);
        Gizmos.DrawLine(bounds.UPLEFT   ,bounds.UPRIGHT);
        Gizmos.DrawLine(bounds.UPRIGHT  ,bounds.DOWNRIGHT);
        Gizmos.DrawLine(bounds.DOWNRIGHT,bounds.DOWNLEFT);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(BerdInterface))]
public class BerdInterfaceEditor : Editor{
    BerdInterface script;
    SerializedProperty berds;
    SerializedProperty Active;
    SerializedProperty walk;
    SerializedProperty drag;
    SerializedProperty spawn;
    SerializedProperty despawn;
    protected virtual void OnEnable()
    {
        if (target == null || serializedObject == null)
            return;
        script  = (BerdInterface)target;
        berds   = serializedObject.FindProperty("berds");
        Active  = serializedObject.FindProperty("ActiveBerds");
        walk    = serializedObject.FindProperty("<WalkBound>k__BackingField");
        drag    = serializedObject.FindProperty("<DragBound>k__BackingField");
        spawn   = serializedObject.FindProperty("<SpawnBound>k__BackingField");
        despawn = serializedObject.FindProperty("<DespawnBound>k__BackingField");
    }
    public override void OnInspectorGUI(){
        if (target == null || serializedObject == null)
            return;

        if (GUILayout.Button("Find Berds"))
            script.FindAllBerds();

        EditorGUILayout.PropertyField(berds);
        EditorGUILayout.PropertyField(Active);
        EditorGUILayout.PropertyField(walk);
        EditorGUILayout.PropertyField(drag);
        EditorGUILayout.PropertyField(spawn);
        EditorGUILayout.PropertyField(despawn);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif