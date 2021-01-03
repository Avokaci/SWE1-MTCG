using System;
using System.Collections.Generic;
using System.Text;

namespace RESTHTTPWebservice
{
    public class RequestContext
    {

        private string _verb;
        private string _path;
        private string _httpVersion;
        private Dictionary<string, string> _headerLines;
        private string _payload;

        public string Verb { get => _verb; set => _verb = value; }
        public string Path { get => _path; set => _path = value; }
        public string HttpVersion { get => _httpVersion; set => _httpVersion = value; }
        public string Payload { get => _payload; set => _payload = value; }
        public Dictionary<string, string> HeaderLines { get => _headerLines; set => _headerLines = value; }

        public RequestContext(string verb, string path, string httpVersion, Dictionary<string, string> headerLines, string payload)
        {
            this.Verb = verb;
            this.Path = path;
            this.HttpVersion = httpVersion;
            this.HeaderLines = headerLines;
            this.Payload = payload;
        }

        //for debuging purposes
        public override string ToString()
        {
            string header = Verb + " " + Path + " " + HttpVersion + "\n";
            string headerLines = "";
            foreach (KeyValuePair<string, string> item in HeaderLines)
            {
                headerLines += item.Key + ": " + item.Value + "\n";
            }
            return header + headerLines + "\n" + Payload;
        }

    }
}
