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

namespace SistemaAuditoria.Controllers
{
    [Authorize]
    public class ListadoMateriasController : Controller
    {

        List<Juzgadosddl> jddlF = new List<Juzgadosddl>();
        public static List<Juzgadosddl> ListaRespaldoJuzgado = new List<Juzgadosddl>();
        public static List<ExpedientesIniciados> Exportei = new List<ExpedientesIniciados>();

        // GET: ListadoMaterias
        public ActionResult Index(FormCollection collection)
        {
            try
            {
                ViewBag.fecIni = collection["fecIni"];
                ViewBag.fecFin = collection["fecFin"];
                if (collection["matddl"] == null)
                    ViewBag.D = 1;
                else
                    ViewBag.D = collection["matddl"];
                //ViewBag.juzddl = collection["juzddl"];
                string materia = collection["matddl"];
                string fechaInicio = collection["fecIni"];
                string fechaFinal = collection["fecFin"];
                string juzgado = collection["juzddl"];

                Juzgadosddl juzgadoSeleccionado = new Juzgadosddl();
                if (juzgado != null && int.Parse(juzgado) != 0)
                    juzgadoSeleccionado = ListaRespaldoJuzgado.Where(x => x.idJuzgado == int.Parse(juzgado)).FirstOrDefault();

                List<ExpedientesIniciados> ei = new List<ExpedientesIniciados>();

                if (materia != null && fechaInicio != null && fechaFinal != null)
                {
                    if (materia != null && int.Parse(materia) == 1 && juzgado != "")
                    {
                        Exportei.Clear();
                        ei = GeneraSIGEJUPE_PEA(juzgadoSeleccionado, fechaInicio, fechaFinal, int.Parse(materia));
                        Exportei = ei;
                    }
                    else if (materia != null && int.Parse(materia) == 2 && juzgado != "")
                    {
                        Exportei.Clear();
                        ei = GeneraEXLAB(juzgadoSeleccionado, fechaInicio, fechaFinal, int.Parse(materia));
                        Exportei = ei;
                    }
                    else if (materia != null && int.Parse(materia) == 3 && juzgado != "")
                    {
                        Exportei.Clear();
                        ei = GeneraSIGEJUPE_PEA(juzgadoSeleccionado, fechaInicio, fechaFinal, int.Parse(materia));
                        Exportei = ei;
                    }
                    else if (materia != null && int.Parse(materia) == 4 && juzgado != "")
                    {
                        Exportei.Clear();
                        ei = GeneraJuzgadoLaboral(juzgadoSeleccionado, fechaInicio, fechaFinal);
                        Exportei = ei;
                    }
                    else if (materia != null && int.Parse(materia) == 5 && juzgado != "")
                    {
                        Exportei.Clear();
                        ei = GeneraEXLAB(juzgadoSeleccionado, fechaInicio, fechaFinal, int.Parse(materia));
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
                sql = "SELECT cveJuzgado,desJuzgado FROM htsj_sigejupe.tbljuzgados"
                + " where activo = 'S' and desJuzgado NOT like '%FICTICIO%';";
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
                //string filename = @"prueba" + DateTime.Now.Ticks.ToString() + ".xlsx";
                var file = new FileInfo(filename);
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage(file))
                {

                    var sheet = package.Workbook.Worksheets.Add("Expedientes");
                    sheet.Cells[1, 1].Value = "Radicados";
                    sheet.Cells[2, 1].Value = "Fecha Radicado";
                    sheet.Cells[2, 2].Value = "Numero Expediente";
                    sheet.Cells[2, 3].Value = "Juzgado";

                    sheet.Cells[1, 6].Value = "Termiandos";
                    sheet.Cells[2, 6].Value = "Fecha Termino";
                    sheet.Cells[2, 7].Value = "Numero Expediente";
                    sheet.Cells[2, 8].Value = "Juzgado";

                    sheet.Cells["A1:C1"].Merge = true;
                    sheet.Cells["F1:H1"].Merge = true;

                    sheet.Cells["A1:C2"].Style.Font.Bold = true;
                    sheet.Cells["F1:H2"].Style.Font.Bold = true;
                    int rowIterator = 2;

                    string fechaRadicacion = "", fechaTermino = "";


                    foreach (ExpedientesIniciados item in Radicados)
                    {
                        rowIterator++;
                        fechaRadicacion = item.fechaRadicacion.ToString();
                        sheet.Cells[rowIterator, 1].Value = fechaRadicacion;
                        sheet.Cells[rowIterator, 2].Value = item.Expediente;
                        sheet.Cells[rowIterator, 3].Value = item.Juzgado;


                    }
                    int rowIteratorT = 2;
                    foreach (ExpedientesIniciados item in Terminados)
                    {
                        rowIteratorT++;
                        fechaTermino = item.fechaTermino.ToString();
                        sheet.Cells[rowIteratorT, 6].Value = fechaTermino;
                        sheet.Cells[rowIteratorT, 7].Value = item.Expediente;
                        sheet.Cells[rowIteratorT, 8].Value = item.Juzgado;


                    }

                    sheet.Column(1).AutoFit();
                    sheet.Column(2).AutoFit();
                    sheet.Column(3).AutoFit();
                    sheet.Column(4).AutoFit();
                    sheet.Column(7).AutoFit();
                    sheet.Column(8).AutoFit();

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
                if (juz.idJuzgado != 0)
                {
                    MySqlConnection con = new MySqlConnection();
                    con = new MySqlConnection(System.Configuration.ConfigurationManager.AppSettings[tipoBD.ToString()]);
                    con.Open();
                    using (con)
                    {
                        if (juz.idJuzgado != 0)
                        {
                            string sql = "", sql2 = "";
                            sql = string.Format("select true as 'isRadicado',cj.fechaRadicacion as 'fechaRadicacion',concat(lpad(cj.numero,5,'0'),'/',cj.anio) 'EXPEDIENTE',j.desJuzgado as 'juzgado'"
                            + " from tblcarpetasjudiciales as cj"
                            + " left join tbljuzgados as j on cj.cveJuzgado = j.cveJuzgado"
                            + " where cj.fecharadicacion between '{0} 00:00:00' and '{1} 23:59:59'"
                            + " and cj.activo = 'S' and cj.cvetipocarpeta in (2, 3, 4) and cj.cveJuzgado in ({2});", fechaInicio, fechaFinal, juz.idJuzgado);
                            MySqlCommand _comando = new MySqlCommand(sql, con);
                            MySqlDataReader _reader = _comando.ExecuteReader();
                            while (_reader.Read())
                            {
                                ExpedientesIniciados exSoli = new ExpedientesIniciados();
                                exSoli.isRadicado = true;
                                exSoli.fechaRadicacion = _reader.GetDateTime(1);
                                exSoli.Expediente = _reader.GetString(2);
                                exSoli.Juzgado = juz.nombreJuzgado;

                                LisRet.Add(exSoli);
                            }
                            _reader.Close();
                            sql2 = string.Format("select false as 'isRadicado',cj.fechaTermino as 'fechaTermino',concat(lpad(cj.numero,5,'0'),'/',cj.anio) 'EXPEDIENTE',j.desJuzgado as 'juzgado'"
                            + " from tblcarpetasjudiciales as cj"
                            + " left join tbljuzgados as j on cj.cveJuzgado = j.cveJuzgado"
                            + " where cj.fechaTermino between '{0} 00:00:00' and '{1} 23:59:59'"
                            + " and cj.activo = 'S' and cj.cvetipocarpeta in (2, 3, 4) and cj.cveEstatusCarpeta = 2 and cj.cveJuzgado in ({2}); ", fechaInicio, fechaFinal, juz.idJuzgado);
                            MySqlCommand _comandoT = new MySqlCommand(sql2, con);
                            MySqlDataReader _readerT = _comandoT.ExecuteReader();
                            while (_readerT.Read())
                            {
                                ExpedientesIniciados exSoliT = new ExpedientesIniciados();
                                exSoliT.isRadicado = false;
                                exSoliT.fechaTermino = _readerT.GetDateTime(1);
                                exSoliT.Expediente = _readerT.GetString(2);
                                exSoliT.Juzgado = juz.nombreJuzgado;

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
                    sqlSEJ = string.Format("select true as 'isRadicado',c.fechaRadicacion as 'fechaRadicacion',concat(lpad(c.numero,5,'0'),'/',c.anio) 'EXPEDIENTE',j.desJuzgado as 'juzgado'"
                            + " from tblcarpetasjudiciales c inner join tbljuzgados j on c.cvejuzgado = j.cvejuzgado"
                            + " where c.fecharadicacion between '{0} 00:00:00' and '{1} 23:59:59'"
                            + " and c.activo = 'S' and c.cvetipocarpeta = 1 and c.cvejuzgado in ({2}); ", fechaInicio, fechaFinal, juz.idJuzgado);
                    MySqlCommand _comandoSEJ = new MySqlCommand(sqlSEJ, conSEJ);
                    MySqlDataReader _readerSEJ = _comandoSEJ.ExecuteReader();
                    while (_readerSEJ.Read())
                    {
                        ExpedientesIniciados exSoliSEJ = new ExpedientesIniciados();
                        exSoliSEJ.isRadicado = true;
                        exSoliSEJ.fechaRadicacion = _readerSEJ.GetDateTime(1);
                        exSoliSEJ.Expediente = _readerSEJ.GetString(2);
                        exSoliSEJ.Juzgado = juz.nombreJuzgado;


                        LisRet.Add(exSoliSEJ);
                    }
                    _readerSEJ.Close();
                    sqlSEJT = string.Format("SELECT false as 'isRadicado',cj.fechaTerminacion as 'fechaRadicacion',concat(lpad(cj.numero,5,'0'),'/',cj.anio) 'EXPEDIENTE',jz.desJuzgado as 'juzgado'"
                    + " from htsj_laboral.tblcarpetasjudiciales cj left"
                    + " join htsj_laboral.tbljuzgados as jz on cj.cveJuzgado = jz.cveJuzgado"
                    + " where cj.fechaTerminacion between '{0} 00:00:00' and '{1} 23:59:59' and cj.cvetipocarpeta = 1"
                    + " and cj.cveEstatusCarpetasJudiciales in (2, 10) AND jz.cveJuzgado in ({2}) AND cj.activo = 'S' AND jz.activo = 'S'; ", fechaInicio, fechaFinal, juz.idJuzgado);
                    MySqlCommand _comandoSEJ2 = new MySqlCommand(sqlSEJT, conSEJ);
                    MySqlDataReader _readerSEJ2 = _comandoSEJ2.ExecuteReader();
                    while (_readerSEJ2.Read())
                    {
                        ExpedientesIniciados exSoliSEJ2 = new ExpedientesIniciados();
                        exSoliSEJ2.isRadicado = false;
                        exSoliSEJ2.fechaTermino = _readerSEJ2.GetDateTime(1);
                        exSoliSEJ2.Expediente = _readerSEJ2.GetString(2);
                        exSoliSEJ2.Juzgado = juz.nombreJuzgado;

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
        public ActionResult CerrarSesion()
        {
            FormsAuthentication.SignOut();
            Session["Usuario"] = null;
            return RedirectToAction("Index", "Login");
        }
    }
}