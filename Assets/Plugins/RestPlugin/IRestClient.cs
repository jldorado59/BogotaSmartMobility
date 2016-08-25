using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface IRestClient
{
    string Request(string resource, ERestMethod method, params string[] parameters);

    void AsyncRequest(string resource, ERestMethod method, string[] parameters, Action<string> callback);
}

public enum ERestMethod
{
    GET,
    POST,
    PUT,
    DELETE
}


