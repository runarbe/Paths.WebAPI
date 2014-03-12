using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.ComponentModel;
using System.Data.Odbc;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using euPATHS.AppCode;
using System.Text;


namespace euPATHS
{
    /// <summary>
    /// The Path web service contains methods for creation, editing and deletion of paths and path nodes. Furthermore, it has functions to transfer work space items to nodes in a path and to qury paths and nodes. Paths and nodes are the core dynamic objects in the PATHS Web Service API. A path consist of one or more nodes, a node references an item (or another object) via a URI.
    /// </summary>
    /// <remarks></remarks>
    [System.Web.Script.Services.ScriptService()]
    [System.Web.Services.WebService(Namespace = "http://paths-project.eu/")]
    [System.Web.Services.WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    public class test : System.Web.Services.WebService
    {
        #region --------------------Get Path #232-----------------------
        /// <summary>
        /// Get a single path identified by its URI
        /// </summary>
        /// <param name="path_uri">URI of path to be retrieved</param>
        /// <returns>Path data object</returns>
        /// <remarks></remarks>
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Get()
        {
            int iCount = 0;
            int iCounty = 0;
            //Load modified record and update SOLR
            string strQuery = "";
            DataTable dtab;
            strQuery = "SELECT id, fk_usr_id as dc_creator,uri paths_identifier,access,'' paths_topics,paths_thumbnail, dc_title, dc_description, dc_subject, lom_length as paths_duration,'path' AS dc_type,to_char(tstamp,'yyyy-mm-ddTmm:hh:ss') as dc_date, paths_status FROM path WHERE  isdeleted=false;";
            //strQuery = "SELECT p.*,p.uri paths_identifier,to_char(p.tstamp,'yyyy-mm-ddTmm:hh:ss') as dc_date, u.uri as dc_creator FROM path p, usr u WHERE u.id = p.fk_usr_id AND p.uri = '" + paths_identifier + "' AND p.isdeleted=false;";
            dtab = Utility.DBExecuteDataTable(strQuery);
            //Check if any path was returned
            foreach (DataRow drowp in dtab.Rows)
            {
                //Load related nodes 
                DataTable mNodeTab;
                strQuery = "SELECT uri paths_identifier, dc_title,dc_description, 'node' as dc_type FROM node  WHERE fk_path_id = '" + Convert.ToString(drowp["id"]) + "' AND isdeleted=false";
                mNodeTab = Utility.DBExecuteDataTable(strQuery);

                //If status = published then post to SOLR
                if (Convert.ToString(drowp["access"]).ToLower() == "public")
                {
                    ///*****Updated on 25 Sept 2013 for Ticket no 282*****///
                    strQuery = "select distinct(paths_topics) from node inner join path on path.id = node.fk_path_id where node.isdeleted = false and paths_topics <> '' and path.uri = '" + Convert.ToString(drowp["paths_identifier"]) + "' ;";
                    DataTable dtabTopics = Utility.DBExecuteDataTable(strQuery);
                    if (dtabTopics.Rows.Count > 0)
                    {
                        StringBuilder sbPathTopics = new StringBuilder();
                        foreach (DataRow drow in dtabTopics.Rows)
                        {
                            if (Convert.ToString(drow["paths_topics"]) != "")
                                sbPathTopics.Append(drow["paths_topics"] + ",");
                        }
                        string strPathTopics = sbPathTopics.ToString().Replace("\"", "");
                        strPathTopics = strPathTopics.TrimEnd().Substring(0, strPathTopics.LastIndexOf(","));
                        dtab.Rows[0]["paths_topics"] = strPathTopics;
                        iCounty++;
                    }
                    //*****END UPDATES*****//
                    Utility.PostDataTableToSOLR(dtab, "path");
                    Utility.PostDataTableToSOLR(mNodeTab, "node");
                    //Otherwise, delete uri from SOLR
                }
                iCount++;
            }
            return "Solr Index Done--" + iCount + "-----" + iCounty;
        }
        #endregion
    }
}
