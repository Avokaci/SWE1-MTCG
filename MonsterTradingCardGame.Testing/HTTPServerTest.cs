using Moq;
using MTCG.Lib;
using NUnit.Framework;
using RESTHTTPWebservice;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MTCG.Testing
{
    public class HTTPServerTest
    {
        [Test]  
        public void RequestTest()
        {
//            string testMessage = "POST /users HTTP/1.1\r\n" +
//                "Host: localhost:10001\r\n" +
//                "User-Agent: curl/7.55.1\r\n" +
//                "Accept: */*\r\n" +
//                "Content-Type: application/json\r\n" +
//                "Content-Length: 44\r\n\r\n" +
//                "{\"Username\":\"kienboec\", \"Password\":\"daniel\"}";
//;
            string verb = "POST";
            string path = "/users";
            string httpversion = "HTTP/1.1";
            Dictionary<string, string> headerlines = new Dictionary<string, string>();
            headerlines.Add("Host", "localhost:10001");
            headerlines.Add("User-Agent", "curl/7.55.1");
            headerlines.Add("Accept", "*/*");
            headerlines.Add("Content-Type", "application/json");
            headerlines.Add("Content-Length", "44");
            string payload = "{\"Username\":\"altenhof\", \"Password\":\"markus\"}";

            RequestContext req = new RequestContext(verb, path, httpversion, headerlines, payload);

            Assert.AreEqual(verb, req.Verb);
            Assert.AreEqual(path, req.Path);
            Assert.AreEqual(httpversion, req.HttpVersion);
            Assert.AreEqual(headerlines, req.HeaderLines);
            Assert.AreEqual(payload, req.Payload);
        }



    }
}
