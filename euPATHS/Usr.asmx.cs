using System;
using System.Collections.Generic;
using System.Web.Services;
using System.Web.Script.Services;
using System.Data.Odbc;
using System.Data;
using System.Text;
using euPATHS.AppCode;
using System.Diagnostics;

namespace euPATHS
{
    /// <summary>
    /// The Usr web service contains methods for authenticating users, 
    /// creating and modifying users, logging user behavior and issuing reminder 
    /// e-mails upon forgetting passwords. 
    /// The service is fundamental to web services which require authentication.
    /// </summary>
    [WebService(Namespace = "http://paths-project.eu/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class Usr : System.Web.Services.WebService
    {
        #region --------------------------Comment-----------------------------
        /*
      #region -------------------User Authentication-------------------
      /// <summary>
      /// Perform authentication of user
      /// </summary>
      /// <param name="usr">User name</param>
      ///<param name="pwd">Password</param>
      /// <returns>AuthenticationSucceeded (code=4) on success, AuthenticationFailed (code=1) on wrong username/password, OperationFailed (code=3) on error.</returns>
      ///<summary>    
      [WebMethod(EnableSession = true)]
      [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
      public string Authenticate(string usr, string pwd)
      {
         euPaths clseuPaths = new euPaths();
         try
         {
            int isAuthenticated = clseuPaths.authenticate(usr, pwd);
            if (isAuthenticated > 0)
            {
               if (Context.Session["isTemporary"] != null && Utility.IsNumeric(Convert.ToString(Context.Session["usr_id"])))
               {
                  string strQuery = "UPDATE workspace SET fk_usr_id=" + isAuthenticated + " WHERE fk_usr_id=" + Convert.ToString(Context.Session["usr_id"]) + ";";
                  int rtnID = Utility.DBExecuteNonQuery(strQuery);
                  Context.Session["isTemporary"] = null;
               }
               Context.Session["usr_id"] = isAuthenticated;
               Context.Session["isAuthenticated"] = true;
               return Utility.GetMsg(Utility.msgStatusCodes.AuthenticationSucceeded, Convert.ToString(Context.Session["usr_id"]));
            }
            else
            {
               Context.Session["usr_id"] = null;
               Context.Session["isAuthenticated"] = null;
               return Utility.GetMsg(Utility.msgStatusCodes.AuthenticationFailed);
            }
         }
         catch (Exception ex)
         {
            return Utility.GetMsg(Utility.msgStatusCodes.OperationFailed, ex.Message);
         }
      }
      #endregion

      #region -------------------Logged out User-----------------------
      /// <summary>
      /// Logs the current user out of the system by erasing user information from the session
      /// </summary>
      /// <returns>Always returns LogoutSuccess (code=6)</returns>
      /// <remarks></remarks>
      [WebMethod(EnableSession = true)]
      [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
      public string Logout()
      {
         Context.Session["isAuthenticated"] = null;
         Context.Session["usr_id"] = null;
         Context.Session.Clear();
         Context.Session.Abandon();
         return Utility.GetMsg(Utility.msgStatusCodes.LogoutSuccess, "User logged out");
      }
      #endregion

      #region -------------------Create a new User---------------------
      /// <summary>
      /// Create a new user
      /// </summary>
      /// <param name="fk_cog_style_id">Integer, the primary key id of the cognitive style associated with the user</param>
      /// <param name="usr">Username</param>
      /// <param name="pwd">Password</param>
      /// <param name="foaf_nick">Nickname/display name</param>
      /// <param name="email">E-mail address</param>
      /// <param name="openid">Whether or not the user account is an OpenID account (Boolean, true/false)</param>
      /// <returns>Returns OperationCompletedSuccessfully (code=2) and the user data for the created user</returns>
      /// <remarks></remarks>
      [WebMethod(EnableSession = true)]
      [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
      public string CreateUser(int fk_cog_style_id, string usr, string foaf_nick, string pwd, string email, bool openid)
      {
         DataTable dtab;
         try
         {
            string strQuery = "INSERT INTO usr (fk_cog_style_id,usr,foaf_nick,pwd,email,openid)  VALUES ('" + fk_cog_style_id + "','" + usr + "','" + foaf_nick + "','" + pwd + "','" + email + "','" + openid + "');SELECT currval('usr_id_seq');";
            int rtnID = Utility.DBExecuteScalar(strQuery);
            strQuery = "SELECT * FROM USR where id ='" + rtnID + "';";
            dtab = Utility.DBExecuteDataTable(strQuery);
            string strRV = Utility.DataTableToDictionary(dtab, true);
            return strRV;
         }
         catch (Exception ex)
         {
            return Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message);
         }
      }
      #endregion

      #region -------------------Update User information---------------
      /// <summary>
      /// Modifies information about a user identified by its URI
      /// </summary>
      /// <param name="usr_uri">URI of the user to be modified</param>
      /// <param name="fk_cog_style_id">The id of the users cognitive style (optional)</param>
      /// <param name="usr">Username (optional)</param>
      /// <param name="foaf_nick">Nickname/display name (optional)</param>
      /// <param name="pwd">Password (optional)</param>
      /// <param name="email">E-mail (optional)</param>)
      /// <param name="openid">Whether the user is an OpenID user (Boolean, optional)</param>
      /// <returns>User data object for modified user</returns>
      /// <remarks></remarks>
      [WebMethod(EnableSession = true)]
      [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
      public string ModifyUser(string usr_uri, string fk_cog_style_id, string usr, string foaf_nick, string pwd, string email, string openid)
      {
         DataTable dtab;
         if (Context.Session["isAuthenticated"] == null)
         {
            return Utility.GetMsg(Utility.msgStatusCodes.OperationRequiresAuthentication);
         }
         StringBuilder sqlBuilder = new StringBuilder();
         try
         {
            sqlBuilder.Append("UPDATE usr SET ");
            if (Utility.IsNumeric(fk_cog_style_id))
            {
               sqlBuilder.Append("fk_cog_style_id='" + fk_cog_style_id + "',");
            }
            if (!string.IsNullOrEmpty(usr))
            {
               sqlBuilder.Append("usr='" + usr + "',");
            }
            if (!string.IsNullOrEmpty(foaf_nick))
            {
               sqlBuilder.Append("foaf_nick='" + foaf_nick + "',");
            }
            if (!string.IsNullOrEmpty(pwd))
            {
               sqlBuilder.Append("pwd='" + pwd + "',");
            }
            if (!string.IsNullOrEmpty(email))
            {
               sqlBuilder.Append("email='" + email + "',");
            }
            if (!string.IsNullOrEmpty(openid))
            {
               sqlBuilder.Append("openid='" + openid + "',");
            }
            sqlBuilder.Remove(sqlBuilder.ToString().Length - 1, 1);
            sqlBuilder.Append(" WHERE uri='" + usr_uri + "';");
            string strQuery = sqlBuilder.ToString();
            //Update the existing row.
            Utility.DBExecuteNonQuery(strQuery);
            strQuery = "SELECT uri, fk_cog_style_id, usr,foaf_nick, email, openid, istemporary, tstamp FROM USR where uri = '" + usr_uri + "';";
            dtab = Utility.DBExecuteDataTable(strQuery);
            string strRV = Utility.DataTableToDictionary(dtab);
            dtab = null;
            return strRV;
         }
         catch (Exception ex)
         {
            return Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message + " " + sqlBuilder.ToString());
         }
      }
      #endregion

      #region -------------------Delete User---------------------------
      /// <summary>
      /// Deletes a user from PATHS
      /// </summary>
      /// <param name="usr_uri">URI of the user to be deleted</param>
      /// <returns>OperationCompletedSuccessfully (code=2) if the user was either marked as deleted or did not exist, DatabaseSQLError (code=7) on error.</returns>
      /// <remarks>Method requires authentication</remarks>
      [WebMethod(EnableSession = true)]
      [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
      public string DeleteUser(string usr_uri)
      {
         if (Context.Session["isAuthenticated"] == null)
         {
            return Utility.GetMsg(Utility.msgStatusCodes.OperationRequiresAuthentication);
         }
         try
         {
            //Update the isdeleted flag of the user to TRUE and return status message
            string strQuery = "UPDATE usr SET isdeleted = TRUE, usr = 'deleted_'||usr WHERE uri = '" + usr_uri + "';";
            int intRecAffected = Utility.DBExecuteNonQuery(strQuery);
            //Alok Change Check the condition to check number of record affected.
            if (intRecAffected > 0)
               return Utility.GetMsg(Utility.msgStatusCodes.OperationCompletedSuccessfully, "User successfully marked for deletion");
            else
               return Utility.GetMsg(Utility.msgStatusCodes.NoSuchUser, "No User found matching the criteria");
         }
         catch (Exception ex)
         {
            return Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message);
         }
      }
      #endregion

      #region -------------------Sends an e-mail with Password---------

      /// <summary>
      /// Sends an e-mail with the password of the user corresponding
      /// </summary>
      /// <param name="pUsr">The username of the user to whom the password reminder should be sent</param>
      /// <returns>Always returns OperationCompletedSuccessfully (code=2). If the username is found, an e-mail with the corresponding password is sent to the users e-mail address.</returns>
      /// <remarks></remarks>
      [WebMethod(EnableSession = true)]
      [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
      public string ForgotPassword(string pUsr)
      {
         DataTable dtab;
         string strForgotPassword = string.Empty;
         try
         {
            //Update the isdeleted flag of the user to TRUE and return status message
            string strQuery = "SELECT email, pwd FROM usr WHERE usr ='" + pUsr + "'";
            dtab = Utility.DBExecuteDataTable(strQuery);
            string mEmail = string.Empty;
            string mPwd = string.Empty;
            if (dtab.Rows.Count > 0)
            {
               mEmail = Convert.ToString(dtab.Rows[0]["email"]);
               mPwd = Convert.ToString(dtab.Rows[0]["pwd"]);
               bool mResult = Utility.sendEmail(mEmail, mPwd);
               //Change to 'OK'
               if (mResult)
               {
                  //Return ok if success
                  strForgotPassword = Utility.GetMsg(Utility.msgStatusCodes.OperationCompletedSuccessfully, "Ok");
               }
               else
               {
                  //Return ok anyway to avoid user snooping
                  strForgotPassword = Utility.GetMsg(Utility.msgStatusCodes.OperationCompletedSuccessfully, "Ok");
               }
            }
            else
            {//Alok Change message for User not found
               strForgotPassword = Utility.GetMsg(Utility.msgStatusCodes.NoSuchUser, "No User found matching the criteria");
            }
            return strForgotPassword;
         }
         catch (Exception ex)
         {
            return Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message);
         }
      }
      #endregion

      #region -------------------Get Current User Information----------
      /// <summary>
      /// Gets information about the currently authenticated or temporary user.
      /// </summary>
      /// <returns>User data object for current user</returns>
      /// <remarks></remarks>
      [WebMethod(EnableSession = true)]
      [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
      public string GetCurrentUser()
      {
         DataTable dtab;
         string strRV = string.Empty;
         euPaths clseuPaths = new euPaths();
         if (clseuPaths.requireUser(Context) == false)
         {
            return Utility.GetMsg(Utility.msgStatusCodes.OperationFailed, "Failed to create temporary user");
         }
         try
         {
            string strQuery = "SELECT uri, fk_cog_style_id, usr,foaf_nick, email, istemporary, tstamp FROM usr WHERE id = '" + Convert.ToString(Context.Session["usr_id"]) + "'";
            dtab = Utility.DBExecuteDataTable(strQuery);
            strQuery = "SELECT u.title FROM usr_ugroup uu, ugroup u WHERE uu.fk_usr_id = ''" + Convert.ToString(Context.Session["usr_id"]) + "' and uu.fk_ugroup_id = u.id;";
            DataTable ugroupTable = Utility.DBExecuteDataTable(strQuery);
            //Get user groups
            Dictionary<string, DataTable> myAdditionalTables = new Dictionary<string, DataTable>();
            myAdditionalTables.Add("paths_ugroup", ugroupTable);
            strRV = Utility.DataTableToDictionary(dtab, true, myAdditionalTables);
            return strRV;
         }
         catch (Exception ex)
         {
            return Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message);
         }
      }
      #endregion

      #region -------------------Get User By Uri-----------------------
      /// <summary>
      /// Returns information about the user identified by the specified URI
      /// </summary>
      /// <param name="usr_uri">URI of user</param>
      /// <returns>User data object</returns>
      /// <remarks></remarks>
      [WebMethod(EnableSession = true)]
      [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
      public string GetUserByUri(string usr_uri)
      {
         string strGetUserByUri = string.Empty;
         euPaths clseuPaths = new euPaths();
         DataTable dtab = new DataTable();
         try
         {
            string strQuery = "SELECT uri, fk_cog_style_id, usr, foaf_nick, email, istemporary, tstamp FROM usr WHERE uri = '" + usr_uri + "'";
            dtab = Utility.DBExecuteDataTable(strQuery);
            Dictionary<string, DataTable> myAdditionalTables = new Dictionary<string, DataTable>();
            //Get user groups
            DataTable ugroupTable = new DataTable();
            strQuery = "SELECT u.title FROM usr_ugroup uu, ugroup u WHERE uu.fk_usr_id = '" + Context.Session["usr_id"] + "' and uu.fk_ugroup_id = u.id;";
            ugroupTable = Utility.DBExecuteDataTable(strQuery);
            myAdditionalTables.Add("paths_ugroup", ugroupTable);
            strGetUserByUri = Utility.DataTableToDictionary(dtab, true, myAdditionalTables);
            return strGetUserByUri;
         }
         catch (Exception ex)
         {
            return Utility.GetMsg(Utility.msgStatusCodes.AuthenticationSucceeded, ex.Message);
         }
      }
      #endregion

      #region -------------------Log Page View-------------------------

      /// <summary>
      /// Logs a URI to the browsing history of the user and returns the five last pages visited during the session.
      /// </summary>
      /// <param name="myTargetTitle">Title of web page to log</param>
      /// <param name="myTargetUri">URI of web page to log</param>
      /// <returns>List of five most recent logged page view objects for current session</returns>
      /// <remarks></remarks>
      [WebMethod(EnableSession = true)]
      [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
      public string LogPageView(string myTargetTitle, string myTargetUri)
      {
         string strLogPageView = string.Empty;
         euPaths clseuPaths = new euPaths();
         DataTable dtab;
         //Create user if none exists...
         if (clseuPaths.requireUser(Context) == false)
         {
            return Utility.GetMsg(Utility.msgStatusCodes.FailedToCreateTemporaryUser);
         }
         try
         {
            string strQuery = "SELECT id, usession, target_uri, target_title, source_uri, source_title, stime FROM ubehaviour WHERE fk_usr_id = '" + Context.Session["usr_id"] + "' ORDER BY id DESC LIMIT 5;";
            dtab = Utility.DBExecuteDataTable(strQuery);
            string mySourceTitle = "";
            string mySourceUri = "";
            if (dtab.Rows.Count > 0)
            {
               if (Convert.ToString(dtab.Rows[0]["usession"]).Trim() == Convert.ToString(Context.Session.SessionID).Trim())
               {
                  mySourceTitle = Convert.ToString(dtab.Rows[0]["target_title"]);
                  mySourceUri = Convert.ToString(dtab.Rows[0]["target_uri"]);
               }
            }
            //Insert the new page into the log
            strQuery = "INSERT INTO ubehaviour(fk_usr_id, usession, target_uri, target_title, source_title,source_uri) VALUES ('" + Context.Session["usr_id"] + "', '" + Convert.ToString(Context.Session.SessionID).Trim() + "', '" + myTargetUri.Trim() + "', '" + myTargetTitle.Trim() + "', '" + mySourceTitle.Trim() + "', '" + mySourceUri.Trim() + "');";
            Utility.DBExecuteNonQuery(strQuery);
            //Add most recent page to datatable before transforming to JSON and returning
            DataRow myRow = null;
            myRow = dtab.NewRow();
            myRow["id"] = Context.Session["usr_id"];
            myRow["usession"] = Convert.ToString(Context.Session.SessionID).Trim();
            myRow["target_uri"] = myTargetUri;
            myRow["target_title"] = myTargetTitle;
            myRow["source_uri"] = mySourceUri;
            myRow["source_title"] = mySourceTitle;
            dtab.Rows.InsertAt(myRow, 0);
            strLogPageView = Utility.DataTableToDictionary(dtab);
            return strLogPageView;
         }
         catch (Exception ex)
         {
            return Utility.GetMsg(Utility.msgStatusCodes.AuthenticationSucceeded, ex.Message);
         }
      }

      #endregion

      */
        #endregion
        //New Web Methods
        #region -------------------Get Current #219-----------------------
        /// <summary>
        /// Gets information about the currently authenticated or temporary user.
        /// </summary>
        /// <returns>User data object for current user</returns>
        /// <remarks></remarks>
        [WebMethod(EnableSession = true, Description = "Get Current User")]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Current()
        {
            var mTimer = new Stopwatch();
            mTimer.Start();

            string strRV = string.Empty;
            euPaths clseuPaths = new euPaths();

            if (clseuPaths.requireUser(Context) == false)
            {
                // **** Updated on 12 September 2013
                // commented below line and add new line.
                // return Utility.GetMsg(Utility.msgStatusCodes.OperationFailed, "Failed to create temporary user");
                clseuPaths.requireUser(Context);
                // **** Updated on 12 September 2013
            }
            try
            {
                strRV = clseuPaths.GetCurrentUserDetails(Convert.ToString(Context.Session["usr_id"]));
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

        #region -------------------Add User Breadcrumbs #220------------------
        /// <summary>
        /// Adds the given title and url to the current user's paths_breadcrumbs.
        /// </summary>
        /// <param name="dc_source">Any valid url</param>
        /// <param name="dc_title">Any title string</param>
        /// <returns>JSON:SuccessMessage</returns>
        /// <remarks></remarks>
        [WebMethod(EnableSession = true, Description = "Log User Page")]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string LogPage(string dc_title, string dc_source)
        {
            var mTimer = new Stopwatch();
            mTimer.Start();
            euPaths clseuPaths = new euPaths();
            string strQuery = string.Empty;
            DataTable dtab;
            string strDC = "";
            int dRowCount = 0;
            //Create user if none exists...
            if (clseuPaths.requireUser(Context) == false)
            {
                var mStr = Utility.GetMsg(Utility.msgStatusCodes.FailedToCreateTemporaryUser);
                Utility.LogRequest(mStr, true, mTimer);
                return mStr;
            }
            try
            {
                strQuery = "select * from ubehaviour where usession = '" + Convert.ToString(Context.Session.SessionID).Trim() + "' order by id desc limit 1;";
                dtab = Utility.DBExecuteDataTable(strQuery);
                dRowCount = dtab.Rows.Count;
                if (dRowCount > 0)
                {
                    strDC = Convert.ToString(dtab.Rows[0]["dc_source"]).Trim();
                }
                if (strDC != dc_source)
                {
                    strQuery = "INSERT INTO ubehaviour(fk_usr_id, usession,dc_title ,dc_source) VALUES ('" + Context.Session["usr_id"] + "', '" + Convert.ToString(Context.Session.SessionID).Trim() + "', '" + dc_title.Trim() + "', '" + dc_source.Trim() + "');";
                    Utility.DBExecuteNonQuery(strQuery);
                }
                var mMsg = Utility.GetMsg(Utility.msgStatusCodes.OperationCompletedSuccessfully, "Operation completed successfully");
                Utility.LogRequest(mMsg, false, mTimer);
                return mMsg;
            }
            catch (Exception ex)
            {
                var mMsg = Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message);
                Utility.LogRequest(mMsg, true, mTimer);
                return mMsg;
            }
        }
        #endregion

        #region -------------------Log User action #221-----------------------
        /// <summary>
        /// Log every request of user that is processed 
        /// </summary>       
        /// <param name="dc_source">The URL that was requested by the user</param>
        /// <param name="paths_request">Any request parameters formatted as a JSON structure and serialised as a string</param>
        /// <returns>Returns OperationCompletedSuccessfully (code=2)</returns>
        /// <remarks></remarks>
        [WebMethod(EnableSession = true, Description = "Log User Action")]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string LogAction(string dc_source, string paths_request)
        {
            var mTimer = new Stopwatch();
            mTimer.Start();

            euPaths clseuPaths = new euPaths();
            if (clseuPaths.requireUser(Context) == false)
            {
                var mMsg = Utility.GetMsg(Utility.msgStatusCodes.FailedToCreateTemporaryUser);
                Utility.LogRequest(mMsg, true, mTimer);
                return mMsg;
            }
            try
            {
                string strQuery = "INSERT INTO uaction (fk_usr_id,usession,dc_source,paths_request)  VALUES ('" + Context.Session["usr_id"] + "', '" + Convert.ToString(Context.Session.SessionID).Trim() + "','" + dc_source + "','" + paths_request + "');";
                int rtnID = Utility.DBExecuteNonQuery(strQuery);

                var mMsg = Utility.GetMsg(Utility.msgStatusCodes.OperationCompletedSuccessfully, "Operation completed successfully");
                Utility.LogRequest(mMsg, false, mTimer);
                return mMsg;
            }
            catch (Exception ex)
            {
                var mMsg = Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message);
                Utility.LogRequest(mMsg, true, mTimer);
                return mMsg;
            }
        }
        #endregion

        #region -------------------Get User By ID #222------------------------
        /// <summary>
        /// Returns information about the user identified by the specified identifier
        /// </summary>
        /// <param name="paths_identifier"></param>
        /// <returns>JSON:UserData</returns>
        /// <remarks></remarks>
        [WebMethod(EnableSession = true, Description = "Get User Data")]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string UserGet(string paths_identifier)
        {
            var mTimer = new Stopwatch();
            mTimer.Start();

            string strGetUserByID = string.Empty;
            euPaths clseuPaths = new euPaths();
            DataTable dtab = new DataTable();
            try
            {
                string strQuery = "SELECT uri paths_identifier,foaf_nick, email foaf_mbox ,email_visibility foaf_mbox_visibility,(CASE istemporary when true THEN 'new' ELSE 'registered' END)dc_type,to_char(tstamp,'yyyy-mm-ddTmm:hh:ss') paths_registered FROM usr WHERE uri = '" + paths_identifier + "'";
                dtab = Utility.DBExecuteDataTable(strQuery);
                strGetUserByID = Utility.DataTableToDictionary(dtab, true);
                Utility.LogRequest(strGetUserByID, false, mTimer);
                return strGetUserByID;
            }
            catch (Exception ex)
            {
                var mMsg = Utility.GetMsg(Utility.msgStatusCodes.AuthenticationSucceeded, ex.Message);
                Utility.LogRequest (mMsg, true, mTimer);
                return mMsg;
            }
        }
        #endregion

        #region -------------------Get Current User Path #223-----------------
        /// <summary>
        /// Fetches all of the current user's paths
        /// </summary>
        /// <returns>JSON:UserPaths</returns>
        /// <remarks></remarks>
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Paths()
        {
            var mTimer = new Stopwatch();
            mTimer.Start();

            string strRV = string.Empty;
            try
            {
                strRV = GetPathDetails();
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

        #region--------------------GetResult from Database-------------
        public string GetPathDetails()
        {
            var mTimer = new Stopwatch();
            Path clsPath = new Path();
            string strRV = string.Empty;
            string strRVFinal = string.Empty;
            string strRVN = string.Empty;
            string strRVC = string.Empty;
            string strQuery = string.Empty;
            string paths_identifier = string.Empty;
            int pathID = 0;
            if (Context.Session["isAuthenticated"] == null)
            {
                var mMsg = Utility.GetMsg(Utility.msgStatusCodes.OperationRequiresAuthentication);
                Utility.LogRequest(mMsg, false, mTimer);
                return mMsg;
            }

            try
            {
                strQuery = "select uri paths_identifier,id,fk_usr_id,dc_title,dc_description,dc_subject,lom_length paths_duration,access paths_access,(CASE paths_iscloneable when true THEN 'true' ELSE 'false' END)paths_clone,paths_thumbnail,'#pt#' paths_topics,'#ps#' paths_start,'#pn#' paths_nodes,'#pc#' dc_creator from path where fk_usr_id = '" + Convert.ToString(Context.Session["usr_id"]) + "' and isdeleted = false;";
                DataTable dtab = Utility.DBExecuteDataTable(strQuery);
                if (dtab.Rows.Count > 0)
                {
                    //Get user id
                    int usrID = Convert.ToInt32(dtab.Rows[0]["fk_usr_id"]);
                    foreach (DataRow drow in dtab.Rows)
                    {
                        pathID = Convert.ToInt32(drow["id"]);
                        paths_identifier = Convert.ToString(drow["paths_identifier"]);
                        drow["dc_subject"] = "#s#" + drow["dc_subject"] + "#l#";
                        DataTable dtFrow = dtab.Clone();
                        dtFrow.ImportRow(drow);
                        strRV = clsPath.DTtoJSON(dtFrow);
                        strRV = strRV.Replace("\"#s#", "[").Replace("#l#\"", "]");
                        //Get Path Topic
                        strQuery = "select uri from topic where id in (select fk_topic_id from item_topic where fk_item_uri in (select dc_source from node inner join path on path.id= node.fk_path_id where node.isdeleted=false and path.uri = '" + paths_identifier + "'));";
                        string strTopic = clsPath.GetJsonList("uri", strQuery);
                        strRV = strRV.Replace("\"#pt#\"", strTopic);
                        //Get Path Start
                        strQuery = "select uri from node where node.isdeleted=false and paths_start = true and fk_path_id = " + pathID + ";";
                        string strPStart = clsPath.GetJsonList("uri", strQuery);
                        strRV = strRV.Replace("\"#ps#\"", strPStart);
                        //Get Path nodes
                        strQuery = "select uri paths_identifier,node_order,type paths_type,dc_title,dc_description,dc_source, '#pthmb#' paths_thumbnails,'#pnx#' paths_next,'#ppv#' paths_prev from node where isdeleted=false and fk_path_id =" + pathID + ";";
                        DataTable dtabNode = Utility.DBExecuteDataTable(strQuery);
                        DataTable dtabNodeNP = new DataTable();
                        string strNxt = "", strPrev = "", strThmb = "";
                        if (dtabNode.Rows.Count > 0)
                        {
                            strQuery = "select id, paths_next, paths_prev,paths_thumbnail from node where isdeleted=false and uri ='" + Convert.ToString(dtabNode.Rows[0]["paths_identifier"]) + "';";
                            dtabNodeNP = Utility.DBExecuteDataTable(strQuery);
                            if (dtabNodeNP.Rows.Count > 0)
                            {
                                strNxt = "[" + Convert.ToString(dtabNodeNP.Rows[0]["paths_next"]) + "]";
                                strPrev = "[" + Convert.ToString(dtabNodeNP.Rows[0]["paths_prev"]) + "]";
                                strThmb = "[" + Convert.ToString(dtabNodeNP.Rows[0]["paths_thumbnail"]) + "]";
                            }
                        }
                        strRVN = clsPath.DTtoJSON(dtabNode);
                        strRVN = strRVN.Replace("\"#pnx#\"", strNxt);
                        strRVN = strRVN.Replace("\"#ppv#\"", strPrev);
                        strRVN = strRVN.Replace("\"#pthmb#\"", strThmb);
                        strRV = strRV.Replace("\"#pn#\"", strRVN);
                        //Get User Data
                        strQuery = "SELECT uri paths_identifier,foaf_nick, email foaf_mbox ,email_visibility foaf_mbox_visibility,(CASE istemporary when true THEN 'new' ELSE 'registered' END)dc_type,to_char(tstamp,'yyyy-mm-ddTmm:hh:ss') paths_registered FROM usr WHERE id=" + usrID + ";";
                        DataTable uTable = Utility.DBExecuteDataTable(strQuery);
                        strRVC = clsPath.DTtoJSON(uTable);
                        strRVC = strRVC.TrimEnd().Substring(1, strRVC.LastIndexOf("]") - 1);
                        strRV = strRV.Replace("\"#pc#\"", strRVC);
                        strRV = strRV.TrimEnd().Substring(1, strRV.LastIndexOf("]") - 1);
                        strRVFinal = strRVFinal + strRV + ",";
                    }
                    strRVFinal = strRVFinal.TrimEnd().Substring(0, strRVFinal.LastIndexOf(",") - 1);
                    strRV = "{\"code\":2,\"data\":[" + strRVFinal + "]}";
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
        #endregion

        #region -------------------User Login #224----------------------------
        /// <summary>
        /// Perform user login
        /// </summary>
        /// <param name="foaf_mbox">The user's e-mail address to use as the login</param>
        /// <param name="paths_password">The user's password</param>
        /// <returns>JSON:StatusCode, AuthenticationSucceeded (code=4) on success, AuthenticationFailed (code=1) on wrong username/password, OperationFailed (code=3) on error.</returns>
        ///<summary>    
        [WebMethod(EnableSession = true, Description = "User Login")]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string UserLogin(string foaf_mbox, string paths_password)
        {
            var mTimer = new Stopwatch();
            mTimer.Start();

            euPaths clseuPaths = new euPaths();
            try
            {
                int isAuthenticated = clseuPaths.authenticate(foaf_mbox, paths_password);
                string strQuery = string.Empty;
                string strRV = string.Empty;
                if (isAuthenticated > 0)
                {
                    if (Context.Session["isTemporary"] != null && Utility.IsNumeric(Convert.ToString(Context.Session["usr_id"])))
                    {
                        strQuery = "select id from workspace where isprimary = true and fk_usr_id =" + isAuthenticated + ";";
                        int rtnWSID = Utility.DBExecuteScalar(strQuery);

                        strQuery = "select id from workspace WHERE fk_usr_id=" + Convert.ToString(Context.Session["usr_id"]) + ";";
                        DataTable dtRtnWSTID = Utility.DBExecuteDataTable(strQuery);

                        //delete from workspace where id = 18
                        foreach (DataRow drow in dtRtnWSTID.Rows)
                        {
                            strQuery = "delete from workspace where id =" + drow["id"] + ";";
                            Utility.DBExecuteScalar(strQuery);
                        }

                        strQuery = "delete from usr where id =" + Convert.ToString(Context.Session["usr_id"]) + ";";
                        Utility.DBExecuteScalar(strQuery);

                        foreach (DataRow drow in dtRtnWSTID.Rows)
                        {
                            strQuery = "UPDATE workspace_item SET fk_workspace_id=" + rtnWSID + " WHERE fk_workspace_id=" + drow["id"] + ";";
                            int rtnID = Utility.DBExecuteNonQuery(strQuery);
                        }
                        Context.Session["isTemporary"] = null;
                    }
                    Context.Session["usr_id"] = isAuthenticated;
                    Context.Session["isAuthenticated"] = true;
                    //Get user data after login
                    strRV = clseuPaths.GetCurrentUserDetails(Convert.ToString(Context.Session["usr_id"]));
                    Utility.LogRequest(strRV, false, mTimer);
                    return strRV;
                }
                else
                {
                    Context.Session["usr_id"] = null;
                    Context.Session["isAuthenticated"] = null;
                    var mMsg = Utility.GetMsg(Utility.msgStatusCodes.AuthenticationFailed);
                    Utility.LogRequest(mMsg, false, mTimer);
                    return mMsg;
                }
            }
            catch (Exception ex)
            {
                var mMsg = Utility.GetMsg(Utility.msgStatusCodes.OperationFailed, ex.Message);
                Utility.LogRequest(mMsg);
                return mMsg;
            }
        }
        #endregion

        #region -------------------Logged out User #225-----------------------
        /// <summary>
        /// Logs the current user out of the system by erasing user information from the session
        /// </summary>
        /// <returns>JSON:StatusMessage, always returns LogoutSuccess (code=6)</returns>
        /// <remarks></remarks>
        [WebMethod(EnableSession = true, Description = "User Logout")]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string UserLogout()
        {
            var mTimer = new Stopwatch();
            mTimer.Start();

            euPaths clseuPaths = new euPaths();
            Context.Session["isAuthenticated"] = null;
            Context.Session["usr_id"] = null;
            Context.Session.Clear();
            Context.Session.Abandon();
            string strQuery = string.Empty;
            string strRV = string.Empty;
            if (Context.Session["isAuthenticated"] == null)
            {
                clseuPaths.createTemporaryUser(Context);
            }
            //Get anonymous user data.
            strRV = clseuPaths.GetCurrentUserDetails(Convert.ToString(Context.Session["usr_id"]));
            Utility.LogRequest(strRV, false, mTimer);
            return strRV;
        }
        #endregion

        #region -------------------Register User #226-------------------------
        /// <summary>
        /// Create a new user
        /// </summary>
        /// <param name="foaf_mbox">E-mail address (user name)</param>
        /// <param name="paths_password">Password</param>
        /// <param name="foaf_nick">Nickname/display name</param>
        /// <returns>JSON:StatusMessage OperationCompletedSuccessfully (code=2) and JSON:UserData for the created user</returns>
        /// <remarks></remarks>
        [WebMethod(EnableSession = true, Description = "User Registration")]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string UserRegister(string foaf_nick, string foaf_mbox, string paths_password)
        {
            var mTimer = new Stopwatch();
            mTimer.Start();

            try
            {
                euPaths clseuPaths = new euPaths();
                string strQueryUserCheck = "select id from usr where email = '" + foaf_mbox + "';";
                int rtnUserID = Utility.DBExecuteScalar(strQueryUserCheck);
                if (rtnUserID == 0)
                {
                    string strQuery = "INSERT INTO usr (fk_cog_style_id,usr,foaf_nick,pwd,email)  VALUES ('" + 1 + "','" + foaf_mbox + "','" + foaf_nick + "','" + paths_password + "','" + foaf_mbox + "');SELECT currval('usr_id_seq');";
                    int rtnID = Utility.DBExecuteScalar(strQuery);

                    if (Convert.ToString(Context.Session.SessionID).Trim() != "")
                    {
                        strQuery = "select fk_usr_id from uaction where usession = '" + Convert.ToString(Context.Session.SessionID).Trim() + "';";
                        int rtnUID = Utility.DBExecuteScalar(strQuery);

                        if (Context.Session["usr_id"] == null)
                        {
                            strQuery = "INSERT INTO workspace (fk_usr_id, isprimary) VALUES (" + rtnID + ", true);";
                            int workspaceID = Utility.DBExecuteScalar(strQuery);
                        }
                        else
                        {
                            strQuery = "update workspace set fk_usr_id = " + rtnID + " , isprimary = true where fk_usr_id=" + Context.Session["usr_id"] + ";";
                            int workspaceID = Utility.DBExecuteScalar(strQuery);
                        }
                        strQuery = "update uaction set fk_usr_id = " + rtnID + "  where usession = '" + Convert.ToString(Context.Session.SessionID).Trim() + "';";
                        Utility.DBExecuteNonQuery(strQuery);
                        strQuery = "update ubehaviour set fk_usr_id = " + rtnID + "  where usession = '" + Convert.ToString(Context.Session.SessionID).Trim() + "';";
                        Utility.DBExecuteNonQuery(strQuery);
                    }

                    Context.Session["usr_id"] = rtnID;
                    Context.Session["isAuthenticated"] = true;

                    string strRV = clseuPaths.GetCurrentUserDetails(Convert.ToString(rtnID));
                    Utility.LogRequest(strRV, false, mTimer);
                    return strRV;
                }
                else
                {
                    var mMsg = Utility.GetMsg(Utility.msgStatusCodes.EmailAlreadyExists, "Email already exists");
                    Utility.LogRequest(mMsg,false, mTimer);
                    return mMsg;
                }
            }
            catch (Exception ex)
            {
                var mMsg = Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message);
                Utility.LogRequest(mMsg, true, mTimer);
                return mMsg;
            }
        }
        #endregion

        #region -------------------Update User information--------------------
        /// <summary>
        /// Modifies information about a user identified by its URI
        /// </summary>
        /// <param name="foaf_nick">Nickname/display name (optional)</param>
        /// <param name="foaf_mbox">E-mail address/user name</param>
        /// <param name="foaf_mbox_visibility">Whether the email address should be publicly visible or not</param>
        /// <param name="paths_identifier">URI of the user to be modified</param>
        /// <returns>JSON:UserData object for modified user</returns>
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Update(string paths_identifier, string foaf_nick, string foaf_mbox, string foaf_mbox_visibility)
        {
            var mTimer = new Stopwatch();
            mTimer.Start();

            DataTable dtab;
            if (Context.Session["isAuthenticated"] == null)
            {

                var mMsg = Utility.GetMsg(Utility.msgStatusCodes.OperationRequiresAuthentication);
                Utility.LogRequest(mMsg, false, mTimer);
                return mMsg;
            }
            StringBuilder sqlBuilder = new StringBuilder();
            try
            {
                sqlBuilder.Append("UPDATE usr SET ");
                if (!string.IsNullOrEmpty(foaf_nick))
                {
                    sqlBuilder.Append("foaf_nick='" + foaf_nick + "',");
                }
                if (!string.IsNullOrEmpty(foaf_mbox))
                {
                    sqlBuilder.Append("email='" + foaf_mbox + "',");
                }
                if (!string.IsNullOrEmpty(foaf_mbox_visibility))
                {
                    sqlBuilder.Append("email_visibility='" + foaf_mbox_visibility + "',");
                }
                sqlBuilder.Remove(sqlBuilder.ToString().Length - 1, 1);
                sqlBuilder.Append(" WHERE uri='" + paths_identifier + "';");
                string strQuery = sqlBuilder.ToString();
                //Update the existing row.
                Utility.DBExecuteNonQuery(strQuery);
                strQuery = "SELECT uri paths_identifier,foaf_nick, email foaf_mbox ,email_visibility foaf_mbox_visibility,(CASE istemporary when true THEN 'new' ELSE 'registered' END)dc_type,to_char(tstamp,'yyyy-mm-ddTmm:hh:ss') paths_registered FROM usr WHERE uri = '" + paths_identifier + "'";
                dtab = Utility.DBExecuteDataTable(strQuery);
                string strRV = Utility.DataTableToDictionary(dtab);
                dtab = null;
                Utility.LogRequest(strRV, false, mTimer);
                return strRV;
            }
            catch (Exception ex)
            {
                var mMsg = Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message + " " + sqlBuilder.ToString());
                Utility.LogRequest(mMsg);
                return mMsg;
            }
        }
        #endregion
    }
}
