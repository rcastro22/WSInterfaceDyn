using Microsoft.Dynamics.BusinessConnectorNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

/// <summary>
/// Descripción breve de WSEmpleados
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// Para permitir que se llame a este servicio web desde un script, usando ASP.NET AJAX, quite la marca de comentario de la línea siguiente. 
// [System.Web.Script.Services.ScriptService]
public class WSEmpleados : System.Web.Services.WebService
{

    public WSEmpleados()
    {

        //Elimine la marca de comentario de la línea siguiente si utiliza los componentes diseñados 
        //InitializeComponent(); 
    }

    [WebMethod(Description = "Valida que el empleado exista", EnableSession = false)]
    // 08-06-2020, Roberto Castro, Valida que el empleado exista
    public string existeEmpleado(string _emplId)
    {
        Axapta ax = new Axapta();
        try
        {
            AxaptaRecord axRecord;
            string tableName = "EmplTable";
            string strQuery = "select forupdate %1 where %1.PictureId == '" + _emplId + "'";
            string EmplId = "";

            ax.Logon(null, null, null, null);
            ax.TTSBegin();
            axRecord = ax.CreateAxaptaRecord(tableName);
            axRecord.ExecuteStmt(strQuery);

            if (!axRecord.Found)
            {
                throw new Exception("El código de empleado no existe");
            }
            else
            {
                EmplId = Convert.ToString(axRecord.get_Field("EmplId"));
            }

            ax.TTSCommit();
            ax.Logoff();

            return EmplId;
        }
        catch (Exception e)
        {
            ax.TTSAbort();
            ax.Logoff();
            return "Error encontrado: " + e.Message;
        }
    }

    [WebMethod(Description = "Ingresa la informacion de los familiares del empleado", EnableSession = false)]
    // 08-06-2020, Roberto Castro, Ingresa la informacion de los familiares del empleado
    public string familiares(string _emplId, string _nombreFamiliar, int _genero, int _relacion, string _fechaNacimiento, string _usuarioOracle)
    {
        Axapta ax = new Axapta();
        try
        {
            AxaptaRecord axRecordFamilyControl;
            string familyControl = "PayRollFamilyControl";
            ax.Logon(null, null, null, null);
            ax.TTSBegin();
            axRecordFamilyControl = ax.CreateAxaptaRecord(familyControl);
            axRecordFamilyControl.set_Field("EmplId", _emplId);
            axRecordFamilyControl.set_Field("Name", _nombreFamiliar);
            axRecordFamilyControl.set_Field("Gender", _genero);
            axRecordFamilyControl.set_Field("RelationType", _relacion);
            axRecordFamilyControl.set_Field("BirthDate", Convert.ToDateTime(_fechaNacimiento));
            axRecordFamilyControl.set_Field("UsuarioOracle", _usuarioOracle);
            axRecordFamilyControl.Insert();
            

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

    [WebMethod(Description = "Elimina el detalle de familiares", EnableSession = false)]
    // 08-06-2020, Roberto Castro, Elimina el detalle de familiares
    public string delfamiliares(string _emplId)
    {
        Axapta ax = new Axapta();
        try
        {
            AxaptaRecord axRecordFamilyControl;
            
            string familyControl = "PayRollFamilyControl";
            string strQueryFC = "delete_from %1 where %1.EmplId == '" + _emplId + "'";

            ax.Logon(null, null, null, null);
            ax.TTSBegin();

            axRecordFamilyControl = ax.CreateAxaptaRecord(familyControl);
            axRecordFamilyControl.ExecuteStmt(strQueryFC);

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


    [WebMethod(Description = "Actualiza la información del empleado", EnableSession = false)]
    // 08-06-2020, Roberto Castro, Actualiza la información del empleado
    public string updEmplInfo(string _emplId, string _torre, string _oficina, string _extension)
    {
        Axapta ax = new Axapta();
        try
        {
            AxaptaRecord axRecord;
            string tableName = "EmplTable";
            string strQuery = "select forupdate %1 where %1.EmplId == '" + _emplId + "'";

            ax.Logon(null, null, null, null);
            ax.TTSBegin();
            axRecord = ax.CreateAxaptaRecord(tableName);
            axRecord.ExecuteStmt(strQuery);

            if (axRecord.Found)
            {
                if (_torre != null) axRecord.set_Field("Tower", _torre);
                if (_oficina != null) axRecord.set_Field("Office", _oficina);
                if (_extension != null) axRecord.set_Field("Extension", _extension);
                // Actualiza el registro en Dynamics
                axRecord.Update();
            }
            else
            {
                throw new Exception( "No se encontro registro para actualizar");
            }

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
