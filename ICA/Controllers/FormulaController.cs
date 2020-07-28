using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using ICA.Models;
using ICA.Utilities;

namespace ICA.Controllers
{
    [Autorizar]
    public class FormulaController : Controller
    {
        private ICAEntities db = new ICAEntities();

        // GET: Formula
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Formula, "Leer")]
        public ActionResult Index()
        {
            return View(db.Formula.ToList());
        }

        [VerificarPerfil((int)Utilities.Utilities.Modulos.Formula, "Insertar")]
        public ActionResult Create()
        {
            ViewBag.EstatusList = Helpers.ComboLista("ESTATUS");
            ViewBag.ElementoList = Helpers.ComboLista("FOR_ELEMEN");
            ViewBag.OpList = Helpers.ComboLista("OP_GLOBAL");
            return PartialView("_Create"); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Formula, "Insertar")]
        public ActionResult Create(Formula formula)
        {
            formula.MFechaHora = DateTime.Now;
            formula.MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name;

            if (ModelState.IsValid)
            {
                db.Formula.Add(formula);
                db.SaveChanges();
                Metodos.RegistrarLog(new Log
                {
                    Modulo = "Formula",
                    Accion = "Insertar",
                    Detalle = "Formula insertado: " + formula.Descripcion
                });
                TempData["MensajeClase"] = "alert-success";
                TempData["Mensaje"] = "Cambios guardados correctamente.";
                return Json(true);
            }
            TempData["MensajeClase"] = "alert-danger";
            TempData["Mensaje"] = "Hubo un error al intentar guardar los cambios, porfavor intente nuevamente.";
            return Json(false);
        }

        [VerificarPerfil((int)Utilities.Utilities.Modulos.Formula, "Actualizar")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Formula formula = db.Formula.Find(id);
            if (formula == null)
            {
                return HttpNotFound();
            }
            ViewBag.EstatusList = Helpers.ComboLista("ESTATUS");
            ViewBag.ElementoList = Helpers.ComboLista("FOR_ELEMEN");
            ViewBag.OpList = Helpers.ComboLista("OP_GLOBAL");

            return PartialView("_Edit", formula);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Formula, "Actualizar")]
        public ActionResult Edit(Formula formula)
        {
            if (ModelState.IsValid)
            {
                formula.MFechaHora = DateTime.Now;
                formula.MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name;
       
                db.Entry(formula).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                Metodos.RegistrarLog(new Log
                {
                    Modulo = "Formula",
                    Accion = "Actualizar",
                    Detalle = "Formula modificada: " + formula.Descripcion + " " + formula.Estatus
                });
                TempData["MensajeClase"] = "alert-success";
                TempData["Mensaje"] = "Cambios guardados correctamente.";
                return Json(true);
            }
            TempData["MensajeClase"] = "alert-danger";
            TempData["Mensaje"] = "Hubo un error al intentar guardar los cambios, porfavor intente nuevamente.";
            return Json(false);
        }

        [VerificarPerfil((int)Utilities.Utilities.Modulos.Formula, "Borrar")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Formula formula = db.Formula.Find(id);
            if (formula == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Delete", formula);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Formula, "Borrar")]
        public ActionResult Delete(Formula formula)
        {
            if (ModelState.IsValid)
            {
                Formula formulaE = db.Formula.Find(formula.FORId);
                if (formulaE == null)
                {
                    TempData["MensajeClase"] = "alert-danger";
                    TempData["Mensaje"] = "Formula no encontrada";
                    return Json(false);
                }

                db.Formula.Remove(formulaE);
                db.SaveChanges();
                Metodos.RegistrarLog(new Log
                {
                    Modulo = "Formula",
                    Accion = "Borrar",
                    Detalle = "Formula borrada: " + formula.Descripcion
                });
                TempData["MensajeClase"] = "alert-success";
                TempData["Mensaje"] = "Registro eliminado con éxito.";
                return Json(true);
            }
            TempData["MensajeClase"] = "alert-danger";
            TempData["Mensaje"] = "Error al intentar eliminar";
            return Json(false);

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