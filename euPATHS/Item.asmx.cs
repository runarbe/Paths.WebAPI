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
using System.Text;

namespace euPATHS
{

   /// <summary>
   /// The Item web service contains methods for querying and retrieving information about items. 
   /// PATHS items are information derived from Europeana and Alinari and includes most attributes defined by the 
   /// Europeana Semantic Elements. Items have been enriched with (1) background links, (2) topic links and (3) item similarity links.
   /// </summary>
   [System.Web.Script.Services.ScriptService()]
   [System.Web.Services.WebService(Namespace = "http://paths-project.eu/")]
   [System.Web.Services.WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
   [ToolboxItem(false)]
   public class Item : System.Web.Services.WebService
   {

      #region --------------------Commented---------------------------------
      /*
      #region -------------------Search-------------------------------
      /// <summary>
      /// Experimental function to enable full-text search without using SOLR
      /// </summary>
      /// <param name="myQuery">Query expression</param>
      /// <param name="myLang">One of english, spanish or leave empty </param>
      /// <param name="myLength">How many result records to retrieve</param>
      /// <param name="myOffset">Where to start retrieving in a result set (paging)</param>
      /// <returns>JSON String: List of items</returns>
      /// <remarks></remarks>
      [WebMethod(EnableSession = true)]
      [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
      public string Search(string myQuery, string myLang, string myLength, string myOffset)
      {
         string strRV = string.Empty;
         DataTable dtab;
         try
         {
            if (string.IsNullOrEmpty(myLang))
            {
               myLang = "english";
            }

            if (!Utility.IsNumeric(Convert.ToString(myOffset)))
            {
               myOffset = "0";
            }
            if (!Utility.IsNumeric(Convert.ToString(myLength)))
            {
               myLength = "10";
            }
            if (Convert.ToInt32(myLength) > 100)
            {
               myLength = "100";
            }
            myQuery = string.Join("&", myQuery.Split(' '));

            string strQuery = "SELECT " + Utility.FieldSetItem + " FROM item WHERE idxfti @@ to_tsquery('" + myLang + "', '" + myQuery + "') LIMIT '" + myLength + "' OFFSET '" + myOffset + "';";
            dtab = Utility.DBExecuteDataTable(strQuery);
            if (dtab.Rows.Count == 0)
            {
               strRV = Utility.GetMsg(Utility.msgStatusCodes.QueryDidNotReturnRecords, "Search for items did not yield any results");
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

      #region -------------------GetItemsForTopic---------------------
      /// <summary>
      /// Get all items associated with a specific topic.
      /// </summary>
      /// <param name="topic_uri">URI of topic</param>
      /// <param name="myLimit">Number of results to retrieve</param>
      /// <param name="myStart">Where to start retrieving in a result set (paging)</param>
      /// <returns>JSON String: List of items</returns>
      /// <remarks></remarks>
      [WebMethod(EnableSession = true)]
      [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
      public string GetItemsForTopic(string topic_uri, string myLimit, string myStart)
      {
         string strRV = string.Empty;
         DataTable dtab;
         try
         {
            if (Utility.TryToParseInt(myLimit) == 0)
            {
               myLimit = "10";
            }
            if (Utility.TryToParseInt(myLimit) > 100)
            {
               myLimit = "100";
            }
            if (string.IsNullOrEmpty(myStart))
            {
               myStart = "0";
            }
            string strQuery = "SELECT count(item.id) as count FROM item, item_topic, topic WHERE topic.uri = '" + topic_uri + "' AND item_topic.fk_topic_id = topic.id AND item.id = item_topic.fk_item_id;";
            dtab = Utility.DBExecuteDataTable(strQuery);
            if (dtab.Rows.Count == 0)
            {
               strRV = Utility.GetMsg(Utility.msgStatusCodes.QueryDidNotReturnRecords, "No items associated with specified topic");
            }
            else
            {
               Dictionary<string, DataTable> myAdditionalTables = new Dictionary<string, DataTable>();
               //Get items
               strQuery = "SELECT " + Utility.FieldSetItem + " FROM item, item_topic, topic WHERE topic.uri = '" + topic_uri + "' AND item_topic.fk_topic_id = topic.id AND item.id = item_topic.fk_item_id LIMIT '" + myLimit + "' OFFSET '" + myStart + "';";
               DataTable myItemsTable = Utility.DBExecuteDataTable(strQuery);
               myAdditionalTables.Add("items", myItemsTable);
               strRV = Utility.DataTableToDictionary(dtab, true, myAdditionalTables);
            }
            return strRV;
         }
         catch (Exception ex)
         {
            return Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message);
         }
      }
      #endregion

      #region -------------------GetItemsBY URI-----------------------
      /// <summary>
      /// Get a single item by its URI
      /// </summary>
      /// <param name="item_uri">URI of item</param>
      /// <returns>JSON String: Single item information</returns>
      /// <remarks></remarks>
      [WebMethod(EnableSession = true)]
      [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
      public string GetItemByUri(string item_uri)
      {
         string strRV = string.Empty;
         DataTable dtab;
         try
         {
            string strQuery = "SELECT " + Utility.FieldSetItem + " FROM item WHERE uri = '" + item_uri + "';";
            dtab = Utility.DBExecuteDataTable(strQuery);
            //Create container for additional tables
            Dictionary<string, DataTable> myAdditionalTables = new Dictionary<string, DataTable>();
            //Only add more stuff if one single row
            if (dtab.Rows.Count > 0)
            {
               //Get the URI of the presently selected item
               int ItemID = Utility.TryToParseInt(Convert.ToString(dtab.Rows[0]["id"]));
               //Get topics
               DataTable myTopics;
               strQuery = "SELECT t.uri, t.dc_description, t.dc_subject, t.dc_title, t.topic_hierarchy, t.topic_thumbnails FROM item_topic it, topic t WHERE it.fk_item_id = '" + ItemID + "' and it.fk_topic_id = t.id";
               myTopics = Utility.DBExecuteDataTable(strQuery);
               myAdditionalTables.Add("paths_topic", myTopics);

               //Get wikilinks
               DataTable myBackgroundLinks;
               strQuery = "SELECT DISTINCT link as dc_title, link as uri FROM item_link WHERE fk_rel_uri='" + item_uri + "' AND confidence > 0.2";
               myBackgroundLinks = Utility.DBExecuteDataTable(strQuery);
               myAdditionalTables.Add("paths_background_links", myBackgroundLinks);

               //Get similarity links (potentially very heavy)
               DataTable myRelatedItems;
               strQuery = "SELECT DISTINCT i.uri, i.dc_title, i.europeana_object FROM item_similarity its, item i WHERE its.fk_sitem_uri='" + item_uri + "' AND its.fk_titem_uri = i.uri;";
               myRelatedItems = Utility.DBExecuteDataTable(strQuery);
               myAdditionalTables.Add("paths_related_items", myRelatedItems);

               //Get ratings
               DataTable myRatings;
               strQuery = "SELECT (SELECT COUNT(id) FROM rating WHERE fk_rating_scale_id = 2 AND fk_rel_uri = '" + item_uri + "') AS likes, (SELECT COUNT(id) FROM rating WHERE fk_rating_scale_id = 1 AND fk_rel_uri =  '" + item_uri + "') AS dislikes";
               myRatings = Utility.DBExecuteDataTable(strQuery);
               myAdditionalTables.Add("paths_rating", myRatings);
               strRV = Utility.DataTableToDictionary(dtab, true, myAdditionalTables);
            }
            else
            {
               strRV = Utility.GetMsg(Utility.msgStatusCodes.SpecifiedObjectDoesNotExist, "The specified item doesn't exist.");
            }
            return strRV;
         }
         catch (Exception ex)
         {
            return Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message);
         }
      }
      #endregion

      #region -------------------GetItemsForTopic---------------------
      /// <summary>
      /// Get a single item by its ID
      /// </summary>
      /// <param name="ItemID">ID of item</param>
      /// <returns>JSON String: Single item information</returns>
      /// <remarks></remarks>
      [WebMethod(EnableSession = true)]
      [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
      public string GetItemByID(string ItemID)
      {
         string strRV = string.Empty;
         DataTable dtab;
         try
         {
            string strQuery = "SELECT " + Utility.FieldSetItem + " FROM item WHERE id = '" + ItemID + "';";
            dtab = Utility.DBExecuteDataTable(strQuery);

            //Create container for additional tables
            Dictionary<string, DataTable> myAdditionalTables = new Dictionary<string, DataTable>();

            //Only add more stuff if one single row
            if (dtab.Rows.Count > 0)
            {
               //Get the URI of the presently selected item
               string ItemUri = Convert.ToString(dtab.Rows[0]["uri"]);

               //Get topics
               DataTable myTopics;
               strQuery = "SELECT t.uri, t.dc_description, t.dc_subject, t.dc_title, t.topic_hierarchy, t.topic_thumbnails FROM item_topic it, topic t WHERE it.fk_item_id = '" + ItemID + "' and it.fk_topic_id = t.id";
               myTopics = Utility.DBExecuteDataTable(strQuery);
               myAdditionalTables.Add("paths_topic", myTopics);

               //Get wikilinks
               DataTable myBackgroundLinks;
               strQuery = "SELECT DISTINCT link as dc_title, link as uri FROM item_link WHERE fk_rel_uri='" + ItemUri + "' AND confidence > 0.2;";
               myBackgroundLinks = Utility.DBExecuteDataTable(strQuery);
               myAdditionalTables.Add("paths_background_links", myBackgroundLinks);

               //Get similarity links (potentially very heavy)
               DataTable myRelatedItems;
               strQuery = "SELECT DISTINCT i.uri, i.dc_title, i.europeana_object FROM item_similarity its, item i WHERE its.fk_sitem_uri='" + ItemUri + "' AND its.fk_titem_uri = i.uri;";
               myRelatedItems = Utility.DBExecuteDataTable(strQuery);
               myAdditionalTables.Add("paths_related_items", myRelatedItems);

               //Get ratings
               DataTable myRatings;
               strQuery = "SELECT (SELECT COUNT(id) FROM rating WHERE fk_rating_scale_id = 2 AND fk_rel_uri = '" + ItemUri + "') AS likes, (SELECT COUNT(id) FROM rating WHERE fk_rating_scale_id = 1 AND fk_rel_uri = '" + ItemUri + "') AS dislikes";
               myRatings = Utility.DBExecuteDataTable(strQuery);
               myAdditionalTables.Add("paths_rating", myRatings);
               strRV = Utility.DataTableToDictionary(dtab, true, myAdditionalTables);
            }
            else
            {
               strRV = Utility.GetMsg(Utility.msgStatusCodes.SpecifiedObjectDoesNotExist, "The specified item doesn't exist.");
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

      #region -------------------Get ItemsBY URI #230-----------------------
      /// <summary>
      /// Get a single item by its URI
      /// </summary>
      /// <param name="paths_identifier">URI of item</param>
      /// <returns>JSON String: Single item information</returns>
      /// <remarks></remarks>
      [WebMethod(EnableSession = true)]
      [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
      public string Get(string paths_identifier)
      {
         string strRV = string.Empty;
         DataTable dtab;
         try
         {
             var mTimer = new Stopwatch();
             mTimer.Start();

            string strQuery = "SELECT " + Utility.FieldSetItem + ",'#pt#' paths_topics FROM item WHERE uri = '" + paths_identifier + "';";
            dtab = Utility.DBExecuteDataTable(strQuery);

            strRV = Utility.DataTableToDictionary(dtab, true,null,true,"item");

            //Create container for additional tables                                         
            Dictionary<string, DataTable> myAdditionalTables = new Dictionary<string, DataTable>();

            //Only add more stuff if there is a for it
            if (dtab.Rows.Count > 0)
            {
               strQuery = "select p.uri paths_identifier,n.uri node_identifier,p.dc_title,p. paths_thumbnail from path p inner join node n on p.id = n.fk_path_id  where n.dc_source = '" + paths_identifier + "'";
               DataTable myPaths = Utility.DBExecuteDataTable(strQuery);
               myAdditionalTables.Add("paths_in_paths", myPaths);

               //Add Item_link
               strQuery = "select  distinct link as dc_source ,replace(replace(substring(link from (length(link) - position('/' in reverse(link)) + 2) for 150),'%20',' '),'#',',') as dc_title from item_link where confidence > 0.5 and fk_rel_uri =  '" + paths_identifier + "'";
               DataTable myPathsItemLink = Utility.DBExecuteDataTable(strQuery);
               myAdditionalTables.Add("paths_background_links", myPathsItemLink);

               //Add paths_related_items
               strQuery = "select  dc_title,fk_titem_uri paths_identifier,CASE WHEN isim.dc_type='' THEN 'generic' WHEN isim.dc_type is NULL THEN 'generic' ELSE isim.dc_type END,itm.europeana_object from item_similarity isim  inner join item itm  on isim.fk_titem_uri = itm.uri where confidence > 0.5 and fk_sitem_uri = '" + paths_identifier + "'";
               DataTable myPathsRelItem = Utility.DBExecuteDataTable(strQuery);
               myAdditionalTables.Add("paths_related_items", myPathsRelItem);

               strRV = Utility.DataTableToDictionary(dtab, true, myAdditionalTables,true,"item");

               strQuery = "select fk_topic_id from Item_topic where fk_item_uri = '" + paths_identifier + "'";
               DataTable myPathsTopic = Utility.DBExecuteDataTable(strQuery);
               string strNodeId = "";
               foreach (DataRow drow in myPathsTopic.Rows)
               {
                  strNodeId = strNodeId + "\"" + drow["fk_topic_id"] + "\",";
               }
               if (strNodeId != "")
               {
                  strNodeId = strNodeId.Substring(0, strNodeId.LastIndexOf(","));
               }
               strRV = strRV.Replace("\"#pt#\"", "[" + strNodeId + "]");
               Utility.LogRequest(strRV, false, mTimer);
            }
            else
            {
                strRV = Utility.GetMsg(Utility.msgStatusCodes.SpecifiedObjectDoesNotExist, "The specified item doesn't exist.");
                Utility.LogRequest(strRV, false, mTimer);

            }
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

      #region ----------- Web Service code for item.bbox (#231) ------------
      /// <summary>
      /// Web service returns items based on a bounding box and a limit parameter
      /// </summary>
      /// <param name="bbox">string: a comma separated list of values</param>
      /// <param name="limit">integer: a number of items to return</param>
      /// <returns></returns>
      [WebMethod(EnableSession = true, Description = "Get items by BBox")]
      [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
      public string Bbox(string bbox, int limit)
      {
          var mTimer = new Stopwatch();
          mTimer.Start();

         //Declare temporary values to accrue return value
         GeoJsonFC myGeoJsonFC = new GeoJsonFC();

         //Check that the bbox includes four values
         string[] bboxitms = bbox.Split(',');
         if (bboxitms.Length > 4)
         {
             Utility.LogRequest("wrong bounding box fomat", true, mTimer);
            return "error";
         }

         //Declare data table to hold SQL results
         DataTable myDt;
         try
         {
            //Select
            string strQuery = "SELECT " + Utility.FieldSetItem + ", ST_AsGeoJSON(map_point.geom) as the_geom FROM item, map_point WHERE map_point.item_id = item.usfd_id AND ST_Contains(ST_SetSRID(ST_MakeBox2D(ST_Point(" + bboxitms[0] + ", " + bboxitms[1] + "),ST_Point(" + bboxitms[2] + ", " + bboxitms[3] + ")),900913), map_point.geom) LIMIT " + limit + ";";
            myDt = Utility.DBExecuteDataTable(strQuery);

            //For each item
            foreach (DataRow myDr in myDt.Rows)
            {
               //Declare value to hold paths/nodes that the item is within
               List<Dictionary<string, string>> strInPaths = new List<Dictionary<string, string>>();

               //Check if paths_identifier column exist
               DataColumn myDc = myDt.Columns["paths_identifier"];
               if (myDc != null)
               {

                  //Select paths that the items belong to
                  string strQuery2 = "SELECT p.uri as paths_identifier, n.uri as node_identifier, p.dc_title as dc_title, p.paths_thumbnail as paths_thumbnail FROM node n LEFT JOIN path p ON n.fk_path_id = p.id WHERE n.dc_source = '" + (string)myDr[myDc] + "';";
                  DataTable myDt2 = Utility.DBExecuteDataTable(strQuery2);

                  //For each related path - should only return one value
                  foreach (DataRow myDr2 in myDt2.Rows)
                  {
                     Dictionary<string, string> myPath = new Dictionary<string, string>();
                     foreach (DataColumn myCo2 in myDt2.Columns)
                     {

                        if (!DBNull.Value.Equals(myDr2[myCo2]))
                        {
                           myPath.Add(myCo2.ColumnName, (string)myDr2[myCo2]);
                        }

                     }
                     strInPaths.Add(myPath);

                  }
               }

               //Add features to Feature Collection
               myGeoJsonFC.features.Add(new GeoJsonF(myDt, myDr, "the_geom", strInPaths));
            }
            //Return feature collection as JSON
             var mJson = myGeoJsonFC.getJson();
             Utility.LogRequest(mJson, false, mTimer);
             return mJson;

         }
         catch (Exception ex)
         {
            var mMsg = Utility.GetMsg(Utility.msgStatusCodes.DatabaseSQLError, ex.Message + ex.Source + ex.StackTrace + ex.TargetSite);
            Utility.LogRequest(mMsg, false, mTimer);
            return mMsg;
         }
      }
      #endregion

   }

}
