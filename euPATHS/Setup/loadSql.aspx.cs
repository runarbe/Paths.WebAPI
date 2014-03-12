using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.Odbc;
using System.IO;
using System.Collections;
using euPATHS.AppCode;

namespace euPATHS.Setup
{
   public partial class loadSql : System.Web.UI.Page
   {
      public bool OutputStatus(string pMyStatus, bool pClear = false)
      {
         if (pClear)
         {
            divOutputArea.InnerHtml = "";
         }
         divOutputArea.InnerHtml = divOutputArea.InnerHtml + pMyStatus + "<br/>";
         return true;
      }
      protected void Page_Load(object sender, System.EventArgs e)
      {
         if (!pPwd.Text.Equals("pathspwd"))
         {
            OutputStatus("No password supplied or wrong password!", true);
            return;
         }
         else
         {
            OutputStatus("Working...", true);
         }
         Response.Buffer = false;
         List<string> sqlFileList = new List<string>();
         sqlFileList.Add("drop_v2.sql");
         sqlFileList.Add("create_v2.sql");
         sqlFileList.Add("insert.sql");
         sqlFileList.Add("europeana_items.sql");
         sqlFileList.Add("background_links.sql");
         sqlFileList.Add("topics_items_rating.sql");
         foreach (string sqlFileName in sqlFileList)
         {
            System.IO.TextReader tr = null;
            tr = new System.IO.StreamReader(Request.PhysicalApplicationPath + "sql\\" + sqlFileName);
            string strSQL = tr.ReadToEnd();
            try
            {
               Utility.DBExecuteNonQuery(strSQL);
               OutputStatus("Done: " + Request.PhysicalApplicationPath + "sql\\" + sqlFileName);
            }
            catch (Exception ex)
            {
               tr.Close();
               OutputStatus("Error: " + ex.Message);
            }
            tr.Close();
            tr = null;
         }
         OutputStatus("Completed...");
         pPwd.Text = "";
      }
      public loadSql()
      {
         Load += Page_Load;
      }
   }
}