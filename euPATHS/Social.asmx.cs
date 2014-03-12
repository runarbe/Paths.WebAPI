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
using euPATHS.AppCode;


namespace euPATHS
{
    /// <summary>
    /// The web service Social contains all functionality associated with user generated content which may be attached to paths, nodes and items. UGC elements are associated with resources via a URI and may in principle be attached to any web resource. This reduces the amount of tables required for the connections and simplifies the data management.
    /// </summary>
    /// <remarks></remarks>
    [System.Web.Script.Services.ScriptService()]
    [System.Web.Services.WebService(Namespace = "http://paths-project.eu/")]
    [System.Web.Services.WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    public class Social : System.Web.Services.WebService
    {
        JavaScriptSerializer json = new JavaScriptSerializer();

        #region -------------------Comment Services-------------------
        /// <summary>
        /// Get comments for a web resource with specified URI
        /// </summary>
        /// <param name="fk_rel_uri">URI of web resource for which comments should be retrieved.</param>
        /// <returns>OperationCompletedSuccessfully (code=2) + list of comment data objects on success.</returns>
        /// <remarks></remarks>
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetCommentsForUri(string fk_rel_uri)
        {
            var mTimer = new Stopwatch();
            mTimer.Start();

            string strRV = string.Empty;
            DataTable dtab;
            try
            {
                string strQuery = "SELECT c.comment, to_char(c.tstamp,'yyyy-mm-ddTmm:hh:ss') as dc_date, u.uri as dc_creator_uri, u.foaf_nick as dc_creator_name FROM comment c, usr u WHERE u.id = c.fk_usr_id AND c.isdeleted=false AND c.fk_rel_uri = '" + fk_rel_uri + "';";
                dtab = Utility.DBExecuteDataTable(strQuery);
                if (dtab.Rows.Count == 0)
                {
                    strRV = Utility.GetMsg(Utility.msgStatusCodes.QueryDidNotReturnRecords, "No messages for specified URI");
                }
                else
                {
                    strRV = Utility.DataTableToDictionary(dtab);
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
        /// <summary>
        /// Add new comment to web resource identified by URI
        /// </summary>
        /// <param name="fk_rel_uri">URI of web resource to be commented upon</param>
        /// <param name="comment">Comment text</param>
        /// <returns>OperationCompleteSuccessfully (code=2) + single comment data object</returns>
        /// <remarks>Web method requires user to be authenticated</remarks>
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string AddComment(string fk_rel_uri, string comment)
        {
            var mTimer = new Stopwatch();
            mTimer.Start();

            string strRV = string.Empty;
            DataTable dtab;
            if (Context.Session["isAuthenticated"] == null)
            {
                var mMsg = Utility.GetMsg(Utility.msgStatusCodes.OperationRequiresAuthentication);
                Utility.LogRequest(mMsg, false, mTimer);
                return mMsg;
            }
            try
            {
                string strQuery = "INSERT INTO comment (fk_usr_id, fk_rel_uri, comment) VALUES ('" + Convert.ToString(Context.Session["usr_id"]) + "','" + fk_rel_uri + "','" + comment + "');SELECT currval('comment_id_seq');";
                int mCommentId = Utility.DBExecuteScalar(strQuery);

                string strQuery1 = "SELECT c.id, c.comment, to_char(c.tstamp,'yyyy-mm-ddTmm:hh:ss') as dc_date, u.uri as dc_creator_uri, u.foaf_nick as dc_creator_name FROM comment c, usr u WHERE u.id = c.fk_usr_id AND c.isdeleted=false AND c.id= '" + mCommentId + "';";
                dtab = Utility.DBExecuteDataTable(strQuery1);

                if (dtab.Rows.Count > 0)
                {
                    strRV = Utility.DataTableToDictionary(dtab, true);
                }
                else
                {
                    strRV = Utility.GetMsg(Utility.msgStatusCodes.QueryDidNotReturnRecords, "No comment was returned");
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
        /// <summary>
        /// Deletes comment with specified identifier
        /// </summary>
        /// <param name="comment_id">Unique database identifier of comment to be deleted</param>
        /// <returns>OperationCompletedSuccessfully (code=2) on success.</returns>
        /// <remarks>Method requires a user to be authenticated.</remarks>
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string DeleteComment(int comment_id)
        {
            var mTimer = new Stopwatch();
            mTimer.Start();

            string strRV = string.Empty;
            if (Context.Session["isAuthenticated"] == null)
            {
                var mMsg = Utility.GetMsg(Utility.msgStatusCodes.AuthenticationFailed);
                Utility.LogRequest(mMsg, false, mTimer);
                return mMsg;
            }
            try
            {
                string strQuery = "UPDATE comment SET isdeleted=true WHERE id = '" + comment_id + "';";
                Utility.DBExecuteNonQuery(strQuery);
                var mMsg = Utility.GetMsg(Utility.msgStatusCodes.OperationCompletedSuccessfully);
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

        #region -------------------Tag Services-----------------------
        /// <summary>
        /// Adds a tag (keyword) to a resource identified by a URI
        /// </summary>
        /// <param name="fk_rel_uri">URI of resource which tag should be added to</param>
        /// <param name="tag">Any keyword or keyphrase to be used as tag</param>
        /// <returns>Tag data object and OperationCompletedSuccessfully (code=2) on success</returns>
        /// <remarks>Method requires a user to be authenticated</remarks>
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string AddTag(string fk_rel_uri, string tag)
        {
            string strRV = string.Empty;
            if (Context.Session["isAuthenticated"] == null)
            {
                return Utility.GetMsg(Utility.msgStatusCodes.OperationRequiresAuthentication);
            }
            try
            {
                string strQuery = "SELECT t.id FROM tag t, tagging tg WHERE tg.fk_tag_id = t.id AND tg.fk_usr_id = '" + Convert.ToString(Context.Session["usr_id"]) + "' AND t.label='" + tag + "';";
                int myTagID = Utility.DBExecuteScalar(strQuery);
                if (myTagID <= 0)
                {
                    strQuery = "INSERT INTO tag (label, lang) VALUES ('" + tag + "','en'); SELECT currval('tag_id_seq');";
                    myTagID = Utility.DBExecuteScalar(strQuery);
                }
                strQuery = "INSERT INTO tagging (fk_tag_id, fk_usr_id, fk_rel_uri) VALUES ('" + myTagID + "','" + Convert.ToString(Context.Session["usr_id"]) + "','" + fk_rel_uri + "');";
                Utility.DBExecuteNonQuery(strQuery);

                //Get tag uri
                strQuery = "SELECT uri, label, lang FROM tag WHERE id = '" + myTagID + "'";
                DataTable myTags = Utility.DBExecuteDataTable(strQuery);
                strRV = Utility.DataTableToDictionary(myTags, true);

                return strRV;
            }
            catch (Exception ex)
            {
                return Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message);
            }
        }
        /// <summary>
        /// Delete tag with specified URI
        /// </summary>
        /// <param name="tag_uri">URI of the tag to be deleted</param>
        /// <returns>OperationCompletedSuccessfully (code=2) on success</returns>
        /// <remarks>Method requires a user to be authenticated</remarks>
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string DeleteTag(string tag_uri)
        {
            if (Context.Session["isAuthenticated"] == null)
            {
                return Utility.GetMsg(Utility.msgStatusCodes.NoSuchUser);
            }
            try
            {
                string strQuery = "UPDATE tagging SET isdeleted=true WHERE fk_tag_id = (SELECT id FROM tag WHERE uri='" + tag_uri + "') AND fk_usr_id = '" + Convert.ToString(Context.Session["usr_id"]) + "';";
                Utility.DBExecuteNonQuery(strQuery);
                return Utility.GetMsg(Utility.msgStatusCodes.OperationCompletedSuccessfully, "Successfully deleted tag assignment");
            }
            catch (Exception ex)
            {
                return Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message);
            }
        }
        /// <summary>
        /// Get list of tags associated with a specific resource identified by its URI
        /// </summary>
        /// <param name="fk_rel_uri">URI of resource for which tags should be retrieved</param>
        /// <returns>QueryDidNotReturnRecords (code=8) if no tags are found, OperationCompletedSuccessfully (code=2) and list of tag data objects on success</returns>
        /// <remarks></remarks>
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetTagsForUri(string fk_rel_uri)
        {
            string strRV = string.Empty;
            try
            {
                string strQuery = "SELECT DISTINCT t.uri, t.label, u.uri as dc_creator_uri, u.foaf_nick as dc_creator_name FROM tag t, tagging tg, usr u WHERE tg.isdeleted= false AND tg.fk_rel_uri = '" + fk_rel_uri + "' AND t.id = tg.fk_tag_id AND u.id = tg.fk_usr_id;";
                DataTable dtab = Utility.DBExecuteDataTable(strQuery);
                if (dtab.Rows.Count == 0)
                {
                    strRV = Utility.GetMsg(Utility.msgStatusCodes.QueryDidNotReturnRecords, "No tags available for specified URI");
                }
                else
                {
                    strRV = Utility.DataTableToDictionary(dtab);
                }
                return strRV;
            }
            catch (Exception ex)
            {
                return Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message);
            }
        }
        #endregion

        #region -------------------Rating Services--------------------
        /// <summary>
        /// Add rating to a resource identified by its URI
        /// </summary>
        /// <param name="fk_rating_scale_id">Unique database identifier for rating_scale table. 1 = dislikes, 2=likes</param>
        /// <param name="fk_rel_uri">URI of resource which rating should be added to</param>
        /// <returns>QueryDidNotReturnRecords (code=8) if no rating values exist; OperationCompletedSuccessfully (code=2) and count of ratings </returns>
        /// <remarks>Requires an authenticated or temporary user session</remarks>
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string AddRating(int fk_rating_scale_id, string fk_rel_uri)
        {
            string strRV = string.Empty;
            euPaths clseuPaths = new euPaths();
            if (Context.Session["isAuthenticated"] == null)
            {
                clseuPaths.createTemporaryUser(Context);
            }
            try
            {
                string strQuery = "SELECT count(id) FROM rating WHERE fk_usr_id = '" + Convert.ToString(Context.Session["usr_id"]) + "' and fk_rel_uri = '" + fk_rel_uri + "' and isdeleted = false;";
                int intResult = Utility.DBExecuteScalar(strQuery);
                int mRatingId;
                if (intResult > 0)
                {
                    strQuery = "UPDATE rating SET fk_rating_scale_id = '" + fk_rating_scale_id + "' WHERE fk_usr_id = '" + Convert.ToString(Context.Session["usr_id"]) + "' AND fk_rel_uri = '" + fk_rel_uri + "' AND isdeleted = false;";
                    Utility.DBExecuteNonQuery(strQuery);

                    strQuery = "SELECT id FROM rating WHERE fk_usr_id = '" + Convert.ToString(Context.Session["usr_id"]) + "' and fk_rel_uri = '" + fk_rel_uri + "' AND isdeleted = false;";
                    mRatingId = Utility.DBExecuteScalar(strQuery);
                }
                else
                {
                    strQuery = "INSERT INTO rating (fk_usr_id, fk_rating_scale_id, fk_rel_uri) VALUES ('" + Convert.ToString(Context.Session["usr_id"]) + "','" + fk_rating_scale_id + "','" + fk_rel_uri + "');SELECT currval('rating_id_seq');";
                    mRatingId = Utility.DBExecuteScalar(strQuery);
                }
                strQuery = "SELECT (SELECT COUNT(id) FROM rating WHERE fk_rating_scale_id = 2 AND fk_rel_uri = '" + fk_rel_uri + "' and isdeleted=false) AS likes, (SELECT COUNT(id) FROM rating WHERE fk_rating_scale_id = 1 AND fk_rel_uri = '" + fk_rel_uri + "' and isdeleted=false) AS dislikes";
                DataTable dtab = Utility.DBExecuteDataTable(strQuery);
                if (dtab.Rows.Count > 0)
                {
                    strRV = Utility.DataTableToDictionary(dtab);
                }
                else
                {
                    strRV = Utility.GetMsg(Utility.msgStatusCodes.QueryDidNotReturnRecords, "No comment was returned");
                }
                return strRV;
            }
            catch (Exception ex)
            {
                return Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message);
            }
        }
        /// <summary>
        /// Deletes a rating assigned to a resource identified by its URI
        /// </summary>
        /// <param name="fk_rel_uri">URI of resource for which rating should be deleted</param>
        /// <returns>OperationCompletedSuccessfully (code=2) on success</returns>
        /// <remarks>Method requires user to be authenticated. Only one rating is permitted per resource per user.</remarks>
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string DeleteRatingForUri(string fk_rel_uri)
        {
            string strRV = string.Empty;
            if (Context.Session["isAuthenticated"] == null)
            {
                return Utility.GetMsg(Utility.msgStatusCodes.OperationRequiresAuthentication);
            }
            try
            {
                string strQuery = "UPDATE rating SET isdeleted=true WHERE fk_rel_uri = '" + fk_rel_uri + "' AND fk_usr_id = '" + Convert.ToString(Context.Session["usr_id"]) + "';";
                Utility.DBExecuteNonQuery(strQuery);
                return Utility.GetMsg(Utility.msgStatusCodes.OperationCompletedSuccessfully, "Rating removed successfully");
            }
            catch (Exception ex)
            {
                return Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message);
            }
        }
        #endregion
    }
}
