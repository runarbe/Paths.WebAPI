using System;
using System.ComponentModel;
using System.Data;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;
using euPATHS.AppCode;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
namespace euPATHS
{

    /// <summary>
    /// The topic web service provides functions to interact with the PATHS topic hierarchy.
    /// </summary>
    [System.Web.Script.Services.ScriptService()]
    [System.Web.Services.WebService(Namespace = "http://paths-project.eu/")]
    [System.Web.Services.WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    public class Topic : System.Web.Services.WebService
    {
        //JavaScriptSerializer json = new JavaScriptSerializer();

        #region ----------- Web Service code for topic.get() ----------------
        /// <summary>
        /// Method to return a nested set of topics
        /// </summary>
        /// <param name="paths_identifiers">a comma separated list of topic identifiers</param>
        /// <returns>JSON::NestedTopics</returns>
        [WebMethod(EnableSession = true, Description = "get nested topics by identifiers")]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Get(string paths_identifiers)
        {
            var mTimer = new Stopwatch();
            mTimer.Start();

            try
            {
                JavaScriptSerializer js = new JavaScriptSerializer();
                StringBuilder strQuery = new StringBuilder();

                string[] tmp_paths_identifiers = paths_identifiers.Split(',');

                strQuery.Append("SELECT DISTINCT termID, term, parents, getTopicJson(termID)::varchar as json FROM getTopicsWithChildren(array['" + string.Join("','", tmp_paths_identifiers) + "']) as m ORDER BY term ASC;");
                // Utility.DebugToFile(strQuery.ToString());
                DataTable myDt = Utility.DBExecuteDataTable(strQuery.ToString());

                m_topic tmpTopic = null;
                List<m_topic> topicList = new List<m_topic>();
                List<string> mAllTopicIdsInSelection = new List<string>();
                m_topic mRootTopic = null;

                //Create list for processed topic ids
                List<m_topic> mProcessedTopics = new List<m_topic>();

                //Load all topics
                foreach (DataRow dr in myDt.Rows)
                {
                    // Create object from db json
                    tmpTopic = js.Deserialize<m_topic>((string)dr["json"]);

                    // Split parent fields into strings
                    string tmpParents = (string)dr["parents"];
                    tmpTopic.parents = new List<string>(tmpParents.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));

                    // Add id of topic to a list of all ids in set
                    mAllTopicIdsInSelection.Add(tmpTopic.paths_identifier);

                    // If no parent exists, assume that the node is the root
                    if (tmpParents.Trim() == "")
                    {
                        mRootTopic = tmpTopic;
                        mProcessedTopics.Add(tmpTopic);
                    }
                    // Otherwise add to list of topics
                    else
                    {
                        topicList.Add(tmpTopic);
                    }
                }

                // Define a counter and a limit for number of iterations to avoid infinity loops
                int iCtr = 0;
                int iLimit = 100000;

                // Loop through all topics until there is only one root item in the tree
                // or until the limit is reached
                while (topicList.Count > 0 && iCtr < iLimit)
                {
                    // For each topic that has already been processed
                    for (int h = 0; h < mProcessedTopics.Count; h++)
                    {
                        m_topic mOuterTopic = mProcessedTopics[h];

                        // Compare it to all other topics
                        for (int i = 0; i < topicList.Count; i++)
                        {
                            m_topic mInnerTopic = topicList[i];

                            // Compare the parent IDs of all other topics to the id of processed topic
                            for (int j = 0; j < mInnerTopic.parents.Count; j++)
                            {
                                string mInnerTopicParent = mInnerTopic.parents[j];
                                // Check for obsolete parent ids
                                if (!mAllTopicIdsInSelection.Contains(mInnerTopicParent))
                                {
                                    mInnerTopic.parents.Remove(mInnerTopicParent);
                                }
                                //Check for parent-child matches
                                else if (mOuterTopic.paths_identifier == mInnerTopicParent)
                                {
                                    if (mOuterTopic.CheckIfTopicIdentifierIsDirectChild(mInnerTopic.paths_identifier))
                                    {
                                        mOuterTopic.paths_children.Add(mInnerTopic);
                                    }
                                    mProcessedTopics.Add(mInnerTopic);
                                    mInnerTopic.parents.Remove(mInnerTopicParent);
                                    if (mInnerTopic.parents.Count == 0)
                                    {
                                        topicList.Remove(mInnerTopic);
                                    }
                                }
                            }
                            //Updated On 17 September 2013 for Odering the items
                            mOuterTopic.paths_children.Sort(delegate(m_topic P1, m_topic P2)
                            {
                                return P1.dc_title.CompareTo(P2.dc_title);
                            });
                            //Update End
                        }
                    }
                    iCtr++;
                }
                //Serialize, wrap and return JSON string
                string rV;
                if (topicList.Count == 0)
                {
                    rV = "{\"code\":2,\"data\":" + js.Serialize(mRootTopic) + "}";
                    //rV = "{\"code\":2,\"data\":" + js.Serialize(mRootTopic) + ",\"debug\":"+js.Serialize(topicList)+",\"dbc\":"+iCtr+"}"; // Debug                      
                }
                else
                {
                    var mMsg = Utility.GetMsg(Utility.msgStatusCodes.OperationFailed, "The input branch of topics contains sub-topics that cannot be referenced to a parent in the selection set");
                    Utility.LogRequest(mMsg, true, mTimer);
                    return mMsg;
                }
                Utility.LogRequest(rV, false, mTimer);
                return rV;
            }
            catch (Exception ex)
            {
                var mMsg = Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message + ex.Source + ex.StackTrace + ex.TargetSite);
                Utility.LogRequest(mMsg, true, mTimer);
                return mMsg;
            }
        }
        #endregion

        #region ----------- Web Service code for #245 topic.branch.bbox() ----------------
        /// <summary>
        /// Returns all the topic branches within a specific bounding box
        /// </summary>
        /// <param name="bbox">A comma separated string of coordinates minx,miny,maxx,maxy</param>
        /// <returns>JSON::NestedTopics</returns>
        [WebMethod(EnableSession = true, Description = "get topics by bbox")]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string BranchBbox(string bbox)
        {
            var mTimer = new Stopwatch();
            mTimer.Start();

            try
            {
                JavaScriptSerializer js = new JavaScriptSerializer();
                StringBuilder strQuery = new StringBuilder();
                List<string> myBbox = new List<string>(bbox.Split(','));

                strQuery.Append("SELECT m.topic_id FROM getTopicsBbox(" + bbox + ") as m;");

                DataTable myDt = Utility.DBExecuteDataTable(strQuery.ToString());
                if (myDt.Rows.Count > 0)
                {
                    List<string> myValues = new List<string>();

                    foreach (DataRow dr in myDt.Rows)
                    {
                        myValues.Add((string)dr["topic_id"]);
                    }
                    //Serialize, wrap and return JSON string
                    string rV = "{\"code\":2,\"data\":" + js.Serialize(myValues) + "}";
                    Utility.LogRequest (rV, false, mTimer);
                    return rV;
                }
                else
                {
                    var mMsg = Utility.GetMsg(Utility.msgStatusCodes.QueryDidNotReturnRecords);
                    Utility.LogRequest(mMsg, false, mTimer);
                    return mMsg;
                }

            }
            catch (Exception ex)
            {
                var mMsg = Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message + ex.Source + ex.StackTrace + ex.TargetSite);
                Utility.LogRequest(mMsg, true, mTimer);
                return mMsg;
            }
        }

        #endregion

        #region ----------- Web Service code for topic.branches () ----------------

        /// <summary>
        /// Returns all possible paths to the root topic for a given topic
        /// </summary>
        /// <param name="paths_identifier">The identifier of a topic</param>
        /// <returns>JSON:NestedTopics</returns>
        [WebMethod(EnableSession = true, Description = "get topic branches for topicid")]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Branches(string paths_identifier)
        {
            var mTimer = new Stopwatch();
            mTimer.Start();

            try
            {
                JavaScriptSerializer js = new JavaScriptSerializer();
                StringBuilder strQuery = new StringBuilder();

                strQuery.Append("SELECT getTopicJson(termID) AS json, level FROM getTopicBranchesToRoot('" + paths_identifier + "') AS m");
                Utility.DebugToFile(strQuery.ToString());
                DataTable myDt = Utility.DBExecuteDataTable(strQuery.ToString());
                List<string> myRetList = new List<string>();
                //m_topic tmpParentTopic = null;
                List<m_topic> tmpPathList = new List<m_topic>();

                m_topic tmpChildTopic = null;
                m_topic theLeafTopic = null;
                List<m_topic> theLeafTopicChildren = new List<m_topic>();
                List<List<m_topic>> mListOfReturnTopicPaths = new List<List<m_topic>>();

                int previousTopicLevel = -999;
                if (myDt.Rows.Count >= 1)
                {
                    foreach (DataRow dr in myDt.Rows)
                    {
                        int currentTopicLevel = (int)dr["level"];

                        //if first branch or new leaf on any other branch
                        if (currentTopicLevel < 0 && currentTopicLevel > previousTopicLevel)
                        {

                            tmpPathList.Add(js.Deserialize<m_topic>((string)dr["json"]));

                        }
                        else if (currentTopicLevel < 0 && currentTopicLevel < previousTopicLevel)
                        //if new branch
                        {
                            mListOfReturnTopicPaths.Add(tmpPathList);
                            tmpPathList = new List<m_topic>();
                            tmpPathList.Add(js.Deserialize<m_topic>((string)dr["json"]));
                            previousTopicLevel = -9999;
                        }
                        else if (currentTopicLevel == 0)
                        //if leaf topic
                        {
                            theLeafTopic = js.Deserialize<m_topic>((string)dr["json"]);
                        }
                        else
                        //if children of leaf topic
                        {
                            //theLeafTopicChildren.Add(js.Deserialize<m_topic>((string)dr["json"]));
                        }

                        //Assign value for previous and curren topic level
                        previousTopicLevel = currentTopicLevel;

                    }

                    // If there is only one path to root, this must be run to add it
                    if (tmpPathList.Count > 0 && !mListOfReturnTopicPaths.Contains(tmpPathList))
                    {
                        mListOfReturnTopicPaths.Add(tmpPathList);
                    }


                    // If the leaf topic is set
                    if (theLeafTopic != null)
                    {

                        // Add code to add children to leaf topic here
                        // theLeafTopic.paths_children = theLeafTopicChildren; // Probably not necessary

                        //Loop through all branches and add leaf topic to end
                        for (int i = 0; i < mListOfReturnTopicPaths.Count; i++)
                        {
                            mListOfReturnTopicPaths[i].Add(theLeafTopic);
                            mListOfReturnTopicPaths[i].Reverse();
                        }
                        ///******Updated on 11 September 2013 for Bug 261 ******///
                        if (mListOfReturnTopicPaths.Count == 0)
                        {
                            tmpPathList.Add(js.Deserialize<m_topic>((string)myDt.Rows[0]["json"]));
                            mListOfReturnTopicPaths.Add(tmpPathList);
                        }
                        ///******End Updated on 11 September 2013 for Bug 261 ******///
                    }

                    //Serialize, wrap and return JSON string
                    string rV = "{\"code\":2,\"data\":" + js.Serialize(mListOfReturnTopicPaths) + "}";
                    Utility.LogRequest(rV, false, mTimer);
                    return rV;
                }
                else
                {
                    var mMsg = Utility.GetMsg(Utility.msgStatusCodes.QueryDidNotReturnRecords);
                    Utility.LogRequest(mMsg, false, mTimer);
                    return mMsg;
                }

            }
            catch (Exception ex)
            {
                var mMsg = Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message + ex.Source + ex.StackTrace + ex.TargetSite);
                Utility.LogRequest(mMsg, true, mTimer);
                return mMsg;
            }
        }

        #endregion

        #region ----------- Web Service code for old version of topic.branches () ----------------

        [WebMethod(EnableSession = true, Description = "get topic branches for topicid")]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string BranchesOld(string paths_identifier)
        {
            try
            {
                JavaScriptSerializer js = new JavaScriptSerializer();
                StringBuilder strQuery = new StringBuilder();

                strQuery.Append("select getTopicJson(termID) as json, level from getTopicBranchesToRoot('" + paths_identifier + "') as m");
                Utility.DebugToFile(strQuery.ToString());
                DataTable myDt = Utility.DBExecuteDataTable(strQuery.ToString());
                List<string> myRetList = new List<string>();
                m_topic tmpParentTopic = null;
                m_topic tmpChildTopic = null;
                m_topic theLeafTopic = null;
                List<m_topic> theLeafTopicChildren = new List<m_topic>();
                List<m_topic> myListOfReturnTopics = new List<m_topic>();

                int previousTopicLevel = -999;
                if (myDt.Rows.Count >= 1)
                {
                    foreach (DataRow dr in myDt.Rows)
                    {
                        int currentTopicLevel = (int)dr["level"];

                        //if first branch or new leaf on any other branch
                        if (currentTopicLevel < 0 && currentTopicLevel > previousTopicLevel)
                        {

                            if (tmpParentTopic == null)
                            {
                                tmpParentTopic = js.Deserialize<m_topic>((string)dr["json"]);
                            }
                            else
                            {
                                tmpChildTopic = js.Deserialize<m_topic>((string)dr["json"]);
                                tmpParentTopic.AddLeaf(tmpChildTopic);
                            }

                        }
                        else if (currentTopicLevel < 0 && currentTopicLevel < previousTopicLevel)
                        //if new branch
                        {
                            myListOfReturnTopics.Add(tmpParentTopic);
                            tmpParentTopic = js.Deserialize<m_topic>((string)dr["json"]);
                            previousTopicLevel = -9999;
                        }
                        else if (currentTopicLevel == 0)
                        //if leaf topic
                        {
                            theLeafTopic = js.Deserialize<m_topic>((string)dr["json"]);
                        }
                        else
                        //if children of leaf topic
                        {
                            theLeafTopicChildren.Add(js.Deserialize<m_topic>((string)dr["json"]));
                        }

                        //Assign value for previous and curren topic level
                        previousTopicLevel = currentTopicLevel;

                    }

                    // If there is only one path to root, this must be run to add it
                    if (!myListOfReturnTopics.Contains(tmpParentTopic))
                    {
                        myListOfReturnTopics.Add(tmpParentTopic);
                    }


                    // If the leaf topic is set
                    if (theLeafTopic != null)
                    {

                        // Add code to add children to leaf topic here
                        // theLeafTopic.paths_children = theLeafTopicChildren; // Probably not necessary

                        //Loop through all branches and add leaf topic to end
                        for (int i = 0; i < myListOfReturnTopics.Count; i++)
                        {
                            myListOfReturnTopics[i].AddLeaf(theLeafTopic);
                        }

                    }

                    //Serialize, wrap and return JSON string
                    string rV = "{\"code\":2,\"data\":" + js.Serialize(myListOfReturnTopics) + "}";
                    return rV;
                }
                else
                {
                    return Utility.GetMsg(Utility.msgStatusCodes.QueryDidNotReturnRecords);
                }

            }
            catch (Exception ex)
            {
                return Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message + ex.Source + ex.StackTrace + ex.TargetSite);
            }
        }
        #endregion
    }
}