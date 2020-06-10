using Microsoft.Dynamics.BusinessConnectorNet;
using System;
using System.Collections.Generic;
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

    public WSFacturas()
    {

        //Elimine la marca de comentario de la línea siguiente si utiliza los componentes diseñados 
        //InitializeComponent(); 
    }

    [WebMethod(Description = "Ingresa las facturas del personal", EnableSession = false)]
    // 07-04-2020, Roberto Castro, Ingresa las facturas de docentes y administrativos que facturan para la Universidad
    public String agregarFactura(string _proveedor, string _facturaserie, string _facturanumero, string _fechafactura, string _descripcion, decimal _monto, string _iddetalle, int _typeStaff, string _usuariooracle)
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
            dr["Formapago"] = Convert.ToString(axRecord.get_Field("IPaymMode")).Substring(0,2);
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

}
