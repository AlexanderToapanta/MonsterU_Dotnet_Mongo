using System;
using System.Web.Mvc;

namespace Monster_University.Controllers
{
    
    public class HomeController : Controller
    {
        // GET: Home/Index - Dashboard principal
        public ActionResult Index()
        {
            ViewBag.Usuario = Session["Usuario"]?.ToString() ?? "N/A";
            ViewBag.Estado = Session["UsuarioEstado"]?.ToString() ?? "N/A";
            ViewBag.UsuarioID = Session["UsuarioID"]?.ToString() ?? "N/A";

            return View();
        }


        // GET: Home/About - Acerca de
        public ActionResult About()
        {
            return View();
        }

        // GET: Home/Contact - Contacto
        public ActionResult Contact()
        {
            return View();
        }
    }
}