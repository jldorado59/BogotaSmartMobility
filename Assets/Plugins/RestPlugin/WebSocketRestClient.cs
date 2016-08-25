using UnityEngine;
using System.Collections;
using System;
using WebSocketSharp;

public class WebSocketRestClient : IRestClient
{
    string _endPoint;
    string _contentType;
    string _message;
    bool _hasNewMessage;
    WebSocketSharp.WebSocket _socket;

    public WebSocketRestClient(string endPoint, string contentType)
    {
        _endPoint = endPoint;
        _contentType = contentType;

        _socket = new WebSocket(_endPoint);
        _socket.Connect();
        _socket.OnMessage += OnMessage;
    }

    public string Request(string resource, ERestMethod method, params string[] parameters)
    { 
        _hasNewMessage = false;
        string request = CreateRequest(resource, method, parameters);
        _socket.Send(request);       

        string response = string.Empty;
        switch (method)
        {
            case ERestMethod.GET:

                while (!_hasNewMessage)
                    DoNothing();

                response = _message;

                break;

        }

        _hasNewMessage = false;
        return response;
    }

    public void AsyncRequest(string resource, ERestMethod method, string[] parameters, Action<string> callback)
    {
        
    }

    public string CreateRequest(string resource, ERestMethod method, params string[] parameters)
    {
        string query = string.Empty;

        if (parameters != null && parameters.Length > 0)
        {
            string[] data = new string[parameters.Length / 2];

            for (int i = 0; i < parameters.Length; i = i + 2)
                data[i] = string.Format("{0}={1}", Uri.EscapeDataString(parameters[i]), Uri.EscapeUriString(parameters[i + 1]));                

            query = "&" + string.Join("&", data);
        }

        return method + resource + query;
    }

    void OnMessage(object sender, MessageEventArgs args)
    {
        _hasNewMessage = true;
        _message = args.Data;
    }

    void DoNothing()
    {
    }

}
