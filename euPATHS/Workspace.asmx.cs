using System;
using System.ComponentModel;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;
using euPATHS.AppCode;
using System.Data;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Web;
using System.Diagnostics;

namespace euPATHS
{
   /// <summary>
   /// The Workspace web service contains methods for creating, managing, querying and deleting workspace items. 
   /// A workspace item can be considered a node which has not yet been completed and/or assigned ot a Path. 
   /// Workspace items can refer to any object identifiable by a URI and most commonly references records from the Items table.
   /// </summary>
   /// <remarks></remarks>
   [System.Web.Script.Services.ScriptService()]
   [System.Web.Services.WebService(Namespace = "http://paths-project.eu/")]
   [System.Web.Services.WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
   [ToolboxItem(false)]
   public class Workspace : System.Web.Services.WebService
   {
      #region -------------------Commented--------------------------------
      /*
      #region -------------------Add Workspace Item-------------------
      /// <summary>
      /// Adds an item to the present users workspace.
      /// </summary>
      /// <param name="fk_rel_uri">Any URI, but commonly a reference to the URI of a PATHS Item</param>
      /// <param name="dc_title">Title of workspace item</param>
      /// <param name="dc_description">Description of workspace item (optional)</param>
      /// <param name="type">Type of workspace item (optional, used?)</param>
      /// <returns>JSON String: Workspace item</returns>
      /// <remarks></remarks>
      [WebMethod(EnableSession = true)]
      [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
      public string AddWorkspaceItem(string fk_rel_uri, string dc_title, string dc_description, string type)
      {
         string strRV = string.Empty;
         euPaths clseuPaths = new euPaths();
         DataTable dtab;
         clseuPaths.requireUser(Context);
         try
         {
            string strQuery = "INSERT INTO workspace (fk_usr_id, fk_rel_uri, dc_title, dc_description, type) VALUES ('" + Convert.ToString(Context.Session["usr_id"]) + "','" + fk_rel_uri + "','" + dc_title + "','" + dc_description + "','" + type + "');SELECT currval('workspace_id_seq');";
            int NewId = Utility.DBExecuteScalar(strQuery);

            string strQuery1 = "SELECT * FROM workspace WHERE id = '" + NewId + "';";
            dtab = Utility.DBExecuteDataTable(strQuery1);
            strRV = Utility.DataTableToDictionary(dtab);
            return strRV;
         }
         catch (Exception ex)
         {
            return Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message);
         }
      }
      #endregion

      #region -------------------Delete Workspace Item----------------
      /// <summary>
      /// Deletes an item from the workspace.
      /// </summary>
      /// <param name="workspace_id">Unique datbase identifier of workspace item to be deleted</param>
      /// <returns>JSON String: OperationCompletedSuccessfully (code=2) on success, DatabaseSQLError (code=7) on error.</returns>
      /// <remarks></remarks>
      [WebMethod(EnableSession = true)]
      [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
      public string DeleteWorkspaceItem(string workspace_id)
      {
         string strRV = string.Empty;
         euPaths clseuPaths = new euPaths();
         clseuPaths.requireUser(Context);
         try
         {
            string strQuery = "DELETE FROM workspace WHERE id = '" + workspace_id + "' and fk_usr_id='" + Convert.ToString(Context.Session["usr_id"]) + "'";
            Utility.DBExecuteNonQuery(strQuery);
            return Utility.GetMsg(Utility.msgStatusCodes.OperationCompletedSuccessfully, "Item deleted from workspace");
         }
         catch (Exception ex)
         {
            return Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message);
         }
      }
      #endregion

      #region -------------------Update Workspace Item----------------
      /// <summary>
      /// Updates the information about an item in the users workspace
      /// </summary>
      /// <param name="workspace_id">Unique database identifier of the workspace item to be updated.</param>
      /// <param name="fk_rel_uri">URI of referenced object</param>
      /// <param name="dc_title">Title of workspace item</param>
      /// <param name="dc_description">Description of workspace item (optional)</param>
      /// <param name="type">Type of workspace item (optional)</param>
      /// <returns>JSON String: Single workspace item information</returns>
      /// <remarks></remarks>
      [WebMethod(EnableSession = true)]
      [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
      public string UpdateWorkspaceItem(int workspace_id, string fk_rel_uri, string dc_title, string dc_description, string type)
      {
         string strRV = string.Empty;
         euPaths clseuPaths = new euPaths();
         DataTable dtab;
         clseuPaths.requireUser(Context);
         StringBuilder sqlBuilder = new StringBuilder();
         try
         {
            sqlBuilder.Append("UPDATE workspace SET ");
            if (!string.IsNullOrEmpty(fk_rel_uri))
            {
               sqlBuilder.Append("fk_rel_uri='" + fk_rel_uri + "',"); ;
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
            sqlBuilder.Append("fk_usr_id='" + Convert.ToString(Context.Session["usr_id"]) + "'");
            sqlBuilder.Append(" WHERE id='" + workspace_id + "';");
            Utility.DBExecuteNonQuery(sqlBuilder.ToString());

            string strQuery = "SELECT * FROM workspace WHERE id = '" + workspace_id + "';";
            dtab = Utility.DBExecuteDataTable(strQuery);
            strRV = Utility.DataTableToDictionary(dtab);
            return strRV;
         }
         catch (Exception ex)
         {
            return Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message + sqlBuilder.ToString());
         }
      }
      #endregion

      #region -------------------Get Workspace Item By ID-------------
      /// <summary>
      /// Get a workspace item by its ID
      /// </summary>
      /// <param name="workspace_id">Unique database identifier of workspace item to be retrieved.</param>
      /// <returns>JSON String: Single workspace item information</returns>
      /// <remarks></remarks>
      [WebMethod(EnableSession = true)]
      [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
      public string GetWorkspaceItem(string workspace_id)
      {
         string strRV = string.Empty;
         euPaths clseuPaths = new euPaths();
         DataTable dtab;
         clseuPaths.requireUser(Context);
         try
         {
            string strQuery = "SELECT * FROM workspace WHERE id = '" + workspace_id + "' AND fk_usr_id = '" + Convert.ToString(Context.Session["usr_id"]) + "'";
            dtab = Utility.DBExecuteDataTable(strQuery);

            strRV = Utility.DataTableToDictionary(dtab);
            return strRV;
         }
         catch (Exception ex)
         {
            return Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message);
         }
      }
      #endregion

      #region -------------------Get Workspace Items------------------
      /// <summary>
      /// Get all workspace items for the current authenticated or temporary user.
      /// </summary>
      /// <returns>JSON String: List of workspace items information</returns>
      /// <remarks></remarks>
      [WebMethod(EnableSession = true)]
      [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
      public string GetWorkspaceItems()
      {
         string strRV = string.Empty;
         euPaths clseuPaths = new euPaths();
         DataTable dtab;
         clseuPaths.requireUser(Context);
         try
         {
            string strQuery = "SELECT * FROM workspace WHERE fk_usr_id = '" + Convert.ToString(Context.Session["usr_id"]) + "'";
            dtab = Utility.DBExecuteDataTable(strQuery);
            strRV = Utility.DataTableToDictionary(dtab);
            if (!(dtab.Rows.Count > 0))
            {
               return Utility.GetMsg(Utility.msgStatusCodes.QueryDidNotReturnRecords, "No workspace items for specified user");
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

      #region -------------------Get Workspace Item  #240------------------
      /// <summary>
      /// Fetches a single item from the workspace
      /// </summary>
      /// <param name="paths_identifier>Unique database identifier of workspace item to be retrieved.</param>
      /// <param name="paths_item_identifier">Unique identifier of source item</param>
      /// <returns>JSON:WorkspaceItem</returns>
      [WebMethod(EnableSession = true)]
      [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
      public string Item(string paths_identifier, string paths_item_identifier)
      {
         var mTimer = new Stopwatch();
         mTimer.Start();
         string strRV = string.Empty;
         euPaths clseuPaths = new euPaths();
         DataTable dtab;
         clseuPaths.requireUser(Context);
         try
         {
            string strQuery = "select wsi.uri paths_identifier,wsi.dc_title,wsi.dc_description,wsi.dc_source,wsi.paths_thumbnail,wsi.type paths_type from workspace ws inner join workspace_item wsi on wsi.fk_workspace_id=ws.id where ws.uri='" + paths_identifier + "' and wsi.uri='" + paths_item_identifier + "'";
            dtab = Utility.DBExecuteDataTable(strQuery);
            ////Updated on 23 September to decode dc_source
            //foreach (DataRow drow in dtab.Rows)
            //{
            //    drow["dc_source"] = HttpUtility.UrlEncode(Utility.FromBase64ForUrlString(Convert.ToString(drow["dc_source"])));
            //}
            ////END Update
            strRV = Utility.DataTableToDictionary(dtab);
            strRV = strRV.Replace("[", "").Replace("]", "");
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

      #region -------------------Get Workspace Items #241------------------
      /// <summary>
      /// Get all workspace items for the current authenticated or temporary user.
      /// </summary>
      /// <param name="paths_identifier">Unique identifier of the workspace item.</param>
      /// <returns>JSON String: List of workspace items information</returns>
      /// <remarks></remarks>
      [WebMethod(EnableSession = true)]
      [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
      public string Items(string paths_identifier)
      {
          var mTimer = new Stopwatch();
          mTimer.Start();
         string strRV = string.Empty;
         string strQuery = string.Empty;
         euPaths clseuPaths = new euPaths();
         DataTable dtab;
         clseuPaths.requireUser(Context);
         try
         {
            DataTable tblURI = new DataTable();
            DataColumn colURI = new DataColumn("paths_identifier");
            tblURI.Columns.Add(colURI);
            DataRow rowURI = tblURI.NewRow();
            rowURI[colURI] = paths_identifier;
            tblURI.Rows.Add(rowURI);

            strQuery = "select id from workspace where uri ='" + paths_identifier + "' ;";
            int workspaceID = Utility.DBExecuteScalar(strQuery);

            strQuery = "select uri paths_identifier from usr where id in (select fk_usr_id from workspace where id=" + workspaceID + " union select fk_usr_id from usr_workspace where fk_workspace_id = " + workspaceID + ");";
            dtab = Utility.DBExecuteDataTable(strQuery);

            strQuery = "select wsi.uri paths_identifier,wsi.dc_title,wsi.dc_description,wsi.dc_source,wsi.paths_thumbnail,wsi.type paths_type from workspace ws inner join workspace_item wsi on wsi.fk_workspace_id=ws.id where wsi.fk_workspace_id=" + workspaceID + " order by paths_order;";
            DataTable dtab1 = Utility.DBExecuteDataTable(strQuery);
            ////Updated on 23 September to decode dc_source
            //foreach (DataRow drow in dtab1.Rows)
            //{
            //    drow["dc_source"] = HttpUtility.UrlEncode(Utility.FromBase64ForUrlString(Convert.ToString(drow["dc_source"])));                
            //}
            ////END Update
            Dictionary<string, DataTable> myAdditionalTables = new Dictionary<string, DataTable>();
            myAdditionalTables.Add("paths_authorised", dtab);
            myAdditionalTables.Add("paths_items", dtab1);

            strRV = Utility.DataTableToDictionary(tblURI, true, myAdditionalTables);
            //strRV = strRV.Replace("[", "").Replace("]", "");
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

      #region -------------------Add Workspace Item #242-------------------
      /// <summary>
      /// This function will first check for the given uri in workspace table if record presents it will insert new record in workspace_item with exiting workspace id.
      /// Otherwise will insert new record in both table (workspace,workspace_item)
      /// </summary>
      /// <param name="paths_identifier">URI reference to the Workspace</param>
      /// <param name="dc_title">Title of workspace item</param>
      /// <param name="dc_description">Description of workspace item (optional)</param>
      /// <param name="dc_source">The item's source data (optional)</param>
      /// <param name="paths_thumbnail"> The item's thumbnail</param>
      /// <param name="paths_type">Type of workspace item</param>
      /// <returns>JSON:WorkspaceItem</returns>
      /// <remarks></remarks>
      [WebMethod(EnableSession = true)]
      [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
      public string Add(string paths_identifier, string dc_title, string dc_description, string dc_source, string paths_thumbnail, string paths_type)
      {
          var mTimer = new Stopwatch();
          mTimer.Start();
         string strRV = string.Empty;
         euPaths clseuPaths = new euPaths();
         string strQuery = string.Empty;
         int workspaceID = 0;
         DataTable dtab;
         clseuPaths.requireUser(Context);
         try
         {
            strQuery = "select * from workspace where uri='" + paths_identifier + "';";
            workspaceID = Utility.DBExecuteScalar(strQuery);
            if (workspaceID == 0)
            {
               strQuery = "INSERT INTO workspace (fk_usr_id,isprimary) VALUES ('" + Convert.ToString(Context.Session["usr_id"]) + "',false);SELECT currval('workspace_id_seq');";
               workspaceID = Utility.DBExecuteScalar(strQuery);
            }
            //Insert data into workspace_item table
            strQuery = "insert into workspace_item (fk_workspace_id, dc_title, dc_description, dc_source,type,paths_thumbnail) VALUES ('" + workspaceID + "','" + dc_title.Replace("'", "''") + "','" + dc_description.Replace("'", "''") + "','" + dc_source + "','" + paths_type + "','" + paths_thumbnail.Replace("'", "''") + "');SELECT currval('workspace_item_id_seq');";
            int workspaceItemID = Utility.DBExecuteScalar(strQuery);
            strQuery = "select wsi.uri paths_identifier,wsi.dc_title,wsi.dc_description,wsi.dc_source,wsi.paths_thumbnail,wsi.type paths_type from workspace ws inner join workspace_item wsi on wsi.fk_workspace_id=ws.id where ws.id='" + workspaceID + "' and wsi.id='" + workspaceItemID + "'";
            dtab = Utility.DBExecuteDataTable(strQuery);
            strRV = Utility.DataTableToDictionary(dtab);
            strRV = strRV.Replace("[", "").Replace("]", "");
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

      #region -------------------Update Workspace Item #243----------------
      /// <summary>
      /// Updates the information about an item in the users workspace
      /// </summary>
      /// <param name="paths_identifier">Unique identifier of the workspace item.</param>
      /// <param name="paths_item_identifier">URI of referenced object in workspace_item table </param>
      /// <param name="dc_description">Description of workspace item</param>
      /// <returns>JSON String: Single workspace item information</returns>
      /// <remarks></remarks>
      [WebMethod(EnableSession = true)]
      [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
      public string Update(string paths_identifier, string paths_item_identifier, string dc_description)
      {
         string strRV = string.Empty;
         euPaths clseuPaths = new euPaths();
         DataTable dtab;
         clseuPaths.requireUser(Context);
         StringBuilder sqlBuilder = new StringBuilder();
         string strQuery = string.Empty;
         try
         {
            strQuery = "select id from workspace where uri ='" + paths_identifier + "' ;";
            int workspaceID = Utility.DBExecuteScalar(strQuery);
            sqlBuilder.Append("UPDATE workspace_item SET ");            
            if (dc_description !=null)
            {
               sqlBuilder.Append("dc_description='" + dc_description + "'");
            }
            sqlBuilder.Append(" WHERE fk_workspace_id='" + workspaceID + "' and ");
            sqlBuilder.Append(" uri='" + paths_item_identifier + "';");
            Utility.DBExecuteNonQuery(sqlBuilder.ToString());

            strQuery = "select wsi.uri paths_identifier,wsi.dc_title,wsi.dc_description,wsi.dc_source,wsi.paths_thumbnail,wsi.type paths_type from workspace ws inner join workspace_item wsi on wsi.fk_workspace_id=ws.id where wsi.fk_workspace_id='" + workspaceID + "' and wsi.uri='" + paths_item_identifier + "'";
            dtab = Utility.DBExecuteDataTable(strQuery);
            strRV = Utility.DataTableToDictionary(dtab);
            strRV = strRV.Replace("[", "").Replace("]", "");
            return strRV;
         }
         catch (Exception ex)
         {
            return Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message + sqlBuilder.ToString());
         }
      }
      #endregion

      #region -------------------Update Workspace Item #243----------------
      /// <summary>
      /// Updates the information about an item in the users workspace
      /// </summary>
      /// <param name="paths_identifier">Unique identifier of the workspace item.</param>
      /// <param name="paths_item_identifier">URI of referenced object in workspace_item table </param>
      /// <param name="paths_order">Order of workspace item</param>
      /// <returns>JSON String: Single workspace item information</returns>
      /// <remarks></remarks>
      [WebMethod(EnableSession = true)]
      [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
      public string UpdateOrder(string paths_identifier, string paths_item_identifier, string paths_order)
      {
          var mTimer = new Stopwatch();
          mTimer.Start();

         string strRV = string.Empty;
         euPaths clseuPaths = new euPaths();
         DataTable dtab;
         clseuPaths.requireUser(Context);
         StringBuilder sqlBuilder = new StringBuilder();
         string strQuery = string.Empty;
         try
         {
            strQuery = "select id from workspace where uri ='" + paths_identifier + "' ;";
            int workspaceID = Utility.DBExecuteScalar(strQuery);
            sqlBuilder.Append("UPDATE workspace_item SET ");
            if (!string.IsNullOrEmpty(paths_order))
            {
               if (Utility.IsNumeric(paths_order))
                  sqlBuilder.Append("paths_order='" + paths_order + "'");
            }            
            sqlBuilder.Append(" WHERE fk_workspace_id='" + workspaceID + "' and ");
            sqlBuilder.Append(" uri='" + paths_item_identifier + "';");
            Utility.DBExecuteNonQuery(sqlBuilder.ToString());

            strQuery = "select wsi.uri paths_identifier,wsi.dc_title,wsi.dc_description,wsi.dc_source,wsi.paths_thumbnail,wsi.type paths_type from workspace ws inner join workspace_item wsi on wsi.fk_workspace_id=ws.id where wsi.fk_workspace_id='" + workspaceID + "' and wsi.uri='" + paths_item_identifier + "'";
            dtab = Utility.DBExecuteDataTable(strQuery);
            strRV = Utility.DataTableToDictionary(dtab);
            strRV = strRV.Replace("[", "").Replace("]", "");
            Utility.LogRequest(strRV, false, mTimer);
            return strRV;
         }
         catch (Exception ex)
         {
             var mMsg = Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message + sqlBuilder.ToString());
             Utility.LogRequest(mMsg, true, mTimer);
            return mMsg;
         }
      }
      #endregion

      #region -------------------Delete Workspace Item #244----------------
      /// <summary>
      /// Deletes an item from the workspace.
      /// </summary>
      /// <param name="paths_identifier">Unique identifier of the workspace item.</param>
      /// <param name="paths_item_identifier">URI of referenced object in workspace_item table </param>
      /// <returns>JSON String: OperationCompletedSuccessfully (code=2) on success, DatabaseSQLError (code=7) on error.</returns>
      /// <remarks></remarks>
      [WebMethod(EnableSession = true)]
      [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
      public string Delete(string paths_identifier, string paths_item_identifier)
      {
          var mTimer = new Stopwatch();
          mTimer.Start();
         string strRV = string.Empty;
         DataTable dtab;
         euPaths clseuPaths = new euPaths();
         string strQuery = string.Empty;
         clseuPaths.requireUser(Context);
         try
         {
            strQuery = "select id from workspace where uri ='" + paths_identifier + "' ;";
            int workspaceID = Utility.DBExecuteScalar(strQuery);

            strQuery = "select wsi.uri paths_identifier,wsi.dc_title,wsi.dc_description,wsi.dc_source,wsi.paths_thumbnail,wsi.type paths_type from workspace ws inner join workspace_item wsi on wsi.fk_workspace_id=ws.id where wsi.fk_workspace_id='" + workspaceID + "' and wsi.uri='" + paths_item_identifier + "'";
            dtab = Utility.DBExecuteDataTable(strQuery);
            strRV = Utility.DataTableToDictionary(dtab);

            strQuery = "DELETE FROM workspace_item WHERE fk_workspace_id = '" + workspaceID + "' and uri='" + paths_item_identifier + "'";
            Utility.DBExecuteNonQuery(strQuery);
            strRV = strRV.Replace("[", "").Replace("]", "");
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