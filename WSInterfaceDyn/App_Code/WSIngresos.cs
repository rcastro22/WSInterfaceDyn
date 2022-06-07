using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data.SqlClient;

/// <summary>
/// Descripción breve de WSIngresos
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// Para permitir que se llame a este servicio web desde un script, usando ASP.NET AJAX, quite la marca de comentario de la línea siguiente. 
// [System.Web.Script.Services.ScriptService]
public class WSIngresos : System.Web.Services.WebService
{

    SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqldax"].ConnectionString);
    OracleConnection cn = new OracleConnection(ConfigurationManager.ConnectionStrings["dynamics"].ConnectionString);
    public WSIngresos()
    {

        //Elimine la marca de comentario de la línea siguiente si utiliza los componentes diseñados 
        //InitializeComponent(); 
    }

    [WebMethod(Description = "Obtiene los registros de la tabla INGRESOS", EnableSession = false)]
    // 15-06-2021, Roberto Castro, Obtiene los registros de la tabla INGRESOS
    public DataTable ingresos(string _origen, string _fromDate, string _toDate)
    {
        DataTable dt = new DataTable();
        try
        {
            OracleCommand cmd;
            DataSet ds = new DataSet();

            string sqlSelect;
            sqlSelect = "SELECT RECIBO, TO_CHAR(FECHA,'DD/MM/YYYY') FECHA, CARRERA, TIPO, TOTAL, ORIG, ";
            sqlSelect += "NVL(STATUS,'<>') STATUS, NVL(CARNE,'<>') CARNE, NVL(ALUMNO,'<>') ALUMNO, NVL(CARNOMBRE,'<>') CARNOMBRE, NVL(TIPOCLIENTE,'<>') TIPOCLIENTE, ";
            sqlSelect += "NVL(CCODE,'<>') CCODE, NVL(CFACULTAD,'<>') CFACULTAD, NVL(FOPERATE,TO_DATE('01/01/1900','DD/MM/YYYY')) FOPERATE, ";
            sqlSelect += "NVL(FREGISTER,TO_DATE('01/01/1900','DD/MM/YYYY')) FREGISTER, NVL(FUP,TO_DATE('01/01/1900','DD/MM/YYYY')) FUP, ROWID, NVL(CARRERA_SEDE,'<>') CARRERA_SEDE, ";
            sqlSelect += "NVL(RAZON_SOCIAL,'<>') RAZON_SOCIAL, NVL(NIT,'<>') NIT, NVL(SERIE,'<>') SERIE, NVL(PREIMPRESO,0) PREIMPRESO, NVL(AUTORIZACION,'<>') AUTORIZACION, NVL(FECHA_EMISIONFEL,TO_DATE('01/01/1900','DD/MM/YYYY')) FECHA_EMISIONFEL, ";
            sqlSelect += "NVL(STATUS_FEL,'<>') STATUS_FEL ";
            sqlSelect += "FROM DYNAMICS.INGRESOS WHERE TRUNC(FREGISTER) >= to_date('" + _fromDate + "','dd/mm/yyyy') AND TRUNC(FREGISTER) <= to_date('" + _toDate + "','dd/mm/yyyy')";
            sqlSelect += "AND ORIG = '" + _origen + "'";
            sqlSelect += "ORDER BY 1";

            cn.Open();
            cmd = new OracleCommand(sqlSelect, cn);
            OracleDataReader rd;
            rd = cmd.ExecuteReader();
            dt.Load(rd);

            cn.Close();
            cmd.Dispose();

            return dt;
        }
        catch (Exception e)
        {
            return dt;
        }
    }


    [WebMethod(Description = "Obtiene los registros de la tabla INGRESOS_CONCEPTOS", EnableSession = false)]
    // 15-06-2021, Roberto Castro, Obtiene los registros de la tabla INGRESOS_CONCEPTOS
    public DataTable ingresos_conceptos(string _recibo, string _fecha, string _tipo)
    {
        DataTable dt = new DataTable();
        try
        {
            OracleCommand cmd;
            DataSet ds = new DataSet();

            string sqlSelect;
            sqlSelect = "SELECT CONCEPTO, VALOR, TO_CHAR(FECHA,'DD/MM/YYYY') FECHA, TIPO, CORRELATIVO, ROWID ";
            sqlSelect += "FROM DYNAMICS.INGRESOS_CONCEPTOS";
            sqlSelect += " WHERE RECIBO = '" + _recibo + "'";
            sqlSelect += " AND TRUNC(FECHA) = TO_DATE('" + _fecha + "','DD/MM/YYYY')";
            sqlSelect += " AND TIPO = '" + _tipo + "'";

            cn.Open();
            cmd = new OracleCommand(sqlSelect, cn);
            OracleDataReader rd;
            rd = cmd.ExecuteReader();
            dt.Load(rd);

            cn.Close();
            cmd.Dispose();

            return dt;
        }
        catch (Exception e)
        {
            return dt;
        }
    }



    [WebMethod(Description = "Inserta registros en la tabla HINGRESOS", EnableSession = false)]
    // 15-06-2021, Roberto Castro, Inserta registros en la tabla HINGRESOS
    public int insertHingresos(string _recibo, string _fecha, string _tipo)
    {
        int contador = 0;
        try
        {
            OracleCommand cmd;

            string sqlInsert;
            sqlInsert = "INSERT INTO DYNAMICS.HINGRESOS (RECIBO, FECHA, CARRERA, TIPO, TOTAL, ORIG, STATUS, CARNE, ALUMNO, CARNOMBRE, TIPOCLIENTE, CCODE, CFACULTAD, FOPERATE, FREGISTER, FUP, FECHA_TRASLADO,CARRERA_SEDE, RAZON_SOCIAL, NIT, SERIE, PREIMPRESO, AUTORIZACION, FECHA_EMISIONFEL, STATUS_FEL) ";
            sqlInsert += " SELECT RECIBO, FECHA, CARRERA, TIPO, TOTAL, ORIG, STATUS, CARNE, ALUMNO, CARNOMBRE, TIPOCLIENTE, CCODE, CFACULTAD, FOPERATE, FREGISTER, FUP, SYSDATE,CARRERA_SEDE, RAZON_SOCIAL, NIT, SERIE, PREIMPRESO, AUTORIZACION, FECHA_EMISIONFEL, STATUS_FEL ";
            sqlInsert += " FROM DYNAMICS.INGRESOS";
            sqlInsert += " WHERE RECIBO = '" + _recibo + "'";
            sqlInsert += "  AND TRUNC(FECHA) = TO_DATE('" + _fecha + "','DD/MM/YYYY')";
            sqlInsert += "  AND TIPO = '" + _tipo + "'";

            cn.Open();
            cmd = new OracleCommand(sqlInsert, cn);
            contador = cmd.ExecuteNonQuery();

            cn.Close();
            cmd.Dispose();

            return contador;
        }
        catch (Exception e)
        {
            return contador;
        }
    }

    [WebMethod(Description = "Inserta registros en la tabla HINGRESOS_CONCEPTOS", EnableSession = false)]
    // 15-06-2021, Roberto Castro, Inserta registros en la tabla HINGRESOS_CONCEPTOS
    public int insertHingresos_Conceptos(string _recibo, string _fecha, string _tipo)
    {
        int contador = 0;
        try
        {
            OracleCommand cmd;

            string sqlInsert;
            sqlInsert = "INSERT INTO DYNAMICS.HINGRESOS_CONCEPTOS (RECIBO, CONCEPTO, VALOR, FECHA, TIPO, CORRELATIVO, FECHA_TRASLADO, FREGISTER)";
            sqlInsert += " SELECT RECIBO, CONCEPTO, VALOR, FECHA, TIPO, CORRELATIVO, SYSDATE, FREGISTER ";
            sqlInsert += " FROM DYNAMICS.INGRESOS_CONCEPTOS";
            sqlInsert += " WHERE RECIBO = '" + _recibo + "'";
            sqlInsert += "  AND TRUNC(FECHA) = TO_DATE('" + _fecha + "','DD/MM/YYYY')";
            sqlInsert += "  AND TIPO = '" + _tipo + "'";

            cn.Open();
            cmd = new OracleCommand(sqlInsert, cn);
            contador = cmd.ExecuteNonQuery();

            cn.Close();
            cmd.Dispose();

            return contador;
        }
        catch (Exception e)
        {
            return contador;
        }
    }

    [WebMethod(Description = "Inserta registros en la tabla HINGRESOS_PAGO", EnableSession = false)]
    // 15-06-2021, Roberto Castro, Inserta registros en la tabla HINGRESOS_PAGO
    public int insertHingresos_pago(string _recibo, string _fecha, string _tipo)
    {
        int contador = 0;
        try
        {
            OracleCommand cmd;

            string sqlInsert;
            sqlInsert = "INSERT INTO DYNAMICS.HINGRESOS_PAGO (RECIBO, FECHA, TIPO, CORRELATIVO, TIPOPAGO, EMISOR, DOCUMENTO, MONTO, FECHA_TRASLADO, FREGISTER)";
            sqlInsert += " SELECT RECIBO, FECHA, TIPO, CORRELATIVO, TIPOPAGO, EMISOR, DOCUMENTO, MONTO, SYSDATE, FREGISTER ";
            sqlInsert += " FROM DYNAMICS.INGRESOS_PAGO ";
            sqlInsert += " WHERE RECIBO = '" + _recibo + "'";
            sqlInsert += "  AND TRUNC(FECHA) = TO_DATE('" + _fecha + "','DD/MM/YYYY')";
            sqlInsert += "  AND TIPO = '" + _tipo + "'";

            cn.Open();
            cmd = new OracleCommand(sqlInsert, cn);
            contador = cmd.ExecuteNonQuery();

            cn.Close();
            cmd.Dispose();

            return contador;
        }
        catch (Exception e)
        {
            return contador;
        }
    }

    [WebMethod(Description = "Elimina registros en la tabla INGRESOS", EnableSession = false)]
    // 15-06-2021, Roberto Castro, Elimina registros en la tabla INGRESOS
    public int deleteIngresos(string _recibo, string _fecha, string _tipo)
    {
        int contador = 0;
        try
        {
            OracleCommand cmd;

            string sqlDelete;
            sqlDelete = "DELETE FROM DYNAMICS.INGRESOS ";
            sqlDelete += " WHERE RECIBO = '" + _recibo + "'";
            sqlDelete += "  AND TRUNC(FECHA) = TO_DATE('" + _fecha + "','DD/MM/YYYY')";
            sqlDelete += "  AND TIPO = '" + _tipo + "'";

            cn.Open();
            cmd = new OracleCommand(sqlDelete, cn);
            contador = cmd.ExecuteNonQuery();

            cn.Close();
            cmd.Dispose();

            return contador;
        }
        catch (Exception e)
        {
            return contador;
        }
    }

    [WebMethod(Description = "Elimina registros en la tabla INGRESOS_CONCEPTOS", EnableSession = false)]
    // 15-06-2021, Roberto Castro, Elimina registros en la tabla INGRESOS_CONCEPTOS
    public int deleteIngresos_Conceptos(string _recibo, string _fecha, string _tipo)
    {
        int contador = 0;
        try
        {
            OracleCommand cmd;

            string sqlDelete;
            sqlDelete = "DELETE FROM DYNAMICS.INGRESOS_CONCEPTOS ";
            sqlDelete += " WHERE RECIBO = '" + _recibo + "'";
            sqlDelete += "  AND TRUNC(FECHA) = TO_DATE('" + _fecha + "','DD/MM/YYYY')";
            sqlDelete += "  AND TIPO = '" + _tipo + "'";

            cn.Open();
            cmd = new OracleCommand(sqlDelete, cn);
            contador = cmd.ExecuteNonQuery();

            cn.Close();
            cmd.Dispose();

            return contador;
        }
        catch (Exception e)
        {
            return contador;
        }
    }

    [WebMethod(Description = "Elimina registros en la tabla INGRESOS_PAGO", EnableSession = false)]
    // 15-06-2021, Roberto Castro, Elimina registros en la tabla INGRESOS_PAGO
    public int deleteIngresos_pago(string _recibo, string _fecha, string _tipo)
    {
        int contador = 0;
        try
        {
            OracleCommand cmd;

            string sqlDelete;
            sqlDelete = "DELETE FROM DYNAMICS.INGRESOS_PAGO ";
            sqlDelete += " WHERE RECIBO = '" + _recibo + "'";
            sqlDelete += "  AND TRUNC(FECHA) = TO_DATE('" + _fecha + "','DD/MM/YYYY')";
            sqlDelete += "  AND TIPO = '" + _tipo + "'";

            cn.Open();
            cmd = new OracleCommand(sqlDelete, cn);
            contador = cmd.ExecuteNonQuery();

            cn.Close();
            cmd.Dispose();

            return contador;
        }
        catch (Exception e)
        {
            return contador;
        }
    }


    [WebMethod(Description = "Cuenta registros en la tabla INGRESOS_CONCEPTOS", EnableSession = false)]
    // 15-06-2021, Roberto Castro, Cuenta registros en la tabla INGRESOS_CONCEPTOS
    public int countIngresos_conceptos(string _origen, string _fromDate, string _toDate)
    {
        int contador = 0;
        try
        {
            OracleCommand cmd;

            string sqlSelect;
            sqlSelect = "select count(*)";
            sqlSelect += " from dynamics.ingresos_conceptos a, dynamics.ingresos b";
            sqlSelect += " where b.tipo = a.tipo";
            sqlSelect += " and b.fecha = a.fecha";
            sqlSelect += " and b.recibo = a.recibo";
            sqlSelect += " and b.ORIG = '" + _origen + "' AND TRUNC(b.FREGISTER) >= to_date('" + _fromDate + "','dd/mm/yyyy') AND TRUNC(b.FREGISTER) <= to_date('" + _toDate + "','dd/mm/yyyy')";

            cn.Open();
            cmd = new OracleCommand(sqlSelect, cn);
            contador = Convert.ToInt32(cmd.ExecuteScalar());

            cn.Close();
            cmd.Dispose();

            return contador;
        }
        catch (Exception e)
        {
            return contador;
        }
    }

    [WebMethod(Description = "Cuenta registros en la tabla INGRESOS_PAGO", EnableSession = false)]
    // 15-06-2021, Roberto Castro, Cuenta registros en la tabla INGRESOS_PAGO
    public int countIngresos_pago(string _origen, string _fromDate, string _toDate)
    {
        int contador = 0;
        try
        {
            OracleCommand cmd;

            string sqlSelect;
            sqlSelect = "select count(*)";
            sqlSelect += " from dynamics.INGRESOS_PAGO a, dynamics.ingresos b";
            sqlSelect += " where b.tipo = a.tipo";
            sqlSelect += " and b.fecha = a.fecha";
            sqlSelect += " and b.recibo = a.recibo";
            sqlSelect += " and b.ORIG = '" + _origen + "' AND TRUNC(b.FREGISTER) >= to_date('" + _fromDate + "','dd/mm/yyyy') AND TRUNC(b.FREGISTER) <= to_date('" + _toDate + "','dd/mm/yyyy')";

            cn.Open();
            cmd = new OracleCommand(sqlSelect, cn);
            contador = Convert.ToInt32(cmd.ExecuteScalar());

            cn.Close();
            cmd.Dispose();

            return contador;
        }
        catch (Exception e)
        {
            return contador;
        }
    }

    [WebMethod(Description = "Cuenta registros en la tabla INGRESOS", EnableSession = false)]
    // 15-06-2021, Roberto Castro, Cuenta registros en la tabla INGRESOS
    public int countIngresos(string _origen, string _fromDate, string _toDate)
    {
        int contador = 0;
        try
        {
            OracleCommand cmd;

            string sqlSelect;
            sqlSelect = "select count(*) from DYNAMICS.INGRESOS WHERE ORIG = '" + _origen + "' AND TRUNC(FREGISTER) >= to_date('" + _fromDate + "','dd/mm/yyyy') AND TRUNC(FREGISTER) <= to_date('" + _toDate + "','dd/mm/yyyy')";

            cn.Open();
            cmd = new OracleCommand(sqlSelect, cn);
            contador = Convert.ToInt32(cmd.ExecuteScalar());

            cn.Close();
            cmd.Dispose();

            return contador;
        }
        catch (Exception e)
        {
            return contador;
        }
    }

    [WebMethod(Description = "Listado de carreras que estan en Oracle pero no en dynamics para UG: Fecha:dd/MM/YYYY", EnableSession = false)]
    public DataSet carrerasFaltantes(string _origen, string _fromDate, string _toDate)
    {
        string sql = "";

        sql = "SELECT CARRERA ";
        sql += "FROM DYNAMICS.INGRESOS WHERE TRUNC(FREGISTER) >= to_date('" + _fromDate + "','dd/mm/yyyy') AND TRUNC(FREGISTER) <= to_date('" + _toDate + "','dd/mm/yyyy') ";
        sql += "AND ORIG = '" + _origen + "' ";
        sql += "GROUP BY CARRERA";
                

        conn.Open();

        OracleDataAdapter da = new OracleDataAdapter(sql, cn);
        DataTable dt = new DataTable();

        da.Fill(dt);

        SqlCommand cmd = new SqlCommand("", conn);

        DataTable table = new DataTable("cat");
        DataColumn column = new DataColumn();
        column.DataType = System.Type.GetType("System.String");
        column.AllowDBNull = false;
        column.Caption = "CARRERA";
        column.ColumnName = "CARRERA";

        table.Columns.Add(column);

        string codigos = "";

        foreach (DataRow row in dt.Rows)
        {
            cmd = new SqlCommand("dbo.buscaCarrerasIngresos", conn);
            SqlParameter parm = new SqlParameter("@iCareersId", row["CARRERA"]);
            cmd.Parameters.Add(parm);
            cmd.CommandType = CommandType.StoredProcedure;
            SqlDataReader rdr = cmd.ExecuteReader();
            int contar = 0;
            while (rdr.Read())
            {
                contar = Convert.ToInt32(rdr["CONTEO"]);
            }

            if (contar == 0)
            {
                DataRow r = table.NewRow();
                r["CARRERA"] = row["CARRERA"];
                table.Rows.Add(r);

            }
        }


        DataSet ds = new DataSet();

        ds.Tables.Add(table);

        //sql = "select COUNT(*) from iEmplVendCodeRefTable where IEMPLIDUGA in(" + codigos +")";

        cn.Close();
        conn.Close();

        return ds;
    }

    [WebMethod(Description = "Listado de carreras que estan en Oracle pero no en dynamics para UG: Fecha:dd/MM/YYYY", EnableSession = false)]
    public DataSet conceptosFaltantes(string _origen, string _fromDate, string _toDate)
    {
        string sql = "";

        sql = "select b.CONCEPTO CONCEPTO ";
        sql += "from dynamics.ingresos a, dynamics.ingresos_conceptos b ";
        sql += "where a.recibo = b.recibo and a.fecha = b.fecha and a.tipo = b.tipo and a.fregister = b.fregister ";
        sql += "AND TRUNC(a.FREGISTER) >= to_date('" + _fromDate + "','dd/mm/yyyy') AND TRUNC(a.FREGISTER) <= to_date('" + _toDate + "','dd/mm/yyyy') ";
        sql += "AND a.ORIG = '" + _origen + "' ";
        sql += "GROUP BY b.CONCEPTO";


        conn.Open();

        OracleDataAdapter da = new OracleDataAdapter(sql, cn);
        DataTable dt = new DataTable();

        da.Fill(dt);

        SqlCommand cmd = new SqlCommand("", conn);

        DataTable table = new DataTable("cat");
        DataColumn column = new DataColumn();
        column.DataType = System.Type.GetType("System.String");
        column.AllowDBNull = false;
        column.Caption = "CONCEPTO";
        column.ColumnName = "CONCEPTO";

        table.Columns.Add(column);

        string codigos = "";

        foreach (DataRow row in dt.Rows)
        {
            cmd = new SqlCommand("dbo.buscaConcepto", conn);
            SqlParameter parm = new SqlParameter("@iJournalCodeRefId", row["CONCEPTO"]);
            SqlParameter parm2 = new SqlParameter("@orig", (_origen == "UGA" ? 2 : 1));
            cmd.Parameters.Add(parm);
            cmd.Parameters.Add(parm2);
            cmd.CommandType = CommandType.StoredProcedure;
            SqlDataReader rdr = cmd.ExecuteReader();
            int contar = 0;
            while (rdr.Read())
            {
                contar = Convert.ToInt32(rdr["CONTEO"]);
            }

            if (contar == 0)
            {
                DataRow r = table.NewRow();
                r["CONCEPTO"] = row["CONCEPTO"];
                table.Rows.Add(r);

            }
        }


        DataSet ds = new DataSet();

        ds.Tables.Add(table);

        //sql = "select COUNT(*) from iEmplVendCodeRefTable where IEMPLIDUGA in(" + codigos +")";

        cn.Close();
        conn.Close();

        return ds;
    }

}
