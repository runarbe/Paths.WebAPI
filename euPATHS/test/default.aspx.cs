using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace euPATHS.TestSuite
{
    public partial class _default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //var mItemSvc = new Item();
            //Response.Write(mItemSvc.Get("http://www.vads.ac.uk/large.php?uid=17716"));
            //Response.Write(mItemSvc.Get("http://www.fitzmuseum.cam.ac.uk/opacdirect/58477.html"));

            var mPathSvc = new Path();
            //Response.Write(mPathSvc.Get("http://paths-project.eu/path/100094"));
            Response.Write(mPathSvc.Get2("http://paths-project.eu/path/100094"));

        }
    }
}