using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ICA.Models;
using ICA.Utilities;

namespace ICA.Controllers
{
    [Autorizar]
    public class TipoProductoController : Controller
    {
        private ICAEntities db = new ICAEntities();

        // GET: TipoProducto
        [VerificarPerfil((int)Utilities.Utilities.Modulos.TipoProducto, "Leer")]
        public ActionResult Index()
        {
            return View(db.TipoProducto.ToList());
        }

        // GET: TipoProducto/Create
        [VerificarPerfil((int)Utilities.Utilities.Modulos.TipoProducto, "Insertar")]
        public ActionResult Create()
        {
            ViewBag.EstatusList = Helpers.ComboLista("ESTATUS");
            ViewBag.CategoriaList = Helpers.ComboLista("CATEGOTP");
            ViewBag.UnidadList = Helpers.ComboLista("UNIDADTP");
            return PartialView("_Create");
        }

        // POST: TipoProducto/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [VerificarPerfil((int)Utilities.Utilities.Modulos.TipoProducto, "Insertar")]
        public ActionResult Create(TipoProducto tipoProducto)
        {

            if (ModelState.IsValid)
            {
                tipoProducto.MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name;
                tipoProducto.MFechaHora = DateTime.Now;
                db.TipoProducto.Add(tipoProducto);
                db.SaveChanges();
                Metodos.RegistrarLog(new Log
                {
                    Modulo = "TipoProducto",
                    Accion = "Insertar",
                    Detalle = "Tipo insertado: " + tipoProducto.Producto
                });
                TempData["MensajeClase"] = "alert-success";
                TempData["Mensaje"] = "Cambios guardados correctamente.";
                return Json(true);
            }
            TempData["MensajeClase"] = "alert-danger";
            TempData["Mensaje"] = "Hubo un error al intentar guardar los cambios, porfavor intente nuevamente.";
            return Json(false);
        }

        // GET: TipoProducto/Edit/5
        [VerificarPerfil((int)Utilities.Utilities.Modulos.TipoProducto, "Actualizar")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoProducto tipoProducto = db.TipoProducto.Find(id);
            if (tipoProducto == null)
            {
                return HttpNotFound();
            }
            ViewBag.EstatusList = Helpers.ComboLista("ESTATUS");
            ViewBag.CategoriaList = Helpers.ComboLista("CATEGOTP");
            ViewBag.UnidadList = Helpers.ComboLista("UNIDADTP");

            return PartialView("_Edit", tipoProducto);
        }

        // POST: TipoProducto/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [VerificarPerfil((int)Utilities.Utilities.Modulos.TipoProducto, "Actualizar")]
        public ActionResult Edit(TipoProducto tipoProducto)
        {
            if (ModelState.IsValid)
            {
                tipoProducto.MFechaHora = DateTime.Now;
                tipoProducto.MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name;
                db.Entry(tipoProducto).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                Metodos.RegistrarLog(new Log
                {
                    Modulo = "TipoProducto",
                    Accion = "Actualizar",
                    Detalle = "TipoProducto modificado: " + tipoProducto.Producto
                });
                TempData["MensajeClase"] = "alert-success";
                TempData["Mensaje"] = "Cambios guardados correctamente.";
                return Json(true);
            }
            TempData["MensajeClase"] = "alert-danger";
            TempData["Mensaje"] = "Hubo un error al intentar guardar los cambios, porfavor intente nuevamente.";
            return Json(false);
        }


        // GET: TipoProducto/Delete/5
        [VerificarPerfil((int)Utilities.Utilities.Modulos.TipoProducto, "Borrar")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoProducto tipoProducto = db.TipoProducto.Find(id);
            if (tipoProducto == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Delete", tipoProducto);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [VerificarPerfil((int)Utilities.Utilities.Modulos.TipoProducto, "Borrar")]
        public ActionResult Delete(TipoProducto tipoProducto)
        {
            if (ModelState.IsValid)
            {
                TipoProducto TipoProductoE = db.TipoProducto.Find(tipoProducto.TPRId);
                if (TipoProductoE == null)
                {
                    TempData["MensajeClase"] = "alert-danger";
                    TempData["Mensaje"] = "Tipo no encontrado";
                    return Json(false);
                }

                if (db.Articulo.Where(a => a.TipoProducto == TipoProductoE.TPRId.ToString()).Count() > 0 )
                {
                    TipoProductoE.Estatus = Utilities.Utilities.Estatus()["Inactivo"];
                    TipoProductoE.MFechaHora = DateTime.Now;
                    TipoProductoE.MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name;
                    db.Entry(TipoProductoE).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    Metodos.RegistrarLog(new Log
                    {
                        Modulo = "TipoProducto",
                        Accion = "Actualizar",
                        Detalle = "TipoProducto modificado: " + TipoProductoE.Producto + " " + TipoProductoE.Estatus
                    });
                    TempData["MensajeClase"] = "alert-danger";
                    TempData["Mensaje"] = "El registro no puede ser eliminado, ya que se encuentra asociado a otros registros";
                    return Json(false);
                }

                db.TipoProducto.Remove(TipoProductoE);
                db.SaveChanges();
                Metodos.RegistrarLog(new Log
                {
                    Modulo = "TipoProducto",
                    Accion = "Borrar",
                    Detalle = "TipoProducto borrado: " + TipoProductoE.Producto
                });
                TempData["MensajeClase"] = "alert-success";
                TempData["Mensaje"] = "Registro eliminado con éxito.";
                return Json(true);
            }
            TempData["MensajeClase"] = "alert-danger";
            TempData["Mensaje"] = "Error al intentar eliminar";
            return Json(false);

        }
    }
}