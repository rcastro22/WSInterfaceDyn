using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Microsoft.Dynamics.BusinessConnectorNet;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using System.Configuration;
using System.Data;

[WebService(Namespace = "http://galileo.edu/dynamicsax/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
// [System.Web.Script.Services.ScriptService]

public class WSInterfaceDynamics : System.Web.Services.WebService
{
    OracleConnection cn = new OracleConnection(ConfigurationManager.ConnectionStrings["galileo"].ConnectionString);
    public WSInterfaceDynamics()
    {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }

    [WebMethod(Description = "Actualiza los datos en Microsoft Dynamics AX de Oracle", EnableSession = false)]
    // 18-02-2011, Roberto Castro, Metodo web para la actualizacion de las carreras en Dynamics AX
    public String UpdDynCareers()
    {
        String col = "";
        try
        {
            // Variables para el tratamiento de datos de Dynamics AX
            Axapta ax = new Axapta();
            AxaptaRecord axRecord;
            string tableName = "UGLCareersTable";
            String strQuery = "delete_from %1";

            // Variables para la conexion y extraccion de datos de Oracle                
            DataSet ds = new DataSet();
            // Variables de uso
            int insertrecord = 0;

            //NetworkCredential cred = new NetworkCredential("Dynamics"

            //////////// PROCESO /////////////
            // Configuracion de los parametros necesarios para la conexion a la Base de datos de Oracle                                           
            OracleCommand cmd = new OracleCommand();
            OracleDataAdapter adapter = new OracleDataAdapter(cmd);

            cmd.CommandText = "DBAFISICC.dynamics.CaCarrerasxDirector";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Connection = cn;
            cmd.Parameters.Add("o_remCursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            // Llena el DataSet con los datos obtenidos del Procedimiento Almacenado                
            adapter.Fill(ds, "Carees");

            // Inicia sesion en Dynamics
            ax.Logon(null, null, null, null);
            // Crea um registo de la tabla respectiva enviada
            axRecord = ax.CreateAxaptaRecord(tableName);
            // Elimina todos los registros de la tabla
            axRecord.ExecuteStmt(strQuery);
            // Recorre los registros del DataSet
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                if (row.ItemArray.Length == 6)
                {
                    col += "\n";
                    string CareerId, Description, DirectoName, dim1, dim2, CareerPrincipal, Status;
                    CareerId = Convert.ToString(row[0]);
                    Description = Convert.ToString(row[1]);
                    DirectoName = Convert.ToString(row[2]);
                    dim2 = Convert.ToString(row[3]);
                    CareerPrincipal = Convert.ToString(row[4]);
                    Status = Convert.ToString(row[5]);
                    if (dim2.Length >= 6)
                    {
                        dim1 = dim2.Substring(0, 3);
                    }
                    else
                    {
                        dim1 = "";
                        dim2 = "";
                    }
                    col += row[0] + ",";
                    // Provee los valores para cada uno de los campos del registro de la tabla UGLCareersTable
                    axRecord.set_Field("CAREERID", CareerId);
                    axRecord.set_Field("DESCRIPTION", Description);
                    axRecord.set_Field("DIRECTORNAME", DirectoName);
                    axRecord.set_Field("DIMENSION[1]", dim1);
                    axRecord.set_Field("DIMENSION[2]", dim2);
                    axRecord.set_Field("PRINCIPALCAREER", CareerPrincipal);
                    axRecord.set_Field("STATUS", (Status == "A" ? "Activo" : "Inactivo"));

                    // Inserta el registro en Dynamics
                    axRecord.Insert();
                    // Lleva un conteo de los registros insertados
                    insertrecord = insertrecord + 1;
                }
            }
            // Cierra sesion en Dynamics
            ax.Logoff();

            return "Cantidad de registros insertados: " + Convert.ToString(insertrecord);
        }
        catch (Exception e)
        {
            return col + "Error encontrado: " + e.Message;
            // Take other error action as needed.
        }
    }

    [WebMethod(Description = "Retorna Byte de la foto de los administrativos", EnableSession = false)]
    // 11-09-2015, Roberto Castro, Retorna Byte de la foto de los administrativos
    public string foto(int _correlativo)
    {
        string er = "";
        OracleCommand cmd = new OracleCommand();
        cmd.CommandText = "DBAFISICC.CARNET.FOTO2";
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Connection = cn;
        cmd.BindByName = true;

        cmd.Parameters.Add("RETVAL", OracleDbType.RefCursor, 200);
        cmd.Parameters.Add("PCORRELATIVO", _correlativo);
        cmd.Parameters["RETVAL"].Direction = ParameterDirection.Output;
        DataTable dt = new DataTable();
        try
        {
            cn.Open();

            cmd.ExecuteNonQuery();

            OracleRefCursor RETVAL = (OracleRefCursor)(cmd.Parameters["RETVAL"].Value);


            OracleDataAdapter da = new OracleDataAdapter();
            da.Fill(dt, RETVAL);

            da.Dispose();
        }
        catch (Exception ex)
        {
            //throw ex;
            er = ex.Message;
        }
        finally
        {
            cn.Close();
            cmd.Dispose();
        }

        return er;
    }


    [WebMethod(Description = "Actualiza Torres y Salones de Oracle a Dynamics", EnableSession = false)]
    // 02-04-2014, Roberto Castro, Metodo web para la actualizacion de las Torres y Salones en Dynamics AX
    public DataSet UpdDynTorresSalones()
    {
        String col = "";
        try
        {
            // Variables de uso
            int insertrecord = 0;
            // Variables para el tratamiento de datos de Dynamics AX
            Axapta ax = new Axapta();
            AxaptaRecord axRecord, axRecord2;
            string tableName = "UGTorres", tableName2 = "UGSalones";
            String strQuery = "delete_from %1";

            // Variables para la conexion y extraccion de datos de Oracle                
            OracleCommand cmd = new OracleCommand();
            DataSet ds = new DataSet();
            DataSet ds2 = new DataSet();
            OracleDataAdapter adapter = new OracleDataAdapter(cmd);
            // Variables de uso


            //OracleCommand cmd = new OracleCommand();
            cmd.CommandText = "DBAFISICC.DYNAMICS.SEL_CATORRES";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Connection = cn;
            cmd.BindByName = true;

            cmd.Parameters.Add("O_REMCURSOR", OracleDbType.RefCursor, 200);
            cmd.Parameters["O_REMCURSOR"].Direction = ParameterDirection.Output;

            try
            {
                cn.Open();

                cmd.ExecuteNonQuery();

                OracleRefCursor O_REMCURSOR = (OracleRefCursor)(cmd.Parameters["O_REMCURSOR"].Value);

                OracleDataAdapter da = new OracleDataAdapter();
                da.Fill(ds, O_REMCURSOR);

                da.Dispose();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                cn.Close();
                cmd.Dispose();
            }

            return ds;

            /*cmd.Parameters.Clear();
            cmd.CommandText = "DBAFISICC.DYNAMICS.SEL_CASALONES";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Connection = cn;
            cmd.BindByName = true;

            cmd.Parameters.Add("O_REMCURSOR", OracleDbType.RefCursor, 200);
            cmd.Parameters["O_REMCURSOR"].Direction = ParameterDirection.Output;

            try
            {
                cn.Open();

                cmd.ExecuteNonQuery();

                OracleRefCursor O_REMCURSOR = (OracleRefCursor)(cmd.Parameters["O_REMCURSOR"].Value);

                OracleDataAdapter da = new OracleDataAdapter();
                da.Fill(ds2, O_REMCURSOR);

                da.Dispose();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                cn.Close();
                cmd.Dispose();
            }*/


            // Configuracion de los parametros necesarios para la conexion a la Base de datos de Oracle
            /*cmd.CommandText = "DBAFISICC.dynamics.SEL_CaTorres";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Connection = cn;
            cmd.Parameters.Add("o_remCursor", OracleType.Cursor).Direction = ParameterDirection.Output;
            // Llena el DataSet con los datos obtenidos del Procedimiento Almacenado                
            adapter.Fill(ds, "Torres");

            cmd.Dispose();
            cmd.Parameters.Clear();
            cmd.CommandText = "DBAFISICC.dynamics.SEL_CaSalones";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Connection = cn;
            cmd.Parameters.Add("o_remCursor", OracleType.Cursor).Direction = ParameterDirection.Output;
            // Llena el DataSet con los datos obtenidos del Procedimiento Almacenado                
            adapter.Fill(ds2, "Salones");*/




            /*
            // Inicia sesion en Dynamics
            //string AxConf = @"C:\\Users\\rcastro\\Desktop\\AxConfig.axc";
            //string AxConf = "AxConfig.axc";
            //ax.Logon("UG", "es-mx", "MyObjectServer", AxConf);
            ax.Logon(null, null, null, null);

            // Crea um registo de la tabla respectiva enviada
            axRecord = ax.CreateAxaptaRecord(tableName);
            axRecord2 = ax.CreateAxaptaRecord(tableName2);
            // Elimina todos los registros de la tabla
            axRecord.ExecuteStmt(strQuery);
            axRecord2.ExecuteStmt(strQuery);
            // Recorre los registros del DataSet
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                if (row.ItemArray.Length == 2)
                {
                    col += "\n";
                    string CodTorre, Description;
                    CodTorre = Convert.ToString(row[0]);
                    Description = Convert.ToString(row[1]);                        

                    col += row[0] + ",";
                    // Provee los valores para cada uno de los campos del registro de la tabla UGLCareersTable
                    axRecord.set_Field("TORREID", CodTorre);
                    axRecord.set_Field("DESCRIPTION", Description);


                    // Inserta el registro en Dynamics
                    axRecord.Insert();
                    // Lleva un conteo de los registros insertados
                    insertrecord = insertrecord + 1;
                }
            }

            foreach (DataRow row in ds2.Tables[0].Rows)
            {
                if (row.ItemArray.Length == 2)
                {
                    col += "\n";
                    string CodTorre, CodSalon;
                    CodTorre = Convert.ToString(row[0]);
                    CodSalon = Convert.ToString(row[1]);

                    col += row[0] + ",";
                    // Provee los valores para cada uno de los campos del registro de la tabla UGLCareersTable
                    axRecord2.set_Field("TORREID", CodTorre);
                    axRecord2.set_Field("SALONID", CodSalon);


                    // Inserta el registro en Dynamics
                    axRecord2.Insert();
                    // Lleva un conteo de los registros insertados
                    insertrecord = insertrecord + 1;
                }
            }
            // Cierra sesion en Dynamics
            //ax.Logoff();

            return "Cantidad de registros insertados: " + Convert.ToString(insertrecord);*/
        }
        catch (Exception e)
        {
            DataSet ds = new DataSet();
            return ds;
            //return col + "Error encontrado: " + e.Message;
            // Take other error action as needed.
        }
    }




    [WebMethod(Description = "Retorna la DateSet de las carreras desde Oracle", EnableSession = false)]
    // 18-02-2011, Roberto Castro, Metodo web para obtener la carreras desde la base de datos Oracle
    public DataSet GetDatOra()
    {
        // Variables para la conexion y extraccion de datos de Oracle                
        OracleCommand cmd = new OracleCommand();
        DataSet ds = new DataSet();
        OracleDataAdapter adapter = new OracleDataAdapter(cmd);
        // Variables de uso


        //////////// PROCESO /////////////
        // Configuracion de los parametros necesarios para la conexion a la Base de datos de Oracle
        cmd.CommandText = "DBAFISICC.dynamics.CaCarrerasxDirector";
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Connection = cn;
        cmd.Parameters.Add("o_remCursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
        // Llena el DataSet con los datos obtenidos del Procedimiento Almacenado                
        adapter.Fill(ds, "Carees");

        return ds;
    }

    [WebMethod(Description = "Prueba de conexión a Microsoft Dynamics AX", EnableSession = false)]
    // 09-03-2012, Roberto Castro, Metodo web para la actualizacion de las carreras en Dynamics AX
    public string testConnection()
    {
        string ret;
        try
        {
            Axapta ax = new Axapta();
            //ax.LogonAs("rcastro","gdynamics",null,null, null, null, null);
            ax.Logon(null, null, null, null);
            ret = "correcto";
            ax.Logoff();
        }
        catch (Exception ex)
        {
            ret = ex.Message;
            //throw new Exception(ex.Message);
        }
        return ret;
    }


    [WebMethod(Description = "Ingresa los docentes y tutores según su numero de NIT", EnableSession = false)]
    // 15-10-2012, Roberto Castro, Ingresa los docentes y tutores según su numero de NIT desde la aplicacion de nuevo Docente en Informatica.galileo.edu
    public String nuevoTutorDocenteDAX(string _orig, string _nit, string _nombre, string _codigo, bool _nacional)
    {
        string ret = "";
        Axapta ax = new Axapta();
        try
        {
            if (_nit != "" && _nit != null && ((_nacional && validaNIt(_nit)) || !_nacional))
            {
                // Variables para el tratamiento de datos de Dynamics AX                
                string[] parms = new string[] { _orig, _nit, _nombre, _codigo };

                AxaptaRecord axRecord;
                string tableName = "IEmplVendCodeRefTable";
                string fieldName = "";

                switch (_orig.ToUpper())
                {
                    case "IDEA":
                        fieldName = "IEmplIdIDEA";
                        break;

                    case "UGA":
                        fieldName = "IEmplIdUGA";
                        break;

                    default:
                        ret = "Interfaz " + _orig + " no soportada";
                        return ret;
                }

                String strQuery = "select forupdate %1 where %1.IEmplVendCodeRefId == '" + _nit + "'";

                //////////// PROCESO /////////////
                // Inicia sesion en Dynamics
                ax.Logon(null, null, null, null);
                // Realiza una llamada al metodo estatico de Dynamics
                //ret = Convert.ToString(ax.CallStaticClassMethod("WSInterface", "newIEmplVendCodeRefTable", parms));
                //ax.CallStaticClassMethod("WSInterface", "newIEmplVendCodeRefTable", parms);
                ax.TTSBegin();
                axRecord = ax.CreateAxaptaRecord(tableName);
                axRecord.ExecuteStmt(strQuery);

                if (axRecord.Found)
                {
                    // Provee los valores para cada uno de los campos del registro de la tabla UGLCareersTable
                    axRecord.set_Field(fieldName, _codigo);

                    // Actualiza el registro en Dynamics
                    axRecord.Update();
                }
                else
                {
                    // Provee los valores para cada uno de los campos del registro de la tabla UGLCareersTable
                    axRecord.set_Field("IEMPLVENDCODEREFID", _nit);
                    axRecord.set_Field("INAME", _nombre);
                    axRecord.set_Field(fieldName, _codigo);

                    // Inserta el registro en Dynamics
                    axRecord.Insert();
                }
                ax.TTSCommit();
                //ret = axRecord.get_Field("IName").ToString();
                // Cierra sesion en Dynamics 
                ax.Logoff();
            }
            else
            {
                if (_nit == "" || _nit == null)
                {
                    ret = "-1|El NIT es obligatorio";
                }
                else if (!validaNIt(_nit))
                {
                    ret = "-1|El NIT es inválido";
                }
            }

            if (ret == "")
            {
                ret = "Correcto";
            }

            return ret;
        }
        catch (Exception e)
        {
            ax.TTSAbort();
            ax.Logoff();
            return "Error encontrado: " + e.Message;
            // Take other error action as needed.
        }
    }

    [WebMethod(Description = "Crea los empleados en Dynamics desde la aplicacion de Carne", EnableSession = false)]
    // 21-08-2013, Roberto Castro, Crea los empleados en Dynamics desde la aplicacion de Carne
    public String nuevoEmpleadoDynamics(string _emplId, string _dpi, string _primerNombre, string _segundoNombre, string _primerApellido, string _segundoApellido,
                                        string _apellidoCasada, string _fechaNac, int _estadoCivil, int _sexo, string _paisNac,
                                        string _depNac, string _muniNac, string _depDpi, string _muniDpi)
    {
        // Variables para el tratamiento de datos de Dynamics AX
        Axapta ax = new Axapta();
        string[] ParmsClass = new string[15];
        string resp;

        ParmsClass[0] = _dpi;
        ParmsClass[1] = _primerNombre;
        ParmsClass[2] = _segundoNombre;
        ParmsClass[3] = _primerApellido;
        ParmsClass[4] = _segundoApellido;
        ParmsClass[5] = _apellidoCasada;
        ParmsClass[6] = _fechaNac;
        ParmsClass[7] = Convert.ToString(_estadoCivil);
        ParmsClass[8] = Convert.ToString(_sexo);
        ParmsClass[9] = _paisNac;
        ParmsClass[10] = _depNac;
        ParmsClass[11] = _muniNac;
        ParmsClass[12] = _depDpi; ;
        ParmsClass[13] = _muniDpi;
        ParmsClass[14] = (_emplId != "" ? _emplId : "");


        //////////// PROCESO /////////////
        // Configuracion de los parametros necesarios para la conexion a la Base de datos de Oracle
        // Inicia sesion en Dynamics
        ax.Logon(null, null, null, null);
        // Crea um registo de la tabla respectiva enviada
        resp = Convert.ToString(ax.CallStaticClassMethod("WSInterface", "newEmplInterface", ParmsClass));

        return resp;
    }

    [WebMethod(Description = "Obtiene el nombre del empleado de Dynamics según el codigo", EnableSession = false)]
    // 21-08-2013, Roberto Castro, Crea los empleados en Dynamics desde la aplicacion de Carne
    public String obtenerNombre(string _emplId)
    {
        string ret = "";
        Axapta ax = new Axapta();
        try
        {

            AxaptaRecord axRecordET, axRecirdDPT;
            string EmplTable = "EmplTable", DirPartyTable = "DirPartyTable";
            string fieldName = "";

            String strQuery = "select firstfast %1 where %1.EmplId == '" + _emplId + "'";

            //////////// PROCESO /////////////
            // Inicia sesion en Dynamics
            ax.Logon(null, null, null, null);
            // Realiza una llamada al metodo estatico de Dynamics
            //ret = Convert.ToString(ax.CallStaticClassMethod("WSInterface", "newIEmplVendCodeRefTable", parms));
            //ax.CallStaticClassMethod("WSInterface", "newIEmplVendCodeRefTable", parms);
            ax.TTSBegin();
            axRecordET = ax.CreateAxaptaRecord(EmplTable);
            axRecordET.ExecuteStmt(strQuery);

            if (axRecordET.Found)
            {
                // Provee los valores para cada uno de los campos del registro de la tabla UGLCareersTable
                //axRecord.set_Field(fieldName, _codigo);

                // Actualiza el registro en Dynamics
                //axRecord.Update();
                String strQuery2 = "select firstfast %1 where %1.PartyId == '" + axRecordET.get_Field("PARTYID").ToString() + "'";
                axRecirdDPT = ax.CreateAxaptaRecord(DirPartyTable);
                axRecirdDPT.ExecuteStmt(strQuery2);

                if (axRecirdDPT.Found)
                {
                    ret = axRecirdDPT.get_Field("NAME").ToString();
                }
                else
                {
                    ret = "Nombre no encontrado";
                }

            }
            else
            {
                ret = "El código de empleado no existe";
            }
            ax.TTSCommit();
            //ret = axRecord.get_Field("IName").ToString();
            // Cierra sesion en Dynamics 
            ax.Logoff();

            if (ret == "")
            {
                ret = "Correcto";
            }

            return ret;
        }
        catch (Exception e)
        {
            ax.TTSAbort();
            ax.Logoff();
            return "Error encontrado: " + e.Message;
            // Take other error action as needed.
        }
    }

    enum DirPartyTable { Ninguno = 0, Persona = 1, Organizacion = 2 };
    [WebMethod(Description = "Busca empleados para la aplicacion de windows", EnableSession = false)]
    // 09-09-2015, Roberto Castro, Busca los empleados segun un criterio de busqueda para la aplicacion de Windows
    public DataTable Buscar(string _txtBusqueda)
    {
        DataTable dt = new DataTable("EmplTable");
        dt.Columns.Add("Correlativo", typeof(string));
        dt.Columns.Add("Codigo", typeof(string));
        dt.Columns.Add("Nombre", typeof(string));
        dt.Columns.Add("Tipo", typeof(string));
        dt.Columns.Add("Extension", typeof(string));
        dt.Columns.Add("Foto", typeof(string));

        // Variables para el tratamiento de datos de Dynamics AX
        Axapta ax = new Axapta();
        AxaptaRecord axEmpleado;
        AxaptaRecord axGrupo;
        AxaptaRecord axGrupoPosicion;
        AxaptaRecord axDepartamentos;
        string TBL_Empleado = "EmplTable";
        string TBL_Grupo = "DirPartyTable";
        string TBL_GrupoPosicion = "HRPPartyPositionTableRelationship";
        string TBL_Departementos = "DirPartyInternalOrganizationTable";

        String sqlEmplado = "select * "
            + "from %1 "
            + "join %2 "
            + "where %2.PARTYID == %1.PARTYID "
            + "&& %1.STATUS == 1 "
            + "&& ((%1.EMPLID LIKE '*" + _txtBusqueda + "*' || '" + _txtBusqueda + "' == '') "
            + "|| (%1.EXTENSION LIKE '*" + _txtBusqueda + "*' || '" + _txtBusqueda + "' == '') "
            + "|| (%2.NAME LIKE '*" + _txtBusqueda.Replace(" ", "*") + "*' || '" + _txtBusqueda + "' == '')) ";

        String sqlEmplado2 = "select * "
            + "from %1 "
            + "join %2 "
            + "where %2.PARTYID == %1.PARTYID "
            + "&& %1.STATUS == 1 "
            + "join %3 "
            + "WHERE %3.REFERENCE == %1.EMPLID "
            + "&& %3.ValidFromDateTime <= DateTimeUtil::getSystemDateTime() "
            + "&& %3.ValidToDateTime >= DateTimeUtil::getSystemDateTime() "
            + "JOIN %4 "
            + "WHERE %4.OrganizationUnitId == %3.OrganizationUnitId"
            + "&& (%4.Description LIKE '*" + _txtBusqueda + "*' || '" + _txtBusqueda + "' == '')";

        // Inicia sesion en Dynamics
        ax.Logon(null, null, null, null);

        // Crea un registo de la tabla respectiva enviada
        axEmpleado = ax.CreateAxaptaRecord(TBL_Empleado);
        axGrupo = ax.CreateAxaptaRecord(TBL_Grupo);
        axGrupoPosicion = ax.CreateAxaptaRecord(TBL_GrupoPosicion);
        axDepartamentos = ax.CreateAxaptaRecord(TBL_Departementos);

        // Elimina todos los registros de la tabla
        ax.ExecuteStmt(sqlEmplado, axEmpleado, axGrupo, axGrupoPosicion);

        // Lee el registro en Dynamics
        while (axEmpleado.Found)
        {
            //Sacar parametros
            DataRow dr = dt.NewRow();
            int Tipo;

            int.TryParse(Convert.ToString(axGrupo.get_Field("TYPE")), out Tipo);

            dr["Correlativo"] = Convert.ToString(axEmpleado.get_Field("PICTUREID"));
            dr["Codigo"] = Convert.ToString(axEmpleado.get_Field("EMPLID"));
            dr["Nombre"] = Convert.ToString(axGrupo.get_Field("NAME"));
            dr["Tipo"] = (DirPartyTable)Tipo;
            dr["Extension"] = Convert.ToString(axEmpleado.get_Field("EXTENSION"));
            dr["Foto"] = "123";

            dt.Rows.Add(dr);

            axEmpleado.Next();
        }

        ax.ExecuteStmt(sqlEmplado2, axEmpleado, axGrupo, axGrupoPosicion, axDepartamentos);

        // Lee el registro en Dynamics
        while (axEmpleado.Found)
        {
            //Sacar parametros
            DataRow dr = dt.NewRow();
            int Tipo;

            int.TryParse(Convert.ToString(axGrupo.get_Field("TYPE")), out Tipo);

            dr["Correlativo"] = Convert.ToString(axEmpleado.get_Field("PICTUREID"));
            dr["Codigo"] = Convert.ToString(axEmpleado.get_Field("EMPLID"));
            dr["Nombre"] = Convert.ToString(axGrupo.get_Field("NAME"));
            dr["Tipo"] = (DirPartyTable)Tipo;
            dr["Extension"] = Convert.ToString(axEmpleado.get_Field("EXTENSION"));
            dr["Foto"] = "123";

            dt.Rows.Add(dr);

            axEmpleado.Next();
        }

        // Cierra sesion en Dynamics
        ax.Logoff();

        return dt;
    }

    [WebMethod(Description = "Busca un empleado especifico para la aplicacion de windows", EnableSession = false)]
    // 10-09-2015, Roberto Castro, Busca un empleado especifico segun el codigo para la aplicacion de Windows
    public void Cargar(string Codigo, out string Nombre, out string Torre, out string Oficina, out string Extension,
        out string Correlativo, out string Departamento, out string Puesto)
    {
        Nombre = string.Empty;
        Torre = string.Empty;
        Oficina = string.Empty;
        Extension = string.Empty;
        Correlativo = string.Empty;
        Departamento = string.Empty;
        Puesto = string.Empty;

        // Variables para el tratamiento de datos de Dynamics AX
        Axapta ax = new Axapta();
        AxaptaRecord axEmpleado;
        AxaptaRecord axGrupo;
        AxaptaRecord axGrupoPosicion;
        AxaptaRecord axDepartamento;
        AxaptaRecord axPuesto;
        string TBL_Empleado = "EmplTable";
        string TBL_Grupo = "DirPartyTable";
        string TBL_GrupoPosicion = "HRPPartyPositionTableRelationship";
        string TBL_Departamento = "DirPartyInternalOrganizationTable";
        string TBL_Puesto = "HRPPartyJobTableRelationship";

        String sqlEmplado = "select * "
            + "from %1 "
            + "WHERE %1.EMPLID == '" + Codigo + "' "
            + "join %2 "
            + "where %2.PARTYID == %1.PARTYID "
            + "outer join %3 "
            + "WHERE %3.REFERENCE == %1.EMPLID "
            + "outer join %4 "
            + "where %4.OrganizationUnitID == %3.OrganizationUnitID "
            + "outer join %5 "
            + "where %5.JobID == %3.JobID";

        // Inicia sesion en Dynamics
        ax.Logon(null, null, null, null);

        // Crea un registo de la tabla respectiva enviada
        axEmpleado = ax.CreateAxaptaRecord(TBL_Empleado);
        axGrupo = ax.CreateAxaptaRecord(TBL_Grupo);
        axGrupoPosicion = ax.CreateAxaptaRecord(TBL_GrupoPosicion);
        axDepartamento = ax.CreateAxaptaRecord(TBL_Departamento);
        axPuesto = ax.CreateAxaptaRecord(TBL_Puesto);

        // Elimina todos los registros de la tabla
        ax.ExecuteStmt(sqlEmplado, axEmpleado, axGrupo, axGrupoPosicion, axDepartamento, axPuesto);

        // Lee el registro en Dynamics
        if (axEmpleado.Found)
        {
            //Sacar parametros
            Torre = Convert.ToString(axEmpleado.get_Field("TOWER"));
            Oficina = Convert.ToString(axEmpleado.get_Field("OFFICE"));
            Extension = Convert.ToString(axEmpleado.get_Field("EXTENSION"));
            Correlativo = Convert.ToString(axEmpleado.get_Field("PICTUREID"));
            Nombre = Convert.ToString(axGrupo.get_Field("NAME"));
            Departamento = Convert.ToString(axDepartamento.get_Field("DESCRIPTION"));
            Puesto = Convert.ToString(axPuesto.get_Field("DESCRIPTION"));
        }

        // Cierra sesion en Dynamics
        ax.Logoff();
    }

    [WebMethod(Description = "Actualiza el empleado de Dynamics desde aplicacion de windows", EnableSession = false)]
    // 10-09-2015, Roberto Castro, Actualiza el empleado de Dynamics desde aplicacion de windows
    public void Guardar(string Codigo, string Torre, string Oficina, string Extension)
    {
        // Variables para el tratamiento de datos de Dynamics AX
        Axapta ax = new Axapta();
        AxaptaRecord axEmpleado;

        string TBL_Empleado = "EmplTable";

        String sqlEmpleado = "select forupdate * "
            + "from %1 "
            + "where %1.EMPLID == '" + Codigo + "'";

        // Inicia sesion en Dynamics
        ax.Logon(null, null, null, null);

        // Crea un registo de la tabla respectiva enviada
        axEmpleado = ax.CreateAxaptaRecord(TBL_Empleado);

        // Ejecuta la consulta
        ax.ExecuteStmt(sqlEmpleado, axEmpleado);

        ax.TTSBegin();

        if (axEmpleado.Found)
        {
            // Valores nuevos para cada campo
            axEmpleado.set_Field("TOWER", Torre);
            axEmpleado.set_Field("OFFICE", Oficina);
            axEmpleado.set_Field("EXTENSION", Extension);

            // Actualiza el registro en Dynamics
            axEmpleado.Update();
        }

        ax.TTSCommit();

        // Cierra sesion en Dynamics
        ax.Logoff();
    }

    [WebMethod(Description = "Obtiene las Torres de Oracle", EnableSession = false)]
    // 14-09-2015, Roberto Castro, Obtiene las Torres Oracle
    public DataSet retTorres()
    {
        try
        {
            // Variables para la conexion y extraccion de datos de Oracle                
            OracleCommand cmd = new OracleCommand();
            DataSet ds = new DataSet();
            DataSet ds2 = new DataSet();
            OracleDataAdapter adapter = new OracleDataAdapter(cmd);
            // Variables de uso

            //OracleCommand cmd = new OracleCommand();
            cmd.CommandText = "DBAFISICC.PKG_CATALOGO.TORRES";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Connection = cn;
            cmd.BindByName = true;

            cmd.Parameters.Add("RETVAL", OracleDbType.RefCursor, 200);
            cmd.Parameters["RETVAL"].Direction = ParameterDirection.Output;

            try
            {
                cn.Open();

                cmd.ExecuteNonQuery();

                OracleRefCursor O_REMCURSOR = (OracleRefCursor)(cmd.Parameters["RETVAL"].Value);

                OracleDataAdapter da = new OracleDataAdapter();
                da.Fill(ds, O_REMCURSOR);

                da.Dispose();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                cn.Close();
                cmd.Dispose();
            }

            return ds;
        }
        catch (Exception e)
        {
            DataSet ds = new DataSet();
            return ds;
        }
    }

    [WebMethod(Description = "Obtiene los Salones de Oracle", EnableSession = false)]
    // 14-09-2015, Roberto Castro, Obtiene las Torres Oracle
    public DataSet retSalones(string _torre)
    {
        try
        {
            // Variables para la conexion y extraccion de datos de Oracle                
            OracleCommand cmd = new OracleCommand();
            DataSet ds = new DataSet();
            DataSet ds2 = new DataSet();
            OracleDataAdapter adapter = new OracleDataAdapter(cmd);
            // Variables de uso

            //OracleCommand cmd = new OracleCommand();
            cmd.CommandText = "DBAFISICC.PKG_CATALOGO.SALONES";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Connection = cn;
            cmd.BindByName = true;

            cmd.Parameters.Add("PTORRE", _torre);
            cmd.Parameters.Add("RETVAL", OracleDbType.RefCursor, 200);
            cmd.Parameters["RETVAL"].Direction = ParameterDirection.Output;

            try
            {
                cn.Open();

                cmd.ExecuteNonQuery();

                OracleRefCursor O_REMCURSOR = (OracleRefCursor)(cmd.Parameters["RETVAL"].Value);

                OracleDataAdapter da = new OracleDataAdapter();
                da.Fill(ds, O_REMCURSOR);

                da.Dispose();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                cn.Close();
                cmd.Dispose();
            }

            return ds;
        }
        catch (Exception e)
        {
            DataSet ds = new DataSet();
            return ds;
        }
    }


    [WebMethod(Description = "Cambiar numero y tipo de cuenta bancario de Docentes en Dynamics", EnableSession = false)]
    // 09-01-2017, Roberto Castro, Cambiar numero y tipo de cuenta bancario de Docentes en Dynamics
    public String cambiaCuentaBancariaDocentesDAX(string _nit, string _codigo, string _banco, string _tipoCuenta, string _numeroCuenta, string _usuarioOracle)
    {
        string ret = "";
        string date = "";
        Axapta ax = new Axapta();
        try
        {
            // Variables para el tratamiento de datos de Dynamics AX                
            AxaptaRecord axRecord;
            string tableName = "IEmplVendCodeRefTable";
            string fieldName = "";

            String strQuery = "select forupdate %1 where %1.IEmplVendCodeRefId == '" + _nit + "' && %1.IEmplIdUGA == '" + _codigo + "'";

            //////////// PROCESO /////////////
            // Inicia sesion en Dynamics
            ax.Logon(null, null, null, null);
            // Realiza una llamada al metodo estatico de Dynamics
            //ret = Convert.ToString(ax.CallStaticClassMethod("WSInterface", "newIEmplVendCodeRefTable", parms));
            //ax.CallStaticClassMethod("WSInterface", "newIEmplVendCodeRefTable", parms);
            ax.TTSBegin();
            axRecord = ax.CreateAxaptaRecord(tableName);
            axRecord.ExecuteStmt(strQuery);

            if (axRecord.Found)
            {
                // Provee los valores para cada uno de los campos del registro de la tabla UGLCareersTable
                axRecord.set_Field("BankAccountEmpl", _numeroCuenta);
                axRecord.set_Field("BankAccountTransId", _banco);
                axRecord.set_Field("TypeBankAccountId", _tipoCuenta);
                axRecord.set_Field("TypeBankAccountId", _tipoCuenta);
                axRecord.set_Field("ModifiedDateCuentaBI", DateTime.Now);
                axRecord.set_Field("UsuarioOracle", _usuarioOracle);

                // Actualiza el registro en Dynamics
                axRecord.Update();
            }
            else
            {
                ret = "No se encontro registro para actualizar";
            }
            ax.TTSCommit();
            //ret = axRecord.get_Field("IName").ToString();
            // Cierra sesion en Dynamics 
            ax.Logoff();

            if (ret == "")
            {
                ret = "Correcto";
            }

            return ret;
        }
        catch (Exception e)
        {
            ax.TTSAbort();
            ax.Logoff();
            return "Error encontrado: " + e.Message;
            // Take other error action as needed.
        }
    }

    [WebMethod(Description = "Cambiar numero y tipo de cuenta bancario de Docentes en Dynamics", EnableSession = false)]
    // 09-01-2017, Roberto Castro, Cambiar numero y tipo de cuenta bancario de Docentes en Dynamics
    public List<DocenteDAX> infoDocentesDAX(string _codPers)
    {
        string ret = "";
        string date = "";
        Axapta ax = new Axapta();
        List<DocenteDAX> docenteList = new List<DocenteDAX>();
        try
        {
            // Variables para el tratamiento de datos de Dynamics AX                
            AxaptaRecord axRecord;
            string tableName = "IEmplVendCodeRefTable";

            String strQuery = "select %1 where %1.IEmplIdUGA == '" + _codPers + "'";

            //////////// PROCESO /////////////
            // Inicia sesion en Dynamics
            ax.Logon(null, null, null, null);
            // Realiza una llamada al metodo estatico de Dynamics
            //ret = Convert.ToString(ax.CallStaticClassMethod("WSInterface", "newIEmplVendCodeRefTable", parms));
            //ax.CallStaticClassMethod("WSInterface", "newIEmplVendCodeRefTable", parms);
            ax.TTSBegin();
            axRecord = ax.CreateAxaptaRecord(tableName);
            axRecord.ExecuteStmt(strQuery);


            while (axRecord.Found)
            {
                docenteList.Add(new DocenteDAX
                {
                    Nit = Convert.ToString(axRecord.get_Field("IEmplVendCodeRefId"))
                                                ,
                    Nombre = Convert.ToString(axRecord.get_Field("IName"))
                                                ,
                    CodTutor = Convert.ToString(axRecord.get_Field("IEmplIdIDEA"))
                                                ,
                    CodProveedor = Convert.ToString(axRecord.get_Field("VendAccount"))
                                                ,
                    FormaPago = Convert.ToString(axRecord.get_Field("IPaymMode"))
                                                ,
                    CodigoBanco = Convert.ToString(axRecord.get_Field("BankAccountTransId"))
                                                ,
                    CodigoTipoBanco = Convert.ToString(axRecord.get_Field("TypeBankAccountId"))
                                                ,
                    NumeroCuenta = Convert.ToString(axRecord.get_Field("BankAccountEmpl"))
                });

                axRecord.Next();
            }

            ax.TTSCommit();
            // Cierra sesion en Dynamics 
            ax.Logoff();

            if (ret == "")
            {
                ret = "Correcto";
            }

            return docenteList;
        }
        catch (Exception e)
        {
            ax.TTSAbort();
            ax.Logoff();
            //return "Error encontrado: " + e.Message;
            return docenteList;
            // Take other error action as needed.
        }
    }

    public bool validaNIt(string _nitNum)
    {
        string numero;
        int verificador, i, total = 0, modulo,salida;
        bool valid = true;
        ;

        // Obtengo las variables del NIT
        _nitNum = _nitNum.Replace("-", "").Replace(" ", "");

        for (i = 0; i < _nitNum.Length; i++)
        {
            if (!int.TryParse(_nitNum.Substring(i, 1), out salida))
            {
                if (!(i == _nitNum.Length && _nitNum.Substring(i, 1) == "K"))
                {
                    valid = false;
                }
            }
        }

        if (valid)
        {
            numero = _nitNum.Substring(0, _nitNum.Length - 1);
            verificador = Convert.ToInt32((_nitNum.Substring(_nitNum.Length - 1, 1).ToUpper() == "K" ? "10" : _nitNum.Substring(_nitNum.Length - 1, 1)));

            // Se aplica algoritmo
            for (i = 0; i < numero.Length; i++)
            {
                total = total + (Convert.ToInt32(numero.Substring(i, 1)) * ((numero.Length + 1) - (i - 1)));
            }
            //info(strfmt("%1",total));
            modulo = total % 11;
            modulo = 11 - modulo;
            modulo = modulo % 11;


            if (modulo == verificador && total != 0)
            {
                valid = true;
            }
            else
            {
                valid = false;
            }
        }
        
        return valid;
    }

    public class DocenteDAX
    {
        public string Nit { get; set; }
        public string Nombre { get; set; }
        public string CodTutor { get; set; }
        public string CodProveedor { get; set; }
        public string FormaPago { get; set; }
        public string CodigoBanco { get; set; }
        public string CodigoTipoBanco { get; set; }
        public string NumeroCuenta { get; set; }
    }
}