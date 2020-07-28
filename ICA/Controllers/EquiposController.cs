using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ICA.Models;
using ICA.Utilities;

namespace ICA.Controllers
{
    [Autorizar]
    public class EquiposController : Controller
    {
        private ICAEntities db = new ICAEntities();

        // GET: Equipo
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Equipos, "Leer")]
        public ActionResult Index()
        {
            return View(db.Equipo.ToList());
        }

        // GET: Equipo/Create
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Equipos, "Insertar")]
        public ActionResult Create()
        {
            ViewBag.EstatusList = Helpers.ComboLista("ESTATUS");

            return PartialView("_Create");
        }

        // POST: Equipos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Equipos, "Insertar")]
        public ActionResult Create(Equipo equipo)
        {

            if (ModelState.IsValid)
            {
                equipo.MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name;
                equipo.MFechaHora = DateTime.Now;
                db.Equipo.Add(equipo);
                db.SaveChanges();
                Metodos.RegistrarLog(new Log
                {
                    Modulo = "Equipos",
                    Accion = "Insertar",
                    Detalle = "Equipo insertado: " + equipo.Nombre
                });
                TempData["MensajeClase"] = "alert-success";
                TempData["Mensaje"] = "Cambios guardados correctamente.";
                return Json(true);
            }
            TempData["MensajeClase"] = "alert-danger";
            TempData["Mensaje"] = "Hubo un error al intentar guardar los cambios, porfavor intente nuevamente.";
            return Json(false);
        }

        // GET: Equipos/Edit/5
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Equipos, "Actualizar")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Equipo equipo = db.Equipo.Find(id);
            if (equipo == null)
            {
                return HttpNotFound();
            }
            ViewBag.EstatusList = Helpers.ComboLista("ESTATUS");
            return PartialView("_Edit", equipo);
        }

        // POST: Perfiles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Equipos, "Actualizar")]
        public ActionResult Edit(Equipo equipo)
        {
            if (ModelState.IsValid)
            {
                equipo.MFechaHora = DateTime.Now;
                equipo.MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name;
                db.Entry(equipo).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                Metodos.RegistrarLog(new Log
                {
                    Modulo = "Equipos",
                    Accion = "Actualizar",
                    Detalle = "Equipo modificado: " + equipo.Nombre + " " + equipo.Estatus
                });
                TempData["MensajeClase"] = "alert-success";
                TempData["Mensaje"] = "Cambios guardados correctamente.";
                return Json(true);
            }
            TempData["MensajeClase"] = "alert-danger";
            TempData["Mensaje"] = "Hubo un error al intentar guardar los cambios, porfavor intente nuevamente.";
            return Json(false);
        }

        // GET: Equipos/Delete/5
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Equipos, "Borrar")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Equipo equipo = db.Equipo.Find(id);
            if (equipo == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Delete", equipo);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Equipos, "Borrar")]
        public ActionResult Delete(Equipo equipo)
        {
            if (ModelState.IsValid)
            {
                Equipo equipoE = db.Equipo.Find(equipo.EQPId);
                if (equipoE == null)
                {
                    TempData["MensajeClase"] = "alert-danger";
                    TempData["Mensaje"] = "Equipo no encontrado";
                    return Json(false);
                }

                if (equipoE.EquipoCriterio.Count > 0)
                {
                    equipoE.Estatus = Utilities.Utilities.Estatus()["Inactivo"];
                    equipoE.MFechaHora = DateTime.Now;
                    equipoE.MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name;
                    db.Entry(equipoE).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    Metodos.RegistrarLog(new Log
                    {
                        Modulo = "Equipos",
                        Accion = "Actualizar",
                        Detalle = "Equipo modificado: " + equipoE.Nombre + " " + equipoE.Estatus
                    });
                    TempData["MensajeClase"] = "alert-danger";
                    TempData["Mensaje"] = "El registro no puede ser eliminado, ya que se encuentra asociado a otros registros";
                    return Json(false);
                }

                db.Equipo.Remove(equipoE);
                db.SaveChanges();
                Metodos.RegistrarLog(new Log
                {
                    Modulo = "Equipos",
                    Accion = "Borrar",
                    Detalle = "Equipo borrado: " + equipoE.Nombre
                });
                TempData["MensajeClase"] = "alert-success";
                TempData["Mensaje"] = "Registro eliminado con éxito.";
                return Json(true);
            }
            TempData["MensajeClase"] = "alert-danger";
            TempData["Mensaje"] = "Error al intentar eliminar";
            return Json(false);

        }



        public ActionResult Criterios()
        {

            return PartialView("_Criterios");

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