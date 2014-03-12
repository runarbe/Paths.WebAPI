using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Web.Script.Services;
using System.ComponentModel;
using System.Web.Script.Serialization;
using System.Data.Odbc;
using euPATHS.AppCode;
using System.Text;

namespace euPATHS
{
    /// <summary>
    /// Summary description for myTest
    /// </summary>
    [System.Web.Script.Services.ScriptService()]
    [System.Web.Services.WebService(Namespace = "http://paths-project.eu/")]
    [System.Web.Services.WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class myTest : System.Web.Services.WebService
    {

        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string HelloWorld()
        {
            NameValueCollection mHttpRequestParameters = Context.Request.Params;
            Dictionary<string, string> mParameterDict = new Dictionary<string, string>();
            foreach (string key in mHttpRequestParameters)
            {
                mParameterDict.Add(key, mHttpRequestParameters[key]);
            }

            if (mParameterDict.ContainsKey("testvalue"))
            {
                if (mParameterDict["testvalue"] != "")
                {
                    return "Should be set to " + mParameterDict["testvalue"];
                }
                else
                {
                    return "Should be set to null";
                }

            }

            return "No key, no problem";

        }

    }
}
