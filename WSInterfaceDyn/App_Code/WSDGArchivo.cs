using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;

/// <summary>
/// Descripción breve de WSDGArchivo
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// Para permitir que se llame a este servicio web desde un script, usando ASP.NET AJAX, quite la marca de comentario de la línea siguiente. 
// [System.Web.Script.Services.ScriptService]
public class WSDGArchivo : System.Web.Services.WebService
{

    public WSDGArchivo()
    {

        //Elimine la marca de comentario de la línea siguiente si utiliza los componentes diseñados 
        //InitializeComponent(); 
    }

    protected string encripta(decimal id)
    {
        EncryptedQueryString args = new EncryptedQueryString();
        args["id"] = id.ToString();
        string res = args.ToString();
        return res;
    }

    [WebMethod(Description = "Obtiene el id encriptado del arhcivo", EnableSession = false)]
    // 12-04-2020, RC, Obtiene el id encriptado del arhcivo
    public string obtenerIdArchivo(string _application, int _category, string _labels)
    {
        char[] separador = { '&' };
        char[] separador2 = { '=' };
        String[] listLabel = _labels.Split(separador);

        string fileEnc = "";
        int filaFechaMaxima = 0, cont = 0;
        WSExpediente.Service ser1 = new WSExpediente.Service();
        WSExpediente.Etiquetas[] et = new WSExpediente.Etiquetas[listLabel.Length];
        WSExpediente.Etiquetas arch;

        int i = 0;
        foreach (String label in listLabel)
        {
            arch = new WSExpediente.Etiquetas();
            String[] ValueLabel = label.Split(separador2);
            arch.Etiqueta = Int16.Parse(ValueLabel[0]);
            arch.Valor = ValueLabel[1];
            et[i] = arch;
            i++;
        }

        DataSet dsNew = new DataSet();
        dsNew = ser1.ObtenerArchivos(_application, _category, et);

        if (dsNew.Tables[0].Rows.Count > 0)
        {
            string fecha = "";
            foreach (DataRow row in dsNew.Tables[0].Rows)
            {
                cont++;
                if (fecha == "")
                {
                    filaFechaMaxima = cont;
                    fecha = Convert.ToString(row["FECHA"]);
                }
                else
                {
                    if (Convert.ToDateTime(fecha) < Convert.ToDateTime(row["FECHA"]))
                    {
                        filaFechaMaxima = cont;
                        fecha = Convert.ToString(row["FECHA"]);
                    }
                }
            }

            fileEnc = encripta(Convert.ToDecimal(dsNew.Tables[0].Rows[filaFechaMaxima - 1][0]));
        }
        return fileEnc;
    }

}
