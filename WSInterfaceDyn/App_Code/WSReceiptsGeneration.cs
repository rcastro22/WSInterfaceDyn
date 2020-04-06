using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using System.Configuration;
using Microsoft.Dynamics.BusinessConnectorNet;
using System.Data.SqlClient;
using System.Data;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.IO;

/// <summary>
/// Summary description for WSReceiptsGeneration
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
// [System.Web.Script.Services.ScriptService]
public class WSReceiptsGeneration : System.Web.Services.WebService
{
    SqlConnection conn = new SqlConnection("Data Source=srvdynamicsax;Initial Catalog=DAXPRODGALILEO;Integrated Security=True;MultipleActiveResultSets=true");
    OracleConnection cn = new OracleConnection(ConfigurationManager.ConnectionStrings["galileo"].ConnectionString);
    public WSReceiptsGeneration()
    {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }

    [WebMethod]
    // Boletas de transaccion de nomina - Fin de mes
    public string generateTransReceipt(string _transId)
    {
        int contador = 0;
        string transId = _transId;
        string sequencialMonth;
        Axapta ax = new Axapta();
        string[] ParmsClass = new string[1];

        ax.Logon(null, null, null, null);
        try
        {
            ParmsClass[0] = Convert.ToString(transId);
            sequencialMonth = Convert.ToString(ax.CallStaticClassMethod("WSInterface", "generateNumberReceipt", ParmsClass));
            ax.Logoff();
        }
        catch
        {
            ax.Logoff();
            sequencialMonth = "Number Error";
        }

        conn.Open();
        SqlCommand cmd = new SqlCommand("dbo.TransReceipt_Head", conn);
        SqlParameter parm = new SqlParameter("@transId", transId);
        //parm.Value = "000286_160";
        cmd.Parameters.Add(parm);
        cmd.CommandType = CommandType.StoredProcedure;
        SqlDataReader rdr = cmd.ExecuteReader();
        //conn.Close();
        while (rdr.Read())
        {
            /*Console.WriteLine(
                "Product: {0,-25} Price: ${1,6:####.00}",
                rdr["TenMostExpensiveProducts"],
                rdr["UnitPrice"]);*/
            try
            {
                Document document = new Document(new iTextSharp.text.Rectangle(612f, 396.27f));
                document.SetMargins(40, 40, 35, 5);
                document.PageSize.Rotate();
                document.AddAuthor("Universidad Galileo");
                document.AddCreator("Universidad Galileo");
                document.AddCreationDate();
                document.AddTitle("Recibo de Pago");
                document.AddSubject("Recibo de pago");
                document.AddKeywords("Recibo ; Pago ; Boleta");
                byte[] arch = null;

                using (MemoryStream inStream = new MemoryStream())
                {
                    PdfWriter writer = PdfWriter.GetInstance(document, inStream);

                    document.Open();

                    //Capa 1 - Imagenes
                    PdfLayer layer = new PdfLayer("MyLayer1", writer);
                    PdfContentByte cb = writer.DirectContent;
                    cb.BeginLayer(layer);
                    iTextSharp.text.Image logoPeq = iTextSharp.text.Image.GetInstance(@"C:\CS\Img\LogoGalileoPequeño.png");
                    logoPeq.SetAbsolutePosition(40, document.PageSize.Height - Convert.ToInt16(logoPeq.Height * 0.4) - 20);
                    logoPeq.ScalePercent(40);
                    cb.AddImage(logoPeq);

                    iTextSharp.text.Image logoGran = iTextSharp.text.Image.GetInstance(@"C:\CS\Img\LogoGalileoGrande.png");
                    logoGran.SetAbsolutePosition((document.PageSize.Width / 2) - (Convert.ToInt16(logoGran.Width * 0.4) / 2), (document.PageSize.Height / 2) - (Convert.ToInt16(logoGran.Height * 0.4) / 2));
                    logoGran.ScalePercent(40);
                    cb.AddImage(logoGran);
                    cb.EndLayer();
                    //Fin capa 1

                    iTextSharp.text.Font myfont1 = new iTextSharp.text.Font(FontFactory.GetFont(FontFactory.HELVETICA, 8, iTextSharp.text.Font.NORMAL));

                    Phrase myPhrase;

                    //Capa 2 - Datos
                    layer = new PdfLayer("MyLayer2", writer);
                    cb.BeginLayer(layer);
                    Paragraph myParagraph = new Paragraph();
                    myPhrase = new Phrase("Universidad Galileo", new iTextSharp.text.Font(FontFactory.GetFont(FontFactory.HELVETICA, 16, iTextSharp.text.Font.BOLD)));
                    myParagraph.Add(myPhrase);
                    myParagraph.Alignment = Element.ALIGN_CENTER;
                    document.Add(myParagraph);

                    myParagraph = new Paragraph();
                    myPhrase = new Phrase("RECIBO DE PAGO", new iTextSharp.text.Font(FontFactory.GetFont(FontFactory.HELVETICA, 15, iTextSharp.text.Font.BOLD)));
                    myParagraph.Alignment = Element.ALIGN_CENTER;
                    myParagraph.Add(myPhrase);
                    document.Add(myParagraph);


                    SqlCommand cmdInfo = new SqlCommand("dbo.PayRollTransInfo", conn);
                    cmdInfo.Parameters.Add(new SqlParameter("@transId", transId));
                    cmdInfo.CommandType = CommandType.StoredProcedure;
                    SqlDataReader rdrInfo = cmdInfo.ExecuteReader();
                    while (rdrInfo.Read())
                    {
                        myParagraph = new Paragraph();
                        myPhrase = new Phrase(rdrInfo["DESCRIPTION"] + " del " + Convert.ToDateTime(rdrInfo["FROMDATE"]).ToString("dd/MM/yyyy") + " al " + Convert.ToDateTime(rdrInfo["TODATE"]).ToString("dd/MM/yyyy"), new iTextSharp.text.Font(FontFactory.GetFont(FontFactory.HELVETICA, 14, iTextSharp.text.Font.BOLD)));
                        myParagraph.Alignment = Element.ALIGN_CENTER;
                        myParagraph.Add(myPhrase);
                        document.Add(myParagraph);
                    }


                    sequencialMonth = rdr["SEQUENCIALMONTH"].ToString();
                    /*if (sequencialMonth == "")
                    {                       
                        ax.Logon(null, null, null, null);
                        try
                        {                            
                            ParmsClass[0] = Convert.ToString(transId);
                            ParmsClass[1] = rdr["EMPLID"].ToString();
                            sequencialMonth = Convert.ToString(ax.CallStaticClassMethod("WSInterface", "generateNumberReceipt", ParmsClass));
                            ax.Logoff();
                        }
                        catch (Exception ex)
                        {
                            ax.Logoff();
                            sequencialMonth = "Number Error";
                        }
                    }*/

                    myParagraph = new Paragraph();
                    myPhrase = new Phrase("Recibo No.: " + sequencialMonth, myfont1);
                    myParagraph.Alignment = Element.ALIGN_RIGHT;
                    myParagraph.Add(myPhrase); myParagraph.Leading = 24f;
                    document.Add(myParagraph);

                    myParagraph = new Paragraph();
                    myPhrase = new Phrase(" ");
                    myParagraph.Alignment = Element.ALIGN_LEFT;
                    myParagraph.Add(myPhrase); myParagraph.Leading = 10.0f;
                    document.Add(myParagraph);

                    //////////////////////// TABLA ENCABEZADO //////////////////////////
                    PdfPCell cell2 = new PdfPCell();
                    cell2.BorderWidth = 0;
                    iTextSharp.text.pdf.PdfPTable tableHead = new PdfPTable(2);
                    tableHead.TotalWidth = document.PageSize.Width - 80;
                    tableHead.SetWidths(new Single[] { 3.0F, 3.0F });
                    tableHead.LockedWidth = true;

                    myPhrase = new Phrase("Empleado: " + rdr["EMPLID"], myfont1);
                    cell2.Phrase = myPhrase; cell2.HorizontalAlignment = Element.ALIGN_LEFT;
                    tableHead.AddCell(cell2);

                    myPhrase = new Phrase("Cuentas bancarias del empleado: " + rdr["BANKACCOUNTEMPL"], myfont1);
                    cell2.Phrase = myPhrase; cell2.HorizontalAlignment = Element.ALIGN_RIGHT;
                    tableHead.AddCell(cell2);

                    myPhrase = new Phrase("Nombre: " + rdr["EMPLNAME"], myfont1);
                    cell2.Phrase = myPhrase; cell2.HorizontalAlignment = Element.ALIGN_LEFT;
                    tableHead.AddCell(cell2);

                    myPhrase = new Phrase("Salario líquido: " + Convert.ToString(decimal.Round(Convert.ToDecimal(rdr["LIQUID"]), 2).ToString("###,###.00")), myfont1);
                    cell2.Phrase = myPhrase; cell2.HorizontalAlignment = Element.ALIGN_RIGHT;
                    tableHead.AddCell(cell2);

                    myPhrase = new Phrase("Departamento: " + rdr["DESCRIPTION"], myfont1);
                    cell2.Phrase = myPhrase; cell2.HorizontalAlignment = Element.ALIGN_LEFT;
                    tableHead.AddCell(cell2);

                    myPhrase = new Phrase();
                    cell2.Phrase = myPhrase;
                    tableHead.AddCell(cell2);

                    document.Add(tableHead);
                    //////////////////////// TABLA ENCABEZADO //////////////////////////


                    ////////////////////////// TABLA DATOS /////////////////////////////
                    iTextSharp.text.pdf.PdfPTable table = new PdfPTable(3);
                    iTextSharp.text.pdf.PdfPTable table2 = new PdfPTable(3);
                    iTextSharp.text.pdf.PdfPTable table3 = new PdfPTable(2);
                    table.TotalWidth = document.PageSize.Width - 80;
                    table.LockedWidth = true;

                    table.SetWidths(new Single[] { 2.0F, 0.1f, 1.5F });           //Tabla Principal
                    table2.SetWidths(new Single[] { 3.0F, 1.0F, 1.0F });    //Tabla Ingresos
                    table3.SetWidths(new Single[] { 2.0F, 1.0F });          //Tabla Deducciones

                    PdfPCell cell;

                    // Encabezado ingresos
                    myPhrase = new Phrase("INGRESOS", myfont1);
                    cell = new PdfPCell(myPhrase); cell.HorizontalAlignment = Element.ALIGN_LEFT; cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                    table2.AddCell(cell);
                    myPhrase = new Phrase("CANT.", myfont1);
                    cell = new PdfPCell(myPhrase); cell.HorizontalAlignment = Element.ALIGN_RIGHT; cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                    table2.AddCell(cell);
                    myPhrase = new Phrase("MONTO", myfont1);
                    cell = new PdfPCell(myPhrase); cell.HorizontalAlignment = Element.ALIGN_RIGHT; cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                    table2.AddCell(cell);

                    // Obtencion de datos - Ingresos
                    SqlCommand cmd2 = new SqlCommand("dbo.TransReceipt", conn);
                    cmd2.Parameters.Add("@transId", SqlDbType.NVarChar, 10).Value = transId;
                    cmd2.Parameters.Add("@emplId", SqlDbType.NVarChar, 10).Value = rdr["EMPLID"];
                    cmd2.Parameters.Add("@typeElement", SqlDbType.Int).Value = 0;
                    cmd2.CommandType = CommandType.StoredProcedure;
                    SqlDataReader qrdr = cmd2.ExecuteReader();
                    //conn.Close();
                    while (qrdr.Read())
                    {
                        myPhrase = new Phrase(Convert.ToString(qrdr["CONCEPT"]), myfont1);
                        cell = new PdfPCell(myPhrase); cell.BorderWidth = 0;
                        table2.AddCell(cell);
                        myPhrase = new Phrase(Convert.ToString(decimal.Round(Convert.ToDecimal(qrdr["QUANTITY"]), 2)), myfont1);
                        cell = new PdfPCell(myPhrase); cell.HorizontalAlignment = Element.ALIGN_RIGHT; cell.BorderWidth = 0;
                        table2.AddCell(cell);
                        myPhrase = new Phrase(Convert.ToString(decimal.Round(Convert.ToDecimal(qrdr["AMOUNT"]), 2).ToString("###,###.00")), myfont1);
                        cell = new PdfPCell(myPhrase); cell.HorizontalAlignment = Element.ALIGN_RIGHT; cell.BorderWidth = 0;
                        table2.AddCell(cell);

                    }
                    qrdr.Close();

                    // Encabezado deducciones
                    myPhrase = new Phrase("DEDUCCIONES", myfont1);
                    cell = new PdfPCell(myPhrase); cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                    table3.AddCell(cell);
                    myPhrase = new Phrase("MONTO", myfont1);
                    cell = new PdfPCell(myPhrase); cell.HorizontalAlignment = Element.ALIGN_RIGHT; cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                    table3.AddCell(cell);

                    // Obetencion de datos - deducciones
                    cmd2 = new SqlCommand("dbo.TransReceipt", conn);
                    cmd2.Parameters.Add("@transId", SqlDbType.NVarChar, 10).Value = transId;
                    cmd2.Parameters.Add("@emplId", SqlDbType.NVarChar, 10).Value = rdr["EMPLID"];
                    cmd2.Parameters.Add("@typeElement", SqlDbType.Int).Value = 1;
                    cmd2.CommandType = CommandType.StoredProcedure;
                    qrdr = cmd2.ExecuteReader();

                    while (qrdr.Read())
                    {
                        myPhrase = new Phrase(Convert.ToString(qrdr["CONCEPT"]), myfont1);
                        cell = new PdfPCell(myPhrase); cell.BorderWidth = 0;
                        table3.AddCell(cell);
                        myPhrase = new Phrase(Convert.ToString(decimal.Round(Convert.ToDecimal(qrdr["AMOUNT"]), 2).ToString("###,###.00")), myfont1);
                        cell = new PdfPCell(myPhrase); cell.HorizontalAlignment = Element.ALIGN_RIGHT; cell.BorderWidth = 0;
                        table3.AddCell(cell);

                    }
                    qrdr.Close();

                    cell = new PdfPCell(table2);
                    cell.BorderWidth = 0;
                    table.AddCell(cell);
                    cell = new PdfPCell();
                    cell.BorderWidth = 0;
                    table.AddCell(cell);
                    cell = new PdfPCell(table3);
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // Totales
                    cell = new PdfPCell(new Phrase(Convert.ToString(decimal.Round(Convert.ToDecimal(rdr["REVENUE"]), 2).ToString("###,###.00")), myfont1));
                    cell.BorderWidth = 0; cell.HorizontalAlignment = Element.ALIGN_RIGHT; cell.BorderWidthTop = 0.5f;
                    table.AddCell(cell);
                    cell = new PdfPCell();
                    cell.BorderWidth = 0;
                    table.AddCell(cell);
                    cell = new PdfPCell(new Phrase(Convert.ToString(decimal.Round(Convert.ToDecimal(rdr["DEDUCTION"]), 2).ToString("###,###.00")), myfont1));
                    cell.BorderWidth = 0; cell.HorizontalAlignment = Element.ALIGN_RIGHT; cell.BorderWidthTop = 0.5f;
                    table.AddCell(cell);

                    document.Add(table);
                    ////////////////////////// TABLA DATOS /////////////////////////////


                    cell2 = new PdfPCell();
                    cell2.BorderWidth = 0;
                    iTextSharp.text.pdf.PdfPTable tableFoot = new PdfPTable(1);
                    tableFoot.TotalWidth = document.PageSize.Width - 80;
                    tableFoot.SetWidths(new Single[] { 3.0F });
                    tableFoot.LockedWidth = true;

                    myParagraph = new Paragraph();
                    myPhrase = new Phrase("Guatemala, " + DateTime.Now.Day.ToString() + " de " + DateTime.Now.ToString("MMMM") + " de " + DateTime.Now.Year.ToString(), myfont1);
                    myParagraph.Alignment = Element.ALIGN_LEFT;
                    myParagraph.Add(myPhrase); myParagraph.Leading = 24f;
                    cell2.AddElement(myParagraph);


                    myParagraph = new Paragraph();
                    myPhrase = new Phrase("Elaborado por:                                                          Verificado por:                                                            Autorizado por:", myfont1);
                    myParagraph.Alignment = Element.ALIGN_LEFT;
                    myParagraph.Add(myPhrase); myParagraph.Leading = 30f;
                    cell2.AddElement(myParagraph);


                    myParagraph = new Paragraph();
                    myPhrase = new Phrase("Recibo de conformidad, el importe neto indicado, el cual cubre", new iTextSharp.text.Font(FontFactory.GetFont(FontFactory.HELVETICA, 6, iTextSharp.text.Font.NORMAL)));
                    myParagraph.Alignment = Element.ALIGN_LEFT;
                    myParagraph.Add(myPhrase); //myParagraph.Leading = 30f;                    
                    cell2.AddElement(myParagraph);

                    myParagraph = new Paragraph();
                    myPhrase = new Phrase("la totalidad de los servicios prestados durante el periodo.", new iTextSharp.text.Font(FontFactory.GetFont(FontFactory.HELVETICA, 6, iTextSharp.text.Font.NORMAL)));
                    myParagraph.Alignment = Element.ALIGN_LEFT;
                    myParagraph.Add(myPhrase); myParagraph.Leading = 7f;
                    cell2.AddElement(myParagraph);

                    /*myParagraph = new Paragraph();
                    myPhrase = new Phrase("                                                                                    Recibí Conforme  ___________________________________", new iTextSharp.text.Font(FontFactory.GetFont(FontFactory.HELVETICA, 10, iTextSharp.text.Font.NORMAL)));
                    myParagraph.Alignment = Element.ALIGN_LEFT;
                    myParagraph.Add(myPhrase); myParagraph.Leading = 30f;
                    cell2.AddElement(myParagraph);*/

                    tableFoot.AddCell(cell2);
                    document.Add(tableFoot);


                    iTextSharp.text.Image lineaF = iTextSharp.text.Image.GetInstance(@"C:\CS\Img\lineaFirma.png");
                    lineaF.SetAbsolutePosition((document.PageSize.Width - Convert.ToInt16(lineaF.Width * 0.66)) - 50, 40);
                    lineaF.ScalePercent(66);
                    cb.AddImage(lineaF);

                    myParagraph = new Paragraph();
                    document.Add(myParagraph);
                    myParagraph.Clear();
                    cb.EndLayer();
                    document.Close();
                    arch = inStream.GetBuffer();

                    WSExpediente.Service ser = new WSExpediente.Service();
                    string fileName = sequencialMonth + "-" + rdr["PICTUREID"] + "-" + DateTime.Now.ToString("ddMMyyyy") + "-0.pdf";
                    if (validaExisteBoleta(sequencialMonth, rdr["PICTUREID"].ToString()) && Convert.ToDecimal(rdr["LIQUID"]) > 0)
                    {
                        ser.Guardar("PR", 35, fileName, "application/pdf", arch, "DBAFISICC");
                        //File.WriteAllBytes(@"c:\\prueba\"+fileName, arch);
                        contador++;
                    }
                    inStream.Dispose();
                }
            }
            catch (Exception ex)
            {
                System.Web.HttpContext.Current.Response.Write(ex.Message);
            }
            //break;
        }
        rdr.Close();

        return "Cantidad de boletas generadas: " + contador.ToString();
    }



    [WebMethod]
    // Boletas de transaccion de nomina - Quincena
    public string generateAdvanceReceipt(string _transId)
    {
        int contador = 0;
        string transId = _transId; //"000341_160";
        string sequencialMonth;
        Axapta ax = new Axapta();
        string[] ParmsClass = new string[2];

        conn.Open();
        SqlCommand cmd = new SqlCommand("dbo.PayRollAdvanceReceipt_Head", conn);
        SqlParameter parm = new SqlParameter("@transId", transId);
        //parm.Value = "000286_160";
        cmd.Parameters.Add(parm);
        cmd.CommandType = CommandType.StoredProcedure;
        SqlDataReader rdr = cmd.ExecuteReader();
        //conn.Close();
        while (rdr.Read())
        {
            try
            {
                Document document = new Document(new iTextSharp.text.Rectangle(612f, 396.27f));
                document.SetMargins(40, 40, 50, 5);
                document.PageSize.Rotate();
                document.AddAuthor("Universidad Galileo");
                document.AddCreator("Universidad Galileo");
                document.AddCreationDate();
                document.AddTitle("Recibo de Pago de anticipo quincenal");
                document.AddSubject("Recibo de pago de anticipo quincenal");
                document.AddKeywords("Recibo ; Pago ; Boleta ; Anticipo");
                byte[] arch = null;

                using (MemoryStream inStream = new MemoryStream())
                {
                    PdfWriter writer = PdfWriter.GetInstance(document, inStream);

                    document.Open();

                    //Capa 1 - Imagenes
                    PdfLayer layer = new PdfLayer("MyLayer1", writer);
                    PdfContentByte cb = writer.DirectContent;
                    cb.BeginLayer(layer);
                    iTextSharp.text.Image logoPeq = iTextSharp.text.Image.GetInstance(@"C:\CS\Img\LogoGalileoPequeño.png");
                    logoPeq.SetAbsolutePosition(40, document.PageSize.Height - Convert.ToInt16(logoPeq.Height * 0.4) - 20);
                    logoPeq.ScalePercent(40);
                    cb.AddImage(logoPeq);

                    iTextSharp.text.Image logoGran = iTextSharp.text.Image.GetInstance(@"C:\CS\Img\LogoGalileoGrande.png");
                    logoGran.SetAbsolutePosition((document.PageSize.Width / 2) - (Convert.ToInt16(logoGran.Width * 0.4) / 2), (document.PageSize.Height / 2) - (Convert.ToInt16(logoGran.Height * 0.4) / 2));
                    logoGran.ScalePercent(40);
                    cb.AddImage(logoGran);
                    cb.EndLayer();
                    //Fin capa 1

                    iTextSharp.text.Font myfont1 = new iTextSharp.text.Font(FontFactory.GetFont(FontFactory.HELVETICA, 8, iTextSharp.text.Font.NORMAL));

                    Phrase myPhrase;

                    //Capa 2 - Datos
                    layer = new PdfLayer("MyLayer2", writer);
                    cb.BeginLayer(layer);
                    Paragraph myParagraph = new Paragraph();
                    myPhrase = new Phrase("Universidad Galileo", new iTextSharp.text.Font(FontFactory.GetFont(FontFactory.HELVETICA, 16, iTextSharp.text.Font.BOLD)));
                    myParagraph.Add(myPhrase);
                    myParagraph.Alignment = Element.ALIGN_CENTER;
                    document.Add(myParagraph);

                    myParagraph = new Paragraph();
                    myPhrase = new Phrase("RECIBO DE PAGO DE ANTICIPO QUINCENAL", new iTextSharp.text.Font(FontFactory.GetFont(FontFactory.HELVETICA, 15, iTextSharp.text.Font.BOLD)));
                    myParagraph.Alignment = Element.ALIGN_CENTER;
                    myParagraph.Add(myPhrase);
                    document.Add(myParagraph);

                    SqlCommand cmdInfo = new SqlCommand("dbo.PayRollTransInfo", conn);
                    cmdInfo.Parameters.Add(new SqlParameter("@transId", transId));
                    cmdInfo.CommandType = CommandType.StoredProcedure;
                    SqlDataReader rdrInfo = cmdInfo.ExecuteReader();

                    DateTime dtimeReceipt = DateTime.Now;
                    while (rdrInfo.Read())
                    {
                        dtimeReceipt = Convert.ToDateTime(rdrInfo["FROMDATE"]);
                        myParagraph = new Paragraph();
                        myPhrase = new Phrase(rdrInfo["DESCRIPTION"] + " del " + Convert.ToDateTime(rdrInfo["FROMDATE"]).ToString("dd/MM/yyyy") + " al " + Convert.ToDateTime(rdrInfo["TODATE"]).ToString("dd/MM/yyyy"), new iTextSharp.text.Font(FontFactory.GetFont(FontFactory.HELVETICA, 14, iTextSharp.text.Font.BOLD)));
                        myParagraph.Alignment = Element.ALIGN_CENTER;
                        myParagraph.Add(myPhrase);
                        document.Add(myParagraph);
                    }

                    sequencialMonth = dtimeReceipt.ToString("yyyy") + dtimeReceipt.ToString("MM") + rdr["EMPLID"].ToString();

                    myParagraph = new Paragraph();
                    myPhrase = new Phrase(" ");
                    myParagraph.Alignment = Element.ALIGN_LEFT;
                    myParagraph.Add(myPhrase); myParagraph.Leading = 24.0f;
                    document.Add(myParagraph);

                    //////////////////////// TABLA ENCABEZADO //////////////////////////
                    PdfPCell cell2 = new PdfPCell();
                    cell2.BorderWidth = 0;
                    iTextSharp.text.pdf.PdfPTable tableHead = new PdfPTable(2);
                    tableHead.TotalWidth = document.PageSize.Width - 80;
                    tableHead.SetWidths(new Single[] { 3.0F, 3.0F });
                    tableHead.LockedWidth = true;

                    myPhrase = new Phrase("La cantidad de: ***" + enletras(decimal.Round(Convert.ToDecimal(rdr["ADVANCE"]), 2).ToString()).ToLower() + "***", myfont1);
                    cell2.Phrase = myPhrase; cell2.HorizontalAlignment = Element.ALIGN_LEFT;
                    tableHead.AddCell(cell2);

                    myPhrase = new Phrase("No.: " + sequencialMonth + "      Recibo No.: ", myfont1);
                    cell2.Phrase = myPhrase; cell2.HorizontalAlignment = Element.ALIGN_RIGHT;
                    tableHead.AddCell(cell2);

                    myPhrase = new Phrase("Departamento: " + rdr["ORGANIZATIONID"], myfont1);
                    cell2.Phrase = myPhrase; cell2.HorizontalAlignment = Element.ALIGN_LEFT;
                    tableHead.AddCell(cell2);

                    myPhrase = new Phrase("Planilla: " + rdr["PAYROLLID"], myfont1);
                    cell2.Phrase = myPhrase; cell2.HorizontalAlignment = Element.ALIGN_RIGHT;
                    tableHead.AddCell(cell2);

                    myPhrase = new Phrase("Puesto: " + rdr["JOBID"], myfont1);
                    cell2.Phrase = myPhrase; cell2.HorizontalAlignment = Element.ALIGN_LEFT;
                    tableHead.AddCell(cell2);

                    myPhrase = new Phrase("Cuenta: " + rdr["BANKACCOUNTEMPL"], myfont1);
                    cell2.Phrase = myPhrase; cell2.HorizontalAlignment = Element.ALIGN_RIGHT;
                    tableHead.AddCell(cell2);

                    myPhrase = new Phrase();
                    cell2.Phrase = myPhrase;
                    tableHead.AddCell(cell2);

                    document.Add(tableHead);
                    //////////////////////// TABLA ENCABEZADO //////////////////////////

                    myParagraph = new Paragraph();
                    myPhrase = new Phrase(" ");
                    myParagraph.Alignment = Element.ALIGN_LEFT;
                    myParagraph.Add(myPhrase); myParagraph.Leading = 10.0f;
                    document.Add(myParagraph);

                    ////////////////////////// TABLA DATOS /////////////////////////////
                    //iTextSharp.text.pdf.PdfPTable table = new PdfPTable(3);
                    iTextSharp.text.pdf.PdfPTable table2 = new PdfPTable(2);
                    table2.TotalWidth = document.PageSize.Width - 300;
                    table2.LockedWidth = true;
                    iTextSharp.text.pdf.PdfPTable table3 = new PdfPTable(2);
                    table3.TotalWidth = document.PageSize.Width - 80;
                    table3.LockedWidth = true;

                    table2.SetWidths(new Single[] { 1.0F, 4.0F });    //Tabla Ingresos
                    table3.SetWidths(new Single[] { 1.0F, 5.0F });          //Tabla Deducciones

                    PdfPCell cell;

                    myPhrase = new Phrase("Anticipo ..................:", myfont1);
                    cell = new PdfPCell(myPhrase); cell.BorderWidth = 0;
                    table3.AddCell(cell);

                    myPhrase = new Phrase(Convert.ToString(decimal.Round(Convert.ToDecimal(rdr["ADVANCE"]), 2).ToString("###,###.00")), myfont1);
                    cell = new PdfPCell(myPhrase); cell.BorderWidth = 0;
                    table3.AddCell(cell);

                    document.Add(table3);

                    myParagraph = new Paragraph();
                    myPhrase = new Phrase(" ");
                    myParagraph.Alignment = Element.ALIGN_LEFT;
                    myParagraph.Add(myPhrase); myParagraph.Leading = 10.0f;
                    document.Add(myParagraph);

                    /*myPhrase = new Phrase("FIRMA:", myfont1);
                    cell = new PdfPCell(myPhrase); cell.HorizontalAlignment = Element.ALIGN_RIGHT; cell.BorderWidth = 0;
                    table2.AddCell(cell);
                    myPhrase = new Phrase("_____________________________________________", myfont1);
                    cell = new PdfPCell(myPhrase); cell.HorizontalAlignment = Element.ALIGN_LEFT; cell.BorderWidth = 0;
                    table2.AddCell(cell);*/
                    myPhrase = new Phrase("NOMBRE", myfont1);
                    cell = new PdfPCell(myPhrase); cell.HorizontalAlignment = Element.ALIGN_RIGHT; cell.BorderWidth = 0;
                    table2.AddCell(cell);
                    myPhrase = new Phrase(Convert.ToString(rdr["EMPLNAME"]), myfont1);
                    cell = new PdfPCell(myPhrase); cell.HorizontalAlignment = Element.ALIGN_LEFT; cell.BorderWidth = 0;
                    table2.AddCell(cell);
                    myPhrase = new Phrase("CEDULA O DPI:", myfont1);
                    cell = new PdfPCell(myPhrase); cell.HorizontalAlignment = Element.ALIGN_RIGHT; cell.BorderWidth = 0;
                    table2.AddCell(cell);
                    myPhrase = new Phrase(Convert.ToString(rdr["EMPLIDENTNUMBER"]), myfont1);
                    cell = new PdfPCell(myPhrase); cell.HorizontalAlignment = Element.ALIGN_LEFT; cell.BorderWidth = 0;
                    table2.AddCell(cell);

                    document.Add(table2);

                    ////////////////////////// TABLA DATOS /////////////////////////////


                    cell2 = new PdfPCell();
                    cell2.BorderWidth = 0;
                    iTextSharp.text.pdf.PdfPTable tableFoot = new PdfPTable(1);
                    tableFoot.TotalWidth = document.PageSize.Width - 80;
                    tableFoot.SetWidths(new Single[] { 3.0F });
                    tableFoot.LockedWidth = true;

                    myParagraph = new Paragraph();
                    myPhrase = new Phrase("Guatemala, " + DateTime.Now.Day.ToString() + " de " + DateTime.Now.ToString("MMMM") + " de " + DateTime.Now.Year.ToString(), myfont1);
                    myParagraph.Alignment = Element.ALIGN_LEFT;
                    myParagraph.Add(myPhrase); myParagraph.Leading = 24f;
                    cell2.AddElement(myParagraph);

                    myParagraph = new Paragraph();
                    myPhrase = new Phrase("Recibo de conformidad, el importe neto indicado, el cual cubre", new iTextSharp.text.Font(FontFactory.GetFont(FontFactory.HELVETICA, 6, iTextSharp.text.Font.NORMAL)));
                    myParagraph.Alignment = Element.ALIGN_LEFT;
                    myParagraph.Add(myPhrase); //myParagraph.Leading = 30f;                    
                    cell2.AddElement(myParagraph);

                    myParagraph = new Paragraph();
                    myPhrase = new Phrase("la totalidad de los servicios prestados durante el periodo.", new iTextSharp.text.Font(FontFactory.GetFont(FontFactory.HELVETICA, 6, iTextSharp.text.Font.NORMAL)));
                    myParagraph.Alignment = Element.ALIGN_LEFT;
                    myParagraph.Add(myPhrase); myParagraph.Leading = 7f;
                    cell2.AddElement(myParagraph);

                    tableFoot.AddCell(cell2);
                    document.Add(tableFoot);

                    iTextSharp.text.Image lineaF = iTextSharp.text.Image.GetInstance(@"C:\CS\Img\lineaFirma.png");
                    lineaF.SetAbsolutePosition((document.PageSize.Width - Convert.ToInt16(lineaF.Width * 0.66)) - 50, 40);
                    lineaF.ScalePercent(66);
                    cb.AddImage(lineaF);

                    myParagraph = new Paragraph();
                    document.Add(myParagraph);
                    myParagraph.Clear();

                    cb.EndLayer();
                    document.Close();
                    arch = inStream.GetBuffer();

                    WSExpediente.Service ser = new WSExpediente.Service();
                    string fileName = sequencialMonth + "-" + rdr["PICTUREID"] + "-" + DateTime.Now.ToString("ddMMyyyy") + "-0.pdf";
                    if (validaExisteBoleta(sequencialMonth, rdr["PICTUREID"].ToString()) && Convert.ToDecimal(rdr["ADVANCE"]) > 0)
                    {
                        ser.Guardar("PR", 35, fileName, "application/pdf", arch, "DBAFISICC");
                        //File.WriteAllBytes(@"c:\\prueba\"+fileName, arch);
                        contador++;
                    }
                    inStream.Dispose();
                }
            }
            catch (Exception ex)
            {
                System.Web.HttpContext.Current.Response.Write(ex.Message);
            }
            //break;
        }
        rdr.Close();

        return "Cantidad de boletas generadas: " + contador.ToString();
    }


    [WebMethod]
    // Boletas de facturacion adm - Fin de mes
    public string generateInvoiceReceipt(string _journalId)
    {
        int contador = 0;
        string journalId = _journalId; //"000341_160";
        string sequencialMonth;
        Axapta ax = new Axapta();
        string[] ParmsClass = new string[2];

        conn.Open();
        SqlCommand cmd = new SqlCommand("dbo.InvoiceTransReceipt_Head", conn);
        SqlParameter parm = new SqlParameter("@journalId", journalId);
        cmd.Parameters.Add(parm);
        cmd.CommandType = CommandType.StoredProcedure;
        SqlDataReader rdr = cmd.ExecuteReader();

        while (rdr.Read())
        {
            Document document = new Document(new iTextSharp.text.Rectangle(612f, 396.27f));
            document.SetMargins(40, 40, 40, 5);
            document.PageSize.Rotate();
            document.AddAuthor("Universidad Galileo");
            document.AddCreator("Universidad Galileo");
            document.AddCreationDate();
            document.AddTitle("Recibo de Pago por Facturacion");
            document.AddSubject("Recibo de pago por Facturacion");
            document.AddKeywords("Recibo ; Pago ; Boleta ; Facturacion");
            byte[] arch = null;

            using (MemoryStream inStream = new MemoryStream())
            {
                PdfWriter writer = PdfWriter.GetInstance(document, inStream);

                document.Open();

                //Capa 1 - Imagenes
                PdfLayer layer = new PdfLayer("MyLayer1", writer);
                PdfContentByte cb = writer.DirectContent;
                cb.BeginLayer(layer);
                iTextSharp.text.Image logoPeq = iTextSharp.text.Image.GetInstance(@"C:\CS\Img\LogoGalileoPequeño.png");
                logoPeq.SetAbsolutePosition(40, document.PageSize.Height - Convert.ToInt16(logoPeq.Height * 0.4) - 20);
                logoPeq.ScalePercent(40);
                cb.AddImage(logoPeq);

                iTextSharp.text.Image logoGran = iTextSharp.text.Image.GetInstance(@"C:\CS\Img\LogoGalileoGrande.png");
                logoGran.SetAbsolutePosition((document.PageSize.Width / 2) - (Convert.ToInt16(logoGran.Width * 0.4) / 2), (document.PageSize.Height / 2) - (Convert.ToInt16(logoGran.Height * 0.4) / 2));
                logoGran.ScalePercent(40);
                cb.AddImage(logoGran);
                cb.EndLayer();
                //Fin capa 1

                iTextSharp.text.Font myfont1 = new iTextSharp.text.Font(FontFactory.GetFont(FontFactory.HELVETICA, 8, iTextSharp.text.Font.NORMAL));

                Phrase myPhrase;

                //Capa 2 - Datos
                layer = new PdfLayer("MyLayer2", writer);
                cb.BeginLayer(layer);
                Paragraph myParagraph = new Paragraph();
                myPhrase = new Phrase("Universidad Galileo", new iTextSharp.text.Font(FontFactory.GetFont(FontFactory.HELVETICA, 16, iTextSharp.text.Font.BOLD)));
                myParagraph.Add(myPhrase);
                myParagraph.Alignment = Element.ALIGN_CENTER;
                document.Add(myParagraph);

                SqlCommand cmdInfo = new SqlCommand("dbo.JournalTableInfo", conn);
                cmdInfo.Parameters.Add(new SqlParameter("@journalId", journalId));
                cmdInfo.CommandType = CommandType.StoredProcedure;
                SqlDataReader rdrInfo = cmdInfo.ExecuteReader();

                DateTime dtimeReceipt = DateTime.Now;
                while (rdrInfo.Read())
                {
                    myParagraph = new Paragraph();
                    myPhrase = new Phrase(Convert.ToString(rdrInfo["NAME"]), new iTextSharp.text.Font(FontFactory.GetFont(FontFactory.HELVETICA, 15, iTextSharp.text.Font.NORMAL)));
                    myParagraph.Alignment = Element.ALIGN_CENTER;
                    myParagraph.Add(myPhrase);
                    document.Add(myParagraph);

                    myParagraph = new Paragraph();
                    myPhrase = new Phrase("RECIBO DE PAGO POR FACTURACION", new iTextSharp.text.Font(FontFactory.GetFont(FontFactory.HELVETICA, 15, iTextSharp.text.Font.BOLD)));
                    myParagraph.Alignment = Element.ALIGN_CENTER;
                    myParagraph.Add(myPhrase);
                    document.Add(myParagraph);

                    dtimeReceipt = Convert.ToDateTime(rdrInfo["FROMDATE"]);
                    myParagraph = new Paragraph();
                    myPhrase = new Phrase("Periodo: Del " + Convert.ToDateTime(rdrInfo["FROMDATE"]).ToString("dd/MM/yyyy") + " al " + Convert.ToDateTime(rdrInfo["TODATE"]).ToString("dd/MM/yyyy"), new iTextSharp.text.Font(FontFactory.GetFont(FontFactory.HELVETICA, 14, iTextSharp.text.Font.BOLD)));
                    myParagraph.Alignment = Element.ALIGN_CENTER;
                    myParagraph.Add(myPhrase);
                    document.Add(myParagraph);
                }

                sequencialMonth = rdr["SEQUENCIALRECEIPT"].ToString();

                myParagraph = new Paragraph();
                myPhrase = new Phrase(" ");
                myParagraph.Alignment = Element.ALIGN_LEFT;
                myParagraph.Add(myPhrase); myParagraph.Leading = 24.0f;
                document.Add(myParagraph);

                //////////////////////// TABLA ENCABEZADO //////////////////////////
                PdfPCell cell2 = new PdfPCell();
                cell2.BorderWidth = 0;
                iTextSharp.text.pdf.PdfPTable tableHead = new PdfPTable(2);
                tableHead.TotalWidth = document.PageSize.Width - 80;
                tableHead.SetWidths(new Single[] { 3.0F, 3.0F });
                tableHead.LockedWidth = true;

                myPhrase = new Phrase("Proveedor: " + rdr["VENDACCOUNT"].ToString(), myfont1);
                cell2.Phrase = myPhrase; cell2.HorizontalAlignment = Element.ALIGN_LEFT;
                tableHead.AddCell(cell2);

                myPhrase = new Phrase("Recibo No.: " + sequencialMonth, myfont1);
                cell2.Phrase = myPhrase; cell2.HorizontalAlignment = Element.ALIGN_RIGHT;
                tableHead.AddCell(cell2);

                myPhrase = new Phrase("Empleado: " + rdr["EMPLID"], myfont1);
                cell2.Phrase = myPhrase; cell2.HorizontalAlignment = Element.ALIGN_LEFT;
                tableHead.AddCell(cell2);

                myPhrase = new Phrase("Cuenta: " + rdr["BANKACCOUNTEMPL"], myfont1);
                cell2.Phrase = myPhrase; cell2.HorizontalAlignment = Element.ALIGN_RIGHT;
                tableHead.AddCell(cell2);

                myPhrase = new Phrase(rdr["NAME"].ToString(), myfont1);
                cell2.Phrase = myPhrase; cell2.HorizontalAlignment = Element.ALIGN_LEFT;
                tableHead.AddCell(cell2);

                myPhrase = new Phrase("Liquido:  Q. " + Convert.ToString(decimal.Round(Convert.ToDecimal(rdr["LIQUID"]) - Convert.ToDecimal(rdr["TAX"]), 2).ToString("###,##0.00")), myfont1);
                cell2.Phrase = myPhrase; cell2.HorizontalAlignment = Element.ALIGN_RIGHT;
                tableHead.AddCell(cell2);


                myPhrase = new Phrase();
                cell2.Phrase = myPhrase;
                tableHead.AddCell(cell2);

                document.Add(tableHead);
                //////////////////////// TABLA ENCABEZADO //////////////////////////

                myParagraph = new Paragraph();
                myPhrase = new Phrase(" ");
                myParagraph.Alignment = Element.ALIGN_LEFT;
                myParagraph.Add(myPhrase); myParagraph.Leading = 10.0f;
                document.Add(myParagraph);

                ////////////////////////// TABLA DATOS /////////////////////////////
                iTextSharp.text.pdf.PdfPTable table3 = new PdfPTable(3);
                table3.TotalWidth = document.PageSize.Width - 120;
                table3.LockedWidth = true;

                table3.SetWidths(new Single[] { 4.0F, 2.0F, 2.0F });

                PdfPCell cell;

                // Encabezado ingresos
                myPhrase = new Phrase("DESCRIPCION", myfont1);
                cell = new PdfPCell(myPhrase); cell.HorizontalAlignment = Element.ALIGN_LEFT; cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                table3.AddCell(cell);
                myPhrase = new Phrase("CREDITO", myfont1);
                cell = new PdfPCell(myPhrase); cell.HorizontalAlignment = Element.ALIGN_RIGHT; cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                table3.AddCell(cell);
                myPhrase = new Phrase("DEBITO", myfont1);
                cell = new PdfPCell(myPhrase); cell.HorizontalAlignment = Element.ALIGN_RIGHT; cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                table3.AddCell(cell);

                // Obtencion de datos - Ingresos
                SqlCommand cmd2 = new SqlCommand("dbo.InvoiceTransReceipt_Body", conn);
                cmd2.Parameters.Add("@journalId", SqlDbType.NVarChar, 10).Value = journalId;
                cmd2.Parameters.Add("@vendAccount", SqlDbType.NVarChar, 10).Value = rdr["VENDACCOUNT"];
                cmd2.CommandType = CommandType.StoredProcedure;
                SqlDataReader qrdr = cmd2.ExecuteReader();
                //conn.Close();
                decimal amount = 0;
                while (qrdr.Read())
                {
                    myPhrase = new Phrase(Convert.ToString(qrdr["DESCRIPTION"]), myfont1);
                    cell = new PdfPCell(myPhrase); cell.BorderWidth = 0;
                    table3.AddCell(cell);
                    myPhrase = new Phrase(Convert.ToString(decimal.Round(Convert.ToDecimal(qrdr["AMOUNT"]), 2).ToString("###,##0.00")), myfont1);
                    cell = new PdfPCell(myPhrase); cell.HorizontalAlignment = Element.ALIGN_RIGHT; cell.BorderWidth = 0;
                    table3.AddCell(cell);
                    myPhrase = new Phrase(Convert.ToString(decimal.Round(Convert.ToDecimal("0"), 2).ToString("0.00")), myfont1);
                    cell = new PdfPCell(myPhrase); cell.HorizontalAlignment = Element.ALIGN_RIGHT; cell.BorderWidth = 0;
                    table3.AddCell(cell);

                    amount += Convert.ToDecimal(qrdr["AMOUNT"]);
                }

                myPhrase = new Phrase("ISR PROVEEDORES", myfont1);
                cell = new PdfPCell(myPhrase); cell.BorderWidth = 0;
                table3.AddCell(cell);
                myPhrase = new Phrase(Convert.ToString(decimal.Round(Convert.ToDecimal("0"), 2).ToString("0.00")), myfont1);
                cell = new PdfPCell(myPhrase); cell.HorizontalAlignment = Element.ALIGN_RIGHT; cell.BorderWidth = 0;
                table3.AddCell(cell);
                myPhrase = new Phrase(Convert.ToString(decimal.Round(Convert.ToDecimal(rdr["TAX"]), 2).ToString("###,##0.00")), myfont1);
                cell = new PdfPCell(myPhrase); cell.HorizontalAlignment = Element.ALIGN_RIGHT; cell.BorderWidth = 0;
                table3.AddCell(cell);

                myPhrase = new Phrase("ANTICIPO QUINCENAL", myfont1);
                cell = new PdfPCell(myPhrase); cell.BorderWidth = 0;
                table3.AddCell(cell);
                myPhrase = new Phrase(Convert.ToString(decimal.Round(Convert.ToDecimal("0"), 2).ToString("0.00")), myfont1);
                cell = new PdfPCell(myPhrase); cell.HorizontalAlignment = Element.ALIGN_RIGHT; cell.BorderWidth = 0;
                table3.AddCell(cell);
                myPhrase = new Phrase(Convert.ToString(decimal.Round(Convert.ToDecimal(rdr["ADVANCE"]), 2).ToString("###,##0.00")), myfont1);
                cell = new PdfPCell(myPhrase); cell.HorizontalAlignment = Element.ALIGN_RIGHT; cell.BorderWidth = 0;
                table3.AddCell(cell);

                myPhrase = new Phrase("DEPOSITO A SU CUENTA", myfont1);
                cell = new PdfPCell(myPhrase); cell.BorderWidth = 0;
                table3.AddCell(cell);
                myPhrase = new Phrase(Convert.ToString(decimal.Round(Convert.ToDecimal("0"), 2).ToString("0.00")), myfont1);
                cell = new PdfPCell(myPhrase); cell.HorizontalAlignment = Element.ALIGN_RIGHT; cell.BorderWidth = 0;
                table3.AddCell(cell);
                myPhrase = new Phrase(Convert.ToString(decimal.Round(Convert.ToDecimal(rdr["LIQUID"]) - Convert.ToDecimal(rdr["TAX"]), 2).ToString("###,##0.00")), myfont1);
                cell = new PdfPCell(myPhrase); cell.HorizontalAlignment = Element.ALIGN_RIGHT; cell.BorderWidth = 0;
                table3.AddCell(cell);

                document.Add(table3);

                myParagraph = new Paragraph();
                myPhrase = new Phrase(" ");
                myParagraph.Alignment = Element.ALIGN_LEFT;
                myParagraph.Add(myPhrase); myParagraph.Leading = 10.0f;
                document.Add(myParagraph);

                ////////////////////////// TABLA DATOS /////////////////////////////


                cell2 = new PdfPCell();
                cell2.BorderWidth = 0;
                iTextSharp.text.pdf.PdfPTable tableFoot = new PdfPTable(1);
                tableFoot.TotalWidth = document.PageSize.Width - 80;
                tableFoot.SetWidths(new Single[] { 3.0F });
                tableFoot.LockedWidth = true;

                myParagraph = new Paragraph();
                myPhrase = new Phrase("Guatemala, " + DateTime.Now.Day.ToString() + " de " + DateTime.Now.ToString("MMMM") + " de " + DateTime.Now.Year.ToString(), myfont1);
                myParagraph.Alignment = Element.ALIGN_LEFT;
                myParagraph.Add(myPhrase); myParagraph.Leading = 24f;
                cell2.AddElement(myParagraph);

                myParagraph = new Paragraph();
                myPhrase = new Phrase("Recibo de conformidad, el importe neto indicado, el cual cubre", new iTextSharp.text.Font(FontFactory.GetFont(FontFactory.HELVETICA, 6, iTextSharp.text.Font.NORMAL)));
                myParagraph.Alignment = Element.ALIGN_LEFT;
                myParagraph.Add(myPhrase); //myParagraph.Leading = 30f;                    
                cell2.AddElement(myParagraph);

                myParagraph = new Paragraph();
                myPhrase = new Phrase("la totalidad de los servicios prestados durante el periodo.", new iTextSharp.text.Font(FontFactory.GetFont(FontFactory.HELVETICA, 6, iTextSharp.text.Font.NORMAL)));
                myParagraph.Alignment = Element.ALIGN_LEFT;
                myParagraph.Add(myPhrase); myParagraph.Leading = 7f;
                cell2.AddElement(myParagraph);

                tableFoot.AddCell(cell2);
                document.Add(tableFoot);

                iTextSharp.text.Image lineaF = iTextSharp.text.Image.GetInstance(@"C:\CS\Img\lineaFirma.png");
                lineaF.SetAbsolutePosition((document.PageSize.Width - Convert.ToInt16(lineaF.Width * 0.66)) - 50, 40);
                lineaF.ScalePercent(66);
                cb.AddImage(lineaF);

                myParagraph = new Paragraph();
                document.Add(myParagraph);
                myParagraph.Clear();

                cb.EndLayer();
                document.Close();
                arch = inStream.GetBuffer();

                WSExpediente.Service ser = new WSExpediente.Service();
                string fileName = sequencialMonth + "-" + rdr["PICTUREID"] + "-" + DateTime.Now.ToString("ddMMyyyy") + "-0.pdf";
                if (validaExisteBoleta(sequencialMonth, rdr["PICTUREID"].ToString()) && Convert.ToDecimal(rdr["LIQUID"]) > 0)
                {
                    ser.Guardar("PR", 35, fileName, "application/pdf", arch, "DBAFISICC");
                    //File.WriteAllBytes(@"c:\\prueba\"+fileName, arch);
                    contador++;
                }
                inStream.Dispose();
            }

            //break;
        }
        rdr.Close();

        return "Cantidad de boletas generadas: " + contador.ToString();
    }


    [WebMethod]
    // Boletas de facturacion adm - quincena
    public string generateInvoiceAdvanceReceipt(string _journalId)
    {
        int contador = 0;
        string journalId = _journalId; //"000341_160";
        string sequencialMonth;
        Axapta ax = new Axapta();
        string[] ParmsClass = new string[2];

        conn.Open();
        SqlCommand cmd = new SqlCommand("dbo.InvoiceTransReceipt_Head", conn);
        SqlParameter parm = new SqlParameter("@journalId", journalId);
        cmd.Parameters.Add(parm);
        cmd.CommandType = CommandType.StoredProcedure;
        SqlDataReader rdr = cmd.ExecuteReader();

        while (rdr.Read())
        {
            Document document = new Document(new iTextSharp.text.Rectangle(612f, 396.27f));
            document.SetMargins(40, 40, 40, 5);
            document.PageSize.Rotate();
            document.AddAuthor("Universidad Galileo");
            document.AddCreator("Universidad Galileo");
            document.AddCreationDate();
            document.AddTitle("Recibo de Pago por Facturacion");
            document.AddSubject("Recibo de pago por Facturacion");
            document.AddKeywords("Recibo ; Pago ; Boleta ; Facturacion");
            byte[] arch = null;

            using (MemoryStream inStream = new MemoryStream())
            {
                PdfWriter writer = PdfWriter.GetInstance(document, inStream);

                document.Open();

                //Capa 1 - Imagenes
                PdfLayer layer = new PdfLayer("MyLayer1", writer);
                PdfContentByte cb = writer.DirectContent;
                cb.BeginLayer(layer);
                iTextSharp.text.Image logoPeq = iTextSharp.text.Image.GetInstance(@"C:\CS\Img\LogoGalileoPequeño.png");
                logoPeq.SetAbsolutePosition(40, document.PageSize.Height - Convert.ToInt16(logoPeq.Height * 0.4) - 20);
                logoPeq.ScalePercent(40);
                cb.AddImage(logoPeq);

                iTextSharp.text.Image logoGran = iTextSharp.text.Image.GetInstance(@"C:\CS\Img\LogoGalileoGrande.png");
                logoGran.SetAbsolutePosition((document.PageSize.Width / 2) - (Convert.ToInt16(logoGran.Width * 0.4) / 2), (document.PageSize.Height / 2) - (Convert.ToInt16(logoGran.Height * 0.4) / 2));
                logoGran.ScalePercent(40);
                cb.AddImage(logoGran);
                cb.EndLayer();
                //Fin capa 1

                iTextSharp.text.Font myfont1 = new iTextSharp.text.Font(FontFactory.GetFont(FontFactory.HELVETICA, 8, iTextSharp.text.Font.NORMAL));

                Phrase myPhrase;

                //Capa 2 - Datos
                layer = new PdfLayer("MyLayer2", writer);
                cb.BeginLayer(layer);
                Paragraph myParagraph = new Paragraph();
                myPhrase = new Phrase("Universidad Galileo", new iTextSharp.text.Font(FontFactory.GetFont(FontFactory.HELVETICA, 16, iTextSharp.text.Font.BOLD)));
                myParagraph.Add(myPhrase);
                myParagraph.Alignment = Element.ALIGN_CENTER;
                document.Add(myParagraph);

                SqlCommand cmdInfo = new SqlCommand("dbo.JournalTableInfo", conn);
                cmdInfo.Parameters.Add(new SqlParameter("@journalId", journalId));
                cmdInfo.CommandType = CommandType.StoredProcedure;
                SqlDataReader rdrInfo = cmdInfo.ExecuteReader();

                DateTime dtimeReceipt = DateTime.Now;
                while (rdrInfo.Read())
                {
                    myParagraph = new Paragraph();
                    myPhrase = new Phrase(Convert.ToString(rdrInfo["NAME"]), new iTextSharp.text.Font(FontFactory.GetFont(FontFactory.HELVETICA, 15, iTextSharp.text.Font.NORMAL)));
                    myParagraph.Alignment = Element.ALIGN_CENTER;
                    myParagraph.Add(myPhrase);
                    document.Add(myParagraph);

                    myParagraph = new Paragraph();
                    myPhrase = new Phrase("RECIBO DE PAGO POR FACTURACION", new iTextSharp.text.Font(FontFactory.GetFont(FontFactory.HELVETICA, 15, iTextSharp.text.Font.BOLD)));
                    myParagraph.Alignment = Element.ALIGN_CENTER;
                    myParagraph.Add(myPhrase);
                    document.Add(myParagraph);

                    dtimeReceipt = Convert.ToDateTime(rdrInfo["FROMDATE"]);
                    myParagraph = new Paragraph();
                    myPhrase = new Phrase("Periodo: Del " + Convert.ToDateTime(rdrInfo["FROMDATE"]).ToString("dd/MM/yyyy") + " al " + Convert.ToDateTime(rdrInfo["TODATE"]).ToString("dd/MM/yyyy"), new iTextSharp.text.Font(FontFactory.GetFont(FontFactory.HELVETICA, 14, iTextSharp.text.Font.BOLD)));
                    myParagraph.Alignment = Element.ALIGN_CENTER;
                    myParagraph.Add(myPhrase);
                    document.Add(myParagraph);
                }

                sequencialMonth = rdr["ADVANCESEQUENCIALRECEIPT"].ToString();

                myParagraph = new Paragraph();
                myPhrase = new Phrase(" ");
                myParagraph.Alignment = Element.ALIGN_LEFT;
                myParagraph.Add(myPhrase); myParagraph.Leading = 24.0f;
                document.Add(myParagraph);

                //////////////////////// TABLA ENCABEZADO //////////////////////////
                PdfPCell cell2 = new PdfPCell();
                cell2.BorderWidth = 0;
                iTextSharp.text.pdf.PdfPTable tableHead = new PdfPTable(2);
                tableHead.TotalWidth = document.PageSize.Width - 80;
                tableHead.SetWidths(new Single[] { 3.0F, 3.0F });
                tableHead.LockedWidth = true;

                myPhrase = new Phrase("Proveedor: " + rdr["VENDACCOUNT"].ToString(), myfont1);
                cell2.Phrase = myPhrase; cell2.HorizontalAlignment = Element.ALIGN_LEFT;
                tableHead.AddCell(cell2);

                myPhrase = new Phrase("Recibo No.: " + sequencialMonth, myfont1);
                cell2.Phrase = myPhrase; cell2.HorizontalAlignment = Element.ALIGN_RIGHT;
                tableHead.AddCell(cell2);

                myPhrase = new Phrase("Empleado: " + rdr["EMPLID"], myfont1);
                cell2.Phrase = myPhrase; cell2.HorizontalAlignment = Element.ALIGN_LEFT;
                tableHead.AddCell(cell2);

                myPhrase = new Phrase("Cuenta: " + rdr["BANKACCOUNTEMPL"], myfont1);
                cell2.Phrase = myPhrase; cell2.HorizontalAlignment = Element.ALIGN_RIGHT;
                tableHead.AddCell(cell2);

                myPhrase = new Phrase(rdr["NAME"].ToString(), myfont1);
                cell2.Phrase = myPhrase; cell2.HorizontalAlignment = Element.ALIGN_LEFT;
                tableHead.AddCell(cell2);

                myPhrase = new Phrase("Liquido:  Q. " + Convert.ToString(decimal.Round(Convert.ToDecimal(rdr["ADVANCE"]), 2).ToString("###,##0.00")), myfont1);
                cell2.Phrase = myPhrase; cell2.HorizontalAlignment = Element.ALIGN_RIGHT;
                tableHead.AddCell(cell2);


                myPhrase = new Phrase();
                cell2.Phrase = myPhrase;
                tableHead.AddCell(cell2);

                document.Add(tableHead);
                //////////////////////// TABLA ENCABEZADO //////////////////////////

                myParagraph = new Paragraph();
                myPhrase = new Phrase(" ");
                myParagraph.Alignment = Element.ALIGN_LEFT;
                myParagraph.Add(myPhrase); myParagraph.Leading = 10.0f;
                document.Add(myParagraph);

                ////////////////////////// TABLA DATOS /////////////////////////////
                iTextSharp.text.pdf.PdfPTable table3 = new PdfPTable(3);
                table3.TotalWidth = document.PageSize.Width - 120;
                table3.LockedWidth = true;

                table3.SetWidths(new Single[] { 4.0F, 2.0F, 2.0F });

                PdfPCell cell;

                // Encabezado ingresos
                myPhrase = new Phrase("DESCRIPCION", myfont1);
                cell = new PdfPCell(myPhrase); cell.HorizontalAlignment = Element.ALIGN_LEFT; cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                table3.AddCell(cell);
                myPhrase = new Phrase("CREDITO", myfont1);
                cell = new PdfPCell(myPhrase); cell.HorizontalAlignment = Element.ALIGN_RIGHT; cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                table3.AddCell(cell);
                myPhrase = new Phrase("DEBITO", myfont1);
                cell = new PdfPCell(myPhrase); cell.HorizontalAlignment = Element.ALIGN_RIGHT; cell.BorderWidth = 0; cell.BorderWidthBottom = 0.5f;
                table3.AddCell(cell);

                // Obtencion de datos - Ingresos
                myPhrase = new Phrase("ANTICIPO QUINCENAL", myfont1);
                cell = new PdfPCell(myPhrase); cell.BorderWidth = 0;
                table3.AddCell(cell);
                myPhrase = new Phrase(Convert.ToString(decimal.Round(Convert.ToDecimal(rdr["ADVANCE"]), 2).ToString("###,##0.00")), myfont1);
                cell = new PdfPCell(myPhrase); cell.HorizontalAlignment = Element.ALIGN_RIGHT; cell.BorderWidth = 0;
                table3.AddCell(cell);
                myPhrase = new Phrase(Convert.ToString(decimal.Round(Convert.ToDecimal("0"), 2).ToString("0.00")), myfont1);
                cell = new PdfPCell(myPhrase); cell.HorizontalAlignment = Element.ALIGN_RIGHT; cell.BorderWidth = 0;
                table3.AddCell(cell);

                myPhrase = new Phrase("DEPOSITO A CUENTA", myfont1);
                cell = new PdfPCell(myPhrase); cell.BorderWidth = 0;
                table3.AddCell(cell);
                myPhrase = new Phrase(Convert.ToString(decimal.Round(Convert.ToDecimal("0"), 2).ToString("0.00")), myfont1);
                cell = new PdfPCell(myPhrase); cell.HorizontalAlignment = Element.ALIGN_RIGHT; cell.BorderWidth = 0;
                table3.AddCell(cell);
                myPhrase = new Phrase(Convert.ToString(decimal.Round(Convert.ToDecimal(rdr["ADVANCE"]), 2).ToString("###,##0.00")), myfont1);
                cell = new PdfPCell(myPhrase); cell.HorizontalAlignment = Element.ALIGN_RIGHT; cell.BorderWidth = 0;
                table3.AddCell(cell);

                document.Add(table3);

                myParagraph = new Paragraph();
                myPhrase = new Phrase(" ");
                myParagraph.Alignment = Element.ALIGN_LEFT;
                myParagraph.Add(myPhrase); myParagraph.Leading = 10.0f;
                document.Add(myParagraph);

                ////////////////////////// TABLA DATOS /////////////////////////////


                cell2 = new PdfPCell();
                cell2.BorderWidth = 0;
                iTextSharp.text.pdf.PdfPTable tableFoot = new PdfPTable(1);
                tableFoot.TotalWidth = document.PageSize.Width - 80;
                tableFoot.SetWidths(new Single[] { 3.0F });
                tableFoot.LockedWidth = true;

                myParagraph = new Paragraph();
                myPhrase = new Phrase("Guatemala, " + DateTime.Now.Day.ToString() + " de " + DateTime.Now.ToString("MMMM") + " de " + DateTime.Now.Year.ToString(), myfont1);
                myParagraph.Alignment = Element.ALIGN_LEFT;
                myParagraph.Add(myPhrase); myParagraph.Leading = 24f;
                cell2.AddElement(myParagraph);

                myParagraph = new Paragraph();
                myPhrase = new Phrase("Recibo de conformidad, el importe neto indicado, el cual cubre", new iTextSharp.text.Font(FontFactory.GetFont(FontFactory.HELVETICA, 6, iTextSharp.text.Font.NORMAL)));
                myParagraph.Alignment = Element.ALIGN_LEFT;
                myParagraph.Add(myPhrase); //myParagraph.Leading = 30f;                    
                cell2.AddElement(myParagraph);

                myParagraph = new Paragraph();
                myPhrase = new Phrase("la totalidad de los servicios prestados durante el periodo.", new iTextSharp.text.Font(FontFactory.GetFont(FontFactory.HELVETICA, 6, iTextSharp.text.Font.NORMAL)));
                myParagraph.Alignment = Element.ALIGN_LEFT;
                myParagraph.Add(myPhrase); myParagraph.Leading = 7f;
                cell2.AddElement(myParagraph);

                tableFoot.AddCell(cell2);
                document.Add(tableFoot);

                iTextSharp.text.Image lineaF = iTextSharp.text.Image.GetInstance(@"C:\CS\Img\lineaFirma.png");
                lineaF.SetAbsolutePosition((document.PageSize.Width - Convert.ToInt16(lineaF.Width * 0.66)) - 50, 40);
                lineaF.ScalePercent(66);
                cb.AddImage(lineaF);

                myParagraph = new Paragraph();
                document.Add(myParagraph);
                myParagraph.Clear();

                cb.EndLayer();
                document.Close();
                arch = inStream.GetBuffer();

                WSExpediente.Service ser = new WSExpediente.Service();
                string fileName = sequencialMonth + "-" + rdr["PICTUREID"] + "-" + DateTime.Now.ToString("ddMMyyyy") + "-0.pdf";
                if (validaExisteBoleta(sequencialMonth, rdr["PICTUREID"].ToString()) && Convert.ToDecimal(rdr["ADVANCE"]) > 0)
                {
                    ser.Guardar("PR", 35, fileName, "application/pdf", arch, "DBAFISICC");
                    //File.WriteAllBytes(@"c:\\prueba\"+fileName, arch);
                    contador++;
                }
                inStream.Dispose();
            }

            //break;
        }
        rdr.Close();

        return "Cantidad de boletas generadas: " + contador.ToString();
    }




    [WebMethod]
    // 
    public string obtenerExpediente(string _emplId)
    {
        string fileEnc = "";
        int filaFechaMaxima = 0, cont = 0;
        WSExpediente.Service ser1 = new WSExpediente.Service();
        WSExpediente.Etiquetas[] et = new WSExpediente.Etiquetas[1];
        WSExpediente.Etiquetas arch = new WSExpediente.Etiquetas();
        arch.Etiqueta = 26;
        arch.Valor = _emplId;
        /*arch.Etiqueta = 24;
        arch.Valor = "207931";*/
        et[0] = arch;
        DataSet dsNew = new DataSet();
        dsNew = ser1.ObtenerArchivos("PR", 36, et);

        if (dsNew.Tables[0].Rows.Count > 0)
        {
            string boleta = "", people = "", fecha = "";
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


    [WebMethod]
    public string obtenerRTU(string _codpers)
    {
        string fileEnc = "";
        int filaFechaMaxima = 0, cont = 0;
        WSExpediente.Service ser1 = new WSExpediente.Service();
        WSExpediente.Etiquetas[] et = new WSExpediente.Etiquetas[1];
        WSExpediente.Etiquetas arch = new WSExpediente.Etiquetas();
        arch.Etiqueta = 9;
        arch.Valor = _codpers;
        et[0] = arch;
        DataSet dsNew = new DataSet();
        dsNew = ser1.ObtenerArchivos("DO", 29, et);

        if (dsNew.Tables[0].Rows.Count > 0)
        {
            string boleta = "", people = "", fecha = "";
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

    protected string encripta(decimal id)
    {
        EncryptedQueryString args = new EncryptedQueryString();
        args["id"] = id.ToString();
        string res = args.ToString();
        return res;
    }



    public bool validaExisteBoleta(string boleta, string persona)
    {
        WSExpediente.Service ser = new WSExpediente.Service();
        WSExpediente.Etiquetas[] et = new WSExpediente.Etiquetas[2];
        WSExpediente.Etiquetas arch = new WSExpediente.Etiquetas();
        arch.Etiqueta = 18;
        arch.Valor = boleta;
        et[0] = arch;
        arch = new WSExpediente.Etiquetas();
        arch.Etiqueta = 24;
        arch.Valor = persona;
        et[1] = arch;
        DataSet dsNew = new DataSet();
        dsNew = ser.ObtenerArchivos("PR", 35, et);

        if (dsNew.Tables[0].Rows.Count == 0)
            return true;
        else
            return false;
    }


    [WebMethod]
    public bool validaExisteBoletaTest(string boleta, string persona)
    {
        WSExpediente.Service ser = new WSExpediente.Service();
        WSExpediente.Etiquetas[] et = new WSExpediente.Etiquetas[2];
        WSExpediente.Etiquetas arch = new WSExpediente.Etiquetas();
        arch.Etiqueta = 18;
        arch.Valor = boleta;
        et[0] = arch;
        arch = new WSExpediente.Etiquetas();
        arch.Etiqueta = 24;
        arch.Valor = persona;
        et[1] = arch;
        DataSet dsNew = new DataSet();
        dsNew = ser.ObtenerArchivos("PR", 35, et);

        if (dsNew.Tables[0].Rows.Count == 0)
            return true;
        else
            return false;
    }


    public string enletras(string num)
    {
        string res, dec = "";
        Int64 entero;
        int decimales;
        double nro;
        try
        {
            nro = Convert.ToDouble(num);
        }
        catch
        {
            return "";
        }
        entero = Convert.ToInt64(Math.Truncate(nro));
        decimales = Convert.ToInt32(Math.Round((nro - entero) * 100, 2));
        //if (decimales > 0)
        //{
        dec = " CON " + decimales.ToString("00") + "/100";
        //}
        res = toText(Convert.ToDouble(entero)) + dec;
        return res;
    }

    private string toText(double value)
    {
        string Num2Text = "";

        value = Math.Truncate(value);

        if (value == 0) Num2Text = "CERO";
        else if (value == 1) Num2Text = "UNO";
        else if (value == 2) Num2Text = "DOS";
        else if (value == 3) Num2Text = "TRES";
        else if (value == 4) Num2Text = "CUATRO";
        else if (value == 5) Num2Text = "CINCO";
        else if (value == 6) Num2Text = "SEIS";
        else if (value == 7) Num2Text = "SIETE";
        else if (value == 8) Num2Text = "OCHO";
        else if (value == 9) Num2Text = "NUEVE";
        else if (value == 10) Num2Text = "DIEZ";
        else if (value == 11) Num2Text = "ONCE";
        else if (value == 12) Num2Text = "DOCE";
        else if (value == 13) Num2Text = "TRECE";
        else if (value == 14) Num2Text = "CATORCE";
        else if (value == 15) Num2Text = "QUINCE";
        else if (value < 20) Num2Text = "DIECI" + toText(value - 10);
        else if (value == 20) Num2Text = "VEINTE";
        else if (value < 30) Num2Text = "VEINTI" + toText(value - 20);
        else if (value == 30) Num2Text = "TREINTA";
        else if (value == 40) Num2Text = "CUARENTA";
        else if (value == 50) Num2Text = "CINCUENTA";
        else if (value == 60) Num2Text = "SESENTA";
        else if (value == 70) Num2Text = "SETENTA";
        else if (value == 80) Num2Text = "OCHENTA";
        else if (value == 90) Num2Text = "NOVENTA";
        else if (value < 100) Num2Text = toText(Math.Truncate(value / 10) * 10) + " Y " + toText(value % 10);
        else if (value == 100) Num2Text = "CIEN";
        else if (value < 200) Num2Text = "CIENTO " + toText(value - 100);
        else if ((value == 200) || (value == 300) || (value == 400) || (value == 600) || (value == 800)) Num2Text = toText(Math.Truncate(value / 100)) + "CIENTOS";
        else if (value == 500) Num2Text = "QUINIENTOS";
        else if (value == 700) Num2Text = "SETECIENTOS";
        else if (value == 900) Num2Text = "NOVECIENTOS";
        else if (value < 1000) Num2Text = toText(Math.Truncate(value / 100) * 100) + " " + toText(value % 100);
        else if (value == 1000) Num2Text = "MIL";
        else if (value < 2000) Num2Text = "MIL " + toText(value % 1000);
        else if (value < 1000000)
        {
            Num2Text = toText(Math.Truncate(value / 1000)) + " MIL";
            if ((value % 1000) > 0) Num2Text = Num2Text + " " + toText(value % 1000);
        }
        else if (value == 1000000) Num2Text = "UN MILLON";
        else if (value < 2000000) Num2Text = "UN MILLON " + toText(value % 1000000);
        else if (value < 1000000000000)
        {
            Num2Text = toText(Math.Truncate(value / 1000000)) + " MILLONES ";
            if ((value - Math.Truncate(value / 1000000) * 1000000) > 0) Num2Text = Num2Text + " " + toText(value - Math.Truncate(value / 1000000) * 1000000);
        }
        else if (value == 1000000000000) Num2Text = "UN BILLON";
        else if (value < 2000000000000) Num2Text = "UN BILLON " + toText(value - Math.Truncate(value / 1000000000000) * 1000000000000);
        else
        {
            Num2Text = toText(Math.Truncate(value / 1000000000000)) + " BILLONES";
            if ((value - Math.Truncate(value / 1000000000000) * 1000000000000) > 0) Num2Text = Num2Text + " " + toText(value - Math.Truncate(value / 1000000000000) * 1000000000000);
        }
        return Num2Text;
    }

}
