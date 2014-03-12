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
using System.Linq;
using System.Web;


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

    public class Path : System.Web.Services.WebService
    {

        #region ------------------------Commencted-----------------------
        /*
      #region -------------------DeletePathNode-------------------
      /// <summary>
      /// Delete a node identified by its URI
      /// </summary>
      /// <param name="node_uri">URI of node to be deleted</param>
      /// <returns>JSON String: Single node information</returns>
      /// <remarks>Method requires authentication</remarks>
      [WebMethod(EnableSession = true)]
      [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
      public string DeletePathNode(string node_uri)
      {
         string strRV = string.Empty;
         DataTable dtab;
         if (Context.Session["isAuthenticated"] == null)
         {
            return Utility.GetMsg(Utility.msgStatusCodes.OperationRequiresAuthentication);
         }
         try
         {
            string strQuery = "SELECT * FROM node WHERE uri='" + node_uri + "' AND isdeleted=false";
            dtab = Utility.DBExecuteDataTable(strQuery);
            if (dtab.Rows.Count > 0)
            {
               strQuery = "UPDATE node SET isdeleted=true WHERE uri='" + node_uri + "';";
               int rtnID = Utility.DBExecuteNonQuery(strQuery);
               strRV = Utility.DataTableToDictionary(dtab, true);
               Utility.DeleteDocumentFromSOLR(new string[] { node_uri });
            }
            else
            {
               strRV = Utility.GetMsg(Utility.msgStatusCodes.SpecifiedObjectDoesNotExist);
            }
            return strRV;
         }
         catch (Exception ex)
         {
            return Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message);
         }
      }
      #endregion

      #region -------------------DeletePath-----------------------
      /// <summary>
      /// Delete a path identified by its URI
      /// </summary>
      /// <param name="path_uri">URI of path to be deleted</param>
      /// <returns>JSON String: OperationCompletedSuccessfully (code=2) on success.</returns>
      /// <remarks>Method requires authentication</remarks>
      [WebMethod(EnableSession = true)]
      [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
      public string DeletePath(string path_uri)
      {
         string strRV = string.Empty;
         if (Context.Session["isAuthenticated"] == null)
         {
            return Utility.GetMsg(Utility.msgStatusCodes.OperationRequiresAuthentication);
         }
         try
         {
            string strQuery = "UPDATE path SET isdeleted=true WHERE uri='" + path_uri + "';";
            int rtnID = Utility.DBExecuteNonQuery(strQuery);
            Utility.DeleteDocumentFromSOLR(new string[] { path_uri });
            return Utility.GetMsg(Utility.msgStatusCodes.OperationCompletedSuccessfully, "Path marked as deleted");
         }
         catch (Exception ex)
         {
            return Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message);
         }
      }
      #endregion

      #region -------------------UpdatePathNode-------------------
      /// <summary>
      /// Update information of a node identified by its URI
      /// </summary>
      /// <param name="node_uri">URI of node to be updated</param>
      /// <param name="fk_path_id">Unique database identifier of path node should be assigned to (Integer, optional)</param>
      /// <param name="fk_rel_uri">URI of object referenced by node. Often an item but can be any object identifiable by a URI (URI, optional)</param>
      /// <param name="dc_title">Title of node (optional)</param>
      /// <param name="dc_description">Description of node (optional)</param>
      /// <param name="type">Type of node (optional, used?)</param>
      /// <param name="node_order">Number indicating the position of the node within a path (Double, optional)</param>
      /// <returns>JSON String: OperationCompletedSuccessfully (code=2) on success</returns>
      /// <remarks>Method requires authentication</remarks>
      [WebMethod(EnableSession = true)]
      [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
      public string UpdatePathNode(string node_uri, string fk_path_id, string fk_rel_uri, string dc_title, string dc_description, string type, string node_order)
      {
         DataTable dtab;
         if (Context.Session["isAuthenticated"] == null)
         {
            return Utility.GetMsg(Utility.msgStatusCodes.OperationRequiresAuthentication);
         }
         StringBuilder sqlBuilder = new StringBuilder();
         try
         {
            sqlBuilder.Append("UPDATE node SET ");
            if (Utility.IsNumeric(fk_path_id))
            {
               sqlBuilder.Append("fk_path_id='" + fk_path_id + "',");
            }
            if (Utility.IsNumeric(fk_rel_uri))
            {
               sqlBuilder.Append("fk_rel_uri='" + fk_rel_uri + "',");
            }
            if (!string.IsNullOrEmpty(dc_title))
            {
               sqlBuilder.Append("dc_title='" + dc_title + "',");
            }
            if (!string.IsNullOrEmpty(dc_description))
            {
               sqlBuilder.Append("dc_description='" + dc_description + "',");
            }
            if (!string.IsNullOrEmpty(type))
            {
               sqlBuilder.Append("type='" + type + "',");
            }
            if (Utility.IsNumeric(node_order))
            {
               sqlBuilder.Append("node_order='" + node_order + "',");
            }
            //Remove any trailing comma
            sqlBuilder.Remove(sqlBuilder.ToString().Length - 1, 1);
            //Finalize SQL statement
            sqlBuilder.Append(" WHERE uri='" + node_uri + "'");
            //Add command and execute
            string strQuery = sqlBuilder.ToString();
            int rtnID = Utility.DBExecuteNonQuery(strQuery);
            //Update SOLR
            //Load modified record and update SOLR
            strQuery = "SELECT * FROM node WHERE uri='" + node_uri + "' AND isdeleted=false";
            dtab = Utility.DBExecuteDataTable(strQuery);

            Utility.PostDataTableToSOLR(dtab, "node");
            return Utility.GetMsg(Utility.msgStatusCodes.OperationCompletedSuccessfully);
         }
         catch (Exception ex)
         {
            return Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message);
         }
      }
      #endregion

      #region -------------------AddNodeFromWorkspaceToPath-------
      /// <summary>
      /// Add a workspace item from the users workspace to a path as a node.
      /// </summary>
      /// <param name="path_uri">URI of path to which node should be added</param>
      /// <param name="workspace_id">Unique database identifier of workspace item</param>
      /// <param name="node_order">Number indicating the position of the node within the path, defaults to the highest number + 1 (Double, optional)</param>
      /// <returns>JSON String: Single node information</returns>
      /// <remarks>Metod requires a user to be authenticated</remarks>
      [WebMethod(EnableSession = true)]
      [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
      public string AddNodeFromWorkspaceToPath(string path_uri, string workspace_id, string node_order)
      {
         string strRV = string.Empty;
         string strQuery;
         DataTable dtab;
         if (Context.Session["isAuthenticated"] == null)
         {
            return Utility.GetMsg(Utility.msgStatusCodes.OperationRequiresAuthentication);
         }
         try
         {
            double mNodeOrder = 0;
            if (Utility.IsNumeric(node_order))
            {
               mNodeOrder = Convert.ToDouble(node_order);
            }
            else
            {
               //Find highest present node sort-order
               strQuery = "SELECT COALESCE(max(n.node_order),0) + 1 AS node_order FROM node n, path p WHERE n.fk_path_id = p.id and p.uri='" + path_uri + "';";
               mNodeOrder = Utility.DBExecuteScalarDouble(strQuery);
            }
            //Insert workspace item into node
            strQuery = "INSERT INTO node (fk_path_id, fk_rel_uri, dc_title, dc_description, type, node_order) SELECT (SELECT id FROM path WHERE uri='" + path_uri + "') as fk_path_id, fk_rel_uri, dc_title, dc_description, type, '" + mNodeOrder + "' as node_order FROM workspace WHERE id = '" + workspace_id + "';SELECT currval('node_id_seq');";
            int new_id = Utility.DBExecuteScalar(strQuery);
            //Get new id
            //Bug if call this function with invalid workspace id
            //Delete workspace item
            strQuery = "DELETE FROM workspace WHERE id = '" + workspace_id + "';";
            Utility.DBExecuteNonQuery(strQuery);
            //Get the new node
            strQuery = "SELECT n.*, p.paths_status FROM node n, path p WHERE n.id = '" + new_id + "' AND n.fk_path_id = p.id AND p.isdeleted=false;";
            dtab = Utility.DBExecuteDataTable(strQuery);
            //Insert into SOLR if path is published
            if (dtab.Rows.Count > 0)
            {
               if (Convert.ToString(dtab.Rows[0]["paths_status"]).ToLower() == "published")
               {
                  Utility.PostDataTableToSOLR(dtab, "node");
               }
            }
            //My blank ratings
            Dictionary<string, DataTable> mBlankRatings = new Dictionary<string, DataTable>();
            mBlankRatings.Add("rating", new DataTable());
            strRV = Utility.DataTableToDictionary(dtab, true);
            return strRV;
         }
         catch (Exception ex)
         {
            return Utility.GetMsg(Utility.msgStatusCodes.OperationFailed, ex.Message);
         }
      }
      #endregion

      #region -------------------UpdatePath-----------------------
      /// <summary>
      /// Updates path data for a path identified by its URI
      /// </summary>
      /// <param name="path_uri">URI of path to be modified</param>
      /// <param name="dc_title">Modified title of path (string, optional)</param>
      /// <param name="dc_subject">Modified subject of path (optional) separater multiple entries by a semicolon ";"</param>
      /// <param name="dc_description">Modified description of path (optional)</param>
      /// <param name="dc_rights">Modified rights statement of path (optional)</param>
      /// <param name="access">Modified access information for path (optional)</param>
      /// <param name="lom_audience">Modified audience for path (optional)</param>
      /// <param name="lom_length">Modified length/duration of path (optional)</param>
      /// <returns>OperationCompletedSuccessfully (code=2) on success</returns>
      /// <remarks></remarks>
      [WebMethod(EnableSession = true)]
      [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
      public string UpdatePath(string path_uri, string dc_title, string dc_subject, string dc_description, string dc_rights, string access, string lom_audience, string lom_length, string paths_status)
      {
         string strQuery;
         DataTable dtab;
         if (Context.Session["isAuthenticated"] == null)
         {
            return Utility.GetMsg(Utility.msgStatusCodes.OperationRequiresAuthentication);
         }
         StringBuilder sqlBuilder = new StringBuilder();
         try
         {
            sqlBuilder.Append("UPDATE path SET ");
            if (!string.IsNullOrEmpty(dc_title))
            {
               sqlBuilder.Append("dc_title='" + dc_title + "',");
            }
            if (!string.IsNullOrEmpty(dc_subject))
            {
               sqlBuilder.Append("dc_subject='" + dc_subject + "',");
            }
            if (!string.IsNullOrEmpty(dc_description))
            {
               sqlBuilder.Append("dc_description='" + dc_description + "',");
            }
            if (!string.IsNullOrEmpty(dc_rights))
            {
               sqlBuilder.Append("dc_rights='" + dc_rights + "',");
            }
            if (!string.IsNullOrEmpty(access))
            {
               sqlBuilder.Append("access='" + access + "',");
            }
            if (!string.IsNullOrEmpty(lom_audience))
            {
               sqlBuilder.Append("lom_audience='" + lom_audience + "',");
            }
            if (!string.IsNullOrEmpty(lom_length))
            {
               sqlBuilder.Append("lom_length='" + lom_length + "',");
            }
            if (!string.IsNullOrEmpty(paths_status))
            {
               sqlBuilder.Append("paths_status='" + paths_status + "',");
            }
            sqlBuilder.Append("fk_usr_id='" + Convert.ToString(Context.Session["usr_id"]) + "'");

            sqlBuilder.Append(" WHERE uri='" + path_uri + "';");

            strQuery = sqlBuilder.ToString();
            Utility.DBExecuteNonQuery(strQuery);

            //Load modified record and update SOLR
            strQuery = "SELECT p.*,to_char(p.tstamp,'yyyy-mm-ddTmm:hh:ss') as dc_date, u.uri as dc_creator FROM path p, usr u WHERE u.id = p.fk_usr_id AND p.uri = '" + path_uri + "' AND p.isdeleted=false;";
            dtab = Utility.DBExecuteDataTable(strQuery);

            //Check if any path was returned
            if (dtab.Rows.Count > 0)
            {
               //Load related nodes 
               DataTable mNodeTab;
               strQuery = "SELECT * FROM node WHERE fk_path_id = '" + Convert.ToString(dtab.Rows[0]["id"]) + "' AND isdeleted=false";
               mNodeTab = Utility.DBExecuteDataTable(strQuery);

               //If status = published then post to SOLR
               if (Convert.ToString(dtab.Rows[0]["paths_status"]).ToLower() == "published")
               {
                  Utility.PostDataTableToSOLR(dtab, "path");
                  Utility.PostDataTableToSOLR(mNodeTab, "node");
                  //Otherwise, delete uri from SOLR
               }
               else
               {
                  List<string> mUris = new List<string>();
                  mUris.Add(Convert.ToString(dtab.Rows[0]["uri"]));
                  foreach (DataRow mNodeRow in mNodeTab.Rows)
                  {
                     mUris.Add(Convert.ToString(mNodeRow["uri"]));
                  }
                  Utility.DeleteDocumentFromSOLR(mUris.ToArray());
               }
               mNodeTab = null;
            }
            return Utility.GetMsg(Utility.msgStatusCodes.OperationCompletedSuccessfully);
         }
         catch (Exception ex)
         {
            return Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message);
         }
      }
      #endregion

      #region -------------------CreatePath-----------------------
      /// <summary>
      /// Create a new path
      /// </summary>
      /// <param name="dc_title">Title of path</param>
      /// <param name="dc_subject">Subject of path, separate multiple values by a semicolon ";"</param>
      /// <param name="dc_description">Description of path</param>
      /// <param name="dc_rights">Rights statement for path</param>
      /// <param name="access">Access information for path</param>
      /// <param name="lom_audience">Audience for path</param>
      /// <param name="lom_length">Length/duration of path</param>
      /// <returns>Path data object for created path</returns>
      /// <remarks>Methods requires a user to be authenticated</remarks>
      [WebMethod(EnableSession = true)]
      [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
      public string CreatePath(string dc_title, string dc_subject, string dc_description, string dc_rights, string access, string lom_audience, string lom_length)
      {
         string strRV = string.Empty;
         if (Context.Session["isAuthenticated"] == null)
         {
            return Utility.GetMsg(Utility.msgStatusCodes.OperationRequiresAuthentication);
         }
         try
         {
            string strQuery = "INSERT INTO path (dc_title, dc_subject, dc_description, dc_rights, access, lom_audience, lom_length, fk_usr_id) VALUES ('" + dc_title + "','" + dc_subject + "','" + dc_description + "','" + dc_rights + "','" + access + "','" + lom_audience + "','" + lom_length + "','" + Convert.ToString(Context.Session["usr_id"]) + "'); SELECT currval('path_id_seq');";
            int mNewPathId = Utility.DBExecuteScalar(strQuery);

            string strQuery1 = "SELECT p.*,to_char(p.tstamp,'yyyy-mm-ddTmm:hh:ss') as dc_date, u.uri as dc_creator FROM path p, usr u WHERE u.id = p.fk_usr_id AND p.id = '" + mNewPathId + "' AND p.isdeleted=false;";
            DataTable dtab = Utility.DBExecuteDataTable(strQuery1);

            strRV = Utility.DataTableToDictionary(dtab);
            //PostDataTableToSOLR(db.dtab, "path") 'Don't post on create path.

            return strRV;
         }
         catch (Exception ex)
         {
            return Utility.GetMsg(Utility.msgStatusCodes.OperationFailed, ex.Message);
         }
      }
      #endregion

      #region -------------------GetPath--------------------------
      /// <summary>
      /// Get a single path identified by its URI
      /// </summary>
      /// <param name="path_uri">URI of path to be retrieved</param>
      /// <returns>Path data object</returns>
      /// <remarks></remarks>
      [WebMethod(EnableSession = true)]
      [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
      public string GetPath(string path_uri)
      {
         string strRV = string.Empty;
         try
         {
            string strQuery = "SELECT p.*,to_char(p.tstamp,'yyyy-mm-ddTmm:hh:ss') as dc_date, u.uri as dc_creator_uri, u.foaf_nick as dc_creator_name FROM path p, usr u WHERE u.id = p.fk_usr_id AND p.uri = '" + path_uri + "' AND p.isdeleted=false;";
            DataTable dtab = Utility.DBExecuteDataTable(strQuery);
            if (dtab.Rows.Count > 0)
            {
               //Get id of path
               int mPathId = Convert.ToInt32(dtab.Rows[0]["id"]);

               //Get URI of path
               string mPathUri = Convert.ToString(dtab.Rows[0]["uri"]);

               //Get nodes
               Dictionary<string, DataTable> mNodeDictionary = new Dictionary<string, DataTable>();

               string strQuery1 = "SELECT uri, fk_rel_uri, dc_title, dc_description, type, to_char(tstamp,'yyyy-mm-ddTmm:hh:ss') as dc_date, node_order, (SELECT COUNT(id) FROM rating WHERE fk_rating_scale_id=1 AND fk_rel_uri=n.uri) as _dislikes, (SELECT COUNT(id) FROM rating WHERE fk_rating_scale_id=2 AND fk_rel_uri=n.uri) as _likes FROM node n WHERE fk_path_id='" + mPathId + "' AND isdeleted=false ORDER BY node_order ASC";
               DataTable ntab = Utility.DBExecuteDataTable(strQuery1);
               mNodeDictionary.Add("nodes", ntab);

               //Get ratings
               string strQuery2 = "SELECT (SELECT COUNT(id) FROM rating WHERE fk_rating_scale_id = 2 AND fk_rel_uri = '" + mPathUri + "') AS likes, (SELECT COUNT(id) FROM rating WHERE fk_rating_scale_id = 1 AND fk_rel_uri = '" + mPathUri + "') AS dislikes";
               DataTable myRatings = Utility.DBExecuteDataTable(strQuery2);
               mNodeDictionary.Add("paths_rating", myRatings);

               //Return JSON
               strRV = Utility.DataTableToDictionary(dtab, true, mNodeDictionary);
            }
            else
            {
               strRV = Utility.GetMsg(Utility.msgStatusCodes.SpecifiedObjectDoesNotExist, "No path with uri: '" + path_uri + "'.");
            }
            return strRV;
         }
         catch (Exception ex)
         {
            return Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message);
         }
      }
      #endregion

      #region -------------------GetCurrentUserPaths--------------
      /// <summary>
      /// Get list of paths created by current authenticated user
      /// </summary>
      /// <returns>OperationCompletedSuccessfully (code=2) + list of path data objects on success; or QueryDidNotReturnRecords (code=8) if current user has no paths</returns>
      /// <remarks>Method requires a user to be authenticated</remarks>
      [WebMethod(EnableSession = true)]
      [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
      public string GetCurrentUserPaths()
      {
         string strRV = null;
         DataTable dtab;
         if (Context.Session["isAuthenticated"] == null)
         {
            return Utility.GetMsg(Utility.msgStatusCodes.OperationRequiresAuthentication);
         }
         try
         {
            string strQuery = "SELECT * FROM path WHERE fk_usr_id = '" + Convert.ToString(Context.Session["usr_id"]) + "' and isdeleted=false;";
            dtab = Utility.DBExecuteDataTable(strQuery);
            strRV = Utility.DataTableToDictionary(dtab);
            if (dtab.Rows.Count == 0)
            {
               strRV = Utility.GetMsg(Utility.msgStatusCodes.QueryDidNotReturnRecords, "No paths for specified user.");
            }
            return strRV;
         }
         catch (Exception ex)
         {
            return Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message);
         }
      }
      #endregion

      #region -------------------GetPathsForItem------------------
      /// <summary>
      /// Get paths associated with a specific item
      /// </summary>
      /// <param name="item_uri">URI of item for which associated paths should be returned</param>
      /// <returns>OperationCompletedSuccessfully (code=2) + list of path data objects on success.</returns>
      /// <remarks></remarks>
      [WebMethod(EnableSession = true)]
      [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
      public string GetPathsForItem(string item_uri)
      {
         string strRV = string.Empty;
         try
         {
            string strQuery = "SELECT DISTINCT p.id, p.uri, p.dc_title, p.dc_subject, p.dc_description, p.access, p.lom_audience, p.lom_length,to_char(p.tstamp,'yyyy-mm-ddTmm:hh:ss') as dc_date, u.uri as dc_creator_uri, u.foaf_nick as dc_creator_name, (SELECT COUNT(id) FROM rating WHERE fk_rel_uri=p.uri AND fk_rating_scale_id=1 and isdeleted=false) AS _dislikes, (SELECT COUNT(id) FROM rating WHERE fk_rel_uri=p.uri AND fk_rating_scale_id=2 AND isdeleted=false) AS _likes FROM (path p LEFT JOIN usr u ON u.id = p.fk_usr_id) LEFT JOIN node n ON n.fk_path_id = p.id WHERE n.isdeleted=false AND p.isdeleted=false AND p.paths_status = 'published' AND n.fk_rel_uri = '" + item_uri + "';";
            DataTable dtab = Utility.DBExecuteDataTable(strQuery);
            //Get corresponding nodes
            string strQuery1 = "SELECT fk_path_id, fk_rel_uri, uri, dc_title, dc_description, type, node_order, to_char(tstamp,'yyyy-mm-ddTmm:hh:ss') as dc_date, (SELECT COUNT(id) FROM rating WHERE fk_rel_uri=n.uri AND fk_rating_scale_id=1 AND isdeleted=false) AS _dislikes, (SELECT COUNT(id) FROM rating WHERE fk_rel_uri=n.uri AND fk_rating_scale_id=1 AND isdeleted=false) AS _likes FROM node n WHERE n.fk_rel_uri='" + item_uri + "'";
            DataTable mNodeTab = Utility.DBExecuteDataTable(strQuery1);
            if (dtab.Rows.Count == 0)
            {
               strRV = Utility.GetMsg(Utility.msgStatusCodes.QueryDidNotReturnRecords);
            }
            else
            {
               strRV = Utility.DataTablesToDictionaryMasterDetail(dtab, mNodeTab);
            }
            return strRV;
         }
         catch (Exception ex)
         {
            return Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message);
         }
      }
      #endregion

      */
        #endregion

        #region -------------------Get Path #232-------------------------
        /// <summary>
        /// Get a single path identified by its URI
        /// </summary>
        /// <param name="paths_identifier">URI of path to be retrieved</param>
        /// <returns>Path data object</returns>
        /// <remarks></remarks>
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Get(string paths_identifier)
        {
            string strRV = string.Empty;
            strRV = GetPathDetails(paths_identifier);
            return strRV;
        }
        #endregion  

        #region -------------------Get Path 2.0 -------------------------
        /// <summary>
        /// Get a single path identified by its URI (with node thumb URIs)
        /// </summary>
        /// <param name="paths_identifier">URI of path to be retrieved</param>
        /// <returns>Path data object</returns>
        /// <remarks></remarks>
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Get2(string paths_identifier)
        {
            string strRV = string.Empty;
            strRV = GetPathDetails2(paths_identifier);
            return strRV;
        }
        #endregion

        #region--------------------GetResult from Database-------------
        public string GetPathDetails(string paths_identifier, bool isDelete = false)
        {
            var mTimer = new Stopwatch();
            mTimer.Start();

            string strRV = string.Empty;
            string strRVN = string.Empty;
            string strRVC = string.Empty;
            string strQuery = string.Empty;
            int pathID = 0;
            try
            {
                if (isDelete)
                {
                    strQuery = "select uri paths_identifier,id,fk_usr_id,dc_title,dc_description,dc_subject,lom_length paths_duration,access paths_access,(CASE paths_iscloneable when true THEN 'true' ELSE 'false' END)paths_clone,paths_thumbnail,'#pt#' paths_topics,'#ps#' paths_start,'#pn#' paths_nodes,'#pc#' dc_creator,dc_language from path where uri = '" + paths_identifier + "';";
                }
                else
                {
                    strQuery = "select uri paths_identifier,id,fk_usr_id,dc_title,dc_description,dc_subject,lom_length paths_duration,access paths_access,(CASE paths_iscloneable when true THEN 'true' ELSE 'false' END)paths_clone,paths_thumbnail,'#pt#' paths_topics,'#ps#' paths_start,'#pn#' paths_nodes,'#pc#' dc_creator,dc_language from path where uri = '" + paths_identifier + "' and isdeleted = false;";
                }
                DataTable dtab = Utility.DBExecuteDataTable(strQuery);
                if (dtab.Rows.Count > 0)
                {
                    //Get user id
                    int usrID = Convert.ToInt32(dtab.Rows[0]["fk_usr_id"]);
                    pathID = Convert.ToInt32(dtab.Rows[0]["id"]);
                    string strAccess = Convert.ToString(dtab.Rows[0]["paths_access"]);
                    if (strAccess == "private")
                    {
                        int usessionId = 0;
                        bool bUid = Utility.IsNumeric(Convert.ToString(Context.Session["usr_id"]));
                        if (bUid)
                        {
                            usessionId = Convert.ToInt32(Context.Session["usr_id"]);
                        }
                        if (usrID != usessionId)
                        {
                            Context.Response.StatusCode = 404;
                            Context.Response.StatusDescription = "Not Found";
                            return Utility.GetMsg(Utility.msgStatusCodes.NotFound);
                        }
                    }
                    dtab.Rows[0]["dc_subject"] = "#s#" + dtab.Rows[0]["dc_subject"] + "#l#";
                    strRV = DTtoJSON(dtab);
                    strRV = strRV.Replace("\"#s#", "[").Replace("#l#\"", "]");
                    //Get Path Topic
                    strQuery = "select id from topic where id in (select fk_topic_id from item_topic where fk_item_uri in (select dc_source from node inner join path on path.id= node.fk_path_id where node.isdeleted=false and path.uri = '" + paths_identifier + "'));";
                    string strTopic = GetJsonList("id", strQuery);
                    strRV = strRV.Replace("\"#pt#\"", strTopic);

                    //Get Path Start
                    strQuery = "select uri from node where node.isdeleted=false and (paths_prev is null or paths_prev = '') AND paths_start = true and fk_path_id = " + pathID + ";";
                    string strPStart = GetJsonList("uri", strQuery);
                    strRV = strRV.Replace("\"#ps#\"", strPStart);

                    //Get Path nodes
                    strQuery = "select uri paths_identifier,node_order,type paths_type,dc_title,dc_description,dc_source, paths_thumbnail paths_thumbnails,paths_topics, paths_next, paths_prev from node where isdeleted=false and fk_path_id =" + pathID + ";";
                    DataTable dtabNode = Utility.DBExecuteDataTable(strQuery);
                    foreach (DataRow drow in dtabNode.Rows)
                    {
                        drow["paths_next"] = "#[" + drow["paths_next"] + "]#";
                        drow["paths_prev"] = "#[" + drow["paths_prev"] + "]#";
                        drow["paths_thumbnails"] = "#[" + drow["paths_thumbnails"] + "]#";
                        drow["paths_topics"] = "#[" + drow["paths_topics"] + "]#";
                    }
                    strRVN = DTtoJSON(dtabNode);
                    strRVN = strRVN.Replace("\"#", "");
                    strRVN = strRVN.Replace("#\"", "");
                    strRV = strRV.Replace("\"#pn#\"", strRVN);

                    //Get User Data
                    strQuery = "SELECT uri paths_identifier,foaf_nick, email foaf_mbox ,email_visibility foaf_mbox_visibility,(CASE istemporary when true THEN 'new' ELSE 'registered' END)dc_type,to_char(tstamp,'yyyy-mm-ddTmm:hh:ss') paths_registered FROM usr WHERE id=" + usrID + ";";
                    DataTable uTable = Utility.DBExecuteDataTable(strQuery);
                    strRVC = DTtoJSON(uTable);
                    strRVC = strRVC.TrimEnd().Substring(1, strRVC.LastIndexOf("]") - 1);

                    strRV = strRV.Replace("\"#pc#\"", strRVC);
                    strRV = strRV.TrimEnd().Substring(1, strRV.LastIndexOf("]") - 1);
                    strRV = "{\"code\":2,\"data\":" + strRV + "}";
                    
                }
                else
                {
                    strRV = Utility.GetMsg(Utility.msgStatusCodes.SpecifiedObjectDoesNotExist);
                }
                Utility.LogRequest(strRV, false, mTimer);
                return strRV;
            }
            catch (Exception ex)
            {
                var mMsg = Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message);
                Utility.LogRequest(mMsg, true, mTimer);
                return mMsg;
            }
        }
        #endregion

        #region--------------------GetResult from Database 2.0 -------------
        public string GetPathDetails2(string paths_identifier, bool isDelete = false)
        {
            var mTimer = new Stopwatch();
            mTimer.Start();

            string strRV = string.Empty;
            string strRVN = string.Empty;
            string strRVC = string.Empty;
            string strQuery = string.Empty;
            int pathID = 0;
            try
            {
                if (isDelete)
                {
                    strQuery = "select uri paths_identifier,id,fk_usr_id,dc_title,dc_description,dc_subject,lom_length paths_duration,access paths_access,(CASE paths_iscloneable when true THEN 'true' ELSE 'false' END)paths_clone,paths_thumbnail,'#pt#' paths_topics,'#ps#' paths_start,'#pn#' paths_nodes,'#pc#' dc_creator,dc_language from path where uri = '" + paths_identifier + "';";
                }
                else
                {
                    strQuery = "select uri paths_identifier,id,fk_usr_id,dc_title,dc_description,dc_subject,lom_length paths_duration,access paths_access,(CASE paths_iscloneable when true THEN 'true' ELSE 'false' END)paths_clone,paths_thumbnail,'#pt#' paths_topics,'#ps#' paths_start,'#pn#' paths_nodes,'#pc#' dc_creator,dc_language from path where uri = '" + paths_identifier + "' and isdeleted = false;";
                }
                DataTable dtab = Utility.DBExecuteDataTable(strQuery);
                if (dtab.Rows.Count > 0)
                {
                    //Get user id
                    int usrID = Convert.ToInt32(dtab.Rows[0]["fk_usr_id"]);
                    pathID = Convert.ToInt32(dtab.Rows[0]["id"]);
                    string strAccess = Convert.ToString(dtab.Rows[0]["paths_access"]);
                    if (strAccess == "private")
                    {
                        int usessionId = 0;
                        bool bUid = Utility.IsNumeric(Convert.ToString(Context.Session["usr_id"]));
                        if (bUid)
                        {
                            usessionId = Convert.ToInt32(Context.Session["usr_id"]);
                        }
                        if (usrID != usessionId)
                        {
                            Context.Response.StatusCode = 404;
                            Context.Response.StatusDescription = "Not Found";
                            return Utility.GetMsg(Utility.msgStatusCodes.NotFound);
                        }
                    }
                    dtab.Rows[0]["dc_subject"] = "#s#" + dtab.Rows[0]["dc_subject"] + "#l#";
                    strRV = DTtoJSON(dtab);
                    strRV = strRV.Replace("\"#s#", "[").Replace("#l#\"", "]");
                    //Get Path Topic
                    strQuery = "select id from topic where id in (select fk_topic_id from item_topic where fk_item_uri in (select dc_source from node inner join path on path.id= node.fk_path_id where node.isdeleted=false and path.uri = '" + paths_identifier + "'));";
                    string strTopic = GetJsonList("id", strQuery);
                    strRV = strRV.Replace("\"#pt#\"", strTopic);

                    //Get Path Start
                    strQuery = "select uri from node where node.isdeleted=false and (paths_prev is null or paths_prev = '') AND paths_start = true and fk_path_id = " + pathID + ";";
                    string strPStart = GetJsonList("uri", strQuery);
                    strRV = strRV.Replace("\"#ps#\"", strPStart);

                    //Get Path nodes
                    strQuery = "select n.uri paths_identifier,node_order,type paths_type,n.dc_title,n.dc_description,n.dc_source, '\"'||i.europeana_object||'\"' as paths_thumbnails, paths_topics, paths_next, paths_prev from node n left join item i on (n.dc_source = i.uri) where isdeleted=false and fk_path_id =" + pathID + ";";
                    DataTable dtabNode = Utility.DBExecuteDataTable(strQuery);
                    foreach (DataRow drow in dtabNode.Rows)
                    {
                        drow["paths_next"] = "#[" + drow["paths_next"] + "]#";
                        drow["paths_prev"] = "#[" + drow["paths_prev"] + "]#";
                        drow["paths_thumbnails"] = "#[" + drow["paths_thumbnails"] + "]#";
                        drow["paths_topics"] = "#[" + drow["paths_topics"] + "]#";
                        drow["dc_description"] = Utility.escapeNewLine(drow["dc_description"].ToString());
                    }
                    strRVN = DTtoJSON(dtabNode);
                    strRVN = strRVN.Replace("\"#", "");
                    strRVN = strRVN.Replace("#\"", "");
                    strRV = strRV.Replace("\"#pn#\"", strRVN);

                    //Get User Data
                    strQuery = "SELECT uri paths_identifier,foaf_nick, email foaf_mbox ,email_visibility foaf_mbox_visibility,(CASE istemporary when true THEN 'new' ELSE 'registered' END)dc_type,to_char(tstamp,'yyyy-mm-ddTmm:hh:ss') paths_registered FROM usr WHERE id=" + usrID + ";";
                    DataTable uTable = Utility.DBExecuteDataTable(strQuery);
                    strRVC = DTtoJSON(uTable);
                    strRVC = strRVC.TrimEnd().Substring(1, strRVC.LastIndexOf("]") - 1);

                    strRV = strRV.Replace("\"#pc#\"", strRVC);
                    strRV = strRV.TrimEnd().Substring(1, strRV.LastIndexOf("]") - 1);
                    strRV = "{\"code\":2,\"data\":" + strRV + "}";

                }
                else
                {
                    strRV = Utility.GetMsg(Utility.msgStatusCodes.SpecifiedObjectDoesNotExist);
                }
                Utility.LogRequest(strRV, false, mTimer);
                return strRV;
            }
            catch (Exception ex)
            {
                var mMsg = Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message);
                Utility.LogRequest(mMsg, true, mTimer);
                return mMsg;
            }
        }
        #endregion

        #region --------------------Get JSON List data------------------
        public string GetJsonList(string strCname, string strQuery)
        {
            string strRV = "";
            StringBuilder sbTopic = new StringBuilder();
            DataTable tTable = Utility.DBExecuteDataTable(strQuery);
            if (tTable.Rows.Count > 0)
            {
                foreach (DataRow drow in tTable.Rows)
                {
                    sbTopic.Append("\"" + drow[strCname] + "\",");
                }
                strRV = sbTopic.ToString().Substring(0, sbTopic.ToString().Length - 1);
            }
            strRV = "[" + strRV + "]";
            return strRV;
        }

        #endregion

        #region --------------------Datatable to Json-------------------
        public string DTtoJSON(DataTable dt)
        {
            string Json = string.Empty;
            string JSONObjectFormat = string.Empty;
            int i = 0;
            StringBuilder builder = new StringBuilder();
            string ColumnName = string.Empty;
            string MapValue = string.Empty;

            string JsonValue = string.Empty;
            try
            {
                string TableName = dt.TableName.ToString();
                foreach (DataColumn col in dt.Columns)
                {
                    if (string.IsNullOrEmpty(JSONObjectFormat))
                        JSONObjectFormat = "\"" + col.ColumnName.ToString() + "\" : \"{" + i + "}\"";
                    else
                        JSONObjectFormat = JSONObjectFormat + ",\"" + col.ColumnName.ToString() + "\" : \"{" + i + "}\"";
                    i++;
                }
                for (int RowNumber = 0; RowNumber < dt.Rows.Count; RowNumber++)
                {
                    object[] par = new object[dt.Columns.Count];
                    for (int ColNumber = 0; ColNumber < dt.Columns.Count; ColNumber++)
                    {
                        ColumnName = ColumnName == string.Empty ? dt.Rows[RowNumber][ColNumber].ToString() : ColumnName + "," + dt.Rows[RowNumber][ColNumber].ToString();
                        par[ColNumber] = dt.Rows[RowNumber][ColNumber].ToString();

                    }
                    MapValue = string.Format(JSONObjectFormat, par);
                    if (string.IsNullOrEmpty(builder.ToString()))
                        builder.AppendLine("{" + MapValue + "}");
                    else
                        builder.AppendLine(",{" + MapValue + "}");
                    ColumnName = string.Empty;
                    MapValue = string.Empty;
                }
                Json = string.Concat("[", builder.ToString(), "]");
            }
            catch (Exception)
            {

            }
            return Json;
        }
        #endregion

        #region -------------------Create #233---------------------------
        /// <summary>
        /// Create a new path
        /// </summary>    
        /// <returns>Path data object for created path</returns>
        /// <remarks>Methods requires a user to be authenticated</remarks>
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Create()
        {
            string strRV = string.Empty;
            string strQuery = string.Empty;
            if (Context.Session["isAuthenticated"] == null)
            {
                Context.Response.StatusCode = 401;
                Context.Response.StatusDescription = "Access Denied";
                return Utility.GetMsg(Utility.msgStatusCodes.Unauthorized);
            }
            try
            {
                strQuery = "INSERT INTO path (dc_title,fk_usr_id) VALUES ('New Path','" + Convert.ToString(Context.Session["usr_id"]) + "'); SELECT currval('path_id_seq');";
                int mNewPathId = Utility.DBExecuteScalar(strQuery);

                strQuery = "select URI as paths_identifier, * FROM path WHERE id = '" + mNewPathId + "' AND isdeleted=false;";
                DataTable dtab = Utility.DBExecuteDataTable(strQuery);

                strRV = GetPathDetails(Convert.ToString(dtab.Rows[0]["uri"]));

                Utility.PostDataTableToSOLR(dtab, "path");
                return strRV;
            }
            catch (Exception ex)
            {
                return Utility.GetMsg(Utility.msgStatusCodes.OperationFailed, ex.Message);
            }
        }
        #endregion

        #region -------------------DeletePath #234-----------------------
        /// <summary>
        /// Delete a path identified by its URI
        /// </summary>
        /// <param name="paths_identifier">URI of path to be deleted</param>
        /// <returns>JSON:Path information for the deleted path</returns>
        /// <remarks>Method requires authentication</remarks>
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Delete(string paths_identifier)
        {
            var mTimer = new Stopwatch();
            mTimer.Start();

            string strRV = string.Empty;
            string strQuery = string.Empty;
            if (Context.Session["isAuthenticated"] == null)
            {
                var mMsg = Utility.GetMsg(Utility.msgStatusCodes.OperationRequiresAuthentication);
                Utility.LogRequest(mMsg, false, mTimer);
                return mMsg;
            }
            try
            {
                strQuery = "select fk_usr_id from path WHERE uri='" + paths_identifier + "';";
                int rtnUserID = Utility.DBExecuteScalar(strQuery);
                if (rtnUserID == Convert.ToInt32(Context.Session["usr_id"]))
                {
                    strQuery = "UPDATE path SET isdeleted=true WHERE uri='" + paths_identifier + "';";
                    int rtnID = Utility.DBExecuteNonQuery(strQuery);
                    Utility.DeleteDocumentFromSOLR(new string[] { paths_identifier });
                    //Updated on 27 Sept 2013
                    strQuery = "select node.id, node.uri  from node inner join path on path.id = node.fk_path_id where path.uri = '" + paths_identifier + "';";
                    DataTable dtabNode = Utility.DBExecuteDataTable(strQuery);
                    foreach (DataRow drow in dtabNode.Rows)
                    {
                        Delete_Node(paths_identifier, Convert.ToString(drow["uri"]));
                    }
                    //END
                    strRV = GetPathDetails(paths_identifier, true);
                    Utility.LogRequest(strRV, false, mTimer);
                    return strRV;
                }
                else
                {
                    Context.Response.StatusCode = 401;
                    Context.Response.StatusDescription = "Access Denied";
                    var mMsg = Utility.GetMsg(Utility.msgStatusCodes.Unauthorized);
                    Utility.LogRequest(mMsg, true, mTimer);
                    return mMsg;
                }
            }
            catch (Exception ex)
            {
                var mMsg = Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message);
                Utility.LogRequest(mMsg, false, mTimer);
                return mMsg;
            }
        }
        #endregion

        #region -------------------UpdatePath #236-----------------------
        /// <summary>
        /// Updates path data for a path identified by its URI
        /// </summary>
        /// <param name="paths_identifier">URI of path to be modified</param>
        /// <param name="dc_title">Modified title of path (string, optional)</param>
        /// <param name="dc_description">Modified description of path (optional)</param>
        /// <param name="paths_duration">Modified length/duration of path (optional)</param>
        /// <param name="paths_access">Modified access information for path (optional)</param>
        /// <param name="paths_clone">True false (optional)</param>  
        /// <param name="paths_thumbnail">Thumbnail URL (optional)</param>  
        /// <param name="dc_subject">Modified subject of path (optional) separater multiple entries by a semicolon ";"</param>
        /// <param name="dc_language">The language of the path</param>
        /// <returns>JSON:Path with the updated path metadata</returns>
        /// <remarks></remarks>
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string UpdateMeta(string paths_identifier, string dc_title, string dc_description, string paths_duration, string paths_access, string paths_clone, string paths_thumbnail, string dc_subject, string dc_language)
        {
            var mTimer = new Stopwatch();
            mTimer.Start();

            DataTable dtab;
            string strQuery;
            if (Context.Session["isAuthenticated"] == null)
            {
                var mMsg = Utility.GetMsg(Utility.msgStatusCodes.OperationRequiresAuthentication);
                Utility.LogRequest(mMsg);
                return mMsg;
            }
            strQuery = "select fk_usr_id from path where uri = '" + paths_identifier + "';";
            int iUid = Utility.DBExecuteScalar(strQuery);
            int uSessionId = 0;
            bool bUid = Utility.IsNumeric(Convert.ToString(Context.Session["usr_id"]));
            if (bUid)
            {
                uSessionId = Convert.ToInt32(Context.Session["usr_id"]);
            }
            if (uSessionId != iUid)
            {
                Context.Response.StatusCode = 401;
                Context.Response.StatusDescription = "Access Denied";
                var mMsg = Utility.GetMsg(Utility.msgStatusCodes.Unauthorized);
                Utility.LogRequest(mMsg, true, mTimer);
                return mMsg;
            }
            StringBuilder sqlBuilder = new StringBuilder();
            try
            {
                sqlBuilder.Append("UPDATE path SET ");

                if (!string.IsNullOrEmpty(dc_title))
                {
                    sqlBuilder.Append("dc_title='" + dc_title + "',");
                }
                if (!string.IsNullOrEmpty(dc_description))
                {
                    sqlBuilder.Append("dc_description='" + dc_description + "',");
                }
                if (!string.IsNullOrEmpty(paths_duration))
                {
                    sqlBuilder.Append("lom_length='" + paths_duration + "',");
                }
                if (!string.IsNullOrEmpty(paths_access))
                {
                    sqlBuilder.Append("access='" + paths_access + "',");
                }
                if (!string.IsNullOrEmpty(paths_clone))
                {
                    sqlBuilder.Append("paths_iscloneable='" + paths_clone + "',");
                }
                if (!string.IsNullOrEmpty(dc_language))
                {
                    sqlBuilder.Append("dc_language='" + dc_language + "',");
                }
                if (!string.IsNullOrEmpty(paths_thumbnail))
                {
                    if (paths_thumbnail == "")
                    {
                        sqlBuilder.Append("paths_thumbnail=NULL,");
                    }
                    else
                    {
                        sqlBuilder.Append("paths_thumbnail='" + paths_thumbnail + "',");
                    }
                }
                if (!string.IsNullOrEmpty(dc_subject))
                {
                    string[] strDcSub;
                    strDcSub = dc_subject.Split(',');
                    StringBuilder sbSub = new StringBuilder();
                    foreach (string strSub in strDcSub)
                    {
                        sbSub.Append("\"" + strSub + "\",");
                    }
                    sqlBuilder.Append("dc_subject='" + sbSub.ToString().Substring(0, sbSub.ToString().LastIndexOf(",")) + "',");
                }
                sqlBuilder.Append("fk_usr_id='" + Convert.ToString(Context.Session["usr_id"]) + "'");
                sqlBuilder.Append(" WHERE uri='" + paths_identifier + "';");
                strQuery = sqlBuilder.ToString();
                Utility.DBExecuteNonQuery(strQuery);
                //Load modified record and update SOLR

                //strQuery = "SELECT id, fk_usr_id as dc_creator,uri paths_identifier,access,'' paths_topics,paths_thumbnail, dc_title, dc_description, dc_subject, lom_length as paths_duration,'path' AS dc_type,to_char(tstamp,'yyyy-mm-ddTmm:hh:ss') as dc_date, paths_status FROM path WHERE  uri = '" + paths_identifier + "' AND  isdeleted=false;";
                strQuery = "SELECT p.id, u.uri as dc_creator,p.uri paths_identifier,access,'' paths_topics,paths_thumbnail, dc_title, dc_description, dc_subject, lom_length as paths_duration,'path' AS dc_type,to_char(p.tstamp,'yyyy-mm-ddTmm:hh:ss') as dc_date, paths_status FROM path p  inner join usr u on p.fk_usr_id = u.id WHERE  p.uri = '" + paths_identifier + "' AND  p.isdeleted=false;";
                //strQuery = "SELECT p.*,p.uri paths_identifier,to_char(p.tstamp,'yyyy-mm-ddTmm:hh:ss') as dc_date, u.uri as dc_creator FROM path p, usr u WHERE u.id = p.fk_usr_id AND p.uri = '" + paths_identifier + "' AND p.isdeleted=false;";
                dtab = Utility.DBExecuteDataTable(strQuery);
                //Check if any path was returned
                if (dtab.Rows.Count > 0)
                {
                    //Load related nodes 
                    DataTable mNodeTab;
                    strQuery = "SELECT uri paths_identifier, dc_title,dc_description, 'node' as dc_type FROM node  WHERE fk_path_id = '" + Convert.ToString(dtab.Rows[0]["id"]) + "' AND isdeleted=false";
                    mNodeTab = Utility.DBExecuteDataTable(strQuery);

                    //If status = published then post to SOLR
                    if (Convert.ToString(dtab.Rows[0]["access"]).ToLower() == "public")
                    {
                        ///*****Updated on 25 Sept 2013 for Ticket no 282*****///
                        strQuery = "select distinct(paths_topics) from node inner join path on path.id = node.fk_path_id where node.isdeleted = false and paths_topics <> '' and path.uri = '" + paths_identifier + "' ;";
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
                        }
                        //*****END UPDATES*****//
                        Utility.PostDataTableToSOLR(dtab, "path");
                        Utility.PostDataTableToSOLR(mNodeTab, "node");
                        //Otherwise, delete uri from SOLR
                    }
                    else
                    {
                        List<string> mUris = new List<string>();
                        mUris.Add(Convert.ToString(dtab.Rows[0]["paths_identifier"]));
                        foreach (DataRow mNodeRow in mNodeTab.Rows)
                        {
                            mUris.Add(Convert.ToString(mNodeRow["paths_identifier"]));
                        }
                        Utility.DeleteDocumentFromSOLR(mUris.ToArray());
                    }
                    mNodeTab = null;
                }
                string strRV = string.Empty;
                strRV = GetPathDetails(paths_identifier);
                Utility.LogRequest(strRV, false, mTimer);
                return strRV;
            }
            catch (Exception ex)
            {
                var mMsg = Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message);
                Utility.LogRequest(mMsg, true, mTimer);
                return mMsg;
            }
        }
        #endregion

        #region -------------------UpdatePath #236 -1--------------------
        /// <summary>
        /// Updates path data for a path identified by its URI
        /// </summary>
        /// <param name="paths_identifier">URI of path to be modified</param>
        /// <param name="paths_node_changes">Change in Node order</param>
        /// <param name="paths_start">A flag indicating whether this is where the PATH starts or not</param>
        /// <returns>JSON:Path information for the updated Path</returns>
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Update(string paths_identifier, string paths_node_changes, string paths_start)
        {
            var mTimer = new Stopwatch();
            mTimer.Start();

            string strQuery;
            if (Context.Session["isAuthenticated"] == null)
            {
                var mMsg = Utility.GetMsg(Utility.msgStatusCodes.OperationRequiresAuthentication);
                Utility.LogRequest(mMsg, false, mTimer);
                return mMsg;
            }
            strQuery = "select fk_usr_id from path where uri = '" + paths_identifier + "';";
            int iUid = Utility.DBExecuteScalar(strQuery);
            int uSessionId = 0;
            bool bUid = Utility.IsNumeric(Convert.ToString(Context.Session["usr_id"]));
            if (bUid)
            {
                uSessionId = Convert.ToInt32(Context.Session["usr_id"]);
            }
            if (uSessionId != iUid)
            {
                Context.Response.StatusCode = 401;
                Context.Response.StatusDescription = "Access Denied";
                var mMsg = Utility.GetMsg(Utility.msgStatusCodes.Unauthorized);
                Utility.LogRequest(mMsg, true, mTimer);
                return mMsg;
            }
            StringBuilder sqlBuilder = new StringBuilder();
            try
            {
                //Update the paths_start value in node
                //{paths_start:['http://paths-project.eu/node/12']}
                if (!string.IsNullOrEmpty(paths_start))
                {
                    string jsonInput = paths_start;
                    JavaScriptSerializer jssNode = new JavaScriptSerializer();
                    //Now with this line the Json string will be converted in PathNode object type
                    PathStart pathNodeObj = jssNode.Deserialize<PathStart>(jsonInput);
                    string[] paths_sNode = pathNodeObj.paths_start;
                    if (paths_sNode != null)
                        foreach (string str_uri in paths_sNode)
                        {
                            strQuery = "update node set paths_start = true where uri  = '" + str_uri.Trim() + "';";
                            Utility.DBExecuteNonQuery(strQuery);
                        }
                }
                //Update the paths_next and path_prev value in node table
                //[{paths_identifier: 'http://paths-project.eu/node/19',paths_next: ['http://paths-project.eu/node/12', 'http://paths-project.eu/node/8'],paths_prev:['http://paths-project.eu/node/10']}]
                if (!string.IsNullOrEmpty(paths_node_changes))
                {
                    if (paths_node_changes != "")
                    {
                        List<PathNodeIdentifier> nodeData;
                        JavaScriptSerializer jss = new JavaScriptSerializer();
                        nodeData = jss.Deserialize<List<PathNodeIdentifier>>(paths_node_changes);
                        string strNext, strPrev = "";
                        strQuery = "";
                        foreach (PathNodeIdentifier pathIdent in nodeData)
                        {
                            strNext = "";
                            strPrev = "";
                            string[] paths_next = pathIdent.paths_next;
                            string[] paths_prev = pathIdent.paths_prev;
                            if (paths_next != null)
                                foreach (string strN in paths_next)
                                {
                                    strNext += "\"" + strN + "\",";
                                }
                            if (paths_prev != null)
                                foreach (string strP in paths_prev)
                                {
                                    strPrev += "\"" + strP + "\",";
                                }
                            if (strNext != "")
                                strNext = strNext.Substring(0, strNext.LastIndexOf(","));
                            if (strPrev != "")
                                strPrev = strPrev.Substring(0, strPrev.LastIndexOf(","));
                            StringBuilder sbQuery = new StringBuilder();
                            if (paths_prev != null)
                            {
                                sbQuery.Append("paths_prev ='" + strPrev + "',");
                            }
                            if (paths_next != null)
                            {
                                sbQuery.Append("paths_next ='" + strNext + "',");
                            }
                            sbQuery.Append("isdeleted=false where uri = '");
                            strQuery = "update node set " + sbQuery.ToString() + pathIdent.paths_identifier + "';";
                            Utility.DBExecuteNonQuery(strQuery);
                        }
                    }
                }
                string strRV = string.Empty;
                strRV = GetPathDetails(paths_identifier);
                Utility.LogRequest(strRV, false, mTimer);
                return strRV;
            }
            catch (Exception ex)
            {
                var mMsg = Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message);
                Utility.LogRequest(mMsg);
                return mMsg;
            }
        }
        #endregion

        #region -------------------AddNode #237--------------------------
        /// <summary>
        /// Add a workspace item from the users workspace to a path as a node.
        /// </summary>
        /// <param name="paths_identifier">URI of path to which node should be added</param>
        /// <param name="paths_object">JSON String</param>
        /// <returns>JSON String: Single node information</returns>
        /// <remarks>Metod requires a user to be authenticated</remarks>
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Add_Node(string paths_identifier, string paths_object)
        {
            string strRV = string.Empty;
            string strQuery;
            DataTable dtab;
            if (Context.Session["isAuthenticated"] == null)
            {
                return Utility.GetMsg(Utility.msgStatusCodes.OperationRequiresAuthentication);
            }
            try
            {
                //{dc_title: "The new node title",paths_type: "The new nodes type",dc_description:"The new node description",dc_source: "The new node source",paths_thumbnail:['List containing thumbnail']}
                string jsonInput = paths_object;
                JavaScriptSerializer jss = new JavaScriptSerializer();
                //Now with this line the Json string will be converted in PathNode object type
                PathNode pathNodeObj = jss.Deserialize<PathNode>(jsonInput);

                //Find path id for given path uri
                strQuery = "select id from path where uri='" + paths_identifier + "';";
                int intPathId = Utility.DBExecuteScalar(strQuery);

                //Find highest present node sort-order
                double mNodeOrder = 0;
                strQuery = "SELECT COALESCE(max(node_order),0) + 1 AS node_order FROM node where fk_path_id =" + intPathId + ";";
                mNodeOrder = Utility.DBExecuteScalarDouble(strQuery);
                string strThumbnail = "";
                if (pathNodeObj.paths_thumbnail.Length > 0)
                {
                    string[] paths_thumbnails = pathNodeObj.paths_thumbnail;
                    foreach (string strThumb in paths_thumbnails)
                    {
                        strThumbnail += "\"" + strThumb + "\",";
                    }
                }
                else
                {
                    strThumbnail = ",";
                }
                //Insert item into node
                strQuery = "INSERT INTO node(fk_path_id, dc_title, dc_description, type, node_order,dc_source,paths_thumbnail) VALUES (" + intPathId + ",'" + pathNodeObj.dc_title.Replace("'", "''").Replace("\"", "\\\"") + "', '" + pathNodeObj.dc_description.Replace("'", "''") + "', '" + pathNodeObj.paths_type + "', " + mNodeOrder + ",'" + pathNodeObj.dc_source + "','" + strThumbnail.Substring(0, strThumbnail.LastIndexOf(",")) + "');SELECT currval('node_id_seq');";
                //Get new id
                int new_id = Utility.DBExecuteScalar(strQuery);

                ///*****Updated on 11 Sept 2013 for Ticket no 282*****///
                //Updating Node topics
                strQuery = "select t.id,t.num_paths,t.topics_above from topic t inner join item_topic it on it.fk_topic_id = t.id where it.fk_item_uri = '" + pathNodeObj.dc_source + "';";
                DataTable dtabNode = Utility.DBExecuteDataTable(strQuery);
                StringBuilder sbTopic = new StringBuilder();
                StringBuilder sbTopicAbove = new StringBuilder();
                if (dtabNode.Rows.Count > 0)
                {
                    foreach (DataRow drow in dtabNode.Rows)
                    {
                        //Update Topic table count
                        sbTopicAbove.Append(drow["topics_above"] + ",");
                    }
                    string strTopicIds = "";
                    string strTopicIdsAll = "";
                    string strTopicIdsArr = "";
                    string strTopicAbove = sbTopicAbove.ToString();
                    strTopicAbove = strTopicAbove.Replace("{", "").Replace("}", "");
                    strTopicAbove = strTopicAbove.Substring(0, strTopicAbove.LastIndexOf(","));
                    string[] strTopicArr = strTopicAbove.Split(',');
                    if (strTopicArr.Length > 0)
                    {
                        foreach (string strID in strTopicArr)
                        {
                            strTopicIds = strTopicIds + "'" + strID + "',";
                            strTopicIdsAll = strTopicIdsAll + strID + ",";
                        }
                        strTopicIds = strTopicIds.TrimEnd().Substring(0, strTopicIds.LastIndexOf(","));
                        string[] arrTid = strTopicIdsAll.Split(',');
                        arrTid = arrTid.Distinct().ToArray();

                        foreach (string str in arrTid)
                        {
                            if (str.Length > 3)
                            {
                                strTopicIdsArr = strTopicIdsArr + "\"" + str + "\",";
                            }
                        }
                        strTopicIdsArr = strTopicIdsArr.TrimEnd().Substring(0, strTopicIdsArr.LastIndexOf(","));
                    }
                    strQuery = "update topic set num_paths = num_paths+1 where id in (" + strTopicIds + ");";
                    Utility.DBExecuteNonQuery(strQuery);

                    strQuery = "update node set paths_topics = '" + strTopicIdsArr + "' Where id = " + new_id + ";";
                    Utility.DBExecuteNonQuery(strQuery);
                }
                //*****END UPDATES*****//

                //Get the new inserted node
                //strQuery = "SELECT id, fk_usr_id as dc_creator,'' paths_topics,uri paths_identifier,access,paths_thumbnail, dc_title, dc_description, dc_subject, lom_length as paths_duration,'path' AS dc_type,to_char(tstamp,'yyyy-mm-ddTmm:hh:ss') as dc_date, paths_status FROM path WHERE  uri = '" + paths_identifier + "' AND  isdeleted=false;";
                strQuery = "SELECT p.id, u.uri as dc_creator,'' paths_topics,p.uri paths_identifier,access,paths_thumbnail, dc_title, dc_description, dc_subject, lom_length as paths_duration,'path' AS dc_type,to_char(p.tstamp,'yyyy-mm-ddTmm:hh:ss') as dc_date, paths_status FROM path p  inner join usr u on p.fk_usr_id = u.id WHERE  p.uri = '" + paths_identifier + "' AND  p.isdeleted=false;";

                //strQuery = "SELECT p.*,p.uri paths_identifier,to_char(p.tstamp,'yyyy-mm-ddTmm:hh:ss') as dc_date, u.uri as dc_creator FROM path p, usr u WHERE u.id = p.fk_usr_id AND p.uri = '" + paths_identifier + "' AND p.isdeleted=false;";
                DataTable dtabPath = Utility.DBExecuteDataTable(strQuery);

                strQuery = "SELECT n.uri,n.uri paths_identifier, n.dc_title, n.dc_description, 'node' as dc_type, p.uri as path_uri,p.access  FROM node n inner join  path p on  n.fk_path_id=p.id  WHERE n.id = '" + new_id + "' and p.access='public' AND p.isdeleted=false AND n.isdeleted=false;";
                dtab = Utility.DBExecuteDataTable(strQuery);

                ///*****Updated on 25 Sept 2013 for Ticket no 282*****///
                strQuery = "select distinct(paths_topics) from node inner join path on path.id = node.fk_path_id where node.isdeleted = false and paths_topics <> '' and path.uri = '" + paths_identifier + "' ;";
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
                    dtabPath.Rows[0]["paths_topics"] = strPathTopics;
                }
                //dtabPath.Rows[0]["dc_creator"] = "http://paths-project.eu/usr/" + Convert.ToString(dtabPath.Rows[0]["dc_creator"]).Trim();
                //*****END UPDATES*****//
                //Insert into SOLR if path is published
                if (dtab.Rows.Count > 0)
                {
                    if (Convert.ToString(dtab.Rows[0]["access"]).ToLower() == "public")
                    {
                        Utility.DeleteDocumentFromSOLR(new string[] { paths_identifier });
                        Utility.PostDataTableToSOLR(dtabPath, "path");
                        Utility.PostDataTableToSOLR(dtab, "node");
                    }
                }
                //Get nodes data     
                strRV = GetNodeData(new_id);
                return strRV;
            }
            catch (Exception ex)
            {
                return Utility.GetMsg(Utility.msgStatusCodes.OperationFailed, ex.Message);
            }
        }

        public string GetNodeData(int nid)
        {
            string strRV = string.Empty;
            string strQuery = string.Empty;
            //Get nodes data in specific formt          
            strQuery = "select uri paths_identifier,fk_path_id ,node_order,type paths_type,dc_title,dc_description,dc_source,paths_thumbnail,paths_next,paths_topics, paths_prev from node where id =  " + nid + ";";
            DataTable nTable = Utility.DBExecuteDataTable(strQuery);
            nTable.Rows[0]["paths_topics"] = "#s#" + nTable.Rows[0]["paths_topics"] + "#e#";
            nTable.Rows[0]["paths_thumbnail"] = "#s#" + nTable.Rows[0]["paths_thumbnail"] + "#e#";
            nTable.Rows[0]["paths_next"] = "#s#" + nTable.Rows[0]["paths_next"] + "#e#";
            nTable.Rows[0]["paths_prev"] = "#s#" + nTable.Rows[0]["paths_prev"] + "#e#";
            strRV = DTtoJSON(nTable);
            strRV = strRV.Replace("\"#s#", "[").Replace("#e#\"", "]");
            strRV = strRV.TrimEnd().Substring(1, strRV.LastIndexOf("]") - 1);
            return strRV;
        }
        #endregion

        #region -------------------Update Node #238----------------------
        /// <summary>
        /// Update information of a node identified by its URI
        /// </summary>
        /// <param name="paths_identifier">URI of path</param>    
        /// <param name="paths_node_identifier">URI of node to be updated</param>        
        /// <param name="dc_title">Title of node</param>
        /// <param name="dc_description">Description of node</param>     
        /// <returns>JSON String: OperationCompletedSuccessfully (code=2) on success</returns>
        /// <remarks>Method requires authentication</remarks>
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Update_Node(string paths_identifier, string paths_node_identifier, string dc_title, string dc_description)
        {
            var mTimer = new Stopwatch();
            mTimer.Start();

            DataTable dtab;
            string strRV = string.Empty;
            string strQuery = string.Empty;
            if (Context.Session["isAuthenticated"] == null)
            {
                var mMsg = Utility.GetMsg(Utility.msgStatusCodes.OperationRequiresAuthentication);
                Utility.LogRequest(mMsg, false, mTimer);
                return mMsg;
            }
            StringBuilder sqlBuilder = new StringBuilder();
            try
            {
                //Get the id from path table.
                strQuery = "SELECT id FROM path WHERE uri='" + paths_identifier + "';";
                int pathID = Utility.DBExecuteScalar(strQuery);
                //Update node table
                sqlBuilder.Append("UPDATE node SET ");
                if (!string.IsNullOrEmpty(dc_title))
                {
                    sqlBuilder.Append("dc_title='" + dc_title.Replace("'", "''").Replace("\"", "\\\"") + "',");
                }
                if (!string.IsNullOrEmpty(dc_description))
                {
                    dc_description = Server.UrlDecode(dc_description);
                    sqlBuilder.Append("dc_description='" + dc_description.Replace("'", "''").Replace("\"", "''") + "',");
                }
                //Remove any trailing comma
                sqlBuilder.Remove(sqlBuilder.ToString().Length - 1, 1);
                //Finalize SQL statement
                sqlBuilder.Append(" WHERE uri='" + paths_node_identifier + "' and fk_path_id=" + pathID + ";");
                //Add command and execute
                strQuery = sqlBuilder.ToString();
                int rtnSuccess = Utility.DBExecuteNonQuery(strQuery);
                if (rtnSuccess > 0)
                {
                    strQuery = "SELECT id FROM node WHERE uri='" + paths_node_identifier + "';";
                    int nodeID = Utility.DBExecuteScalar(strQuery);
                    strRV = GetNodeData(nodeID);
                    //Delete the node from SOLR index
                    Utility.DeleteDocumentFromSOLR(new string[] { paths_node_identifier });
                }
                //Update modified record in SOLR
                strQuery = "SELECT * FROM node WHERE uri='" + paths_node_identifier + "' AND isdeleted=false and fk_path_id=" + pathID + ";";
                dtab = Utility.DBExecuteDataTable(strQuery);
                Utility.PostDataTableToSOLR(dtab, "node");
                Utility.LogRequest(strRV, false, mTimer);
                return strRV;
            }
            catch (Exception ex)
            {
                var mMsg = Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message);
                Utility.LogRequest(mMsg);
                return mMsg;
            }
        }
        #endregion

        #region -------------------Delete Node #239----------------------
        /// <summary>
        /// Delete a node identified by its URI
        /// </summary>
        /// <param name="paths_identifier">URI of path</param>
        /// <param name="paths_node_identifier">URI of node to be deleted</param>
        /// <returns>JSON String: Single node information</returns>
        /// <remarks>Method requires authentication</remarks>
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Delete_Node(string paths_identifier, string paths_node_identifier)
        {
            var mTimer = new Stopwatch();
            mTimer.Start();

            string strRV = string.Empty;
            string strQuery = string.Empty;
            if (Context.Session["isAuthenticated"] == null)
            {
                var mMsg = Utility.GetMsg(Utility.msgStatusCodes.OperationRequiresAuthentication);
                Utility.LogRequest(mMsg, false, mTimer);
                return mMsg;
            }
            try
            {
                //Get the id from path table
                strQuery = "SELECT id FROM path WHERE uri='" + paths_identifier + "';";
                int pathID = Utility.DBExecuteScalar(strQuery);

                strQuery = "UPDATE node SET isdeleted=true WHERE uri='" + paths_node_identifier + "' and fk_path_id=" + pathID + ";";
                int rtnSuccess = Utility.DBExecuteNonQuery(strQuery);

                ///*****Updated on 11 Sept 2013 for Ticket no 282*****///
                //Updating Node topics
                strQuery = "SELECT paths_topics  FROM node WHERE uri='" + paths_node_identifier + "';";
                DataTable dtab = Utility.DBExecuteDataTable(strQuery);

                if (dtab.Rows.Count > 0)
                {
                    string strTopicsID = dtab.Rows[0]["paths_topics"].ToString().Replace("\"", "'");
                    strQuery = "select topics_above,id from topic where id in (" + strTopicsID + ");";
                    DataTable dtabTopicID = Utility.DBExecuteDataTable(strQuery);
                    StringBuilder sbTopic = new StringBuilder();
                    StringBuilder sbTopicAbove = new StringBuilder();
                    if (dtabTopicID.Rows.Count > 0)
                    {
                        foreach (DataRow drow in dtabTopicID.Rows)
                        {
                            sbTopic.Append("\"" + drow["id"] + "\",");
                            //Update Topic table count
                            sbTopicAbove.Append(drow["topics_above"] + ",");
                        }
                        string strTopicIds = "";
                        string strTopicAbove = sbTopicAbove.ToString();
                        strTopicAbove = strTopicAbove.Replace("{", "").Replace("}", "");
                        strTopicAbove = strTopicAbove.Substring(0, strTopicAbove.LastIndexOf(","));
                        string[] strTopicArr = strTopicAbove.Split(',');
                        if (strTopicArr.Length > 0)
                        {
                            foreach (string strID in strTopicArr)
                            {
                                strTopicIds = strTopicIds + "'" + strID + "',";
                            }
                            strTopicIds = strTopicIds.TrimEnd().Substring(0, strTopicIds.LastIndexOf(","));
                        }
                        strQuery = "update topic set num_paths = num_paths-1 where id in (" + strTopicIds + ");";
                        Utility.DBExecuteNonQuery(strQuery);
                    }
                    //END Updates
                }
                if (rtnSuccess > 0)
                {
                    //Get data in specific formt
                    strQuery = "SELECT id FROM node WHERE uri='" + paths_node_identifier + "';";
                    int nodeID = Utility.DBExecuteScalar(strQuery);
                    strRV = GetNodeData(nodeID);
                    //Delete the node from SOLR index
                    Utility.DeleteDocumentFromSOLR(new string[] { paths_node_identifier });
                }
                Utility.LogRequest(strRV, false, mTimer);
                return strRV;
            }
            catch (Exception ex)
            {
                var mMsg = Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message);
                Utility.LogRequest(mMsg, true, mTimer);
                return mMsg;
            }
        }
        #endregion

    }
}
