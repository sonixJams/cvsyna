using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SemanticResumeAnalyzer
{
    public class IbmNluTextAnalyzer
    {
        public string RawResult { get; set; }
        public string Language { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public IbmNluTextAnalyzer() { }

        #region Response helper classes

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        internal class Usage
        {
            public int text_units { get; set; }
            public int text_characters { get; set; }
            public int features { get; set; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        internal class Document
        {
            public double score { get; set; }
            public string label { get; set; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        internal class Sentiment
        {
            public Document document { get; set; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        internal class Sentiment2
        {
            public double score { get; set; }
            public string label { get; set; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        internal class Emotion
        {
            public double sadness { get; set; }
            public double joy { get; set; }
            public double fear { get; set; }
            public double disgust { get; set; }
            public double anger { get; set; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        internal class Keyword
        {
            public string text { get; set; }
            public Sentiment2 sentiment { get; set; }
            public double relevance { get; set; }
            public Emotion emotion { get; set; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        internal class Disambiguation
        {
            public List<string> subtype { get; set; }
            public string name { get; set; }
            public string dbpedia_resource { get; set; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        internal class Entity
        {
            public string type { get; set; }
            public string text { get; set; }
            public Sentiment2 sentiment { get; set; }
            public double relevance { get; set; }
            public Emotion emotion { get; set; }
            public Disambiguation disambiguation { get; set; }
            public int count { get; set; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        internal class Concept
        {
            public string text { get; set; }
            public double relevance { get; set; }
            public string dbpedia_resource { get; set; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        internal class Category
        {
            public double score { get; set; }
            public string label { get; set; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        internal class ResponseDocument
        {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public Usage usage { get; set; }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public Sentiment sentiment { get; set; }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public string language { get; set; }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public List<Keyword> keywords { get; set; }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public List<Entity> entities { get; set; }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public List<Concept> concepts { get; set; }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public List<Category> categories { get; set; }
        }
        #endregion

        #region Http parameters
        private const string HttpParamFeatures = "features";
        private const string HttpParamText = "text";
        private const string HttpParamVersion = "version";
        #endregion

        #region IBM request options
        private const string ServiceVersion = "2017-02-27";
        private const string ServiceOptCategories = "categories";
        private const string ServiceOptConcepts = "concepts";
        private const string ServiceOptEntities = "entities";
        private const string ServiceOptKeywords = "keywords";
        #endregion

        private const string analyzeEngineEndpoint = "https://gateway.watsonplatform.net/natural-language-understanding/api/v1/analyze";

        public void Process(string text)
        {
            List<string> requestServices = new List<string>();
            Dictionary<string, object> requestParams = new Dictionary<string, object>();
            
            string serviceUrl = analyzeEngineEndpoint;

            requestServices.Add(ServiceOptCategories);
            requestServices.Add(ServiceOptConcepts);
            requestServices.Add(ServiceOptEntities);
            requestServices.Add(ServiceOptKeywords);
            
            requestParams.Add(HttpParamVersion, ServiceVersion);
            requestParams.Add(HttpParamText, WebUtility.UrlEncode(text));
            requestParams.Add(HttpParamFeatures, string.Join(",", requestServices.ToArray()));

            string query = string.Join("&", requestParams.Select(kvp => string.Format("{0}={1}", kvp.Key, kvp.Value)));
            
            UriBuilder uriBuilder = new UriBuilder(serviceUrl);
            uriBuilder.Query = query;
            Uri analyzeEngineEndpointUri = uriBuilder.Uri;
            WebRequest request = WebRequest.Create(analyzeEngineEndpointUri);
            request.Credentials = new NetworkCredential(Username, Password);
            using (WebResponse response = request.GetResponse())
            {
                using (Stream dataStream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(dataStream))
                    {
                        string responseFromServer = reader.ReadToEnd();
                        ParseResponse(responseFromServer);
                    }
                }
            }
        }

        private void ParseResponse(string jsonResponse)
        {
            try
            {
                RawResult += (jsonResponse + Environment.NewLine);
                ResponseDocument response = JsonConvert.DeserializeObject<ResponseDocument>(jsonResponse);
                Language = response.language;
            }
            catch (Exception e)
            {
                Trace.TraceError("Error parsing server response", e.ToString());
            }
        }
    }
}
