﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="euPATHS._default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Test Page for Web Services</title>
    <script type="text/javascript" src="jquery-1.3.1.js"></script>
    <script type="text/javascript">
        function getProducts() 
        {
            $.ajax({
                type: "POST",
                url: "/Usr.asmx/Authenticate",
                data: "{'usr':'aldsddok', 'pwd':'alsdsdok'}",
                contentType: "application/json; charset=utf-8",
                success: done
            });
        }
        function done(response) 
        {
            alert('Done');
        }
        function cstatus(jqXHR, textStatus, errorThrown) {
            alert(textStatus + '\n' + errorThrown + '\n' + jqXHR.getAllResponseHeaders());
        }
        function loggedin(data) {alert('Alok');
            var data = $.parseJSON(data.d)
            alert(data.msg)
        }
        
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <input type="button" value="Get All Product" onclick="getProducts();" />
    </div>
    </form>
</body>
</html>
