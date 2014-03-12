using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
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

    #region ----------- Classes needed by topic.get (#228) ----------------
    public class m_paths_counts
    {
        public int item;
        public int topics;
        public int paths;
    }

    public class m_topic
    {
        public string paths_identifier;
        public string dc_title;
        public m_paths_counts paths_counts;
        public string paths_bbox;
        public List<m_topic> paths_children = new List<m_topic>();

        [ScriptIgnore]
        public List<string> parent = new List<string>();

        public bool AddLeaf(m_topic myChildTopic)
        {
            m_topic lastLeaf = null;

            if (this.paths_children.Count > 0)
            {
                lastLeaf = this.paths_children[0];

                while (lastLeaf.paths_children.Count > 0)
                {
                    lastLeaf = lastLeaf.paths_children[0];
                }

                lastLeaf.paths_children.Add(myChildTopic);
            }
            else
            {
                this.paths_children.Add(myChildTopic);
            }

            return true;
        }

        public m_topic checkTopicHierarchy(m_topic myTopic, m_topic myTestTopic, ref int myDepth)
        {
            //Increase counter
            myDepth++;

            //If the the test topic is a child of the root topic itself, add and return.
            if (myTestTopic.parent.Contains(myTopic.paths_identifier))
            {
                return myTopic;
            }

            //If the topic has child topics
            else if (myTopic.paths_children.Count > 0)
            {
                // For each of the child topics
                for (int i = 0; i < myTopic.paths_children.Count; i++)
                {
                    // If the topic is a match, return the topic
                    if (myTestTopic.parent.Contains(myTopic.paths_children[i].paths_identifier))
                    {
                        return myTopic.paths_children[i];
                    }
                    else
                    {
                        if (myDepth < 100000)
                        {
                            return checkTopicHierarchy(myTopic.paths_children[i], myTestTopic, ref myDepth);
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                return null;
            }
            else
            {
                return null;
            }

        }

        public bool AddChildIfParentInTree(m_topic ChildToAdd)
        {
            int count = 0;
            m_topic hitLeaf = this.checkTopicHierarchy(this, ChildToAdd, ref count);

            if (hitLeaf != null)
            {
                if (!hitLeaf.paths_children.Contains(ChildToAdd))
                {
                    hitLeaf.paths_children.Add(ChildToAdd);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

        }


    }

    #endregion

    /// <summary>
    /// Test web service used for development of PATHS web services
    /// </summary>
    [System.Web.Script.Services.ScriptService()]
    [System.Web.Services.WebService(Namespace = "http://paths-project.eu/")]
    [System.Web.Services.WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    public class stc : System.Web.Services.WebService
    {

        #region ----------- Web Service code for topic.get() ----------------
        [WebMethod(EnableSession = true, Description = "get nested topics by identifiers")]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Get(string paths_identifiers)
        {
            try
            {
                JavaScriptSerializer js = new JavaScriptSerializer();
                StringBuilder strQuery = new StringBuilder();

                string[] tmp_paths_identifiers = paths_identifiers.Split(',');

                strQuery.Append("SELECT DISTINCT termID, term, parents, getTopicJson(termID)::varchar as json FROM getTopicsWithChildren(array['" + string.Join("','", tmp_paths_identifiers) + "']) as m;");
                Utility.DebugToFile(strQuery.ToString());
                DataTable myDt = Utility.DBExecuteDataTable(strQuery.ToString());

                m_topic tmpTopic = null;
                List<m_topic> topicList = new List<m_topic>();

                //Load all topics
                foreach (DataRow dr in myDt.Rows)
                {
                    tmpTopic = js.Deserialize<m_topic>((string)dr["json"]);
                    string tmpParents = (string)dr["parents"];
                    tmpTopic.parent = new List<string>(tmpParents.Split(','));
                    topicList.Add(tmpTopic);
                }

                // Define a limit for number of iterations to avoid infinity loops
                int iCtr = 0;
                int iLimit = 10000;

                // Loop through all topics until there is only one root item in the tree
                // or until the limit is reached
                while (topicList.Count > 1 && iCtr < iLimit)
                {
                    // For each topic (1)
                    for (int i = 0; i < topicList.Count; i++)
                    {
                        m_topic tmp1 = topicList[i];

                        // Check against all other topics (2)
                        for (int j = 0; j < topicList.Count; j++)
                        {
                            m_topic tmp2 = topicList[j];

                            // If topic (2) has the identifier of topic (1) in the parent field,
                            // add topic (2) to children of topic (1) and continue
                            if (tmp1.AddChildIfParentInTree(tmp2))
                            {
                                topicList.Remove(tmp2);
                            }
                        }

                    }
                    iCtr++;
                }

                //Serialize, wrap and return JSON string
                string rV = "{\"code\":2,\"data\":" + js.Serialize(topicList) + "}";
                return rV;

            }
            catch (Exception ex)
            {
                return Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message + ex.Source + ex.StackTrace + ex.TargetSite);
            }

        }
        #endregion

        #region ----------- Web Service code for #245 topic.branch.bbox() ----------------
        /// <summary>
        /// Second attempt
        /// </summary>
        /// <param name="paths_identifier"></param>
        /// <returns></returns>
        [WebMethod(EnableSession = true, Description = "get topics by bbox")]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string BranchBbox(string bbox)
        {
            try
            {
                JavaScriptSerializer js = new JavaScriptSerializer();
                StringBuilder strQuery = new StringBuilder();
                List<string> myBbox = new List<string>(bbox.Split(','));

                strQuery.Append("SELECT m.topic_id FROM getTopicsBbox("+bbox+") as m;");

                DataTable myDt = Utility.DBExecuteDataTable(strQuery.ToString());
                if (myDt.Rows.Count > 0) {
                    List<string> myValues = new List<string>();

                    foreach (DataRow dr in myDt.Rows) {
                        myValues.Add((string)dr["topic_id"]);
                    }
                    //Serialize, wrap and return JSON string
                    string rV = "{\"code\":2,\"data\":" + js.Serialize(myValues) + "}";
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


        #region ----------- Web Service code for topic.branches () ----------------

        /// <summary>
        /// Second attempt
        /// </summary>
        /// <param name="paths_identifier"></param>
        /// <returns></returns>
        [WebMethod(EnableSession = true, Description = "get topic branches for topicid")]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Branches(string paths_identifier)
        {
            try
            {
                JavaScriptSerializer js = new JavaScriptSerializer();
                StringBuilder strQuery = new StringBuilder();

                strQuery.Append("select getTopicJson(termID) as json, level from getTopicBranchesToRoot('" + paths_identifier + "') as m");
                //Utility.DebugToFile(strQuery.ToString());
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

                    //If the leaf topic is set
                    if (theLeafTopic != null)
                    {

                        //Add code to add children to leaf topic here
                        //To be done...

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

