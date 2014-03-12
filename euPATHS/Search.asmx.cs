using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Web.Script.Services;
using System.ComponentModel;
using System.Web.Script.Serialization;
using System.Data.Odbc;

using SolrNet;
using SolrNet.Attributes;
using Microsoft.Practices.ServiceLocation;
using euPATHS.AppCode;
namespace euPATHS
{
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.

    /// <summary>
    /// Web service to perform do a simple search against the SolrIndex
    /// </summary>
    [System.Web.Script.Services.ScriptService()]
    [System.Web.Services.WebService(Namespace = "http://paths-project.eu/")]
    [System.Web.Services.WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    public class Search : System.Web.Services.WebService
    {
        #region -------------------DoSearch-------------------

        [WebMethod()]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string DoSearch(string mQuery, string mStart, string mLength)
        {
            var mTimer = new Stopwatch();
            mTimer.Start();

            string strRV = string.Empty;
            ISolrOperations<Dictionary<string, object>> mSolr = default(ISolrOperations<Dictionary<string, object>>);
            // This method must be improved at some stage to remove the unecessary exception
            try
            {
                mSolr = ServiceLocator.Current.GetInstance<ISolrOperations<Dictionary<string, object>>>();
            }
            catch (Exception)
            {
                Startup.Init<Dictionary<string, object>>(Utility.solrInstance);
                mSolr = ServiceLocator.Current.GetInstance<ISolrOperations<Dictionary<string, object>>>();
            }
            SolrQueryResults<Dictionary<string, object>> mSolrResults = new SolrQueryResults<Dictionary<string, object>>();
            SolrNet.Commands.Parameters.QueryOptions mQueryOptions = new SolrNet.Commands.Parameters.QueryOptions();

            //Set offset from start (used to load page number "N")
            if (Utility.IsNumeric(mStart))
            {
                mQueryOptions.Start = Convert.ToInt32(mStart);
            }
            else
            {
                mQueryOptions.Start = 0;
            }

            //Set pagesize
            if (Utility.IsNumeric(mLength))
            {
                mQueryOptions.Rows = Convert.ToInt32(mLength);
            }
            else
            {
                mQueryOptions.Rows = 10;
            }

            //Set default query
            if (string.IsNullOrEmpty(mQuery))
            {
                mQuery = "*:*";
            }

            //Execute query
            try
            {
                mSolrResults = mSolr.Query(mQuery, mQueryOptions);
                //Create result container
                Dictionary<string, object> mDictionary = new Dictionary<string, object>();
                mDictionary.Add("code", Utility.msgStatusCodes.OperationCompletedSuccessfully);
                mDictionary.Add("data", mSolrResults);
                //Create JSON and return
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Utility.LogRequest(strRV, false, mTimer);
                strRV = serializer.Serialize(mDictionary);
            }
            catch (Exception ex)
            {
                Utility.LogRequest(strRV, true, mTimer);
                strRV = Utility.GetMsg(Utility.msgStatusCodes.OperationFailed, "SOLR error: " + ex.Message);

            }
            mSolr = null;
            mSolrResults = null;
            return strRV;
        }
        #endregion
    }
    
}
