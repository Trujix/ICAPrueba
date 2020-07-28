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
    public class AsignacionCriteriosController : Controller
    {
        private ICAEntities db = new ICAEntities();
        // GET: AsignacionCriterios
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Asignacion, "Leer")]
        public ActionResult Index()
        {
            ViewBag.ClasificacionList = Helpers.ComboLista("CLASIFCRIT");
            ViewBag.Equipos = db.Equipo.Where(e => e.Estatus.Equals("A")).ToList();
            ViewBag.TipoProdList =   db.TipoProducto.Where(t => t.Estatus == "A").Select(i => new SelectListItem
            {
                Value = i.TPRId.ToString(),
                Text = i.Producto
            }).ToList();
            ViewBag.TipoEquipo = Helpers.ComboLista("TEQUIPO");
            return View();
        }

        [HttpGet]
        public ActionResult ObtenerProductos(string tprod)
        {
            if (tprod == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var articulos = db.Articulo.Where(a => a.TipoProducto.Equals(tprod)).Where(a => a.Estatus.Equals("A")).Select(a => new { a.ARTId, a.Nombre }).ToList();
            if (articulos == null)
            {
                return HttpNotFound();
            }

            return Json(articulos, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ObtenerCriteriosEquipos(int EQPId)
        {
            var criteriosEquipo = db.EquipoCriterio.Where(c => c.EQPId.Equals(EQPId)).Where(c => c.Estatus.Equals("A")).OrderBy(c => c.Orden).Select(a => new { a.CRTId, a.Criterio.Nombre, a.Orden });

            return Json(criteriosEquipo, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ObtenerCriteriosEquiposClasificacion(int EQPId, string clas, string tipoEquipo)
        {
            var criteriosEquipo = db.EquipoCriterio.Where(c => c.EQPId.Equals(EQPId)).Select(c => c.CRTId);

            var cri = db.Criterio.Where(c => !criteriosEquipo.Contains(c.CRTId)).Where(c => c.Clasificacion.Equals(clas)).Where(c => c.TipoEquipo.Equals(tipoEquipo)).Where(c => c.Estatus.Equals("A")).Select(a => new { a.CRTId, a.Nombre }).ToList();

            return Json(cri, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ObtenerCriteriosArt(int ARTId)
        {
            var criteriosArt = db.ArticuloCriterio.Where(c => c.ARTId.Equals(ARTId)).Where(c => c.Estatus.Equals("A")).OrderBy(c => c.Orden).Select(a => new { a.CRTId, a.Criterio.Nombre, a.Orden }).ToList();

            return Json(criteriosArt, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ObtenerCriteriosArtClasificacion(int ARTId, string clas, string tipoEquipo)
        {
            var criteriosArt = db.ArticuloCriterio.Where(c => c.ARTId.Equals(ARTId)).Select(c => c.CRTId);

            var cri = db.Criterio.Where(c => !criteriosArt.Contains(c.CRTId)).Where(c => c.Clasificacion.Equals(clas)).Where(c => c.TipoEquipo.Equals(tipoEquipo)).Where(c => c.Estatus.Equals("A")).Select(a => new { a.CRTId, a.Nombre }).ToList();

            return Json(cri, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Asignacion, "Borrar")]
        public ActionResult EliminarCriterioEquipo(int EQPId, int CRTId)
        {
            EquipoCriterio criterio = db.EquipoCriterio.Find(EQPId,CRTId);
            if (criterio == null)
            {
                TempData["MensajeClase"] = "alert-danger";
                TempData["Mensaje"] = "Criterio no encontrado";
                return Json(false);
            }
            var evaluaciones = db.EvEquipo.Where(e => e.EQPId.Equals(EQPId)).Where(e => e.CRTId.Equals(CRTId));
            if (evaluaciones.Count() > 0)
            {
                criterio.Estatus = Utilities.Utilities.Estatus()["Inactivo"];
                criterio.MFechaHora = DateTime.Now;
                criterio.MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name;
                db.Entry(criterio).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                Metodos.RegistrarLog(new Log
                {
                    Modulo = "Asignacion Criterios Equipos",
                    Accion = "Actualizar",
                    Detalle = "Criterio modificado: " + criterio.EQPId + " " + criterio.CRTId + " " + criterio.Estatus
                });
                TempData["MensajeClase"] = "alert-danger";
                TempData["Mensaje"] = "El registro no puede ser eliminado, ya que se encuentra asociado a otros registros";
                return Json(false);
            }

            db.EquipoCriterio.Remove(criterio);
            db.SaveChanges();
            Metodos.RegistrarLog(new Log
            {
                Modulo = "Asignacion Criterios Equipos",
                Accion = "Borrar",
                Detalle = "Criterio borrado: " + criterio.EQPId + " " + criterio.CRTId
            });
            TempData["MensajeClase"] = "alert-success";
            TempData["Mensaje"] = "Registro eliminado con éxito.";
            return Json(true);
        }

        [HttpPost]
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Asignacion, "Borrar")]
        public ActionResult EliminarCriterioArt(int ARTId, int CRTId)
        {
            ArticuloCriterio criterio = db.ArticuloCriterio.Find(ARTId, CRTId);
            if (criterio == null)
            {
                TempData["MensajeClase"] = "alert-danger";
                TempData["Mensaje"] = "Criterio no encontrado";
                return Json(false);
            }
            var evaluaciones = db.EvArticulo.Where(e => e.ARTId.Equals(ARTId)).Where(e => e.CRTId.Equals(CRTId));
            if (evaluaciones.Count() > 0)
            {
                criterio.Estatus = Utilities.Utilities.Estatus()["Inactivo"];
                criterio.MFechaHora = DateTime.Now;
                criterio.MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name;
                db.Entry(criterio).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                Metodos.RegistrarLog(new Log
                {
                    Modulo = "Asignacion Criterios Articulos",
                    Accion = "Actualizar",
                    Detalle = "Criterio modificado: " + criterio.ARTId + " " + criterio.CRTId + " " + criterio.Estatus
                });
                TempData["MensajeClase"] = "alert-danger";
                TempData["Mensaje"] = "El registro no puede ser eliminado, ya que se encuentra asociado a otros registros";
                return Json(false);
            }

            db.ArticuloCriterio.Remove(criterio);
            db.SaveChanges();
            Metodos.RegistrarLog(new Log
            {
                Modulo = "Asignacion Criterios Articulos",
                Accion = "Borrar",
                Detalle = "Criterio borrado: " + criterio.ARTId + " " + criterio.CRTId
            });
            TempData["MensajeClase"] = "alert-success";
            TempData["Mensaje"] = "Registro eliminado con éxito.";
            return Json(true);
        }


        [HttpPost]
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Asignacion, "Insertar")]
        public ActionResult AgregarCriterioArt(int ARTId, int[] Criterios)
        {
            foreach (var CRTId in Criterios)
            {
                var index = db.ArticuloCriterio.Where(c => c.ARTId.Equals(ARTId)).Where(c => c.Estatus.Equals("A")).OrderByDescending(c => c.Orden).Select(a => a.Orden).FirstOrDefault();

                index++;    
                ArticuloCriterio cr = new ArticuloCriterio
                {
                    ARTId = ARTId,
                    CRTId = CRTId,
                    Orden = index,
                    MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name,
                    MFechaHora = DateTime.Now,
                    Estatus = Utilities.Utilities.Estatus()["Activo"]
                };

                db.ArticuloCriterio.Add(cr);
                db.SaveChanges();
                Metodos.RegistrarLog(new Log
                {
                    Modulo = "Asignacion Criterios Articulos",
                    Accion = "Insertar",
                    Detalle = "Criterio insertado:" + cr.ARTId + " " + cr.CRTId
                });
            }
           
            TempData["MensajeClase"] = "alert-success";
            TempData["Mensaje"] = "Cambios guardados correctamente.";
            return Json(true);
        }

        [HttpPost]
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Asignacion, "Insertar")]
        public ActionResult AgregarCriterioEquipo(int EQPId, int[] Criterios)
        {
            foreach (var CRTId in Criterios)
            {
                var index = db.EquipoCriterio.Where(c => c.EQPId.Equals(EQPId)).Where(c => c.Estatus.Equals("A")).OrderByDescending(c => c.Orden).Select(a => a.Orden).FirstOrDefault();

                index++;

                EquipoCriterio cr = new EquipoCriterio
                {
                    EQPId = EQPId,
                    CRTId = CRTId,
                    Orden = index,
                    MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name,
                    MFechaHora = DateTime.Now,
                    Estatus = Utilities.Utilities.Estatus()["Activo"]
                };

                db.EquipoCriterio.Add(cr);
                db.SaveChanges();
                Metodos.RegistrarLog(new Log
                {
                    Modulo = "Asignacion Criterios Equipos",
                    Accion = "Insertar",
                    Detalle = "Criterio insertado:" + cr.EQPId + " " + cr.CRTId
                });
            }

            TempData["MensajeClase"] = "alert-success";
            TempData["Mensaje"] = "Cambios guardados correctamente.";
            return Json(true);
        }

        [HttpPost]
        public ActionResult subirCriterioEquipo(int EQPId, int CRTId)
        {
            var aSubir = db.EquipoCriterio.Where(c => c.EQPId.Equals(EQPId)).Where(c => c.CRTId.Equals(CRTId)).FirstOrDefault();

            if(aSubir.Orden > 1)
            {
                var aBajar = db.EquipoCriterio.Where(c => c.EQPId.Equals(EQPId)).Where( c => c.Orden == aSubir.Orden-1).FirstOrDefault();
                aBajar.Orden = aBajar.Orden+1;
                aSubir.Orden = aSubir.Orden-1;

                db.Entry(aBajar).State = System.Data.Entity.EntityState.Modified;
                db.Entry(aSubir).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }

            return Json(true);
        }

        [HttpPost]
        public ActionResult bajarCriterioEquipo(int EQPId, int CRTId)
        {
            var aBajar = db.EquipoCriterio.Where(c => c.EQPId.Equals(EQPId)).Where(c => c.CRTId.Equals(CRTId)).FirstOrDefault();

            if (aBajar.Orden < db.EquipoCriterio.Where(c => c.EQPId.Equals(EQPId)).Count())
            {
                var aSubir = db.EquipoCriterio.Where(c => c.EQPId.Equals(EQPId)).Where(c => c.Orden == aBajar.Orden + 1).FirstOrDefault();
                aBajar.Orden = aBajar.Orden+1;
                aSubir.Orden = aSubir.Orden-1;

                db.Entry(aBajar).State = System.Data.Entity.EntityState.Modified;
                db.Entry(aSubir).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }

            return Json(true);
        }

        [HttpPost]
        public ActionResult subirCriterioArt(int ARTId, int CRTId)
        {
            var aSubir = db.ArticuloCriterio.Where(c => c.ARTId.Equals(ARTId)).Where(c => c.CRTId.Equals(CRTId)).FirstOrDefault();

            if (aSubir.Orden > 1)
            {
                var aBajar = db.ArticuloCriterio.Where(c => c.ARTId.Equals(ARTId)).Where(c => c.Orden == aSubir.Orden - 1).FirstOrDefault();
                aBajar.Orden = aBajar.Orden+1;
                aSubir.Orden = aSubir.Orden-1;

                db.Entry(aBajar).State = System.Data.Entity.EntityState.Modified;
                db.Entry(aSubir).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }

            return Json(true);
        }

        [HttpPost]
        public ActionResult bajarCriterioArt(int ARTId, int CRTId)
        {
            var aBajar = db.ArticuloCriterio.Where(c => c.ARTId.Equals(ARTId)).Where(c => c.CRTId.Equals(CRTId)).FirstOrDefault();

            if (aBajar.Orden < db.ArticuloCriterio.Where(c => c.ARTId.Equals(ARTId)).Count())
            {
                var aSubir = db.ArticuloCriterio.Where(c => c.ARTId.Equals(ARTId)).Where(c => c.Orden == aBajar.Orden + 1).FirstOrDefault();
                aBajar.Orden = aBajar.Orden+1;
                aSubir.Orden = aSubir.Orden-1;

                db.Entry(aBajar).State = System.Data.Entity.EntityState.Modified;
                db.Entry(aSubir).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }

            return Json(true);
        }
    }
}