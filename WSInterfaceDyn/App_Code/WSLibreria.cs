using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using Microsoft.Dynamics.BusinessConnectorNet;

/// <summary>
/// Descripción breve de WSLibreria
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// Para permitir que se llame a este servicio web desde un script, usando ASP.NET AJAX, quite la marca de comentario de la línea siguiente. 
// [System.Web.Script.Services.ScriptService]
public class WSLibreria : System.Web.Services.WebService
{
    OracleConnection cn = new OracleConnection(ConfigurationManager.ConnectionStrings["galileo"].ConnectionString);
    public WSLibreria()
    {

        //Elimine la marca de comentario de la línea siguiente si utiliza los componentes diseñados 
        //InitializeComponent(); 
    }

    [WebMethod(Description = "Obtiene el nombre del alumno", EnableSession = false)]
    // 05-03-2021, Roberto Castro, Obtiene el nombre del alumno
    public string nombreAlumno(string _carne)
    {
        string valor = "";
        try
        {
            OracleCommand cmd = new OracleCommand();

            cmd.CommandText = "DBAFISICC.PKG_ALUMNO.NOMBRE";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Connection = cn;
            cmd.BindByName = true;

            cmd.Parameters.Add("PCARNET", _carne);
            cmd.Parameters.Add("POPCION", 2);

            OracleParameter pnombre = new OracleParameter("VRESP", OracleDbType.Varchar2, 2000);
            pnombre.Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add(pnombre);

            cn.Open();
            cmd.ExecuteNonQuery();
            valor += pnombre.Value.ToString();

            cn.Close();
            cmd.Dispose();

            return valor;
        }
        catch (Exception e)
        {
            return valor;
        }
    }


    [WebMethod(Description = "Obtienelos cursos asignados del alumno", EnableSession = false)]
    // 05-03-2021, Roberto Castro, Obtiene "Obtienelos cursos asignados del alumno
    public DataTable cursosAsigAlumno(string _carne)
    {
        DataTable dt = new DataTable();
        try
        {
            OracleCommand cmd = new OracleCommand();
            DataSet ds = new DataSet();
            OracleDataAdapter adapter = new OracleDataAdapter(cmd);
            cmd.CommandText = "DBAFISICC.PKG_ALUMNO.SEL_CURSOSASIG";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Connection = cn;
            cmd.BindByName = true;

            cmd.Parameters.Add("PCARNET", _carne);
            cmd.Parameters.Add("PCARRERA", null);
            cmd.Parameters.Add("PPERIODO", null);
            cmd.Parameters.Add("RETVAL", OracleDbType.RefCursor, 200);
            cmd.Parameters["RETVAL"].Direction = ParameterDirection.Output;

            cn.Open();
            cmd.ExecuteNonQuery();
            OracleRefCursor O_REMCURSOR = (OracleRefCursor)(cmd.Parameters["RETVAL"].Value);

            OracleDataAdapter da = new OracleDataAdapter();
            da.Fill(ds, O_REMCURSOR);

            da.Dispose();

            cn.Close();
            cmd.Dispose();
            
            dt = ds.Tables[0];

            return dt;
        }
        catch (Exception e)
        {
            return dt;
        }
    }


    [WebMethod(Description = "Crea las ordenes de venta para rebaja de inventario", EnableSession = false)]
    // 10-03-2021, Roberto Castro, Crea las ordenes de venta para rebaja de inventario
    public string createSalesOrder(string _detalleLibros)
    {
        Axapta ax = new Axapta();
        try
        {            
            string[] ParmsClass = new string[1];
            ax.Logon(null, null, null, null);
            ParmsClass[0] = Convert.ToString(_detalleLibros);
            ax.CallStaticClassMethod("WSInterface", "createSalesOrder", ParmsClass);
            ax.Logoff();
            return "correcto";
        }
        catch (Exception e)
        {
            ax.Logoff();
            //return e.Message;
            string err = e.Message.Replace("\u001a", "").Replace("\n"," ");
            return err;
        }
    }


    [WebMethod(Description = "Anula las ordenes de venta para devolver el inventario", EnableSession = false)]
    // 10-03-2021, Roberto Castro, Anula las ordenes de venta para devolver el inventario
    public string cancelSalesOrder(string _recibo)
    {
        Axapta ax = new Axapta();
        try
        {
            string[] ParmsClass = new string[1];
            ax.Logon(null, null, null, null);
            ParmsClass[0] = Convert.ToString(_recibo);
            ax.CallStaticClassMethod("WSInterface", "cancelSalesOrder", ParmsClass);
            ax.Logoff();
            return "correcto";
        }
        catch (Exception e)
        {
            ax.Logoff();
            //return e.Message;
            string err = e.Message.Replace("\u001a", "").Replace("\n", " ");
            return err;
        }
    }

}
