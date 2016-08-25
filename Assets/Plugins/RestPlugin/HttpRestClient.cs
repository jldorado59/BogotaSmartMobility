using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using System.Text.RegularExpressions;

public class HttpRestClient : IRestClient {

    string _endPoint;
    string _contentType;
    Action<string> _callback;

    public HttpRestClient(string endPoint, string contentType)
    {
        _endPoint = endPoint;
        _contentType = contentType;
    }

    public string Request(string resource, ERestMethod method, params string[] parameters)
    { 
        HttpWebRequest request = CreateRequest(resource, method, parameters);
        
        HttpWebResponse response = request.GetResponse() as HttpWebResponse;

        if (response.StatusCode != HttpStatusCode.OK)
        {
            string message = String.Format("Request failed. Received HTTP {0}", response.StatusCode);
            throw new ApplicationException(message);
        }

        StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);            
        return UnescapeString(reader.ReadToEnd());
    }

    public void AsyncRequest(string resource, ERestMethod method, string[] parameters, Action<string> callback)
    { 
        HttpWebRequest request = CreateRequest(resource, method, parameters);

        _callback = callback;

        request.BeginGetResponse(new AsyncCallback(GetResponceCallback), request);
    }

    void GetResponceCallback(IAsyncResult callbackResult)
    {
        HttpWebRequest request = callbackResult.AsyncState as HttpWebRequest;
        HttpWebResponse response = request.EndGetResponse(callbackResult) as HttpWebResponse;

        StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
        _callback(UnescapeString(reader.ReadToEnd()));
    }


    protected HttpWebRequest CreateRequest(string resource, ERestMethod method, string[] parameters)
    {
        HttpWebRequest request = null;

        switch (method)
        {
            case ERestMethod.GET:
                string query = string.Empty;

                if (parameters != null && parameters.Length > 0)
                {
                    string[] data = new string[parameters.Length / 2];

                    for (int i = 0; i < parameters.Length; i = i + 2)
                        data[i] = string.Format("{0}={1}", Uri.EscapeDataString(parameters[i]), Uri.EscapeUriString(parameters[i + 1]));                

                    query = "?" + string.Join("&", data);
                }

                string url = _endPoint + "/" + resource + query;
                request = (HttpWebRequest) WebRequest.Create(Uri.EscapeUriString(url));
                request.Method = WebRequestMethods.Http.Get;

                break;
            case ERestMethod.POST:
                throw new ApplicationException("TODO:");
                break;
        }

        request.ContentLength = 0;
        request.ContentType = _contentType;
        request.Accept = _contentType;

        return request;
    }

    string UnescapeString(string s)
    {
        if (s.StartsWith("\""))
        {
            s = Regex.Unescape(s);
            s = s.Substring(1, s.Length - 2);
        }

        return s;
    }
}

