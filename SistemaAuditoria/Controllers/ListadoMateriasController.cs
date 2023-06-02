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

namespace SistemaAuditoria.Controllers
{
    public class ListadoMateriasController : Controller
    {



        List<Juzgadosddl> jddlF = new List<Juzgadosddl>();
        List<Juzgadosddl> ListaRespaldoJuzgado = new List<Juzgadosddl>();
        public static List<ExpedientesIniciados> Exportei = new List<ExpedientesIniciados>();

        // GET: ListadoMaterias
        public ActionResult Index(FormCollection collection)
        {
            try
            {
                string materia = collection["matddl"];
                string fechaInicio = collection["fecIni"];
                string fechaFinal = collection["fecFin"];
                string juzgado = collection["juzddl"];
                //DateTime fechaInicio = new DateTime();
                //DateTime fechaFinal = new DateTime();
                //if (materia != null && fechaInicio1 != null && fechaFinal1 != null)
                //{
                //    fechaInicio = DateTime.ParseExact(fechaInicio1, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                //    fechaFinal = DateTime.ParseExact(fechaFinal1, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                //}
                if (fechaInicio == "" || fechaFinal == "")
                {
                    if (fechaInicio == "")
                        TempData["MessageFI"] = "*Valor requerido";
                    if (fechaInicio == "")
                        TempData["MessageFF"] = "*Valor requerido";

                }
                Juzgadosddl juzgadoSeleccionado = new Juzgadosddl();
                if (juzgado != null && int.Parse(juzgado) != 0)
                {
                    int idjuz = int.Parse(juzgado);
                    int idMat = int.Parse(materia);
                    string matL = ((materiaE)idMat).ToString();
                    MySqlConnection ListJuzgados = new MySqlConnection();
                    ListJuzgados = ConexionesBDs.ObtenerConexion(2);
                    string sqlListaJuz = string.Format("SELECT idJuzgado, nombre, cveSEJ, cveLaboral, cveSIGEJUPE, cveSIGEJUPE2 FROM die_equivalencias_catalogos.homologado_tbljuzgados"
                        + " WHERE activo = 1 and materia = '{0}';", matL);
                    MySqlCommand _comandoLisJuz = new MySqlCommand(sqlListaJuz, ListJuzgados);
                    MySqlDataReader _readerLisJuz = _comandoLisJuz.ExecuteReader();
                    while (_readerLisJuz.Read())
                    {
                        Juzgadosddl JuzObj = new Juzgadosddl();
                        JuzObj.idJuzgado = _readerLisJuz.GetInt32(0);
                        JuzObj.nombreJuzgado = _readerLisJuz.GetString(1);
                        if (_readerLisJuz.IsDBNull(2))
                            JuzObj.cveSEJ = 0;
                        else
                            JuzObj.cveSEJ = _readerLisJuz.GetInt32(2);
                        if (_readerLisJuz.IsDBNull(3))
                            JuzObj.cveLaboral = 0;
                        else
                            JuzObj.cveLaboral = _readerLisJuz.GetInt32(3);
                        if (_readerLisJuz.IsDBNull(4))
                            JuzObj.cveSIGEJUPE = 0;
                        else
                            JuzObj.cveSIGEJUPE = _readerLisJuz.GetInt32(4);
                        if (_readerLisJuz.IsDBNull(5))
                            JuzObj.cveSIGEJUPE2 = 0;
                        else
                            JuzObj.cveSIGEJUPE2 = _readerLisJuz.GetInt32(5);

                        ListaRespaldoJuzgado.Add(JuzObj);
                    }
                    juzgadoSeleccionado = ListaRespaldoJuzgado.Where(x => x.idJuzgado == idjuz).FirstOrDefault();
                }

                List<ExpedientesIniciados> ei = new List<ExpedientesIniciados>();

                if (materia != null && fechaInicio != null && fechaFinal != null)
                {
                    if (materia != null && int.Parse(materia) == 1 && juzgado != "")
                    {
                        Exportei.Clear();
                        ei = GeneraJuzgadoPenal(juzgadoSeleccionado, fechaInicio, fechaFinal);
                        Exportei = ei;
                    }
                    else if (materia != null && int.Parse(materia) == 2 && juzgado != "")
                    {
                        Exportei.Clear();
                        ei = GeneraJuzgadoCivMerFam(juzgadoSeleccionado, fechaInicio, fechaFinal, int.Parse(materia));
                        Exportei = ei;
                    }
                    else if (materia != null && int.Parse(materia) == 3 && juzgado != "")
                    {
                        Exportei.Clear();
                        ei = GeneraJuzgadoCivMerFam(juzgadoSeleccionado, fechaInicio, fechaFinal, int.Parse(materia));
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
                        ei = GeneraJuzgadoCivMerFam(juzgadoSeleccionado, fechaInicio, fechaFinal, int.Parse(materia));
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
            string materia = ((materiaE)idJuz).ToString();
            MySqlConnection conddlJuzgados = new MySqlConnection();
            conddlJuzgados = ConexionesBDs.ObtenerConexion(2);

            using (conddlJuzgados)
            {
                string sql = string.Format("SELECT idJuzgado, nombre, cveSEJ, cveLaboral, cveSIGEJUPE, cveSIGEJUPE2 FROM die_equivalencias_catalogos.homologado_tbljuzgados"
                + " WHERE activo = 1 and materia = '{0}';", materia);
                MySqlCommand _comando = new MySqlCommand(sql, conddlJuzgados);

                MySqlDataReader _reader = _comando.ExecuteReader();
                while (_reader.Read())
                {
                    Juzgadosddl jddl = new Juzgadosddl();
                    jddl.idJuzgado = _reader.GetInt32(0);
                    jddl.nombreJuzgado = _reader.GetString(1);
                    if (_reader.IsDBNull(2))
                        jddl.cveSEJ = 0;
                    else
                        jddl.cveSEJ = _reader.GetInt32(2);
                    if (_reader.IsDBNull(3))
                        jddl.cveLaboral = 0;
                    else
                        jddl.cveLaboral = _reader.GetInt32(3);
                    if (_reader.IsDBNull(4))
                        jddl.cveSIGEJUPE = 0;
                    else
                        jddl.cveSIGEJUPE = _reader.GetInt32(4);
                    if (_reader.IsDBNull(5))
                        jddl.cveSIGEJUPE2 = 0;
                    else
                        jddl.cveSIGEJUPE2 = _reader.GetInt32(5);

                    jddlF.Add(jddl);
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
                    sheet.Cells[2, 1].Value = "Numero Expediente";
                    sheet.Cells[2, 2].Value = "Materia";
                    sheet.Cells[2, 3].Value = "Juzgado";
                    sheet.Cells[2, 4].Value = "Fecha Radicados";

                    sheet.Cells[1, 7].Value = "Termiandos";
                    sheet.Cells[2, 7].Value = "Numero Expediente";
                    sheet.Cells[2, 8].Value = "Materia";
                    sheet.Cells[2, 9].Value = "Juzgado";
                    sheet.Cells[2, 10].Value = "Fecha Terminados";

                    sheet.Cells["A1:D1"].Merge = true;
                    sheet.Cells["G1:J1"].Merge = true;

                    sheet.Cells["A1:D2"].Style.Font.Bold = true;
                    sheet.Cells["G1:J2"].Style.Font.Bold = true;
                    int rowIterator = 2;

                    string fechaRadicacion = "", fechaTermino = "";


                    foreach (ExpedientesIniciados item in Radicados)
                    {
                        rowIterator++;
                        fechaRadicacion = item.fechaRadicacion.ToString();
                        sheet.Cells[rowIterator, 1].Value = item.Expediente;
                        sheet.Cells[rowIterator, 2].Value = item.Materia;
                        sheet.Cells[rowIterator, 3].Value = item.Juzgado;
                        sheet.Cells[rowIterator, 4].Value = fechaRadicacion;

                    }
                    int rowIteratorT = 2;
                    foreach (ExpedientesIniciados item in Terminados)
                    {
                        rowIteratorT++;
                        fechaTermino = item.fechaTermino.ToString();
                        sheet.Cells[rowIteratorT, 7].Value = item.Expediente;
                        sheet.Cells[rowIteratorT, 8].Value = item.Materia;
                        sheet.Cells[rowIteratorT, 9].Value = item.Juzgado;
                        sheet.Cells[rowIteratorT, 10].Value = fechaTermino;

                    }

                    sheet.Column(1).AutoFit();
                    sheet.Column(2).AutoFit();
                    sheet.Column(3).AutoFit();
                    sheet.Column(4).AutoFit();
                    sheet.Column(7).AutoFit();
                    sheet.Column(8).AutoFit();
                    sheet.Column(9).AutoFit();
                    sheet.Column(10).AutoFit();

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

        //public ActionResult Download()
        //{

        //    if (Session["DownloadExcel_FileManager"] != null)
        //    {
        //        byte[] data = Session["DownloadExcel_FileManager"] as byte[];
        //        return File(data, "application/octet-stream", "FileManager.xlsx");
        //    }
        //    else
        //    {
        //        return new EmptyResult();
        //    }
        //}

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
        public List<ExpedientesIniciados> GeneraJuzgadoPenal(Juzgadosddl juz, string fechaInicio, string fechaFinal)
        {
            try
            {
                List<ExpedientesIniciados> LisRet = new List<ExpedientesIniciados>();
                var tipos = new Int32[] { 2, 3, 4 };
                if (juz.cveSIGEJUPE != 0 || juz.cveSIGEJUPE2 != 0)
                {
                    MySqlConnection con = new MySqlConnection();
                    con = ConexionesBDs.ObtenerConexion(1);
                    using (con)
                    {
                        if (juz.cveSIGEJUPE != 0)
                        {
                            string sql = "", sql2 = "";
                            sql = string.Format("select true as 'isRadicado',concat(lpad(cj.numero,5,'0'),'/',cj.anio) 'EXPEDIENTE',j.desJuzgado as 'juzgado' ,cj.fechaRadicacion as 'fechaRadicacion'"
                            + " from tblcarpetasjudiciales as cj"
                            + " left join tbljuzgados as j on cj.cveJuzgado = j.cveJuzgado"
                            + " where cj.fecharadicacion between '{0}' and '{1}'"
                            + " and cj.activo = 'S' and cj.cvetipocarpeta in (2, 3, 4) and cj.cveJuzgado in ({2});", fechaInicio, fechaFinal, juz.cveSIGEJUPE);
                            MySqlCommand _comando = new MySqlCommand(sql, con);
                            MySqlDataReader _reader = _comando.ExecuteReader();
                            while (_reader.Read())
                            {
                                ExpedientesIniciados exSoli = new ExpedientesIniciados();
                                exSoli.isRadicado = true;
                                exSoli.Expediente = _reader.GetString(1);
                                exSoli.Materia = ObtienTipoPenal(juz);
                                exSoli.Juzgado = juz.nombreJuzgado;
                                exSoli.fechaRadicacion = _reader.GetDateTime(3);

                                LisRet.Add(exSoli);
                            }
                            _reader.Close();
                            sql2 = string.Format("select false as 'isRadicado',concat(lpad(cj.numero,5,'0'),'/',cj.anio) 'EXPEDIENTE',j.desJuzgado as 'juzgado' ,cj.fechaTermino as 'fechaTermino'"
                            + " from tblcarpetasjudiciales as cj"
                            + " left join tbljuzgados as j on cj.cveJuzgado = j.cveJuzgado"
                            + " where cj.fechaTermino between '{0}' and '{1}'"
                            + " and cj.activo = 'S' and cj.cvetipocarpeta in (2, 3, 4) and cj.cveEstatusCarpeta = 2 and cj.cveJuzgado in ({2}); ", fechaInicio, fechaFinal, juz.cveSIGEJUPE);
                            MySqlCommand _comandoT = new MySqlCommand(sql2, con);
                            MySqlDataReader _readerT = _comandoT.ExecuteReader();
                            while (_readerT.Read())
                            {
                                ExpedientesIniciados exSoliT = new ExpedientesIniciados();
                                exSoliT.isRadicado = false;
                                exSoliT.Expediente = _readerT.GetString(1);
                                exSoliT.Materia = ObtienTipoPenal(juz);
                                exSoliT.Juzgado = juz.nombreJuzgado;
                                exSoliT.fechaTermino = _readerT.GetDateTime(3);

                                LisRet.Add(exSoliT);
                            }
                            _readerT.Close();
                        }
                        if (juz.cveSIGEJUPE2 != 0)
                        {
                            string sqlSGJ2 = "", sqlSGJ2T = "";
                            sqlSGJ2 = string.Format("select true as 'isRadicado',concat(lpad(cj.numero,5,'0'),'/',cj.anio) 'EXPEDIENTE',j.desJuzgado as 'juzgado' ,cj.fechaRadicacion as 'fechaRadicacion'"
                                + " from tblcarpetasjudiciales as cj"
                                + " left join tbljuzgados as j on cj.cveJuzgado = j.cveJuzgado"
                                + " where cj.fechaRadicacion between '{0}' and '{1}' "
                                + " and cj.activo = 'S' and cj.cvetipocarpeta in (2, 3, 4) and cj.cveJuzgado in ({2});", fechaInicio, fechaFinal, juz.cveSIGEJUPE2);
                            MySqlCommand _comando2 = new MySqlCommand(sqlSGJ2, con);
                            MySqlDataReader _reader2 = _comando2.ExecuteReader();
                            while (_reader2.Read())
                            {
                                ExpedientesIniciados exSoli2 = new ExpedientesIniciados();
                                exSoli2.isRadicado = true;
                                exSoli2.Expediente = _reader2.GetString(1);
                                exSoli2.Materia = ObtienTipoPenal(juz);
                                exSoli2.Juzgado = juz.nombreJuzgado;
                                exSoli2.fechaRadicacion = _reader2.GetDateTime(3);

                                LisRet.Add(exSoli2);
                            }
                            _reader2.Close();
                            sqlSGJ2T = string.Format("select false as 'isRadicado',concat(lpad(cj.numero,5,'0'),'/',cj.anio) 'EXPEDIENTE',j.desJuzgado as 'juzgado' ,cj.fechaTermino as 'fechaTermino'"
                                + " from tblcarpetasjudiciales as cj"
                                + " left join tbljuzgados as j on cj.cveJuzgado = j.cveJuzgado"
                                + " where cj.fechaTermino between '{0}' and '{1}' "
                                + " and cj.activo = 'S' and cj.cvetipocarpeta in (2, 3, 4) and cj.cveJuzgado in ({2});", fechaInicio, fechaFinal, juz.cveSIGEJUPE2);
                            MySqlCommand _comando2T = new MySqlCommand(sqlSGJ2T, con);
                            MySqlDataReader _reader2T = _comando2T.ExecuteReader();
                            while (_reader2T.Read())
                            {
                                ExpedientesIniciados exSoli2T = new ExpedientesIniciados();
                                exSoli2T.isRadicado = false;
                                exSoli2T.Expediente = _reader2T.GetString(1);
                                exSoli2T.Materia = ObtienTipoPenal(juz);
                                exSoli2T.Juzgado = juz.nombreJuzgado;
                                exSoli2T.fechaTermino = _reader2T.GetDateTime(3);

                                LisRet.Add(exSoli2T);
                            }
                            _reader2T.Close();
                        }
                    }
                }
                if (juz.cveSEJ != 0)
                {
                    MySqlConnection conSEJ = new MySqlConnection();
                    conSEJ = ConexionesBDs.ObtenerConexion(3);
                    string sqlSEJ = "", sqlSEJT = "";
                    sqlSEJ = string.Format("SELECT true as 'isRadicado',concat(lpad(ij.cveExp, 5, '0'), '/', ij.anioExp) 'EXPEDIENTE','' as 'juzgado' ,ij.FechaRad as 'fechaRadicacion'"
                    + " FROM estadistica.tblinijuzgados as ij"
                    + " WHERE ij.FechaRad between '{0}' and '{1}' and CveJuzgado = {2};", fechaInicio, fechaFinal, juz.cveSEJ);
                    MySqlCommand _comandoSEJ = new MySqlCommand(sqlSEJ, conSEJ);
                    MySqlDataReader _readerSEJ = _comandoSEJ.ExecuteReader();
                    while (_readerSEJ.Read())
                    {
                        ExpedientesIniciados exSoliSEJ = new ExpedientesIniciados();
                        exSoliSEJ.isRadicado = true;
                        exSoliSEJ.Expediente = _readerSEJ.GetString(1);
                        exSoliSEJ.Materia = ObtienTipoPenal(juz);
                        exSoliSEJ.Juzgado = juz.nombreJuzgado;
                        exSoliSEJ.fechaRadicacion = _readerSEJ.GetDateTime(3);

                        LisRet.Add(exSoliSEJ);
                    }
                    _readerSEJ.Close();
                    sqlSEJT = string.Format("SELECT false as 'isRadicado',concat(lpad(ij.cveExp, 5, '0'), '/', ij.anioExp) 'EXPEDIENTE','' as 'juzgado' ,ts.FechaTer as 'fechaTerminacion'"
                    + " FROM estadistica.tblinijuzgados as ij left join tblterjuzgados as ts on ij.CveIni = ts.CveIni "
                    + " WHERE ts.FechaTer between '{0}' and '{1}' and ij.CveJuzgado = {2};", fechaInicio, fechaFinal, juz.cveSEJ);
                    MySqlCommand _comandoSEJ2 = new MySqlCommand(sqlSEJT, conSEJ);
                    MySqlDataReader _readerSEJ2 = _comandoSEJ2.ExecuteReader();
                    while (_readerSEJ2.Read())
                    {
                        ExpedientesIniciados exSoliSEJ2 = new ExpedientesIniciados();
                        exSoliSEJ2.isRadicado = false;
                        exSoliSEJ2.Expediente = _readerSEJ2.GetString(1);
                        exSoliSEJ2.Materia = ObtienTipoPenal(juz);
                        exSoliSEJ2.Juzgado = juz.nombreJuzgado;
                        exSoliSEJ2.fechaTermino = _readerSEJ2.GetDateTime(3);

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
        public List<ExpedientesIniciados> GeneraJuzgadoCivMerFam(Juzgadosddl juz, string fechaInicio, string fechaFinal, int tipoMateria)
        {
            try
            {
                List<ExpedientesIniciados> LisRet = new List<ExpedientesIniciados>();
                var tipos = new Int32[] { 2, 3, 4 };
                if (juz.cveSEJ != 0)
                {
                    MySqlConnection conSEJ = new MySqlConnection();
                    conSEJ = ConexionesBDs.ObtenerConexion(3);
                    string sqlSEJ = "", sqlSEJT = "";
                    sqlSEJ = string.Format("SELECT true as 'isRadicado',concat(lpad(ij.cveExp, 5, '0'), '/', ij.anioExp) 'EXPEDIENTE','' as 'juzgado' ,ij.FechaRad as 'fechaRadicacion'"
                    + " FROM estadistica.tblinijuzgados as ij"
                    + " where ij.FechaRad between '{0}' and '{1}' and CveJuzgado = {2};", fechaInicio, fechaFinal, juz.cveSEJ);
                    MySqlCommand _comandoSEJ = new MySqlCommand(sqlSEJ, conSEJ);
                    MySqlDataReader _readerSEJ = _comandoSEJ.ExecuteReader();
                    while (_readerSEJ.Read())
                    {
                        ExpedientesIniciados exSoliSEJ = new ExpedientesIniciados();
                        exSoliSEJ.isRadicado = true;
                        exSoliSEJ.Expediente = _readerSEJ.GetString(1);
                        exSoliSEJ.Materia = ObtienTipoMat(tipoMateria);
                        exSoliSEJ.Juzgado = juz.nombreJuzgado;
                        exSoliSEJ.fechaRadicacion = _readerSEJ.GetDateTime(3);

                        LisRet.Add(exSoliSEJ);
                    }
                    _readerSEJ.Close();
                    sqlSEJT = string.Format("SELECT false as 'isRadicado',concat(lpad(ij.cveExp, 5, '0'), '/', ij.anioExp) 'EXPEDIENTE','' as 'juzgado' ,ts.FechaTer as 'fechaTerminacion'"
                    + " FROM estadistica.tblinijuzgados as ij left join tblterjuzgados as ts on ij.CveIni = ts.CveIni "
                    + " WHERE ts.FechaTer between '{0}' and '{1}' and ij.CveJuzgado = {2};", fechaInicio, fechaFinal, juz.cveSEJ);
                    MySqlCommand _comandoSEJ2 = new MySqlCommand(sqlSEJT, conSEJ);
                    MySqlDataReader _readerSEJ2 = _comandoSEJ2.ExecuteReader();
                    while (_readerSEJ2.Read())
                    {
                        ExpedientesIniciados exSoliSEJ2 = new ExpedientesIniciados();
                        exSoliSEJ2.isRadicado = false;
                        exSoliSEJ2.Expediente = _readerSEJ2.GetString(1);
                        exSoliSEJ2.Materia = ObtienTipoMat(tipoMateria);
                        exSoliSEJ2.Juzgado = juz.nombreJuzgado;
                        exSoliSEJ2.fechaTermino = _readerSEJ2.GetDateTime(3);

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
                if (juz.cveLaboral != 0)
                {
                    MySqlConnection con = new MySqlConnection();
                    con = ConexionesBDs.ObtenerConexion(4);
                    using (con)
                    {
                        if (juz.cveLaboral != 0)
                        {
                            string sql = "", sql2 = "";
                            sql = string.Format("select true as 'isRadicado', concat(lpad(c.numero, 5, '0'), '/', c.anio) 'EXPEDIENTE', j.desJuzgado as 'juzgado', c.fechaRadicacion as 'fechaRadicacion'"
                            + " from tblcarpetasjudiciales c"
                            + " inner join tbljuzgados j on c.cvejuzgado = j.cvejuzgado"
                            + " where c.fecharadicacion between '{0}' and '{1}'"
                            + " and c.activo = 'S' and c.cvetipocarpeta = 1 and c.cvejuzgado in ({2});", fechaInicio, fechaFinal, juz.cveLaboral);
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
                            + " AND jz.cveJuzgado in ({2}) AND cj.activo = 'S' AND jz.activo = 'S';", fechaInicio, fechaFinal, juz.cveLaboral);
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
                if (juz.cveSEJ != 0)
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
    }
}