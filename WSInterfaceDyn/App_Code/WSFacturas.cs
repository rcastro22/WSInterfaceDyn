using Microsoft.Dynamics.BusinessConnectorNet;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;

/// <summary>
/// Descripción breve de WSFacturas
/// </summary>
[WebService(Namespace = "http://galileo.edu/dynamicsax/facturas")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// Para permitir que se llame a este servicio web desde un script, usando ASP.NET AJAX, quite la marca de comentario de la línea siguiente. 
// [System.Web.Script.Services.ScriptService]
public class WSFacturas : System.Web.Services.WebService
{
    OracleConnection cn = new OracleConnection(ConfigurationManager.ConnectionStrings["dynamics"].ConnectionString);
    public WSFacturas()
    {

        //Elimine la marca de comentario de la línea siguiente si utiliza los componentes diseñados 
        //InitializeComponent(); 
    }

    [WebMethod(Description = "Ingresa las facturas del personal", EnableSession = false)]
    // 07-04-2020, Roberto Castro, Ingresa las facturas de docentes y administrativos que facturan para la Universidad
    public String agregarFactura(string _proveedor, string _facturaserie, string _facturanumero, string _fechafactura, string _descripcion, decimal _monto, string _iddetalle, int _typeStaff, string _usuariooracle, string _noTramite)
    {
        
        Axapta ax = new Axapta();
        try
        {
            AxaptaRecord axRecord;
            string tableName = "UG_TmpInvoiceStaff";

            ax.Logon(null, null, null, null);
            ax.TTSBegin();
            axRecord = ax.CreateAxaptaRecord(tableName);
            axRecord.set_Field("VendAccount", _proveedor);
            axRecord.set_Field("InvoiceSerie", _facturaserie);
            axRecord.set_Field("InvoiceNumber", _facturanumero);
            axRecord.set_Field("DocumentDate", Convert.ToDateTime(_fechafactura));
            axRecord.set_Field("DescriptionInvoice", _descripcion);
            axRecord.set_Field("Amount", Convert.ToDecimal(_monto));
            axRecord.set_Field("RefRecIdDetail", _iddetalle);
            axRecord.set_Field("Staff", _typeStaff);
            axRecord.set_Field("UsuarioOracle", _usuariooracle);
            axRecord.set_Field("NoTramiteOracle", _noTramite);
            // Inserta el registro en Dynamics
            axRecord.Insert();

            ax.TTSCommit();
            ax.Logoff();

            return "Correcto";
        }
        catch (Exception e)
        {
            ax.TTSAbort();
            ax.Logoff();
            return "Error encontrado: " + e.Message;
        }
    }


    [WebMethod(Description = "Valida que la factura no exista en Dynamics", EnableSession = false)]
    // 07-04-2020, Roberto Castro, Valida que la factura no exista en Dynamics
    public bool validarFactura(string _proveedor, string _facturaserie, string _facturanumero)
    {
        bool ret;
        Axapta ax = new Axapta();
        try
        {
            AxaptaRecord axRecord;
            string tableName = "vendInvoiceJour";
            string strQuery = String.Format("select InvoiceDate,InvoiceAmount from %1 index hint InvoiceIdx where %1.InvoiceAccount == '{0}' && %1.InvoiceId == '{1}-{2}' && %1.InvoiceAmount != 0",_proveedor,_facturaserie,_facturanumero);

            ax.Logon(null, null, null, null);
            ax.TTSBegin();
            axRecord = ax.CreateAxaptaRecord(tableName);
            axRecord.ExecuteStmt(strQuery);
            if (axRecord.Found)
            {
                ret = true;
            }
            else
            {
                ret = false;
            }

            ax.TTSCommit();
            ax.Logoff();

            return ret;
        }
        catch (Exception e)
        {
            ax.TTSAbort();
            ax.Logoff();
            return false;
        }
    }

    [WebMethod(Description = "Obtiene la informacion bancaria del docente", EnableSession = false)]
    // 07-04-2020, Roberto Castro, Obtiene la informacion bancaria del docente
    public DataTable infoBancariaDocente(string _codpers)
    {
        bool ret;
        Axapta ax = new Axapta();
        DataTable dt = new DataTable("Table");
        dt.Columns.Add("Formapago", typeof(string));
        dt.Columns.Add("Banco", typeof(string));
        dt.Columns.Add("Nombre_banco", typeof(string));
        dt.Columns.Add("Tipocuenta", typeof(string));
        dt.Columns.Add("Nombre_Tipocuenta", typeof(string));
        dt.Columns.Add("Cuenta", typeof(string));
        dt.Columns.Add("Proveedor", typeof(string));
        dt.Columns.Add("Nombre_proveedor", typeof(string));

        AxaptaRecord axRecord,axBancos,axCuentas,axProveedor;

        string tableName = "IEmplVendCodeRefTable";
        string tableBancos = "UG_BankAccountTransfer";
        string tableCuentas = "UG_TypeBankAccountTrans";
        string tableProveedor = "VendTable";

        string strQuery = String.Format(@"select %1
        where %1.IEmplIdUGA == '{0}'
        outer join %2
        where %2.BankAccountTransId == %1.BankAccountTransId
        outer join %3
        where %3.TypeBankAccountId == %1.TypeBankAccountId
        outer join %4
        where %4.AccountNum == %1.VendAccount", _codpers);

        ax.Logon(null, null, null, null);
        ax.TTSBegin();
        axRecord = ax.CreateAxaptaRecord(tableName);
        axBancos = ax.CreateAxaptaRecord(tableBancos);
        axCuentas = ax.CreateAxaptaRecord(tableCuentas);
        axProveedor = ax.CreateAxaptaRecord(tableProveedor);

        ax.ExecuteStmt(strQuery, axRecord, axBancos, axCuentas,axProveedor);
        if (axRecord.Found)
        {
            DataRow dr = dt.NewRow();
            dr["Formapago"] = Convert.ToString(axRecord.get_Field("IPaymMode")).Length > 2 ? Convert.ToString(axRecord.get_Field("IPaymMode")).Substring(0,2) : Convert.ToString(axRecord.get_Field("IPaymMode"));
            dr["Banco"] = Convert.ToString(axRecord.get_Field("BankAccountTransId"));
            dr["Nombre_banco"] = Convert.ToString(axBancos.get_Field("BankAccountTransName"));
            dr["Tipocuenta"] = Convert.ToString(axRecord.get_Field("TypeBankAccountId"));
            dr["Nombre_Tipocuenta"] = Convert.ToString(axCuentas.get_Field("TypeName"));
            dr["Cuenta"] = Convert.ToString(axRecord.get_Field("BankAccountEmpl"));
            dr["Proveedor"] = Convert.ToString(axRecord.get_Field("VendAccount"));
            dr["Nombre_proveedor"] = Convert.ToString(axProveedor.get_Field("Name"));

            dt.Rows.Add(dr);
        }

        ax.TTSCommit();
        ax.Logoff();

        return dt;
    }


    [WebMethod(Description = "Valida si el codpers es de docente", EnableSession = false)]
    // 15-07-2020, Roberto Castro, Valida si el codpers es de docente
    public bool validaEsDocente(string _codpers)
    {
        bool ret;
        Axapta ax = new Axapta();
        try
        {
            AxaptaRecord axRecord;
            string tableName = "IEmplVendCodeRefTable";
            string strQuery = String.Format("select %1 where %1.IEmplIdUGA == '{0}'", _codpers);

            ax.Logon(null, null, null, null);
            ax.TTSBegin();
            axRecord = ax.CreateAxaptaRecord(tableName);
            axRecord.ExecuteStmt(strQuery);
            if (axRecord.Found)
            {
                ret = true;
            }
            else
            {
                ret = false;
            }

            ax.TTSCommit();
            ax.Logoff();

            return ret;
        }
        catch (Exception e)
        {
            ax.TTSAbort();
            ax.Logoff();
            return false;
        }
    }

    [WebMethod(Description = "Retorna la informacion de Pagos por porfacturación", EnableSession = false)]
    // 18-06-2021, Roberto Castro, Retorna la informacion de Pagos por porfacturación
    public DataTable infoPagoFacturacion(string _origen, string _fecha)
    {
        DataTable dt = new DataTable();
        try
        {
            OracleCommand cmd;
            DataSet ds = new DataSet();

            string sqlSelect;
            if (_origen == "IDEA")
            {
                sqlSelect = "SELECT tutor, referencia, monto, to_char(fecha, 'DD/MM/YYYY') fecha, to_char(fpago, 'DD/MM/YYYY') fpago, carrera, id, nvl(nombre, '<>') nombre, ";
                sqlSelect += "nvl(cursos, '<>') cursos, seccion, nvl(codigodenomina, '<>') codigodenomina, nvl(nominaorfacturacion, 0) nominaorfacturacion, anio, ciclo, pago, ROWID, nvl(cuentabi, '0') cuentabi ";
                sqlSelect += "FROM dynamics.pagosidea WHERE fpago <= TO_DATE('" + _fecha + "', 'DD/MM/YYYY')";
            }
            else
            {
                sqlSelect = "SELECT CATEDRATICO, REFERENCIA, MONTO, TO_CHAR(FECHAPROVISION, 'DD/MM/YYYY') FECHAPROVISION, TO_CHAR(FECHAPAGO, 'DD/MM/YYYY') FECHAPAGO, NOMINA, CORRELATIVO, NVL(NOMBRE, '<>') NOMBRE, NVL(PORTAL, '<>') PORTAL, ";
                sqlSelect += "NVL(FECHAIMP,TO_DATE('01/01/1900','DD/MM/YYYY')) FECHAIMP, NVL(CURSOS, '<>') CURSOS, NVL(ALUMNOSCURSO, -1) ALUMNOSCURSO, NVL(FECHA, TO_DATE('01/01/1900', 'DD/MM/YYYY')) FECHA, NVL(TIPONOMINA, 'X') TIPONOMINA, NVL(PUESTO, '<>') PUESTO, ROWID ";
                sqlSelect += "FROM DYNAMICS.PAGOSUGA WHERE FECHAPAGO <= TO_DATE('" + _fecha + "', 'DD/MM/YYYY') ";

                switch (_origen)
                {
                    case "UGA":
                        sqlSelect += "AND ORIGEN IS NULL AND TIPONOMINA != '8'";
                        break;

                    case "IDEANom8":
                        sqlSelect += "AND ORIGEN IS NULL AND TIPONOMINA = '8'";
                        break;

                    case "INAP":
                        sqlSelect += "AND ORIGEN = 'INAP'";
                        break;
                }
            }

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

    [WebMethod(Description = "Retorna la informacion del detalle de Pagos por porfacturación", EnableSession = false)]
    // 18-06-2021, Roberto Castro, Retorna la informacion del detalle de Pagos por porfacturación
    public DataTable infoPagoDetalle(string _nomina, string _correlativo)
    {
        DataTable dt = new DataTable();
        try
        {
            OracleCommand cmd;
            DataSet ds = new DataSet();

            string sqlSelect;
            sqlSelect = "SELECT CARRERA, MONTO, ROWID FROM DYNAMICS.PAGOSUGADETALLE ";
            sqlSelect += "WHERE NOMINA = " + _nomina + " AND CORRELATIVO = " + _correlativo;

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

    [WebMethod(Description = "Inserta registros en la tabla HPAGOSUGA", EnableSession = false)]
    // 15-06-2021, Roberto Castro, Inserta registros en la tabla HPAGOSUGA
    public int insertHpagos(string _nomina, string _correlativo)
    {
        int contador = 0;
        try
        {
            OracleCommand cmd;

            string sqlInsert;
            sqlInsert = "INSERT INTO DYNAMICS.HPAGOSUGA (NOMINA,CORRELATIVO,CATEDRATICO,NOMBRE,PORTAL,FECHAIMP,CURSOS,ALUMNOSCURSO,FECHAPAGO,FECHAPROVISION,FECHA,TIPONOMINA,PUESTO,MONTO,REFERENCIA,FECHA_TRASLADO,ORIGEN) ";
            sqlInsert += "SELECT  NOMINA,CORRELATIVO,CATEDRATICO,NOMBRE,PORTAL,FECHAIMP,CURSOS,ALUMNOSCURSO,FECHAPAGO,FECHAPROVISION,FECHA,TIPONOMINA,PUESTO,MONTO,REFERENCIA,SYSDATE,ORIGEN ";
            sqlInsert += "FROM DYNAMICS.PAGOSUGA ";
            sqlInsert += "WHERE NOMINA = " + _nomina + " ";
            sqlInsert += "AND CORRELATIVO = " + _correlativo;

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

    [WebMethod(Description = "Inserta registros en la tabla HPAGOSUGADETALLE", EnableSession = false)]
    // 15-06-2021, Roberto Castro, Inserta registros en la tabla HPAGOSUGADETALLE
    public int insertHpagosUgaDetalle(string _nomina, string _correlativo)
    {
        int contador = 0;
        try
        {
            OracleCommand cmd;

            string sqlInsert;
            sqlInsert = "INSERT INTO DYNAMICS.HPAGOSUGADETALLE (NOMINA,CORRELATIVO,CARRERA,MONTO,FECHA_TRASLADO) ";
            sqlInsert += "SELECT  NOMINA,CORRELATIVO,CARRERA,MONTO,SYSDATE ";
            sqlInsert += "FROM DYNAMICS.PAGOSUGADETALLE ";
            sqlInsert += "WHERE NOMINA = " + _nomina + " ";
            sqlInsert += "AND CORRELATIVO = " + _correlativo + " ";

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

    [WebMethod(Description = "Elimina registros en la tabla PAGOSUGADETALLE", EnableSession = false)]
    // 15-06-2021, Roberto Castro, Elimina registros en la tabla PAGOSUGADETALLE
    public int deletePagosUgaDetalle(string _nomina, string _correlativo)
    {
        int contador = 0;
        try
        {
            OracleCommand cmd;

            string sqlDelete;
            sqlDelete = "DELETE FROM DYNAMICS.PAGOSUGADETALLE ";
            sqlDelete += "WHERE NOMINA = " + _nomina + " ";
            sqlDelete += "AND CORRELATIVO = " + _correlativo + " ";

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

    [WebMethod(Description = "Elimina registros en la tabla PAGOSUGA", EnableSession = false)]
    // 15-06-2021, Roberto Castro, Elimina registros en la tabla PAGOSUGA
    public int deletePagosUga(string _nomina, string _correlativo)
    {
        int contador = 0;
        try
        {
            OracleCommand cmd;

            string sqlDelete;
            sqlDelete = "DELETE FROM DYNAMICS.PAGOSUGA ";
            sqlDelete += "WHERE NOMINA = " + _nomina + " ";
            sqlDelete += "AND CORRELATIVO = " + _correlativo;

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
}
