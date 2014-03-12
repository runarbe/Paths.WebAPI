using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Net;
using System.Xml.Xsl;
using System.IO;
using System.Text;

namespace euPATHS.Documentation
{
    /// <summary>
    /// 
    /// </summary>
    public partial class DocumentationGeneration : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)

        {

        List<string> myList = new List<string>();
        myList.Add("Usr");
        myList.Add("Item");
        myList.Add("Topic");
        myList.Add("Workspace");
        myList.Add("Path");
        myList.Add("Social");

        XmlTextWriter xW = new XmlTextWriter(File.Open(Server.MapPath("api-doc.docx"), FileMode.Create), Encoding.UTF8);

        XslCompiledTransform  docXSL = new XslCompiledTransform();

        XsltSettings myXSS = new XsltSettings();
        myXSS.EnableDocumentFunction = true;

        XmlSecureResolver resolver = new XmlSecureResolver(new XmlUrlResolver(), "http://localhost:64362/Documentation");
        resolver.Credentials = CredentialCache.DefaultCredentials;

        docXSL.Load(Server.MapPath("wsdl-to-doc.xsl"), myXSS, null);
        try
        {
            foreach (string wsName in myList)
            {
                docXSL.Transform("http://localhost:54004/" + wsName + ".asmx?WSDL", xW);
            }

        }
        catch (Exception ex)
        {

            Response.Write(ex.Message+" "+ex.Source+" "+ex.StackTrace+" "+ex.InnerException.Message);
        }

        xW.Flush();
        xW.Close();

        //Read and output the file to screen
        using (StreamReader sr = File.OpenText(Server.MapPath("api-doc.docx"))) {
            string myHtml = sr.ReadToEnd();
            divDoc.Text = myHtml;
        }

        }
    }
}