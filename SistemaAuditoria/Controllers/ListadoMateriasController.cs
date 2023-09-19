using SistemaAuditoria.Models;
using SistemaAuditoria.Models.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MySql.Data.MySqlClient;
using System.Data;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using System.Web.Security;
using System.Globalization;
using System.Drawing;
using OfficeOpenXml.Style;

namespace SistemaAuditoria.Controllers
{
    [Authorize]
    public class ListadoMateriasController : Controller
    {

        List<Juzgadosddl> jddlF = new List<Juzgadosddl>();
        public static List<Juzgadosddl> ListaRespaldoJuzgado = new List<Juzgadosddl>();
        public static List<ExpedientesIniciados> Exportei = new List<ExpedientesIniciados>();
        public static string fechaI, fechaF;

        // GET: ListadoMaterias
        public ActionResult Index(FormCollection collection)
        {
            try
            {
                ViewBag.message = "";
                ViewBag.Mensaje = "";
                ViewBag.fecIni = collection["fecIni"];
                ViewBag.fecFin = collection["fecFin"];
                ViewBag.juzddl = collection["juzddl"];
                string materia = collection["matddl"];
                string fechaInicio = collection["fecIni"];
                string fechaFinal = collection["fecFin"];
                string juzgado = collection["juzddl"];
                if (collection["matddl"] == null)
                    ViewBag.D = "1";
                else
                {
                    ViewBag.D = collection["matddl"];
                    ViewBag.Mensaje = ViewBag.D;
                    ViewBag.J = collection["juzddl"];
                }
                if (fechaInicio != null && fechaFinal != null)
                {
                    fechaI = DateTime.Parse(fechaInicio).ToString("d MMMM yyyy");
                    fechaF = DateTime.Parse(fechaFinal).ToString("d MMMM yyyy");
                }

                Juzgadosddl juzgadoSeleccionado = new Juzgadosddl();
                if (juzgado != null && int.Parse(juzgado) != 0)
                    juzgadoSeleccionado = ListaRespaldoJuzgado.Where(x => x.idJuzgado == int.Parse(juzgado)).FirstOrDefault();
                if (juzgadoSeleccionado == null)
                {
                    ViewBag.message = "Él elementó de selección de juzgado no se ha cargado completamente";
                    return View();
                }

                List<ExpedientesIniciados> ei = new List<ExpedientesIniciados>();
                List<ExpedientesIniciados> lt = new List<ExpedientesIniciados>();

                if (materia != null && fechaInicio != null && fechaFinal != null)
                {
                    if (materia != null && int.Parse(materia) == 1 && juzgado != "")
                    {
                        Exportei.Clear();
                        lt = GeneraSIGEJUPE_PEA(juzgadoSeleccionado, fechaInicio, fechaFinal, int.Parse(materia));
                        ei = ProcesaNombres(lt);
                        lt.Clear();
                        Exportei = ei;
                    }
                    else if (materia != null && int.Parse(materia) == 2 && juzgado != "")
                    {
                        Exportei.Clear();
                        lt = GeneraEXLAB(juzgadoSeleccionado, fechaInicio, fechaFinal, int.Parse(materia));
                        ei = ProcesaNombres(lt);
                        lt.Clear();
                        Exportei = ei;
                    }
                    else if (materia != null && int.Parse(materia) == 3 && juzgado != "")
                    {
                        Exportei.Clear();
                        lt = GeneraSIGEJUPE_PEA(juzgadoSeleccionado, fechaInicio, fechaFinal, int.Parse(materia));
                        ei = ProcesaNombres(lt);
                        lt.Clear();
                        Exportei = ei;
                    }
                    else if (materia != null && int.Parse(materia) == 4 && juzgado != "")
                    {
                        Exportei.Clear();
                        lt = GeneraJuzgadoLaboral(juzgadoSeleccionado, fechaInicio, fechaFinal);
                        ei = ProcesaNombres(lt);
                        lt.Clear();
                        Exportei = ei;
                    }
                    else if (materia != null && int.Parse(materia) == 5 && juzgado != "")
                    {
                        Exportei.Clear();
                        lt = GeneraEXLAB(juzgadoSeleccionado, fechaInicio, fechaFinal, int.Parse(materia));
                        ei = ProcesaNombres(lt);
                        lt.Clear();
                        Exportei = ei;
                    }

                    return View(ei);
                }
                else
                {
                    //Mensaje de error

                }
                return View();
            }
            catch (Exception ex)
            {
                throw new Exception("Error en: " + ex.Message);
            }

        }
        [HttpGet]
        public JsonResult GetJuzgadosList(int idJuz)
        {
            MySqlConnection conddlJuzgados = new MySqlConnection();
            string sql = "";
            //conddlJuzgados = ConexionesBDs.ObtenerConexion(2);
            ListaRespaldoJuzgado.Clear();
            conddlJuzgados = new MySqlConnection(System.Configuration.ConfigurationManager.AppSettings[idJuz.ToString()]);
            conddlJuzgados.Open();
            if (idJuz == 1 || idJuz == 3)
                sql = "SELECT distinct j.cveJuzgado,j.desJuzgado "
                + " FROM tbljuzgados AS j"
                + " LEFT JOIN tblcarpetasjudiciales AS cj ON j.cveJuzgado = cj.cveJuzgado"
                + " WHERE cj.cveTipoCarpeta in (2, 3, 4)AND cj.activo = 'S' AND j.activo = 'S' and j.desJuzgado NOT like '%FICTICIO%' and j.desJuzgado NOT like '%CODIGO ANTERIOR%'; ";
            else if (idJuz == 2)
                sql = "SELECT cveJuzgado,desJuzgado FROM htsj_laboral.tbljuzgados"
                + " where activo = 'S' and desJuzgado NOT like '%FICTICIO%'; ";

            using (conddlJuzgados)
            {
                MySqlCommand _comando = new MySqlCommand(sql, conddlJuzgados);

                MySqlDataReader _reader = _comando.ExecuteReader();
                while (_reader.Read())
                {
                    Juzgadosddl jddl = new Juzgadosddl();
                    jddl.idJuzgado = _reader.GetInt32(0);
                    jddl.nombreJuzgado = _reader.GetString(1);

                    jddlF.Add(jddl);
                    ListaRespaldoJuzgado.Add(jddl);
                }
                return Json(jddlF, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult ExportExcel()
        {
            try
            {
                List<ExpedientesIniciados> Radicados = new List<ExpedientesIniciados>();
                List<ExpedientesIniciados> Terminados = new List<ExpedientesIniciados>();

                Radicados = Exportei.Where(x => x.isRadicado == true).ToList();
                Terminados = Exportei.Where(x => x.isRadicado == false).ToList();

                string filename = System.IO.Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/Content/tmpFiles/"), @"prueba" + DateTime.Now.Ticks.ToString() + ".xlsx");
                var file = new FileInfo(filename);
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage(file))
                {
                    double rowHeight = 30;


                    var sheet = package.Workbook.Worksheets.Add("Expedientes");
                    sheet.Cells[1, 1].Value = string.Format("FECHA: {0} A {1}", DateTime.Parse(fechaI).ToString("d MMMM yyyy").ToUpper(), DateTime.Parse(fechaF).ToString("d MMMM yyyy").ToUpper());


                    sheet.Cells[2, 1].Value = "Total de radicados: " + Radicados.GroupBy(x => x.Expediente).Count();
                    sheet.Cells[3, 1].Value = "#";
                    sheet.Cells[3, 2].Value = "Fecha Radicado";
                    sheet.Cells[3, 3].Value = "Número Expediente";
                    sheet.Cells[3, 4].Value = "Juzgado";
                    sheet.Cells[3, 5].Value = "Tipo De Delito";

                    sheet.Cells[2, 7].Value = "Total de terminados: " + Terminados.GroupBy(x => x.Expediente).Count();
                    sheet.Cells[3, 7].Value = "#";
                    sheet.Cells[3, 8].Value = "Número Expediente";
                    sheet.Cells[3, 9].Value = "Juzgado";
                    sheet.Cells[3, 10].Value = "Fecha Termino";
                    if (ViewBag.D == "2")
                        sheet.Cells[3, 11].Value = "Sentenciado";
                    else
                        sheet.Cells[3, 11].Value = "Conclusión";
                    sheet.Cells[3, 12].Value = "Tipo De Conclusión";

                    sheet.Cells["A2:E2"].Merge = true;
                    sheet.Cells["G2:L2"].Merge = true;

                    sheet.Cells["A1:E3"].Style.Font.Bold = true;
                    sheet.Cells["G2:L3"].Style.Font.Bold = true;

                    sheet.Cells["A2:E2"].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Double;
                    sheet.Cells["A2:E2"].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Double;
                    sheet.Cells["A2:E2"].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Double;
                    sheet.Cells["A2:E2"].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Double;

                    sheet.Cells["G2:L3"].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Double;
                    sheet.Cells["G2:L3"].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Double;
                    sheet.Cells["G2:L3"].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Double;
                    sheet.Cells["G2:L3"].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Double;

                    sheet.Cells["A3:E3"].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Double;
                    sheet.Cells["A3:E3"].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Double;
                    sheet.Cells["A3:E3"].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Double;
                    sheet.Cells["A3:E3"].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Double;

                    sheet.Cells["G3:L3"].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Double;
                    sheet.Cells["G3:L3"].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Double;
                    sheet.Cells["G3:L3"].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Double;
                    sheet.Cells["G3:L3"].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Double;

                    sheet.Cells["A2:E3"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet.Cells["A2:E3"].Style.Fill.BackgroundColor.SetColor(Color.Red);
                    sheet.Cells["G2:L3"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet.Cells["G2:L3"].Style.Fill.BackgroundColor.SetColor(Color.Red);

                    sheet.Cells["A2:E3"].Style.Font.Color.SetColor(Color.White);
                    sheet.Cells["G2:L3"].Style.Font.Color.SetColor(Color.White);

                    sheet.Column(5).Style.WrapText = true;
                    sheet.Column(12).Style.WrapText = true;

                    int rowIterator = 3, cont = 1;

                    string fechaRadicacion = "", fechaTermino = "";

                    sheet.Column(5).Width = 13;
                    sheet.Column(12).Width = 13;

                    foreach (ExpedientesIniciados item in Radicados)
                    {
                        rowIterator++;
                        sheet.Row(rowIterator).Height = rowHeight;
                        fechaRadicacion = item.fechaRadicacion.ToString();
                        sheet.Cells[rowIterator, 1].Value = cont;
                        sheet.Cells[rowIterator, 2].Value = fechaRadicacion;
                        sheet.Cells[rowIterator, 3].Value = item.Expediente;
                        sheet.Cells[rowIterator, 4].Value = item.Juzgado;
                        sheet.Cells[rowIterator, 5].Value = item.TipoDelito;

                        cont++;
                    }
                    cont = 1;
                    int rowIteratorT = 3;
                    foreach (ExpedientesIniciados item in Terminados)
                    {
                        rowIteratorT++;
                        sheet.Row(rowIterator).Height = rowHeight;
                        fechaTermino = item.fechaTermino.ToString();
                        sheet.Cells[rowIteratorT, 7].Value = cont;
                        sheet.Cells[rowIteratorT, 8].Value = item.Expediente;
                        sheet.Cells[rowIteratorT, 9].Value = item.Juzgado;
                        sheet.Cells[rowIteratorT, 10].Value = fechaTermino;
                        sheet.Cells[rowIteratorT, 11].Value = item.Nombre;
                        sheet.Cells[rowIteratorT, 12].Value = item.TipoDelito;

                        cont++;
                    }

                    sheet.Column(2).AutoFit();
                    sheet.Column(3).AutoFit();
                    sheet.Column(4).AutoFit();
                    sheet.Column(7).AutoFit();
                    sheet.Column(8).AutoFit();
                    sheet.Column(9).AutoFit();
                    sheet.Column(10).AutoFit();
                    sheet.Column(11).AutoFit();

                    package.Save();

                    System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                    System.IO.MemoryStream ms = new System.IO.MemoryStream();
                    fs.CopyTo(ms);
                    byte[] byteStream = ms.ToArray();
                    ms = new MemoryStream();
                    ms.Write(byteStream, 0, byteStream.Length);
                    ms.Position = 0;
                    fs.Close();
                    file.Delete();
                    return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                }

            }
            catch (Exception ex)
            {
                return View();
            }


        }

        [HttpPost]
        public ActionResult Lista(FormCollection collection)
        {

            return View();
        }
        // GET: ListadoMaterias/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ListadoMaterias/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ListadoMaterias/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: ListadoMaterias/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ListadoMaterias/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: ListadoMaterias/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ListadoMaterias/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        enum materiaE
        {
            PENAL = 1,
            CIVIL = 2,
            MERCANTIL = 3,
            LABORAL = 4,
            FAMILIAR = 5
        }
        public List<ExpedientesIniciados> GeneraSIGEJUPE_PEA(Juzgadosddl juz, string fechaInicio, string fechaFinal, int tipoBD)
        {
            try
            {
                List<ExpedientesIniciados> LisRet = new List<ExpedientesIniciados>();
                if (juz.idJuzgado != null && juz.idJuzgado != 0)
                {
                    MySqlConnection con = new MySqlConnection();
                    con = new MySqlConnection(System.Configuration.ConfigurationManager.AppSettings[tipoBD.ToString()]);
                    con.Open();
                    using (con)
                    {
                        if (juz.idJuzgado != 0)
                        {
                            ViewBag.message = "";
                            string sql = "", sql2 = "";
                            sql = string.Format("select true as 'isRadicado',cj.fechaRadicacion as 'fechaRadicacion',concat(lpad(cj.numero,5,'0'),'/',cj.anio) 'EXPEDIENTE',j.desJuzgado as 'juzgado', d.desDelito as 'DELITOS'"
                            + " from tblcarpetasjudiciales as cj"
                            + " left join tbljuzgados as j on cj.cveJuzgado = j.cveJuzgado"
                            + " INNER JOIN tbltiposcarpetas tc on cj.cveTipoCarpeta = tc.cveTipoCarpeta"
                            + " LEFT JOIN tbldelitoscarpetas AS dc ON dc.idCarpetaJudicial = cj.idCarpetaJudicial"
                            + " LEFT JOIN tbldelitos AS d ON d.cveDelito = dc.cveDelito"
                            + " where cj.fecharadicacion between '{0} 00:00:00' and '{1} 23:59:59'"
                            + " and cj.activo = 'S' and cj.cvetipocarpeta in (2, 3, 4) and j.cveJuzgado in ({2});", fechaInicio, fechaFinal, juz.idJuzgado);
                            MySqlCommand _comando = new MySqlCommand(sql, con);
                            MySqlDataReader _reader = _comando.ExecuteReader();
                            while (_reader.Read())
                            {
                                ExpedientesIniciados exSoli = new ExpedientesIniciados();
                                exSoli.isRadicado = true;
                                exSoli.fechaRadicacion = _reader.GetDateTime(1);
                                exSoli.Expediente = _reader.GetString(2);
                                exSoli.Juzgado = juz.nombreJuzgado;
                                exSoli.TipoDelito = _reader.GetString(4);

                                LisRet.Add(exSoli);
                            }
                            _reader.Close();
                            if (tipoBD == 1)
                            {
                                sql2 = string.Format("SELECT false as 'isRadicado',cjt.fechaTermino as 'fechaTermino',concat(lpad(cj.numero, 5, '0'), '/', cj.anio) 'EXPEDIENTE',"
                                + " j.desJuzgado as 'juzgado',d.desDelito as 'DELITOS',concat(ic.nombre, ' ', ic.paterno, ' ', ic.materno) as 'Nomimputado',c.desConclusion as 'conclusion'"
                                + " FROM htsj_sigejupe.tblcarpetasjudiciales cj LEFT JOIN tbldelitoscarpetas AS dc ON dc.idCarpetaJudicial = cj.idCarpetaJudicial LEFT JOIN tbldelitos AS d ON d.cveDelito = dc.cveDelito"
                                + " INNER JOIN htsj_sigejupe.tbljuzgados j  ON cj.cveJuzgado = j.cveJuzgado INNER JOIN htsj_sigejupe.tbltiposcarpetas tc ON cj.cveTipoCarpeta = tc.cveTipoCarpeta"
                                + " INNER JOIN htsj_sigejupe.tblcarpetasjudicialesterminadas cjt ON cj.idCarpetaJudicial = cjt.idCarpetaJudicial INNER JOIN htsj_sigejupe.tblimpofedelcarpetas ioc ON ioc.idCarpetaJudicial = cj.idCarpetaJudicial"
                                + " INNER JOIN htsj_sigejupe.tblimputadoscarpetas ic ON ic.idImputadoCarpeta = ioc.idImputadoCarpeta INNER JOIN htsj_sigejupe.tblimputadoscarpetasconclusiones icc ON icc.idImputadoCarpeta = ic.idImputadoCarpeta"
                                + " INNER JOIN htsj_sigejupe.tblconclusiones as c on icc.cveConclusion = c.cveConclusion WHERE cjt.fechaTermino between '{0} 00:00:00' and '{1} 23:59:59' and cj.cveTipoCarpeta in (2, 3, 4)"
                                + " and j.cveJuzgado in ({2}) and cj.activo = 'S' group by ic.idImputadoCarpeta, c.desConclusion; ", fechaInicio, fechaFinal, juz.idJuzgado);
                            }
                            else if (tipoBD == 3)
                            {
                                sql2 = string.Format("SELECT FALSE AS 'isRadicado',cj.fechaTermino AS 'fechaTermino',CONCAT(LPAD(cj.numero, 5, '0'), '/', cj.anio) 'EXPEDIENTE',j.desJuzgado AS 'juzgado',c.desConclusion AS 'conclusion',"
                                + " CONCAT(ic.nombre,' ',ic.paterno,' ',ic.materno) AS 'Nomimputado',d.desDelito AS 'DELITOS',c.desConclusion AS 'conclusion'"
                                + " FROM tblcarpetasjudiciales AS cj LEFT JOIN tbljuzgados AS j ON cj.cveJuzgado = j.cveJuzgado LEFT JOIN tbldelitoscarpetas AS dc ON dc.idCarpetaJudicial = cj.idCarpetaJudicial"
                                + " LEFT JOIN tbldelitos AS d ON d.cveDelito = dc.cveDelito INNER JOIN tblestatus AS e ON cj.cveEstatusCarpeta = e.cveEstatus INNER JOIN tblimpofedelcarpetas ioc ON ioc.idCarpetaJudicial = cj.idCarpetaJudicial"
                                + " INNER JOIN tblimputadoscarpetas ic ON ic.idImputadoCarpeta = ioc.idImputadoCarpeta INNER JOIN tblimputadoscarpetasconclusiones icc ON icc.idImputadoCarpeta = ic.idImputadoCarpeta INNER JOIN"
                                + " tblconclusiones AS c ON icc.cveConclusion = c.cveConclusion WHERE cj.fechaTermino BETWEEN '{0}' AND '{1}' AND cj.activo = 'S' AND cj.cvetipocarpeta IN(2, 3, 4)"
                                + " AND cj.cveEstatusCarpeta = 2 AND cj.cveJuzgado IN({2}); ", fechaInicio, fechaFinal, juz.idJuzgado);
                            }
                            MySqlCommand _comandoT = new MySqlCommand(sql2, con);
                            MySqlDataReader _readerT = _comandoT.ExecuteReader();
                            while (_readerT.Read())
                            {
                                ExpedientesIniciados exSoliT = new ExpedientesIniciados();
                                exSoliT.isRadicado = false;
                                exSoliT.fechaTermino = _readerT.GetDateTime(1);
                                exSoliT.Expediente = _readerT.GetString(2);
                                if (tipoBD == 3)
                                    exSoliT.TipoDelito = _readerT.GetString(7);
                                else if (tipoBD == 1)
                                    exSoliT.TipoDelito = _readerT.GetString(6);

                                exSoliT.Nombre = _readerT.GetString(5);
                                exSoliT.Juzgado = juz.nombreJuzgado;

                                LisRet.Add(exSoliT);
                            }
                            _readerT.Close();
                        }
                    }
                }
                else
                {
                    ViewBag.message = "Él elementó de selección de juzgado no se ha cargado completamente";
                }
                return LisRet;
            }
            catch (Exception ie)
            {
                throw new Exception("Error" + ie.Message);
            }
        }
        public List<ExpedientesIniciados> GeneraEXLAB(Juzgadosddl juz, string fechaInicio, string fechaFinal, int tipoMateria)
        {
            try
            {
                List<ExpedientesIniciados> LisRet = new List<ExpedientesIniciados>();
                if (juz.idJuzgado != 0)
                {
                    MySqlConnection conSEJ = new MySqlConnection();
                    //conSEJ = ConexionesBDs.ObtenerConexion(3);

                    conSEJ = new MySqlConnection(System.Configuration.ConfigurationManager.AppSettings[tipoMateria.ToString()]);
                    conSEJ.Open();
                    string sqlSEJ = "", sqlSEJT = "";
                    sqlSEJ = string.Format("SELECT TRUE AS 'isRadicado',c.fechaRadicacion AS 'fechaRadicacion',CONCAT(LPAD(c.numero, 5, '0'), '/', c.anio) 'EXPEDIENTE',j.desJuzgado AS 'juzgado',"
                    + " ju.desJuicio AS 'TipoDelito' FROM tblcarpetasjudiciales c INNER JOIN tbljuzgados AS j ON c.cvejuzgado = j.cvejuzgado LEFT JOIN tbljuicios AS ju ON ju.cveJuicio = c.cveJuicio"
                    + " WHERE c.fecharadicacion BETWEEN '{0} 00:00:00' AND '{1} 23:59:59' AND c.activo = 'S' AND c.cvetipocarpeta = 1 AND c.cvejuzgado IN({2}); ", fechaInicio, fechaFinal, juz.idJuzgado);
                    MySqlCommand _comandoSEJ = new MySqlCommand(sqlSEJ, conSEJ);
                    MySqlDataReader _readerSEJ = _comandoSEJ.ExecuteReader();
                    while (_readerSEJ.Read())
                    {
                        ExpedientesIniciados exSoliSEJ = new ExpedientesIniciados();
                        exSoliSEJ.isRadicado = true;
                        exSoliSEJ.fechaRadicacion = _readerSEJ.GetDateTime(1);
                        exSoliSEJ.Expediente = _readerSEJ.GetString(2);
                        exSoliSEJ.Juzgado = juz.nombreJuzgado;
                        exSoliSEJ.TipoDelito = _readerSEJ.GetString(4);


                        LisRet.Add(exSoliSEJ);
                    }
                    _readerSEJ.Close();
                    sqlSEJT = string.Format("SELECT FALSE AS 'isRadicado',cj.fechaTerminacion AS 'fechaRadicacion',CONCAT(LPAD(cj.numero, 5, '0'), '/', cj.anio) 'EXPEDIENTE',jz.desJuzgado AS 'juzgado',"
                    + " CASE WHEN ((p.nombre IS NOT NULL AND LENGTH(p.nombre) > 0) OR(p.paterno IS NOT NULL AND LENGTH(p.paterno) > 0) OR(p.materno IS NOT NULL AND LENGTH(p.materno) > 0)) THEN CONCAT(p.nombre, ' ', p.paterno, ' ', p.materno)"
                    + " WHEN (p.nombreBusqueda IS NOT NULL AND LENGTH(p.nombreBusqueda) > 0) THEN p.nombreBusqueda WHEN (p.razonSocial IS NOT NULL AND LENGTH(p.razonSocial) > 0) THEN p.razonSocial END AS 'Persona', tt.descTipoterminacion AS 'Conclusion'"
                    + " FROM htsj_laboral.tblcarpetasjudiciales cj LEFT JOIN htsj_laboral.tbljuzgados AS jz ON cj.cveJuzgado = jz.cveJuzgado LEFT JOIN htsj_laboral.tblpartescarpetas AS pc ON pc.idCarpetaJudicial = cj.idCarpetaJudicial"
                    + " LEFT JOIN htsj_laboral.tblpartes AS p ON p.idParte = pc.idParte INNER JOIN tblhistoricocarpetas AS hc ON hc.idCarpetaJudicial = cj.idCarpetaJudicial LEFT JOIN htsj_laboral.tbltiposterminaciones AS tt ON tt.cveTipoTerminacion = hc.cveTipoTerminacion"
                    + " WHERE cj.fechaTerminacion BETWEEN '{0} 00:00:00' AND '{1} 23:59:59' AND cj.cvetipocarpeta = 1 AND cj.cveEstatusCarpetasJudiciales IN(2,10) AND jz.cveJuzgado IN({2}) AND cj.activo = 'S'AND jz.activo = 'S'"
                    + " AND hc.cveEstatusCarpetasJudiciales = 10; ", fechaInicio, fechaFinal, juz.idJuzgado);
                    MySqlCommand _comandoSEJ2 = new MySqlCommand(sqlSEJT, conSEJ);
                    MySqlDataReader _readerSEJ2 = _comandoSEJ2.ExecuteReader();
                    while (_readerSEJ2.Read())
                    {
                        ExpedientesIniciados exSoliSEJ2 = new ExpedientesIniciados();
                        exSoliSEJ2.isRadicado = false;
                        exSoliSEJ2.fechaTermino = _readerSEJ2.GetDateTime(1);
                        exSoliSEJ2.Expediente = _readerSEJ2.GetString(2);
                        exSoliSEJ2.Juzgado = juz.nombreJuzgado;
                        exSoliSEJ2.Nombre = _readerSEJ2.GetString(4);
                        exSoliSEJ2.TipoDelito = _readerSEJ2.GetString(5);

                        LisRet.Add(exSoliSEJ2);
                    }
                    _readerSEJ2.Close();
                }

                return LisRet;
            }
            catch (Exception ie)
            {
                throw new Exception("Error" + ie.Message);
            }

        }
        public List<ExpedientesIniciados> GeneraJuzgadoLaboral(Juzgadosddl juz, string fechaInicio, string fechaFinal)
        {
            try
            {
                List<ExpedientesIniciados> LisRet = new List<ExpedientesIniciados>();
                var tipos = new Int32[] { 2, 3, 4 };
                if (juz.idJuzgado != 0)
                {
                    MySqlConnection con = new MySqlConnection();
                    //con = ConexionesBDs.ObtenerConexion(4);

                    con = new MySqlConnection(System.Configuration.ConfigurationManager.AppSettings["4"]);
                    con.Open();
                    using (con)
                    {
                        if (juz.idJuzgado != 0)
                        {
                            string sql = "", sql2 = "";
                            sql = string.Format("select true as 'isRadicado', concat(lpad(c.numero, 5, '0'), '/', c.anio) 'EXPEDIENTE', j.desJuzgado as 'juzgado', c.fechaRadicacion as 'fechaRadicacion'"
                            + " from tblcarpetasjudiciales c"
                            + " inner join tbljuzgados j on c.cvejuzgado = j.cvejuzgado"
                            + " where c.fecharadicacion between '{0}' and '{1}'"
                            + " and c.activo = 'S' and c.cvetipocarpeta = 1 and c.cvejuzgado in ({2});", fechaInicio, fechaFinal, juz.idJuzgado);
                            MySqlCommand _comando = new MySqlCommand(sql, con);
                            MySqlDataReader _reader = _comando.ExecuteReader();
                            while (_reader.Read())
                            {
                                ExpedientesIniciados exSoli = new ExpedientesIniciados();
                                exSoli.isRadicado = true;
                                exSoli.Expediente = _reader.GetString(1);
                                exSoli.Materia = ObtienTipoMat(5);
                                exSoli.Juzgado = juz.nombreJuzgado;
                                exSoli.fechaRadicacion = _reader.GetDateTime(3);

                                LisRet.Add(exSoli);
                            }
                            _reader.Close();
                            sql2 = string.Format("SELECT FALSE as 'isRadicado',concat(lpad(cj.numero,5,'0'),'/',cj.anio) 'EXPEDIENTE',jz.desJuzgado as 'juzgado' ,cj.fechaTerminacion as 'fechaTerminacion' "
                            + " from htsj_laboral.tblcarpetasjudiciales cj left join htsj_laboral.tbljuzgados as jz on cj.cveJuzgado = jz.cveJuzgado"
                            + " where cj.fechaTerminacion between '{0}' and '{1}' and cj.cvetipocarpeta = 1 and cj.cveEstatusCarpetasJudiciales in (2, 10)"
                            + " AND jz.cveJuzgado in ({2}) AND cj.activo = 'S' AND jz.activo = 'S';", fechaInicio, fechaFinal, juz.idJuzgado);
                            MySqlCommand _comandoT = new MySqlCommand(sql2, con);
                            MySqlDataReader _readerT = _comandoT.ExecuteReader();
                            while (_readerT.Read())
                            {
                                ExpedientesIniciados exSoliT = new ExpedientesIniciados();
                                exSoliT.isRadicado = false;
                                exSoliT.Expediente = _readerT.GetString(1);
                                exSoliT.Materia = ObtienTipoMat(5);
                                exSoliT.Juzgado = juz.nombreJuzgado;
                                exSoliT.fechaTermino = _readerT.GetDateTime(3);

                                LisRet.Add(exSoliT);
                            }
                            _readerT.Close();
                        }
                    }
                }
                return LisRet;
            }
            catch (Exception ie)
            {
                throw new Exception("Error" + ie.Message);
            }

        }
        public string ObtienTipoPenal(Juzgadosddl juz)
        {
            try
            {
                if (juz.idJuzgado != 0)
                    return "PENAL TRADICIONAL";
                else
                    return "PENAL";
            }
            catch (Exception ex)
            {
                throw new Exception("Error en: " + ex.Message);
            }
        }
        public string ObtienTipoMat(int Mat)
        {
            try
            {
                if (Mat == 2)
                    return "CIVIL";
                else if (Mat == 3)
                    return "MERCANTIL";
                else if (Mat == 3)
                    return "LABORAL";
                else if (Mat == 5)
                    return "FAMILIAR";
                return "";
            }
            catch (Exception ex)
            {
                throw new Exception("Error en: " + ex.Message);
            }
        }
        public List<ExpedientesIniciados> ProcesaNombres(List<ExpedientesIniciados> ProNombres)
        {
            try
            {
                List<ExpedientesIniciados> LisRetNombresProcesados = new List<ExpedientesIniciados>();
                foreach (var sen in ProNombres)
                {
                    if (sen.Nombre != null && !sen.Nombre.Contains("Conclusión"))
                    {
                        int count = 1;
                        if (LisRetNombresProcesados.Where(x => x.Expediente == sen.Expediente).Count() == 0)
                        {
                            var multi = ProNombres.Where(x => x.Expediente == sen.Expediente).ToList();
                            if (multi.Count() == 1)
                            {
                                sen.Nombre = "Conclusión 1";
                                LisRetNombresProcesados.Add(sen);
                            }
                            else if (multi.Count() > 1)
                            {
                                foreach (var item in multi)
                                {
                                    item.Nombre = "Conclusión " + count;
                                    LisRetNombresProcesados.Add(item);
                                    count++;
                                }
                            }
                        }
                    }
                    else if (sen.Nombre == null)
                    {
                        LisRetNombresProcesados.Add(sen);
                    }
                }
                return LisRetNombresProcesados;
            }
            catch (Exception)
            {
                throw;
            }

        }
        public ActionResult CerrarSesion()
        {
            FormsAuthentication.SignOut();
            Session["Usuario"] = null;
            return RedirectToAction("Index", "Login");
        }
    }
}