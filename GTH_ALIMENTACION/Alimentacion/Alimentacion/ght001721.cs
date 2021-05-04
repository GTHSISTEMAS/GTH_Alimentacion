
/*
**
**    Created by PROGRESS ProxyGen (Versión de Progress 11.7) Tue Feb 04 12:32:12 CST 2020
**
*/

//
// ght001721
//


namespace ght001721x
{
    using System;
    using Progress.Open4GL;
    using Progress.Open4GL.Exceptions;
    using Progress.Open4GL.Proxy;
    using Progress.Open4GL.DynamicAPI;
    using Progress.Common.EhnLog;
    using System.Collections.Specialized;
    using System.Configuration;

    /// <summary>
    /// 
    /// @author GISA Software
    /// @version 1.1
    /// </summary>
    public class ght001721 : AppObject
    {
        private static int proxyGenVersion = 1;
        private const  int PROXY_VER = 5;

    // Create a MetaData object for each temp-table parm used in any and all methods.
    // Create a Schema object for each method call that has temp-table parms which
    // points to one or more temp-tables used in that method call.


	static DataTableMetaData ght001721x_MetaData1;

	static DataTableMetaData ght001721x_MetaData2;




        static ght001721()
        {
		ght001721x_MetaData1 = new DataTableMetaData(0, "wMOVDOSIFICA", 6, false, 0, null, null, null, "ght001721x.StrongTypesNS.wMOVDOSIFICADataTable");
		ght001721x_MetaData1.setFieldDesc(1, "AlmacenCve", 0, Parameter.PRO_CHARACTER, 0, 0, 0, "", "", 0, null, "");
		ght001721x_MetaData1.setFieldDesc(2, "Periodo", 0, Parameter.PRO_INTEGER, 0, 1, 0, "", "", 0, null, "");
		ght001721x_MetaData1.setFieldDesc(3, "ArticuloCve", 0, Parameter.PRO_CHARACTER, 0, 2, 0, "", "", 0, null, "");
		ght001721x_MetaData1.setFieldDesc(4, "Establo", 0, Parameter.PRO_CHARACTER, 0, 3, 0, "", "", 0, null, "");
		ght001721x_MetaData1.setFieldDesc(5, "Etapa", 0, Parameter.PRO_CHARACTER, 0, 4, 0, "", "", 0, null, "");
		ght001721x_MetaData1.setFieldDesc(6, "Cantidad", 0, Parameter.PRO_DECIMAL, 0, 5, 0, "", "", 0, null, "");
		ght001721x_MetaData2 = new DataTableMetaData(0, "wERROR", 6, false, 0, null, null, null, "ght001721x.StrongTypesNS.wERRORDataTable");
		ght001721x_MetaData2.setFieldDesc(1, "AlmacenCve", 0, Parameter.PRO_CHARACTER, 0, 0, 0, "", "", 0, null, "");
		ght001721x_MetaData2.setFieldDesc(2, "Periodo", 0, Parameter.PRO_INTEGER, 0, 1, 0, "", "", 0, null, "");
		ght001721x_MetaData2.setFieldDesc(3, "ArticuloCve", 0, Parameter.PRO_CHARACTER, 0, 2, 0, "", "", 0, null, "");
		ght001721x_MetaData2.setFieldDesc(4, "Establo", 0, Parameter.PRO_CHARACTER, 0, 3, 0, "", "", 0, null, "");
		ght001721x_MetaData2.setFieldDesc(5, "Etapa", 0, Parameter.PRO_CHARACTER, 0, 4, 0, "", "", 0, null, "");
		ght001721x_MetaData2.setFieldDesc(6, "ERROR", 0, Parameter.PRO_CHARACTER, 0, 5, 0, "", "", 0, null, "");


        }


    //---- Constructors
    public ght001721(Connection connectObj) : this(connectObj, false)
    {       
    }
    
    // If useWebConfigFile = true, we are creating AppObject to use with Silverlight proxy
    public ght001721(Connection connectObj, bool useWebConfigFile)
    {
        try
        {
            if (RunTimeProperties.DynamicApiVersion != PROXY_VER)
                throw new Open4GLException(WrongProxyVer, null);

            if ((connectObj.Url == null) || (connectObj.Url.Equals("")))
                connectObj.Url = "ght001721";

            if (useWebConfigFile == true)
                connectObj.GetWebConfigFileInfo("ght001721");

            initAppObject("ght001721",
                          connectObj,
                          RunTimeProperties.tracer,
                          null, // requestID
                          proxyGenVersion);

        }
        catch (System.Exception e)
        {
            throw e;
        }
    }
   
    public ght001721(string urlString,
                        string userId,
                        string password,
                        string appServerInfo)
    {
        Connection connectObj;

        try
        {
            if (RunTimeProperties.DynamicApiVersion != PROXY_VER)
                throw new Open4GLException(WrongProxyVer, null);

            connectObj = new Connection(urlString,
                                        userId,
                                        password,
                                        appServerInfo);

            initAppObject("ght001721",
                          connectObj,
                          RunTimeProperties.tracer,
                          null, // requestID
                          proxyGenVersion);

            /* release the connection since the connection object */
            /* is being destroyed.  the user can't do this        */
            connectObj.ReleaseConnection();

        }
        catch (System.Exception e)
        {
            throw e;
        }
    }


    public ght001721(string userId,
                        string password,
                        string appServerInfo)
    {
        Connection connectObj;

        try
        {
            if (RunTimeProperties.DynamicApiVersion != PROXY_VER)
                throw new Open4GLException(WrongProxyVer, null);

            connectObj = new Connection("ght001721",
                                        userId,
                                        password,
                                        appServerInfo);

            initAppObject("ght001721",
                          connectObj,
                          RunTimeProperties.tracer,
                          null, // requestID
                          proxyGenVersion);

            /* release the connection since the connection object */
            /* is being destroyed.  the user can't do this        */
            connectObj.ReleaseConnection();
        }
        catch (System.Exception e)
        {
            throw e;
        }
    }

    public ght001721()
    {
        Connection connectObj;

        try
        {
            if (RunTimeProperties.DynamicApiVersion != PROXY_VER)
                throw new Open4GLException(WrongProxyVer, null);

            connectObj = new Connection("ght001721",
                                        null,
                                        null,
                                        null);

            initAppObject("ght001721",
                          connectObj,
                          RunTimeProperties.tracer,
                          null, // requestID
                          proxyGenVersion);

            /* release the connection since the connection object */
            /* is being destroyed.  the user can't do this        */
            connectObj.ReleaseConnection();
        }
        catch (System.Exception e)
        {
            throw e;
        }
    }

        /// <summary>
	/// 
	/// </summary> 
	public string ght001721x(ght001721x.StrongTypesNS.wMOVDOSIFICADataTable wMOVDOSIFICA, out ght001721x.StrongTypesNS.wERRORDataTable wERROR)
	{
		RqContext rqCtx = null;
		if (isSessionAvailable() == false)
			throw new Open4GLException(NotAvailable, null);

		Object outValue;
		ParameterSet parms = new ParameterSet(2);

		// Set up input parameters
		parms.setDataTableParameter(1, wMOVDOSIFICA, ParameterSet.INPUT, "ght001721x.StrongTypesNS.wMOVDOSIFICADataTable");


		// Set up input/output parameters


		// Set up Out parameters
		parms.setDataTableParameter(2, null, ParameterSet.OUTPUT, "ght001721x.StrongTypesNS.wERRORDataTable");


		// Setup local MetaSchema if any params are tables
		MetaSchema ght001721x_MetaSchema = new MetaSchema();
		ght001721x_MetaSchema.addDataTableSchema(ght001721x_MetaData1, 1, ParameterSet.INPUT);
		ght001721x_MetaSchema.addDataTableSchema(ght001721x_MetaData2, 2, ParameterSet.OUTPUT);


		// Set up return type
		

		// Run procedure
		rqCtx = runProcedure("ght001721x.p", parms, ght001721x_MetaSchema);


		// Get output parameters
		outValue = parms.getOutputParameter(2);
		wERROR = (ght001721x.StrongTypesNS.wERRORDataTable)outValue;


		if (rqCtx != null) rqCtx.Release();


		// Return output value
		return (string)(parms.ProcedureReturnValue);

	}



    }
}

