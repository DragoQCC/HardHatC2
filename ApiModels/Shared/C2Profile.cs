using System;
using System.Collections.Generic;
using System.Globalization;

namespace ApiModels.Shared
{
    [Serializable]
    public class C2Profile
    {
        public string Name { get; set; }
        public string Desc { get; set; }
        public List<string> Urls { get; set; } = new(); //urls for the implant to check in to
        public List<string> EventUrls { get; set; } = new(); //url for events to post to 
        public List<string> Cookies { get; set; } = new();
        public List<string> RequestHeaders { get; set; } = new(); //headers on the implant
        public List<string> ResponseHeaders { get; set; } = new(); //headers set on the maanger to respond with 
        public string UserAgent { get; set; }
    }
}
