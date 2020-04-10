using Microsoft.Dynamics.BusinessConnectorNet;
using System;
using System.Collections.Generic;
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
        
        string ret = "";
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

}
