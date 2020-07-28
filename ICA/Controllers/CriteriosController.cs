using System;
using System.Collections.Generic;
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
    public class CriteriosController : Controller
    {
        private ICAEntities db = new ICAEntities();


        // GET: Criterios
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Criterios, "Leer")]
        public ActionResult Index()
        {
            return View(db.Criterio.ToList());
        }

        // GET: Criterios/Create
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Criterios, "Insertar")]
        public ActionResult Create()
        {
            ViewBag.EstatusList = Helpers.ComboLista("ESTATUS");
            ViewBag.DecisionList = Helpers.ComboDecision();
            ViewBag.ClasificacionList = Helpers.ComboLista("CLASIFCRIT");
            ViewBag.TRespuestaList = Helpers.ComboLista("TRESPUESTA");
            ViewBag.TEquipoList = Helpers.ComboLista("TEQUIPO");
            ViewBag.ValoresRef = db.ValorReferencia.ToList();
            ViewBag.CEquipoProList = Helpers.ComboLista("CEQUIPOPRO");
            ViewBag.OperacionList = Helpers.ComboLista("OP_GLOBAL");

            return PartialView("_Create");
        }

        // POST: Criterios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Criterios, "Insertar")]
        public ActionResult Create(Criterio criterio)
        {

            if (ModelState.IsValid)
            {
                if((criterio.Condicionante == true && criterio.CRTId_Cond != null) || criterio.Condicionante == false)
                {
                    if (criterio.Condicionante == false)
                        criterio.CRTId_Cond = null;
                    criterio.MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name;
                    criterio.MFechaHora = DateTime.Now;
                    db.Criterio.Add(criterio);
                    db.SaveChanges();
                    Metodos.RegistrarLog(new Log
                    {
                        Modulo = "Criterios",
                        Accion = "Insertar",
                        Detalle = "Criterio insertado: " + criterio.Nombre
                    });
                    if (!string.IsNullOrEmpty(criterio.ValorReferencia))
                    {
                        ValorReferencia valor = new ValorReferencia
                        {
                            VARClave = criterio.ValorReferencia,
                            Modificable = 1,
                            Descripcion = criterio.ValorReferencia,
                            MFechaHora = DateTime.Now,
                            MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name
                        };

                        db.ValorReferencia.Add(valor);
                        db.SaveChanges();
                        Metodos.RegistrarLog(new Log
                        {
                            Modulo = "ValorReferencia",
                            Accion = "Insertar",
                            Detalle = "ValorCreado: " + valor.VARClave + " " + valor.Descripcion
                        });
                    }
                    TempData["MensajeClase"] = "alert-success";
                    TempData["Mensaje"] = "Cambios guardados correctamente.";
                    return Json(true);
                }
                else
                {
                    TempData["MensajeClase"] = "alert-danger";
                    TempData["Mensaje"] = "Seleccione el criterio condicionante";
                    return Json(false);
                }
                
            }
            TempData["MensajeClase"] = "alert-danger";
            TempData["Mensaje"] = "Hubo un error al intentar guardar los cambios, porfavor intente nuevamente.";
            return Json(false);
        }

        [HttpGet]
        public ActionResult Search(string query, string TipoEquipo, string EquipoProducto)
        {
            try
            {
                var criterios = db.Criterio.Where(c => c.Nombre.Contains(query)).Where(c => c.Estatus.Equals("A") && c.TipoRespuesta == "S" && c.TipoEquipo == TipoEquipo && c.EquipoProducto == EquipoProducto).Select(c => new {c.CRTId , c.Nombre }).ToList();
                return Json(criterios, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Criterios/Edit/5
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Criterios, "Actualizar")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Criterio criterio = db.Criterio.Find(id);
            if (criterio == null)
            {
                return HttpNotFound();
            }
            ViewBag.EstatusList = Helpers.ComboLista("ESTATUS");
            ViewBag.DecisionList = Helpers.ComboDecision();
            ViewBag.ClasificacionList = Helpers.ComboLista("CLASIFCRIT");
            ViewBag.TRespuestaList = Helpers.ComboLista("TRESPUESTA");
            ViewBag.TEquipoList = Helpers.ComboLista("TEQUIPO");
            ViewBag.CEquipoProList = Helpers.ComboLista("CEQUIPOPRO");
            ViewBag.OperacionList = Helpers.ComboLista("OP_GLOBAL");
            ViewBag.ValoresRef = db.ValorReferencia.ToList();
            if (string.IsNullOrEmpty(criterio.ValorReferencia))
                ViewBag.ValorReferencia = "disabled";
            else
                ViewBag.ValorReferencia = "required";

            if (criterio.CRTId_Cond == null)
            {
                ViewBag.CondicionanteClass = "d-none";
                ViewBag.CondicionanteD = "";
            }
            else
            {
                ViewBag.CondicionanteClass = "";
                ViewBag.CondicionanteD = db.Criterio.Where(c => c.CRTId == criterio.CRTId_Cond).Select(c => c.Nombre).FirstOrDefault();
            }
                

            return PartialView("_Edit", criterio);
        }

        // POST: Perfiles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Criterios, "Actualizar")]
        public ActionResult Edit(Criterio criterio)
        {
            if (ModelState.IsValid)
            {
                if ((criterio.Condicionante == true && criterio.CRTId_Cond != null) || criterio.Condicionante == false)
                {
                    if (criterio.Condicionante == false)
                        criterio.CRTId_Cond = null;
                    criterio.MFechaHora = DateTime.Now;
                    criterio.MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name;
                    db.Entry(criterio).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    Metodos.RegistrarLog(new Log
                    {
                        Modulo = "Criterios",
                        Accion = "Actualizar",
                        Detalle = "Criterio modificado: " + criterio.Nombre + " " + criterio.Estatus
                    });
                    if (!string.IsNullOrEmpty(criterio.ValorReferencia))
                    {
                        var existe = db.ValorReferencia.Where(va => va.VARClave.Equals(criterio.ValorReferencia)).Count();
                        if(existe == 0)
                        {
                             ValorReferencia valor = new ValorReferencia
                                {
                                    VARClave = criterio.ValorReferencia,
                                    Modificable = 1,
                                    MFechaHora = DateTime.Now,
                                    MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name
                                };

                                db.ValorReferencia.Add(valor);
                                db.SaveChanges();
                                Metodos.RegistrarLog(new Log
                                {
                                    Modulo = "ValorReferencia",
                                    Accion = "Insertar",
                                    Detalle = "ValorCreado: " + valor.VARClave + " " + valor.Descripcion
                                });
                        }
                   
                    }
                    TempData["MensajeClase"] = "alert-success";
                    TempData["Mensaje"] = "Cambios guardados correctamente.";
                    return Json(true);
                }
                else
                {
                    TempData["MensajeClase"] = "alert-danger";
                    TempData["Mensaje"] = "Seleccione el criterio condicionante";
                    return Json(false);
                }
            }
            TempData["MensajeClase"] = "alert-danger";
            TempData["Mensaje"] = "Hubo un error al intentar guardar los cambios, porfavor intente nuevamente.";
            return Json(false);
        }

        // GET: Criterios/Delete/5
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Criterios, "Borrar")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Criterio criterio = db.Criterio.Find(id);
            if (criterio == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Delete", criterio);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Criterios, "Borrar")]
        public ActionResult Delete(Criterio criterio)
        {
            if (ModelState.IsValid)
            {
                Criterio criterioE = db.Criterio.Find(criterio.CRTId);
                if (criterioE == null)
                {
                    TempData["MensajeClase"] = "alert-danger";
                    TempData["Mensaje"] = "Equipo no encontrado";
                    return Json(false);
                }

                if (criterioE.EquipoCriterio.Count > 0 || criterioE.ArticuloCriterio.Count > 0)
                {

                    criterioE.Estatus = Utilities.Utilities.Estatus()["Inactivo"];
                    criterioE.MFechaHora = DateTime.Now;
                    criterioE.MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name;
                    db.Entry(criterioE).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    Metodos.RegistrarLog(new Log
                    {
                        Modulo = "Criterios",
                        Accion = "Actualizar",
                        Detalle = "Criterio modificado: " + criterio.Nombre + " " + criterio.Estatus
                    });
                    TempData["MensajeClase"] = "alert-danger";
                    TempData["Mensaje"] = "El registro no puede ser eliminado, ya que se encuentra asociado a otros registros";
                    return Json(false);
                }

                db.Criterio.Remove(criterioE);
                db.SaveChanges();
                Metodos.RegistrarLog(new Log
                {
                    Modulo = "Criterios",
                    Accion = "Borrar",
                    Detalle = "Criterio borrado: " + criterioE.Nombre
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