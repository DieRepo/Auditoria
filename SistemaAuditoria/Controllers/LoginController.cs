using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SistemaAuditoria.Models;
using SistemaAuditoria.Logica;
using System.Web.Security;

namespace SistemaAuditoria.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(string correo, string contraseña)
        {
            Usuario usu = new LO_Login().CheckLogin(correo, contraseña);
            if (usu.Nombres != null)
            {
                FormsAuthentication.SetAuthCookie(usu.Correo, false);
                Session["Usuario"] = usu;
                return RedirectToAction("Index", "ListadoMaterias");
            }

            return View();
        }
    }
}