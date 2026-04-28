using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConnectUI : MonoBehaviour
{
    [SerializeField] BerdInterface Interface = null;
    [SerializeField] TMP_InputField _username;
    [SerializeField] TMP_InputField _channel;
    [SerializeField] TMP_InputField _OAuth;
    // Start is called before the first frame update
    void Start(){
        string username = PlayerPrefs.GetString("username", "");
        string channel  = PlayerPrefs.GetString("channel", "");
        string OAuth    = PlayerPrefs.GetString("OAuth", "");

        if(username != "") _username.text = username;
        if(channel != "")  _channel.text = channel;
        if(OAuth != "")    _OAuth.text = OAuth;
    }

    public void Connect(){
        if (!Interface.Chatconnect(_username.text, _channel.text, _OAuth.text))
            return;
        PlayerPrefs.SetString("username", _username.text);
        PlayerPrefs.SetString("channel", _channel.text);
        PlayerPrefs.SetString("OAuth", _OAuth.text);
        Destroy(gameObject);
    }
}
