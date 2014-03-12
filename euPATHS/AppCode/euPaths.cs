using System;
using euPATHS.AppCode;
using System.Web;
using System.Data;
using System.Collections.Generic;

namespace euPATHS
{
   /// <summary>
   /// All private utility methods of the PATHS Web Service API are kept in a separate module "euPaths". This reduces the risk of namespace conflicts and offers a convenient way of grouping the code.
   /// </summary>
   /// <remarks></remarks>
   public class euPaths
   {
      #region -------------------User Authentication-------------------
      /// <summary>
      /// Utility function to perform authentication
      /// </summary>
      /// <param name="usr">User name</param>
      /// <param name="pwd">Password</param>
      /// <returns>Integer: ID of authenticate user if present in datbase or -1 if not.</returns>
      /// <remarks></remarks>
      public int authenticate(string usr, string pwd)
      {
         string strQuery = "SELECT id FROM usr WHERE (usr='" + usr + "' or email = '" + usr + "') AND pwd='" + pwd + "' AND isdeleted=false;";
         int intRtnID = Utility.DBExecuteScalar(strQuery);
         if (intRtnID > 0)
            return intRtnID;
         else
            return Convert.ToInt16(Utility.msgStatusCodes.NoSuchUser);
      }
      /// <summary>
      /// Utility function to create a temporary user
      /// </summary>
      /// <param name="myContext">HTTP context of caller web method</param>
      /// <returns>Boolean: true if success in creating temporary user, false if error.</returns>
      /// <remarks></remarks>
      public bool createTemporaryUser(HttpContext myContext)
      {
         DataTable dtab = new DataTable();
         int usr_id;
         try
         {
            string strQuery = "SELECT id from usr WHERE usr = '" + Convert.ToString(myContext.Session.SessionID).Trim() + "';";
            dtab = Utility.DBExecuteDataTable(strQuery);
            if (dtab.Rows.Count > 0)
            {
               usr_id = Convert.ToInt32(dtab.Rows[0]["id"]);
               myContext.Session["usr_id"] = usr_id;
               myContext.Session["isTemporary"] = true;
            }
            else
            {
               strQuery = "INSERT INTO usr (usr, email, fk_cog_style_id, istemporary) VALUES ('" + Convert.ToString(myContext.Session.SessionID).Trim() + "','anonymous@paths-project.eu','1','true');SELECT currval('usr_id_seq');";
               usr_id = Utility.DBExecuteScalar(strQuery);
               myContext.Session["usr_id"] = usr_id;
               myContext.Session["isTemporary"] = true;
            }
            strQuery = "INSERT INTO workspace (fk_usr_id,isprimary) VALUES ('" + usr_id + "',true);SELECT currval('workspace_id_seq');";
            int workspaceID = Utility.DBExecuteScalar(strQuery);
            return true;
         }
         catch (Exception)
         {
            return false;
         }
      }

      public bool requireUser(HttpContext myContext)
      {
         if (Utility.IsNumeric(Convert.ToString(myContext.Session["usr_id"])) == false)
         {
            if (createTemporaryUser(myContext) == true)
            {
               return true;
            }
            else
            {
               return false;
            }
         }
         else
         {
            return true;
         }
      }
      #endregion

      #region -------------------Get Current User Details -------------
      /// <summary>
      /// Utility function to get Current User
      /// </summary>
      /// <param name="usrid">User ID</param>
      /// <returns>Json string</returns>
      /// <remarks></remarks>
      public string GetCurrentUserDetails(string usrid)
      {
         DataTable dtab;
         string strRV = string.Empty;
         string strQuery = "SELECT uri as paths_identifier,foaf_nick,email as foaf_mbox,email_visibility foaf_mbox_visibility, (CASE istemporary when true THEN 'new' ELSE 'registered' END)dc_type,to_char(tstamp,'yyyy-mm-ddTmm:hh:ss')  paths_registered FROM usr WHERE id = '" + usrid + "'";
         dtab = Utility.DBExecuteDataTable(strQuery);

         strQuery = "select dc_title, dc_source from (select * from ubehaviour where fk_usr_id = " + usrid + "  order by id desc limit 10) ubehaviour order by id asc;";
         DataTable uBehaviourTable = Utility.DBExecuteDataTable(strQuery);

         if (Convert.ToString(dtab.Rows[0]["foaf_mbox"]) == "anonymous@paths-project.eu")
         {
            strQuery = "select (CASE isprimary when true THEN 'true' ELSE 'false' END) as primary,uri paths_identifier from workspace WHERE fk_usr_id = " + usrid + " order by id desc limit 1;";
         }
         else
         {
            strQuery = "select (CASE isprimary when true THEN 'true' ELSE 'false' END) as primary,uri paths_identifier from workspace WHERE fk_usr_id = " + usrid + " order by isprimary desc;";
         }
         DataTable uWorkspace = Utility.DBExecuteDataTable(strQuery);

         Dictionary<string, DataTable> myAdditionalTables = new Dictionary<string, DataTable>();
         myAdditionalTables.Add("paths_breadcrumbs", uBehaviourTable);
         myAdditionalTables.Add("paths_workspaces", uWorkspace);

         strRV = Utility.DataTableToDictionary(dtab, true, myAdditionalTables);
         return strRV;
      }
      #endregion
   }
}