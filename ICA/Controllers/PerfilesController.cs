using System;
using System.Collections.Generic;
using System.Data;
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
    public class PerfilesController : Controller
    {
        private ICAEntities db = new ICAEntities();

        // GET: Perfiles
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Perfiles, "Leer")]
        public ActionResult Index()
        {
            return View(db.Perfil.ToList());
        }

        // GET: Perfiles/Create
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Perfiles, "Insertar")]
        public ActionResult Create()
        {
            ViewBag.EstatusList = Helpers.ComboLista("ESTATUS");

            List<Modulo> modulos = db.Modulo.Where(m => m.Estatus == "A").ToList();
            return PartialView("_Create", modulos);
        }

        // POST: Perfiles/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Perfiles, "Insertar")]
        public ActionResult Create(Perfil perfil)
        {
            perfil.ModuloPerfil.ToList().ForEach(p => {
                p.MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name;
                p.MFechaHora = DateTime.Now;
            });
            perfil.MFechaHora = DateTime.Now;
            perfil.MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name;

            if (ModelState.IsValid)
            {
                db.Perfil.Add(perfil);
                db.SaveChanges();
                Metodos.RegistrarLog(new Log
                {
                    Modulo = "Perfiles",
                    Accion = "Insertar",
                    Detalle = "Perfil insertado: " + perfil.Descripcion
                });
                TempData["MensajeClase"] = "alert-success";
                TempData["Mensaje"] = "Cambios guardados correctamente.";
                return Json(true);
            }
            TempData["MensajeClase"] = "alert-danger";
            TempData["Mensaje"] = "Hubo un error al intentar guardar los cambios, porfavor intente nuevamente.";
            return Json(false);
        }

        // GET: Perfiles/Edit/5
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Perfiles, "Actualizar")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Perfil perfil = db.Perfil.Find(id);
            if (perfil == null)
            {
                return HttpNotFound();
            }
            ViewBag.EstatusList = Helpers.ComboLista("ESTATUS");

            List<Modulo> modulos = db.Modulo.Where(m => m.Estatus == "A").ToList();
            ViewBag.Modulos = modulos;
            return PartialView("_Edit", perfil);
        }

        // POST: Perfiles/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Perfiles, "Actualizar")]
        public ActionResult Edit(Perfil perfil)
        {
            if (ModelState.IsValid)
            {
                perfil.MFechaHora = DateTime.Now;
                perfil.MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name;
                perfil.ModuloPerfil.ToList().ForEach(p => {
                    p.MFechaHora = DateTime.Now;
                    p.MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name;
                    var modper = db.ModuloPerfil.Where(mp => mp.PERId == p.PERId && mp.MODId == p.MODId).Any();
                    if (modper)
                    {
                        db.Entry(p).State = System.Data.Entity.EntityState.Modified;
                    }
                    else
                    {
                        db.ModuloPerfil.Add(p);
                    }
                });
                db.Entry(perfil).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                Metodos.RegistrarLog(new Log
                {
                    Modulo = "Perfiles",
                    Accion = "Actualizar",
                    Detalle = "Perfil modificado: " + perfil.Descripcion + " " + perfil.Estatus
                });
                TempData["MensajeClase"] = "alert-success";
                TempData["Mensaje"] = "Cambios guardados correctamente.";
                return Json(true);
            }
            TempData["MensajeClase"] = "alert-danger";
            TempData["Mensaje"] = "Hubo un error al intentar guardar los cambios, porfavor intente nuevamente.";
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