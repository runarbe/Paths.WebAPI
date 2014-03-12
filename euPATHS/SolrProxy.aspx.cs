using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Threading;
using euPATHS;
using euPATHS.AppCode;
using System.Diagnostics;
namespace euPATHS
{
    /// <summary>
    /// The SolrProxy web service provides methods to issue queries to the Solr/Lucene index that contains indexed info about items, paths and nodes.
    /// </summary>
    /// <param name="q">SOLR/Lucene query experession, e.g. dc:type=item</param>
    /// <param name="sort">Sort expression, e.g. [field name] asc, [field name] desc</param>
    /// <param name="start">Offset from start of search result (for paging). Default = 0</param>
    /// <param name="rows">The maximum number of rows to return</param>
    /// <param name="fq">Filter query expression</param>
    /// <param name="fl">Comma separated list of fields to include in response</param>
    /// <example>
    /// <para>The web service can be invoked via HttpGET by sending </para>
    /// <c>SolrProxy.aspx?q=dc_type:path</c>
    /// </example>
    /// <remarks>The service supports all common query parameters supported by Solr select as documented here: http://wiki.apache.org/solr/CommonQueryParameters</remarks>
    public partial class SolrProxy : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var mTimer = new Stopwatch();
            mTimer.Start();

            string myString = null;
            Response.ContentType = "application/json; encoding='UTF-8'";
            try
            {
                StringBuilder qS = new StringBuilder();
                foreach (string mKey in Request.QueryString.Keys)
                {
                    foreach (string mVal in Request.QueryString.GetValues(mKey))
                    {
                        if (mKey == "q" && mVal.IndexOf("dc_creator") == 0)
                        {
                            qS.Append(mKey.ToString() + "=" + System.Web.HttpUtility.UrlEncode("dc_creator:\"" + Utility.FromBase64ForUrlString(mVal.Replace("dc_creator:", "").Replace("\"", "")) + "\"", Encoding.UTF8) + "&");
                        }
                        else
                        {
                            qS.Append(mKey.ToString() + "=" + System.Web.HttpUtility.UrlEncode(mVal, Encoding.UTF8) + "&");
                        }
                    }
                }
                qS.Append("wt=json&");
                myString = WRequest(Utility.solrInstance + "/select/?" + qS.ToString(), "GET", "");
                Response.Write(myString);
                Utility.LogRequest(myString, false, mTimer);
                if (Utility.debugState)
                {
                    Utility.DebugToFile(myString);
                }
            }
            catch (Exception ex)
            {
                if (Utility.debugState)
                {
                    Utility.DebugToFile("SolrProxy error: " + ex.Message);
                }
                Response.Write("{\"code\":3, \"msg\":\"" + ex.Message + "\"}");
            }
        }

        public string WRequest(string pURL, string pHttpMethod, string pPostData)
        {
            string responseData = "";
            try
            {
                System.Net.HttpWebRequest hwrequest = (HttpWebRequest)System.Net.WebRequest.Create(pURL);
                hwrequest.Accept = "*/*";
                hwrequest.AllowAutoRedirect = true;
                hwrequest.UserAgent = "http_requester/0.1";
                hwrequest.Timeout = 60000;
                hwrequest.Method = pHttpMethod;
                if (hwrequest.Method == "POST")
                {
                    hwrequest.ContentType = "application/x-www-form-urlencoded";
                    //Dim encoding As New Text.ASCIIEncoding() 'Use UTF8Encoding for XML requests
                    System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
                    //Use UTF8Encoding for XML requests
                    byte[] postByteArray = encoding.GetBytes(pPostData);
                    hwrequest.ContentLength = postByteArray.Length;
                    System.IO.Stream postStream = hwrequest.GetRequestStream();
                    postStream.Write(postByteArray, 0, postByteArray.Length);
                    postStream.Close();
                }
                System.Net.HttpWebResponse hwresponse = (HttpWebResponse)hwrequest.GetResponse();
                if (hwresponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    System.IO.StreamReader responseStream = new System.IO.StreamReader(hwresponse.GetResponseStream(), System.Text.Encoding.UTF8);
                    responseData = responseStream.ReadToEnd();
                }
                hwresponse.Close();
            }
            catch (Exception ex)
            {
                if (Utility.debugState)
                {
                    Utility.DebugToFile("SolrProxy (WRequest) error: " + ex.Message + "--URL--" + pURL);
                }
                responseData = "{\"code\":3, \"msg\":\"" + ex.Message + "\"}";
            }
            return responseData;
        }
    }
}