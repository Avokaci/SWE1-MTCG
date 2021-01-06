using NUnit.Framework;
using RESTHTTPWebservice;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTCG.Testing
{
    public class RequestContextTest
    {
        [Test]
        public void RequestContextRightVerb()
        {
            string verb ="GET";
            string path="/deck";
            string httpversion="HTTP/1.1";
            Dictionary<string, string> headerlines = new Dictionary<string, string>() ;
            headerlines.Add("Host", "localhost:10001");
            headerlines.Add("User-Agent", "curl/7.55.1");
            headerlines.Add("Accept", "*/*");
            headerlines.Add("Authorization", "Basic altenhof-mtcgToken");
            string payload ="";

            RequestContext request = new RequestContext(verb,path,httpversion,headerlines,payload);

            Assert.AreEqual("GET", request.Verb);
        }

        [Test]
        public void RequestContextRightPath()
        {
            string verb = "GET";
            string path = "/deck";
            string httpversion = "HTTP/1.1";
            Dictionary<string, string> headerlines = new Dictionary<string, string>();
            headerlines.Add("Host", "localhost:10001");
            headerlines.Add("User-Agent", "curl/7.55.1");
            headerlines.Add("Accept", "*/*");
            headerlines.Add("Authorization", "Basic altenhof-mtcgToken");
            string payload = "";

            RequestContext request = new RequestContext(verb, path, httpversion, headerlines, payload);

            Assert.AreEqual("/deck", request.Path);
        }

        [Test]
        public void RequestContextRightAuthorizationToken()
        {
            string verb = "GET";
            string path = "/deck";
            string httpversion = "HTTP/1.1";
            Dictionary<string, string> headerlines = new Dictionary<string, string>();
            headerlines.Add("Host", "localhost:10001");
            headerlines.Add("User-Agent", "curl/7.55.1");
            headerlines.Add("Accept", "*/*");
            headerlines.Add("Authorization", "Basic altenhof-mtcgToken");
            string payload = "";

            RequestContext request = new RequestContext(verb, path, httpversion, headerlines, payload);

            Assert.AreEqual("Basic altenhof-mtcgToken", request.HeaderLines["Authorization"]);
        }
    }
}
