<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="loadSql.aspx.cs" Inherits="euPATHS.Setup.loadSql" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <h1>Initialize PATHS database...</h1>
        <asp:Textbox TextMode="Password" ID="pPwd" runat="server"></asp:TextBox>
        <asp:Button ID="Button1" runat="server" Text="Reinitialize PATHS database" />
        <hr/>
        <div id="divOutputArea" runat="server"></div>
        <hr />
    </div>
    </form>
</body>
</html>