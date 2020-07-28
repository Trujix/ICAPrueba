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
    public class ArticulosController : Controller
    {
        private ICAEntities db = new ICAEntities();

        // GET: Articulos
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Articulos, "Leer")]
        public ActionResult Index()
        {
            return View(db.Articulo.ToList());
        }

        // GET: Articulos/Create
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Articulos, "Insertar")]
        public ActionResult Create()
        {
            ViewBag.EstatusList = Helpers.ComboLista("ESTATUS");
            ViewBag.TipoProdList = db.TipoProducto.Where(t => t.Estatus == "A").Select(i => new SelectListItem
            {
                Value = i.TPRId.ToString(),
                Text = i.Producto
            }).ToList();
            ViewBag.ClasificacionList = Helpers.ComboLista("CLASIFICA");
            //ViewBag.SKUs = db.Skus.ToList();
            return PartialView("_Create");
        }

        // POST: Articulos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Articulos, "Insertar")]
        public ActionResult Create(Articulo articulo)
        {

            if (ModelState.IsValid)
            {
                articulo.MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name;
                articulo.MFechaHora = DateTime.Now;
                db.Articulo.Add(articulo);
                db.SaveChanges();
                Metodos.RegistrarLog(new Log
                {
                    Modulo = "Articulos",
                    Accion = "Insertar",
                    Detalle = "Articulo insertado: " + articulo.Nombre
                });
                TempData["MensajeClase"] = "alert-success";
                TempData["Mensaje"] = "Cambios guardados correctamente.";
                return Json(true);
            }
            TempData["MensajeClase"] = "alert-danger";
            TempData["Mensaje"] = "Hubo un error al intentar guardar los cambios, porfavor intente nuevamente.";
            return Json(false);
        }

        // GET: Articulos/Edit/5
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Articulos, "Actualizar")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Articulo articulo = db.Articulo.Find(id);
            if (articulo == null)
            {
                return HttpNotFound();
            }
            ViewBag.EstatusList = Helpers.ComboLista("ESTATUS");
            ViewBag.TipoProdList = db.TipoProducto.Where(t => t.Estatus == "A").Select(i => new SelectListItem
            {
                Value = i.TPRId.ToString(),
                Text = i.Producto
            }).ToList();
            ViewBag.ClasificacionList = Helpers.ComboLista("CLASIFICA");
            ViewBag.SKUs = db.Skus.ToList();
            return PartialView("_Edit", articulo);
        }

        // POST: Articulos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Articulos, "Actualizar")]
        public ActionResult Edit(Articulo articulo)
        {
            if (ModelState.IsValid)
            {
                articulo.MFechaHora = DateTime.Now;
                articulo.MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name;
                db.Entry(articulo).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                Metodos.RegistrarLog(new Log
                {
                    Modulo = "Articulos",
                    Accion = "Actualizar",
                    Detalle = "Articulo modificado: " + articulo.Nombre + " " + articulo.Estatus
                });
                TempData["MensajeClase"] = "alert-success";
                TempData["Mensaje"] = "Cambios guardados correctamente.";
                return Json(true);
            }
            TempData["MensajeClase"] = "alert-danger";
            TempData["Mensaje"] = "Hubo un error al intentar guardar los cambios, porfavor intente nuevamente.";
            return Json(false);
        }

        // GET: Articulos/Delete/5
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Articulos, "Borrar")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Articulo articulo = db.Articulo.Find(id);
            if (articulo == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Delete", articulo);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Articulos, "Borrar")]
        public ActionResult Delete(Articulo articulo)
        {
            if (ModelState.IsValid)
            {
                Articulo articuloE = db.Articulo.Find(articulo.ARTId);
                if (articuloE == null)
                {
                    TempData["MensajeClase"] = "alert-danger";
                    TempData["Mensaje"] = "Artículo no encontrado";
                    return Json(false);
                }

                if (articuloE.EvArticulo.Count > 0 || articuloE.ArticuloCriterio.Count > 0)
                {
                    articuloE.Estatus = Utilities.Utilities.Estatus()["Inactivo"];
                    articuloE.MFechaHora = DateTime.Now;
                    articuloE.MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name;
                    db.Entry(articuloE).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    Metodos.RegistrarLog(new Log
                    {
                        Modulo = "Articulos",
                        Accion = "Actualizar",
                        Detalle = "Articulo modificado: " + articuloE.Nombre + " " + articuloE.Estatus
                    });
                    TempData["MensajeClase"] = "alert-danger";
                    TempData["Mensaje"] = "El registro no puede ser eliminado, ya que se encuentra asociado a otros registros";
                    return Json(false);
                }

                db.Articulo.Remove(articuloE);
                db.SaveChanges();
                Metodos.RegistrarLog(new Log
                {
                    Modulo = "Articulos",
                    Accion = "Borrar",
                    Detalle = "Articulo borrado: " + articuloE.Nombre
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