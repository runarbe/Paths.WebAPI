using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

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
        public List<string> parents = new List<string>();

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
            if (myTestTopic.parents.Contains(myTopic.paths_identifier))
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
                    if (myTestTopic.parents.Contains(myTopic.paths_children[i].paths_identifier))
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

        public bool CheckIfTopicIdentifierIsDirectChild(string TopicIdentifier)
        {
            bool retVal = true;
            for (int i = 0; i < this.paths_children.Count; i++)
            {
                if (this.paths_children[i].paths_identifier == TopicIdentifier)
                {
                    return false;
                }
            }
            return retVal;
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

    #region ----------- Classes needed by Path -----------------

    public class PathNode
    {
        public string dc_title { get; set; }
        public string paths_type { get; set; }
        public string dc_description { get; set; }
        public string dc_source { get; set; }
        public string[] paths_thumbnail { get; set; }
    }
    public class PathUpdate
    {
        public string dc_title { get; set; }
        public string dc_description { get; set; }
    }
    public class PathStart
    {
        public string[] paths_start { get; set; }
    }
    public class PathNodeIdentifier
    {
        public PathNodeIdentifier() { }
        public string paths_identifier { get; set; }
        public string[] paths_next { get; set; }
        public string[] paths_prev { get; set; }
        public string[] paths_start { get; set; }
    }
    #endregion

    #region ----------- Classes needed by item.bbox (#231) ----------------


    /// <summary>
    /// Class to hold a GeoJSON point geometry
    /// </summary>
    public class GeoJsonPointGeometry
    {
        public string type { get; set; }
        public List<string> coordinates { get; set; }
    }

    /// <summary>
    /// Class to hold a GeoJSON feature
    /// </summary>
    public class GeoJsonF : Dictionary<string, Object>
    {
        [ScriptIgnore]
        JavaScriptSerializer js = new JavaScriptSerializer();

        public GeoJsonF(DataTable myDt, DataRow myDr, string myGC, List<Dictionary<string, string>> myIP)
        {
            Dictionary<string, object> myProperties = new Dictionary<string, object>();

            foreach (DataColumn dc in myDt.Columns)
            {
                if (dc.ColumnName == myGC)
                {
                    GeoJsonPointGeometry tmpPoint = this.js.Deserialize<GeoJsonPointGeometry>((string)myDr[dc]);
                    this.Add("geometry", tmpPoint);
                }
                else
                {
                    myProperties.Add(dc.ColumnName, this.jsonEscape(myDr[dc]));
                }

            }
            if (myIP != null)
            {
                myProperties.Add("paths_in_paths", myIP);
            }

            this.Add("properties", myProperties);
        }

        public object jsonEscape(object mObj)
        {
            if (mObj is string)
            {
                string mString = (string)mObj;
                return mString.Replace("\"", "'");
            }
            else
            { return mObj; }
        }
    }

    /// <summary>
    /// Class to hold a GeoJSON Feature Collection
    /// </summary>
    public class GeoJsonFC : Dictionary<string, object>
    {
        public List<GeoJsonF> features;

        public GeoJsonFC()
        {
            this.Add("type", "FeatureCollection");
            this.Add("features", new List<GeoJsonF>());
            this.features = (List<GeoJsonF>)this["features"];
        }

        public Dictionary<string, object> feature(DataRow mDataRow)
        {
            return new Dictionary<string, object>();

        }

        public string getJson()
        {
            this["features"] = features;
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string strDataTableToDictionary = serializer.Serialize(this);
            strDataTableToDictionary = strDataTableToDictionary.Replace("\\\"", "\"");
            return strDataTableToDictionary;
        }

    }


    #endregion

}

