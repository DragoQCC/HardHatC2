using System;

namespace ApiModels.Shared
{
    [Serializable]
    public class C2Profile
    {
        public string Name { get; set; }
        public string Desc { get; set; }
        public string Urls { get; set; }
        public string Cookies { get; set; }
        public string RequestHeaders { get; set; } //headers on the implant
        public string ResponseHeaders { get; set; } //headers set on the maanger to respond with 
        public string UserAgent { get; set; }
    }
}
