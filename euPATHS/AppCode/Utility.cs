using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using Microsoft.Practices.ServiceLocation;
using System.Data.Odbc;
using System.Configuration;
using SolrNet;
using System.Diagnostics;

namespace euPATHS.AppCode
{
    /// <summary>
    /// The Utility class will have all the common functions.
    /// </summary>
    public sealed class Utility
    {
        #region -------------------Properties----------------------------
        //Enable debug mode true/false - should be false by default when in production
        public static bool debugState = Convert.ToBoolean(ConfigurationManager.AppSettings["Debug"].ToLower());
        public static string FieldSetItem = "item.id, item.uri paths_identifier, item.dc_title, item.dc_creator, item.dc_subject, item.dc_description, item.dc_publisher, item.dc_contributor, item.dc_date, item.dc_type, item.dc_format, item.dc_identifier, item.dc_source, item.dc_language, item.dc_relation, item.dc_rights, item.dc_coverage, item.dcterms_provenance, item.dcterms_ispartof, item.paths_bow, dcterms_temporal, dcterms_spatial, europeana_unstored, europeana_object, europeana_provider, europeana_type, europeana_rights, europeana_dataprovider, europeana_isshownby, europeana_isshownat, europeana_country, europeana_language, europeana_uri, europeana_usertag, europeana_year, europeana_previewnodistribute, europeana_hasobject";
        public static string FieldSetPath = "path.id, path.uri, path.fk_usr_id, path.dc_title, path.dc_subject, path.dc_description, path.dc_rights, path.access, path.lom_audience, path.lom_length, path.tstamp";
        public static string solrInstance = "http://localhost:8080/solr/paths2";
        //public static string solrInstance = "http://localhost:8080/solr/europeana";
        public static string[] pathFields = { "paths_identifier", "dc_creator", "dc_title", "dc_description", "dc_subject", "paths_thumbnail", "paths_topics" };
        public static string[] nodeFields = { "paths_identifier", "dc_title", "dc_description" };
        #endregion

        #region ---- String funct ---

        public static string escapeNewLine(string pStr)
        {
            pStr = pStr.Replace("\r\n", "\\n");
            pStr = pStr.Replace("\r", "\\n");
            pStr = pStr.Replace("\n", "\\n");
            return pStr;
        }

        #endregion

        #region -------------------Debugging and logging-----------------

        /// <summary>
        /// Log the error message to the text file.
        /// </summary>
        /// <param name="pReturnData">The Error msg to log.</param>
        public static void DebugToFile(string pErrorMsg, bool pIsError = false, Stopwatch pStopwatch = null)
        {
            HttpRequest pRequest = HttpContext.Current.Request;

            string mUserID = "-1";
            if (HttpContext.Current.Session["usr_id"] != null)
            {
                mUserID = HttpContext.Current.Session["usr_id"].ToString();
            }

            // Have to find a way to store this in a dynamic location
            string mLogFile = pRequest.PhysicalApplicationPath + "\\debug.txt";

            // If the file is larger than a certain mReturnDataSizeBytes in bytes, overwrite it.
            bool mAppendToExisting = true;
            if (File.Exists(mLogFile))
            {
                if (new FileInfo(mLogFile).Length >= 3146000)
                {
                    mAppendToExisting = false;
                }
            }

            StreamWriter sw = new StreamWriter(mLogFile, mAppendToExisting, Encoding.ASCII);

            sw.Write(DateTime.Now.ToString() + "\t");
            sw.Write("usr:" + mUserID + "\t");
            sw.Write("url:" + pRequest.RawUrl.ToString() + "\t");
            sw.Write("src:" + pRequest.UserHostAddress.ToString() + "\t");
            if (pStopwatch != null)
            {
                sw.Write("time:" + pStopwatch.ElapsedMilliseconds.ToString() + "ms\r\n");
            }

            if (pIsError == true)
            {
                sw.Write("get: " + pRequest.QueryString.ToString() + "\r\n");
                sw.Write("post: " + pRequest.Form.ToString() + "\r\n");
                sw.Write("msg: " + pErrorMsg + "\r\n");
            }

            sw.Write("\r\n");
            sw.Close();
        }

        /// <summary>
        /// Log requests to a text file.
        /// </summary>
        /// <param name="pReturnData">The Error msg to log.</param>
        public static void LogRequest(string pReturnData, bool pIsError = false, Stopwatch pTimer = null)
        {
            HttpRequest pRequest = HttpContext.Current.Request;
            double mReturnDataSizeBytes = (double)System.Text.UnicodeEncoding.Unicode.GetByteCount(pReturnData);
            string mSizeString;

            if (mReturnDataSizeBytes > 1024)
            {
                mSizeString = String.Format("{0:0.00} Kb", mReturnDataSizeBytes / 1024);
            }
            else
            {
                mSizeString = String.Format("{0:0} bytes", mReturnDataSizeBytes);
            }

            long mTime = -1;
            if (pTimer != null)
            {
                mTime = pTimer.ElapsedMilliseconds;
            }
            
            // Get user ID
            string mUserID = "-1";
            if (HttpContext.Current.Session["usr_id"] != null)
            {
                mUserID = HttpContext.Current.Session["usr_id"].ToString();
            }

            // Have to find a way to store this in a dynamic location
            string mLogFile = pRequest.PhysicalApplicationPath + "request-log.txt";

            // If the file is larger than a certain mReturnDataSizeBytes in bytes, overwrite it.
            bool mAppendToExisting = true;
            if (File.Exists(mLogFile))
            {
                if (new FileInfo(mLogFile).Length > 262144)
                {
                    mAppendToExisting = false;
                }
            }

            StreamWriter sw = new StreamWriter(mLogFile, mAppendToExisting, Encoding.ASCII);

            sw.Write(DateTime.Now.ToString() + "\t");
            sw.Write("usr:" + mUserID + "\t");
            sw.Write("stat:" + pIsError.ToString() + "\t");
            sw.Write("url:" + pRequest.RawUrl.ToString() + "\t");
            sw.Write("src:" + pRequest.UserHostAddress.ToString() + "\t");
            sw.Write("time:" + mTime.ToString() + "ms\t");
            sw.Write("size:" + mSizeString  + "\r\n");

            //if (pIsError == true || mTime > 1000)
           if (true)
            {
                sw.Write("get: " + pRequest.QueryString.ToString() + "\r\n");
                sw.Write("post: " + pRequest.Form.ToString() + "\r\n");
                sw.Write("msg: " + pReturnData + "\r\n");
            }

            sw.Write("\r\n");
            sw.Close();
        }

        #endregion

        #region -------------------Database and JSON---------------------

        public static string CheckNull(object myObject)
        {
            if (object.ReferenceEquals(myObject, null))
            {
                return "";
            }
            else
            {
                myObject = Convert.ToString(myObject).Trim();
                return (string)myObject;
            }
        }

        public static string DataTableToDictionary(DataTable myDataTable, bool omitArray = false, Dictionary<string, DataTable> myAdditionalTables = null, bool myReturnJSON = true, string pSource="")
        {
            string strDataTableToDictionary = string.Empty;
            Dictionary<string, object> myDictionary = new Dictionary<string, object>();
            List<object> myList = new List<object>();
            Dictionary<string, object> myRow = null;
            DataColumnCollection myColumnNames = myDataTable.Columns;
            int responseNumber = 1;
            foreach (DataRow myDataRow in myDataTable.Rows)
            {
                myRow = new Dictionary<string, object>();

                foreach (DataColumn myColumnName in myColumnNames)
                {
                    //Special handling of dc_subject
                    if (myColumnName.ColumnName == "dc_subject") 
                    {
                        if (pSource == "item")
                        {
                            string myTmpValue = CheckNull(myDataRow[myColumnName.ColumnName]);
                            //myTmpValue = "[\""+String.Join("\",\"", myTmpValue.Split(';')) + "\"]";
                            myRow.Add("dc_subject", myTmpValue.Split(';'));
                        }
                        else
                        {
                            string  myTmpValue = CheckNull(myDataRow[myColumnName.ColumnName]);
                            myRow.Add(myColumnName.ColumnName, myTmpValue);
                        }
                    }
                    else if (myColumnName.ColumnName == "dc_title")
                    {
                        string myTmpValue = CheckNull(myDataRow[myColumnName.ColumnName]);
                        if (pSource == "item")
                        {
                            if (myTmpValue.Trim() == "")
                            {
                                myRow.Add("dc_title", "Untitled");
                            }
                            else
                            {
                                myRow.Add(myColumnName.ColumnName, myTmpValue);
                            }
                        }
                        else
                        {
                            myRow.Add(myColumnName.ColumnName, myTmpValue);
                        }
                    }
                    else if (myColumnName.ColumnName == "count")
                    {
                        object myTmpValue = CheckNull(myDataRow[myColumnName.ColumnName]);
                        myTmpValue = Convert.ToInt32(myTmpValue);
                        myRow.Add(myColumnName.ColumnName, myTmpValue);
                        //Special handling of ratings
                    }
                    else if (myColumnName.ColumnName == "_likes" | myColumnName.ColumnName == "_dislikes")
                    {
                        object myTmpValue = CheckNull(myDataRow[myColumnName.ColumnName]);
                        if (!myRow.ContainsKey("paths_rating"))
                        {
                            myRow.Add("paths_rating", new Dictionary<string, object>());
                        }
                        //((Dictionary<string, object>)myRow["paths_rating"]).Add(Strings.Mid(myColumnName.ColumnName, 2), myTmpValue);
                    }
                    else
                    {
                        myRow.Add(myColumnName.ColumnName, CheckNull(myDataRow[myColumnName.ColumnName]));
                    }
                }
                myList.Add(myRow);
                responseNumber = responseNumber + 1;
            }

            myDictionary.Add("code", msgStatusCodes.OperationCompletedSuccessfully);

            if ((omitArray == true & myAdditionalTables != null))
            {
                Dictionary<string, object> myRow2 = (Dictionary<string, object>)myList[0];
                foreach (KeyValuePair<string, DataTable> myRecord in myAdditionalTables)
                {
                    myRow2.Add(myRecord.Key, DataTableToList(myRecord.Value));
                }
            }
            if ((omitArray == true & myList.Count == 1))
            {
                myDictionary.Add("data", myList[0]);
            }
            else
            {
                myDictionary.Add("data", myList);
            }
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            strDataTableToDictionary = serializer.Serialize(myDictionary);
            if (debugState)
            {
                DebugToFile(strDataTableToDictionary);
            }
            return strDataTableToDictionary;
        }

        public static List<object> DataTableToList(DataTable myDataTable)
        {
            Dictionary<string, object> myDictionary = new Dictionary<string, object>();
            List<object> myList = new List<object>();
            Dictionary<string, object> myRow = null;
            DataColumnCollection myColumnNames = myDataTable.Columns;
            int responseNumber = 1;
            foreach (DataRow myDataRow in myDataTable.Rows)
            {
                myRow = new Dictionary<string, object>();
                foreach (DataColumn myColumnName in myColumnNames)
                {
                    //Special handling of dc_subject
                    if (myColumnName.ColumnName == "dc_subject")
                    {
                        object myTmpValue = CheckNull(myDataRow[myColumnName.ColumnName]);
                        myTmpValue = myTmpValue.ToString();//myTmpValue.Split(";");
                        myRow.Add(myColumnName.ColumnName, myTmpValue);
                        //Special handling of ratings needs debugging!!!
                    }
                    else if (myColumnName.ColumnName == "_likes" | myColumnName.ColumnName == "_dislikes")
                    {
                        object myTmpValue = CheckNull(myDataRow[myColumnName.ColumnName]);
                        string myCName = myColumnName.ColumnName.Substring(0, 2);
                        if (!myRow.ContainsKey("paths_rating"))
                        {
                            myRow.Add("paths_rating", new Dictionary<string, object>());
                        }
                        ((Dictionary<string, object>)myRow["paths_rating"]).Add(myCName, myTmpValue);
                    }
                    else
                    {
                        myRow.Add(myColumnName.ColumnName, CheckNull(myDataRow[myColumnName.ColumnName]));
                    }
                }
                myList.Add(myRow);
                responseNumber = responseNumber + 1;
            }
            return myList;
        }
        public static string DataTablesToDictionaryMasterDetail(DataTable pMasterDataTable, DataTable pDetailDataTable)
        {
            string strRV = string.Empty;
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            try
            {
                Dictionary<string, object> mMasterDict = serializer.Deserialize<Dictionary<string, object>>(DataTableToDictionary(pMasterDataTable));
                Dictionary<string, object> mDetailDict = serializer.Deserialize<Dictionary<string, object>>(DataTableToDictionary(pDetailDataTable));
                //foreach (Dictionary<string, object> path in mMasterDict["data"])
                //{
                //    List<Dictionary<string, object>> mNodeList = new List<Dictionary<string, object>>();
                //    foreach (Dictionary<string, object> node in mDetailDict["data"])
                //    {
                //        if (node["fk_path_id"] == path["id"])
                //        {
                //            mNodeList.Add(node);
                //        }
                //    }
                //    //For each node
                //    path.Add("nodes", mNodeList);
                //}
                //For each path
                Dictionary<string, object> mReturnValue = new Dictionary<string, object>();
                mReturnValue.Add("code", msgStatusCodes.OperationCompletedSuccessfully);
                mReturnValue.Add("data", mMasterDict["data"]);

                strRV = serializer.Serialize(mReturnValue);
                if (debugState)
                {
                    DebugToFile(strRV);
                }
                return strRV;
            }
            catch (Exception ex)
            {
                strRV = GetMsg(msgStatusCodes.OperationFailed, ex.Message);
                if (debugState)
                {
                    DebugToFile(strRV);
                }
                return strRV;
            }
        }

        #endregion

        #region -------------------Error messages------------------------
        ///<summary>
        ///Enumeration containing status message codes.
        ///</summary> 
        ///<remarks></remarks>
        public enum msgStatusCodes : int
        {
            NoSuchUser = -1,
            AuthenticationFailed = 1,
            OperationCompletedSuccessfully = 2,
            OperationFailed = 3,
            AuthenticationSucceeded = 4,
            OperationRequiresAuthentication = 5,
            LogoutSuccess = 6,
            DatabaseSQLError = 7,
            QueryDidNotReturnRecords = 8,
            FailedToCreateTemporaryUser = 9,
            SpecifiedObjectDoesNotExist = 10,
            EmailAlreadyExists = 11,
            NotImplementedYet = 99,
            Unauthorized = 401,
            NotFound = 404

        }
        /// <summary>
        /// Utility function to return a JSON object with a status message
        /// </summary>
        /// <param name="MsgCode">Enumeration: msgStatusCodes</param>
        /// <param name="CustomMsg">A custom message which will be returned along with the status code in the extmsg parameter of the JSON object</param>
        /// <returns>JOSN object: {"code":1, msg:'Message', extmsg:'More message'}</returns>
        /// <remarks></remarks>
        public static string GetMsg(msgStatusCodes MsgCode, string CustomMsg = "", Stopwatch pStopwatch = null)
        {
            string strGetMsg = string.Empty;
            Dictionary<int, statusMsg> MsgCodes = new Dictionary<int, statusMsg>();
            MsgCodes.Add(-1, new statusMsg(-1, "No such user"));
            MsgCodes.Add(1, new statusMsg(1, "Authentication failed, wrong username or password"));
            MsgCodes.Add(2, new statusMsg(2, "Operation completed successfully"));
            MsgCodes.Add(3, new statusMsg(3, "Operation failed"));
            MsgCodes.Add(4, new statusMsg(4, "Authentication succeeded, user authenticated"));
            MsgCodes.Add(5, new statusMsg(5, "Operation failed, no user authenticated"));
            MsgCodes.Add(6, new statusMsg(6, "User logged out"));
            MsgCodes.Add(7, new statusMsg(7, "Database/SQL error"));
            MsgCodes.Add(8, new statusMsg(8, "Query returns 0 results"));
            MsgCodes.Add(9, new statusMsg(9, "Failed to create temporary user"));
            MsgCodes.Add(10, new statusMsg(10, "The specified object does not exist"));
            MsgCodes.Add(11, new statusMsg(11, "Email already exists"));
            MsgCodes.Add(99, new statusMsg(99, "Not yet implemented"));
            MsgCodes.Add(401, new statusMsg(401, "User unauthorized"));
            MsgCodes.Add(404, new statusMsg(404, "Not Found"));
            statusMsg myMsg = MsgCodes[Convert.ToInt16(MsgCode)];
            if (!string.IsNullOrEmpty(CustomMsg))
            {
                myMsg.Add("extmsg", CustomMsg);
            }
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            strGetMsg = serializer.Serialize(myMsg).ToString();
            if (debugState)
            {
                DebugToFile(strGetMsg);
            }
            return strGetMsg;
        }
        #endregion

        #region -------------------Send EMail----------------------------

        /// <summary>
        /// Utility function to send a password reminder email.
        /// </summary>
        /// <param name="pEmail">E-mail address</param>
        /// <param name="pPassword">Password</param>
        /// <returns>JSON object:code=2 on success, code = 3 on failure.</returns>
        /// <remarks></remarks>
        public static bool sendEmail(string pEmail, string pPassword)
        {
            string strMailServer = "smtp.gmail.com";
            int intMailServerPort = 587;
            string strMailPassword = "$R3!puha";
            string strMailUser = "runarbe@gmail.com";
            string strFromUser = "runarbe@gmail.com";
            try
            {
                SmtpClient SmtpServer = new SmtpClient();
                MailMessage mail = new MailMessage();
                SmtpServer.Credentials = new System.Net.NetworkCredential(strMailUser, strMailPassword);
                SmtpServer.Port = intMailServerPort;
                SmtpServer.Host = strMailServer;
                SmtpServer.EnableSsl = true;
                mail = new MailMessage();
                mail.From = new MailAddress(strFromUser);
                mail.To.Add(pEmail);
                mail.Subject = "Password reminder from PATHS (http://development.paths-project-eu)";
                mail.Body = "Your password is: " + pPassword;
                SmtpServer.Send(mail);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region -------------------Check Is Numeric----------------------
        /// <summary>
        /// Determines if the value contained in a string variable
        /// is a numeric value
        /// </summary>
        /// <param name="text">text value containing number</param>
        /// <returns>true if text is a number</returns>
        public static bool IsNumeric(string text)
        {
            //Test if string is valid
            return string.IsNullOrEmpty(text) ? false :
                //run regular expression to check if string is number
                    Regex.IsMatch(text, @"^\s*\-?\d+(\.\d+)?\s*$");
        }
        #endregion

        #region -------------------DataBase Methods----------------------
        /// <summary>
        /// Take the Database query as string and return integer.
        /// </summary>
        /// <param name="strQuery">Database query</param>
        /// <returns>Integer</returns>
        public static int DBExecuteScalar(string strQuery)
        {
            int intRtnID;
            OdbcConnection connection;
            OdbcCommand cmd;
            using (connection = new OdbcConnection(DBConnection.DefaultConnection))
            {
                try
                {
                    connection.Open();
                    cmd = new OdbcCommand(strQuery, connection);
                    intRtnID = Convert.ToInt32(cmd.ExecuteScalar());
                    return intRtnID;
                }
                catch (Exception)
                {
                    //return Convert.ToInt16(msgStatusCodes.DatabaseSQLError);
                    throw;
                }
                finally
                {
                    cmd = null;
                    connection.Close();
                    connection = null;
                }
            }
        }
        /// <summary>
        /// Take the Database query as string and return string.
        /// </summary>
        /// <param name="strQuery">Database query</param>
        /// <returns>Integer</returns>
        public static string DBExecuteScalarString(string strQuery)
        {
            string strRtn;
            OdbcConnection connection;
            OdbcCommand cmd;
            using (connection = new OdbcConnection(DBConnection.DefaultConnection))
            {
                try
                {
                    connection.Open();
                    cmd = new OdbcCommand(strQuery, connection);
                    strRtn = Convert.ToString(cmd.ExecuteScalar());
                    return strRtn;
                }
                catch (Exception)
                {
                    //return Convert.ToInt16(msgStatusCodes.DatabaseSQLError);
                    throw;
                }
                finally
                {
                    cmd = null;
                    connection.Close();
                    connection = null;
                }
            }
        }
        /// <summary>
        /// Take the Database query as string and return integer.
        /// </summary>
        /// <param name="strQuery">Database query</param>
        /// <returns>Integer</returns>
        public static double DBExecuteScalarDouble(string strQuery)
        {
            double dbleRtnID;
            OdbcConnection connection;
            OdbcCommand cmd;
            using (connection = new OdbcConnection(DBConnection.DefaultConnection))
            {
                try
                {
                    connection.Open();
                    cmd = new OdbcCommand(strQuery, connection);
                    dbleRtnID = Convert.ToDouble(cmd.ExecuteScalar());
                    return dbleRtnID;
                }
                catch (Exception)
                {
                    return Convert.ToDouble(msgStatusCodes.DatabaseSQLError);
                }
                finally
                {
                    cmd = null;
                    connection.Close();
                    connection = null;
                }
            }
        }
        /// <summary>
        /// Take the Database query as DDL statements.
        /// </summary>
        /// <param name="strQuery">Database query</param>
        /// <returns>Integer</returns>
        public static int DBExecuteNonQuery(string strQuery)
        {
            int intRtnID;
            OdbcConnection connection;
            OdbcCommand cmd;
            using (connection = new OdbcConnection(DBConnection.DefaultConnection))
            {
                try
                {
                    connection.Open();
                    cmd = new OdbcCommand(strQuery, connection);
                    intRtnID = cmd.ExecuteNonQuery();
                    return intRtnID;
                }
                catch (Exception)
                {
                    return Convert.ToInt16(msgStatusCodes.DatabaseSQLError);
                }
                finally
                {
                    cmd = null;
                    connection.Close();
                    connection = null;
                }
            }
        }
        /// <summary>
        /// Get the datatable.
        /// </summary>
        /// <param name="strQuery">Database query</param>
        /// <returns>DataTable</returns>
        public static DataTable DBExecuteDataTable(string strQuery)
        {
            OdbcConnection connection;
            OdbcCommand cmd;
            OdbcDataAdapter adapter;
            DataTable dt = new DataTable();
            using (connection = new OdbcConnection(DBConnection.DefaultConnection))
            {
                try
                {
                    cmd = new OdbcCommand(strQuery, connection);
                    adapter = new OdbcDataAdapter(cmd);
                    adapter.Fill(dt);
                    return dt;
                }
                catch (Exception ex)
                {
                    Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message);
                    return dt;
                }
                finally
                {
                    cmd = null;
                    connection = null;
                }
            }
        }

        #endregion

        #region -------------------Parse Numeric Value-------------------
        /// <summary>
        /// Try to convert string variable to Numeric
        /// </summary>
        /// <param name="text">string value containing number</param>
        /// <returns>integer if text is a number else return 0</returns>
        public static int TryToParseInt(string text)
        {
            int numValue;
            bool parsed = Int32.TryParse(text, out numValue);
            if (parsed)
                return numValue;
            else
                return 0;
        }
        #endregion

        #region -------------------Post to SOLR--------------------------
        /// <summary>
        /// Takes the first row of the resulting datatable and posts it to SOLR
        /// </summary>
        /// <param name="pDataTable"></param>
        /// <param name="pDcType"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool PostDataTableToSOLR(DataTable pDataTable, string pDcType)
        {
            if (pDataTable.Rows.Count == 0)
            {
                return false;
            }
            //Select the proper fieldmap
            string[] pType = pathFields;
            if (pDcType == "path")
            {
                pType = pathFields;
            }
            else if (pDcType == "node")
            {
                pType = nodeFields;
            }

            Dictionary<string, object> mDictionary = new Dictionary<string, object>();
            DataColumnCollection myColumnNames = pDataTable.Columns;
            int responseNumber = 1;
            //For each row in data table
            foreach (DataRow myDataRow in pDataTable.Rows)
            {
                mDictionary = new Dictionary<string, object>();

                //For each column in data table

                foreach (DataColumn mColumn in myColumnNames)
                {
                    //Check if value is empty
                    if (Convert.ToString(myDataRow[mColumn.ColumnName]) != "")
                    {
                        if (!(Array.IndexOf(pType, mColumn.ColumnName) > -1))
                        {
                            //Skip the field if it is not useful...
                            //Special handling of dc_subject
                        }
                        else if (mColumn.ColumnName == "dc_subject")
                        {
                            string myTmpValue = CheckNull(myDataRow[mColumn.ColumnName]);
                            string[] myTmpValueArr = myTmpValue.Split(',');
                            for (int i = 0; i <= myTmpValueArr.Length - 1; i++)
                            {
                                myTmpValueArr[i] = myTmpValueArr[i].Trim().Replace("\"", "");
                            }
                            mDictionary.Add(mColumn.ColumnName, myTmpValueArr);
                        }
                        else if (mColumn.ColumnName == "paths_topics")
                        {
                            string myTmpValue = CheckNull(myDataRow[mColumn.ColumnName]);
                            string[] myTmpValueArr = myTmpValue.Split(',');
                            for (int i = 0; i <= myTmpValueArr.Length - 1; i++)
                            {
                                myTmpValueArr[i] = myTmpValueArr[i].Trim().Replace("\"", "");
                            }
                            mDictionary.Add(mColumn.ColumnName, myTmpValueArr);
                        }
                        //Special handling of count
                        else if (mColumn.ColumnName == "count")
                        {
                            object myTmpValue = CheckNull(myDataRow[mColumn.ColumnName]);
                            myTmpValue = Convert.ToInt32(myTmpValue);
                            mDictionary.Add(mColumn.ColumnName, myTmpValue);
                            //Special handling of ratings
                        }
                        else if (mColumn.ColumnName == "_likes" | mColumn.ColumnName == "_dislikes")
                        {
                            object myTmpValue = CheckNull(myDataRow[mColumn.ColumnName]);
                            if (!mDictionary.ContainsKey("paths_rating"))
                            {
                                mDictionary.Add("paths_rating", new Dictionary<string, object>());
                            }
                            ((Dictionary<string, object>)mDictionary["paths_rating"]).Add(mColumn.ColumnName.Substring(0, 2), myTmpValue);
                        }
                        else
                        {
                            mDictionary.Add(mColumn.ColumnName, CheckNull(myDataRow[mColumn.ColumnName]));
                        }
                    }
                    //If current column value is empty
                }
                break; // TODO: might not be correct. Was : Exit For
            }
            mDictionary.Add("dc_type", pDcType);
            if (debugState)
            {
                DebugToFile("PostDataTableToSOLR: parsing DataTable into Dictionary and forwarding to PostDictionaryToSOLR");
            }
            return PostDictionaryToSOLR(mDictionary);
        }

        public static bool DeleteDocumentFromSOLR(string[] pDocURIs)
        {
            ISolrOperations<Dictionary<string, object>> mSolr = default(ISolrOperations<Dictionary<string, object>>);
            // This method must be improved at some stage to remove the unecessary exception
            try
            {
                mSolr = ServiceLocator.Current.GetInstance<ISolrOperations<Dictionary<string, object>>>();
            }
            catch (Exception)
            {
                Startup.Init<Dictionary<string, object>>(solrInstance);
                mSolr = ServiceLocator.Current.GetInstance<ISolrOperations<Dictionary<string, object>>>();
            }

            SolrNet.ResponseHeader mSolrResponseHeader = default(SolrNet.ResponseHeader);
            try
            {
                foreach (string mDocUri in pDocURIs)
                {
                    mSolrResponseHeader = mSolr.Delete(mDocUri);
                }
                mSolr.Commit();
                if (debugState)
                {
                    DebugToFile("DeleteDocumentFromSOLR: deleted document(s) with URI:" + pDocURIs.ToString());
                }
                return true;
            }
            catch (Exception ex)
            {
                if (debugState)
                {
                    DebugToFile("DeleteDocumentFromSOLR: " + ex.Message);
                }
                return false;
            }
            finally
            {
                mSolr = null;
                mSolrResponseHeader = null;
            }
        }

        public static bool PostDictionaryToSOLR(Dictionary<string, object> pDictionary, Dictionary<string, object> pDictionary2 = null)
        {
            ISolrOperations<Dictionary<string, object>> mSolr = default(ISolrOperations<Dictionary<string, object>>);
            // This method must be improved at some stage to remove the unecessary exception         
            try
            {
                mSolr = ServiceLocator.Current.GetInstance<ISolrOperations<Dictionary<string, object>>>();
            }
            catch (Exception)
            {
                Startup.Init<Dictionary<string, object>>(solrInstance);
                mSolr = ServiceLocator.Current.GetInstance<ISolrOperations<Dictionary<string, object>>>();
            }
            SolrNet.ResponseHeader mSolrResponseHeader = default(SolrNet.ResponseHeader);
            try
            {
                mSolrResponseHeader = mSolr.Add(pDictionary);
                mSolr.Commit();
                if (debugState)
                {
                    DebugToFile("PostDictionaryToSOLR: added document with URI " + pDictionary["paths_identifier"] + " to index");
                }
                return true;
            }
            catch (Exception ex)
            {
                if (debugState)
                {
                    DebugToFile("PostDictionaryToSOLR: " + ex.Message);
                }
                return false;
            }
        }
        #endregion

        #region --------------------TopicHierarchyFromDataTable----------
        public static string TopicHierarchyFromDataTable(DataTable myDT)
        {
            string strRV = string.Empty;
            DataSet myDS = new DataSet();
            myDS.Tables.Add(myDT);
            DataRelation myDR = new DataRelation("parentTopics", myDS.Tables[0].Columns["id"], myDS.Tables[0].Columns["fk_parent_topic_id"]);
            myDS.Relations.Add(myDR);

            Dictionary<string, object> myTopic = new Dictionary<string, object>();
            List<Dictionary<string, object>> myTopics = new List<Dictionary<string, object>>();
            JavaScriptSerializer json = new JavaScriptSerializer();
            GetTopic(myDT.Rows[0], ref myDS, myTopics);
            strRV = json.Serialize(myTopics);
            if (debugState)
            {
                DebugToFile(strRV);
            }
            return strRV;
        }
        public static List<Dictionary<string, object>> GetTopic(DataRow myTableRow, ref DataSet myDataSet, List<Dictionary<string, object>> myTopics)
        {
            Dictionary<string, object> myTopic = new Dictionary<string, object>();
            DataRow myParentTopic = myTableRow.GetParentRow("parentTopics");
            myTopic.Add("id", myTableRow["id"]);
            myTopic.Add("uri", myTableRow["id"]);
            myTopic.Add("fk_parent_topic_id", myTableRow["id"]);
            myTopic.Add("dc_title", myTableRow["dc_title"]);
            myTopic.Add("dc_description", myTableRow["dc_description"]);
            myTopic.Add("dc_subject", myTableRow["dc_subject"]);
            myTopic.Add("topic_hierarchy", myTableRow["topic_hierarchy"]);
            myTopic.Add("topic_thumbnails", myTableRow["topic_thumbnails"]);
            if (myTopics.Count == 0)
            {
                myTopics.Add(myTopic);
            }
            else if (myTopics.Count > 0)
            {
                myTopic.Add("children", myTopics.ToArray());
                myTopics.Clear();
                myTopics.Add(myTopic);
            }
            if (myParentTopic == null)
            {
                return myTopics;
            }
            else
            {
                return GetTopic(myParentTopic, ref myDataSet, myTopics);
            }
        }
        #endregion

        #region --------------------URL Decode and Encode----------------
        /// <summary>
        /// Modified Base64 for URL applications ('base64url' encoding)
        /// </summary>
        /// <param name="input"></param>
        /// <returns>Input byte array converted to a base64ForUrl encoded string</returns>
        public static string ToBase64ForUrlString(byte[] input)
        {
            StringBuilder result = new StringBuilder(Convert.ToBase64String(input).TrimEnd('='));
            result.Replace('+', '-');
            result.Replace('/', '_');
            return result.ToString();
        }
        /// <summary>
        /// Modified Base64 for URL applications ('base64url' encoding)
        /// </summary>
        /// <param name="base64ForUrlInput"></param>
        /// <returns>Input base64ForUrl encoded string as the original byte array</returns>
        public static string FromBase64ForUrlString(string base64ForUrlInput)
        {
            string strDecodeValue = base64ForUrlInput;
            try
            {
                int padChars = (base64ForUrlInput.Length % 4) == 0 ? 0 : (4 - (base64ForUrlInput.Length % 4));
                StringBuilder result = new StringBuilder(base64ForUrlInput, base64ForUrlInput.Length + padChars);
                result.Append(String.Empty.PadRight(padChars, '='));
                result.Replace('-', '+');
                result.Replace('_', '/');
                byte[] bufferDecode = Convert.FromBase64String(result.ToString());
                strDecodeValue = Encoding.UTF8.GetString(bufferDecode, 0, bufferDecode.Length);
            }
            catch (Exception ex)
            {
                if (debugState)
                {
                    DebugToFile("FromBase64ForUrlDecodeError: " + ex.Message);
                }
            }
            return strDecodeValue;
        }
        #endregion
    }
    #region -------------------Class StatusMsg-------------------
    public class statusMsg : Dictionary<string, object>
    {
        public statusMsg(int myKey, string myValue)
        {
            this.Add("code", myKey);
            this.Add("msg", myValue);
        }
    }
    #endregion

    #region -------------------Optional parameters handler-------
    class PathsOptionalParameters : Dictionary<string, string>
    {
        public PathsOptionalParameters(System.Web.HttpRequest pRequest)
        {
            foreach (string mKey in pRequest.Params.Keys)
            {
                this.Add(mKey.Trim(), pRequest[mKey].Trim());
            }
        }

        public bool HasParameter(string pParameterName)
        {
            if (this.ContainsKey(pParameterName))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
    #endregion
}