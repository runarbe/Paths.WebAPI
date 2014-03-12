<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:fo="http://www.w3.org/1999/XSL/Format" xmlns:s="http://www.w3.org/2001/XMLSchema" xmlns:soap12="http://schemas.xmlsoap.org/wsdl/soap12/" xmlns:mime="http://schemas.xmlsoap.org/wsdl/mime/" xmlns:tns="http://paths-project.eu/" xmlns:soap="http://schemas.xmlsoap.org/wsdl/soap/" xmlns:tm="http://microsoft.com/wsdl/mime/textMatching/" xmlns:http="http://schemas.xmlsoap.org/wsdl/http/" xmlns:soapenc="http://schemas.xmlsoap.org/soap/encoding/" xmlns:wsdl="http://schemas.xmlsoap.org/wsdl/" exclude-result-prefixes="xsl tm s soap12 mime fo tns soap tm http soapenc wsdl">
<xsl:output method="html" indent="yes" encoding="utf-8"></xsl:output>

<!-- Main template-->
<xsl:template match="/">
    <!-- Put the assembly XML comments into a variable -->
    <xsl:variable name="xd" select="document('../bin/euPATHS.xml')"/>
    
    <!-- Output a header for the web service itself -->
    <xsl:variable name="mWebServiceName" select="//wsdl:service/@name"/>
    <h1>
        Web Service: <xsl:value-of select="$mWebServiceName"/>
    </h1>
    <xsl:variable name="mWebServiceDoc" select="$xd//member[@name=concat('T:euPATHS.', $mWebServiceName)]"/>
    <p>
        <strong>Summary:</strong> <xsl:value-of select="$mWebServiceDoc/summary"/>
    </p>
    <!-- For each method in the web service-->
    <xsl:for-each select="./wsdl:definitions/wsdl:binding[http:binding/@verb='POST']/wsdl:operation">
        <xsl:variable name="mWebMethodName" select="@name"/>
        <h2>
            Web Method: <xsl:value-of select="$mWebMethodName"/>
        </h2>
        <xsl:variable name="mWebMethodDoc" select="$xd//member[starts-with(@name, concat('M:euPATHS.',$mWebServiceName,'.', $mWebMethodName))]"/>
        <p>
            <strong>Summary: </strong>
            <xsl:value-of select="$mWebMethodDoc/summary"/>
        </p>
        <xsl:if test="$mWebMethodDoc/remarks != ''">
            <p>
                <strong>Remark: </strong>
                <xsl:value-of select="$mWebMethodDoc/remarks"/>
            </p>
        </xsl:if>
        <xsl:variable name="myService" select="@name"/>
        
        <!-- Call template to output request parameters -->        
        <xsl:apply-templates mode="writeRequest" select="//wsdl:types/s:schema/s:element[@name=$myService]">
            <xsl:with-param name="attrDoc" select="$mWebMethodDoc"/>
        </xsl:apply-templates >

        <!-- Call template to output response parameters -->
        <xsl:apply-templates mode="writeResponse" select="//wsdl:types/s:schema/s:element[@name=$myService]">
            <xsl:with-param name="attrDoc" select="$mWebMethodDoc"/>
        </xsl:apply-templates>
        <!-- Call template to output example of invoking the web service -->
        <xsl:apply-templates mode="writeExample" select="//wsdl:types/s:schema/s:element[@name=$myService]"/>
        
    </xsl:for-each> <!-- For each web method -->
</xsl:template>

<!-- Request template -->
<xsl:template mode="writeRequest" match="s:element">
    <xsl:param name="attrDoc"/>
	<h3><xsl:value-of select="@name"/> Request Parameters</h3>
    <xsl:if test="count(.//s:element) &gt; 0">
	    <table>
		    <tbody>
			    <tr>
				    <th width="25%">Parameter</th>
				    <th width="25%">Data type</th>
				    <!--th>Min occurs</th>
				    <th>Max occurs</th-->
				    <th width="50%">Description</th>
			    </tr>
			    <xsl:for-each select=".//s:element">
                    <xsl:variable name="mWebMethodAttributeName" select="@name"/>
				    <tr>
					    <td><xsl:value-of select="@name"></xsl:value-of></td>
					    <td><xsl:value-of select="@type"></xsl:value-of></td>
					    <!--td><xsl:value-of select="@minOccurs"></xsl:value-of></td>
					    <td><xsl:value-of select="@maxOccurs"></xsl:value-of></td-->
					    <td>
                            <xsl:value-of select="$attrDoc//param[@name=$mWebMethodAttributeName]"/>
                        </td>
				    </tr>
			    </xsl:for-each> <!-- End for each attribute -->			
		    </tbody>
	    </table>
    </xsl:if>
    <xsl:if test ="count(.//s:element) = 0">
        <p>
            <strong>N/A </strong>
            (this web method does not accept any calling parameters)</p>
    </xsl:if>
</xsl:template>

<!-- Response template -->
<xsl:template mode="writeResponse" match="s:element">
    <xsl:param name="attrDoc"/>
	<h3><xsl:value-of select="@name"></xsl:value-of> Response</h3>
    <table>
        <tbody>
            <tr>
                <th width="25%">Data type</th>
                <th width="75%">Description</th>
            </tr>
            <tr>
                <td>s:string (JSON)</td>
                <td>
                    <xsl:value-of select="$attrDoc/returns"/>
                </td>
            </tr>
        </tbody>
    </table>
</xsl:template>

<!-- Example template -->
<xsl:template mode="writeExample" match="s:element">
	<h3>Example of <xsl:value-of select="@name"/> HttpGet Request</h3>
	<address>
        <strong>Request:</strong>
        <br/>
	    http://api2.paths-project.eu/<xsl:value-of select="//wsdl:service/@name"/>.asmx/<xsl:value-of select="@name"/>?<xsl:for-each select=".//s:element">
					<xsl:value-of select="@name"/>=<xsl:value-of select="@type"/>
					<xsl:if test="position() != last()">&amp;&#8203;</xsl:if>
			</xsl:for-each> <!-- End for each attribute -->
	</address>
    <br/>
    <!--<address>
        <strong>Response:</strong>
        <br/>
    </address>-->
</xsl:template>

</xsl:stylesheet>
