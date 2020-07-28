using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ICA.Models;
using ICA.Utilities;
using System.Web.UI.DataVisualization.Charting;

namespace ICA.Controllers
{
    [Autorizar]
    public class ParametrosController : Controller
    {
        private ICAEntities db = new ICAEntities();

        // GET: Parametros
        public ActionResult Index()
        {
            return View(db.ParametroMuestra.ToList());
        }

        [HttpPost]
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Parametros, "Actualizar")]
        public ActionResult Edit(ParametroMuestra[] parametros)
        {
            double p = 0, a = 0;
            foreach (var item in parametros)
            {
                ParametroMuestra parametro = db.ParametroMuestra.Find(item.PAMId);
                if (parametro.Parametro == "a")
                    a = item.Valor;
                if (parametro.Parametro == "p")
                    p = item.Valor;
                if(parametro.Parametro == "q")
                {
                    item.Valor = 100 - p;
                }
                if(parametro.Parametro == "Za")
                {
                    var chart = new Chart();
                    double temp =(100 -0.5 * (100 - a ))/100;
                    item.Valor = Math.Floor((chart.DataManipulator.Statistics.InverseNormalDistribution(temp)) * 100) / 100; 
                }
            
                if(parametro.Valor != item.Valor)
                {
                    parametro.Valor = item.Valor;
                    parametro.MFechaHora = DateTime.Now;
                    parametro.MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name;
                    db.Entry(parametro).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    Metodos.RegistrarLog(new Log
                    {
                        Modulo = "ParametrosMuestra",
                        Accion = "Actualizar",
                        Detalle = "Parametro modificado: " + parametro.Descripcion + " " + parametro.Valor
                    });
                }
            }
            TempData["MensajeClase"] = "alert-success";
            TempData["Mensaje"] = "Cambios guardados correctamente.";
            return Json(true);
        }

        public ActionResult irHome()
        {

            return PartialView("_Redireccion");

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}