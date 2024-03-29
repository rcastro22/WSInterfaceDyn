﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;

/// <summary>
/// Descripción breve de WSDGArchivo
/// ATENCION!!!!!: Este WS queda en desuso, ya que en el servicio de Dynamics se busca el id a travez del servicio de DG
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

    protected string encripta2(decimal id)
    {
        EncryptedQueryString args = new EncryptedQueryString();
        return args.Base64Encode(id.ToString());
    }

    [WebMethod(Description = "Obtiene el id encriptado del arhcivo", EnableSession = false)]
    // 12-04-2020, RC, Obtiene el id encriptado del arhcivo
    // 21-06-2022, RC, Metodo obsoleto pues se consulta directaente en el api DG de dynamics
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

            fileEnc = encripta2(Convert.ToDecimal(dsNew.Tables[0].Rows[filaFechaMaxima - 1][0]));
        }
        return fileEnc;
    }


    [WebMethod(Description = "Busca archivo de la carpeta repositorio y lo carga a digitalización", EnableSession = false)]
    // 10-06-2016, RC, Busca archivo de la carpeta repositorio y lo carga a digitalización
    public string SubirArchivo(string _nombreArchivo)
    {
        string[] parm = _nombreArchivo.Split('|');
        try
        {
            System.Byte[] nuevoByte = System.IO.File.ReadAllBytes(@"C:\TransferDocs\" + parm[0].ToString());
            WSExpediente.Service ser1 = new WSExpediente.Service();
            ser1.Guardar("PR", 53, parm[0].ToString(), "application/pdf", nuevoByte, parm[1].ToString());
            System.IO.File.Delete(@"C:\TransferDocs\" + parm[0].ToString());
        }
        catch (System.IO.IOException e)
        {
            return e.Message;
        }
        return "Ok";
    }

}
