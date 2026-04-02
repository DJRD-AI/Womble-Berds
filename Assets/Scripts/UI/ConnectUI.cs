using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConnectUI : MonoBehaviour
{
    public static bool Connected{get;private set;} = false;
    [SerializeField] BerdInterface Interface = null;
    [SerializeField] TMP_InputField _username;
    [SerializeField] TMP_InputField _channel;
    [SerializeField] TMP_InputField _OAuth;
    [SerializeField] TMP_InputField _camColour;
    void OnEnable()
    {
        _camColour.onValueChanged.AddListener(SetCamColour);
    }
    void OnDisable()
    {
        _camColour.onValueChanged.RemoveListener(SetCamColour);
    }
    void OnDestroy()
    {
        _camColour.onValueChanged.RemoveListener(SetCamColour);
    }
    // Start is called before the first frame update
    void Start()
    {
        string username = PlayerPrefs.GetString("username", "");
        string channel  = PlayerPrefs.GetString("channel", "");
        string OAuth    = PlayerPrefs.GetString("OAuth", "");
        string camCol   = PlayerPrefs.GetString("CamColour", "");

        if(username != "") _username.text = username;
        if(channel != "") _channel.text = channel;
        if(OAuth != "") _OAuth.text = OAuth;
        if(camCol != "") _camColour.text = camCol;
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public void connect(){
        PlayerPrefs.SetString("username", _username.text);
        PlayerPrefs.SetString("channel", _channel.text);
        PlayerPrefs.SetString("OAuth", _OAuth.text);
        if (!Interface.Chatconnect(_username.text, _channel.text, _OAuth.text))
            return;
        Connected = true;
        Destroy(transform.parent.gameObject);
    }

    public void SetCamColour(string colour = "")
    {
        if (!colour.Contains("#"))
            colour = "#" + colour;
        if (!ColorUtility.TryParseHtmlString(colour,out Color goal))
            return;

        PlayerPrefs.SetString("CamColour", colour);
        goal.a = 0;
        Camera.main.backgroundColor = goal;
    }
}
