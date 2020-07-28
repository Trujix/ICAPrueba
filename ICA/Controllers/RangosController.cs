using ICA.Models;
using ICA.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace ICA.Controllers
{
    public class RangosController : Controller
    {
        private ICAEntities db = new ICAEntities();

        // GET: Rangos
        //[VerificarPerfil((int)Utilities.Utilities.Modulos.Articulos, "Leer")]
        public ActionResult Index()
        {
            dynamic RangosData = new ExpandoObject();

            var RangosTorqueTabla = new List<RangoCriterioTabla>();
            var Criterios = db.Criterio.ToList();
            var RangosCriterio = db.RangoCriterio.ToList();
            foreach (var RangoC in RangosCriterio)
            {
                var CriterioV = (db.Criterio.Where(v => v.CRTId == RangoC.CRTId).FirstOrDefault());
                var VarVal = (db.VARValor.Where(v => /*v.VARClave == "SABORFRA" && */v.VAVClave == RangoC.Base).FirstOrDefault());
                RangosTorqueTabla.Add(new RangoCriterioTabla()
                {
                    RNGId = RangoC.RNGId,
                    NombreCriterio = CriterioV.Nombre,
                    CRTId = CriterioV.CRTId,
                    ClaveBase = RangoC.Base,
                    NombreBase = VarVal.Descripcion,
                    ValorMin = RangoC.ValorMin,
                    ValorMax = RangoC.ValorMax,
                });
            }

            RangosData.RangoCriterio = RangosTorqueTabla;
            RangosData.VariacionPermitida = db.VariacionPermitida.ToList();
            RangosData.RangosTorque = db.RangoTorqueVacio.ToList();
            return View(RangosData);
        }

        // ::::::::::::::: RANGO CRITERIO :::::::::::::::
        // EDICION DE REGISTROS
        //[VerificarPerfil((int)Utilities.Utilities.Modulos.Articulos, "Actualizar")]
        public ActionResult EditViewRC(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var rangocriterio = db.RangoCriterio.Find(id);
            if (rangocriterio == null)
            {
                return HttpNotFound();
            }
            var Criterios = db.Criterio.ToList();
            var CriterioV = (db.Criterio.Where(v => v.CRTId == rangocriterio.CRTId).FirstOrDefault());
            var VarVal = (db.VARValor.Where(v => /*v.VARClave == "SABORFRA" && */v.VAVClave == rangocriterio.Base).FirstOrDefault());
            RangoCriterioTabla RangoCriterioParams = new RangoCriterioTabla()
            {
                RNGId = rangocriterio.RNGId,
                NombreCriterio = CriterioV.Nombre,
                CRTId = CriterioV.CRTId,
                ClaveBase = rangocriterio.Base,
                NombreBase = VarVal.Descripcion,
                ValorMin = rangocriterio.ValorMin,
                ValorMax = rangocriterio.ValorMax,
            };
            return PartialView("_EditRC", RangoCriterioParams);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[VerificarPerfil((int)Utilities.Utilities.Modulos.Articulos, "Actualizar")]
        public ActionResult EditRC(RangoCriterio rangocriterio)
        {

            if (ModelState.IsValid)
            {
                rangocriterio.MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name;
                rangocriterio.MFechaHora = DateTime.Now;
                db.Entry(rangocriterio).State = EntityState.Modified;
                db.SaveChanges();
                Metodos.RegistrarLog(new Log
                {
                    Modulo = "RangoCriterio",
                    Accion = "Actualizar",
                    Detalle = "Rango Criterio modificado: RNGId:" + rangocriterio.RNGId,
                });
                TempData["MensajeClase"] = "alert-success";
                TempData["Mensaje"] = "Cambios guardados correctamente.";
                return Json(true);
            }
            TempData["MensajeClase"] = "alert-danger";
            TempData["Mensaje"] = "Hubo un error al intentar guardar los cambios, porfavor intente nuevamente.";
            return Json(false);
        }

        // ::::::::::::::: VARIACION PERMITIDA :::::::::::::::
        // CREACION DE REGISTROS
        //[VerificarPerfil((int)Utilities.Utilities.Modulos.Articulos, "Insertar")]
        public ActionResult CreateViewVP()
        {
            ViewBag.EstatusList = Helpers.ComboLista("TVARIACION");
            return PartialView("_CreateVP");
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[VerificarPerfil((int)Utilities.Utilities.Modulos.Articulos, "Insertar")]
        public ActionResult SaveVP(VariacionPermitida variacion)
        {

            if (ModelState.IsValid)
            {
                variacion.MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name;
                variacion.MFechaHora = DateTime.Now;
                db.VariacionPermitida.Add(variacion);
                db.SaveChanges();
                Metodos.RegistrarLog(new Log
                {
                    Modulo = "VariacionPermitida",
                    Accion = "Insertar",
                    Detalle = "Variacion insertada: V-MINIMO:" + variacion.ValMin + " V-MAXIMO:" + variacion.ValMax
                });
                var TablaVariaciones = new List<object[]>();
                var ListaVariaciones = db.VariacionPermitida.ToList();
                foreach(var Variacion in ListaVariaciones)
                {
                    TablaVariaciones.Add(new object[]
                    {
                        Variacion.ValMin,
                        Variacion.ValMax,
                        Variacion.VariacionP,
                        (db.VARValor.Where(v => v.VARClave == "TVARIACION" && v.VAVClave == Variacion.Tipo).FirstOrDefault()).Descripcion,
                        "<a class='modal-link btn btn-sm btn-primary' href='/Rangos/EditViewVP/" + Variacion.VAPId + "'>Editar</a>",
                    });
                }
                var respuesta = new Dictionary<string, object>()
                {
                    { "Correcto", true },
                    { "TablaVariaciones", TablaVariaciones },
                };
                return Json(respuesta);
            }
            return Json(false);
        }

        // EDICION DE REGISTROS
        //[VerificarPerfil((int)Utilities.Utilities.Modulos.Articulos, "Actualizar")]
        public ActionResult EditViewVP(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VariacionPermitida variaciones = db.VariacionPermitida.Find(id);
            if (variaciones == null)
            {
                return HttpNotFound();
            }
            ViewBag.EstatusList = Helpers.ComboLista("TVARIACION");
            return PartialView("_EditVP", variaciones);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[VerificarPerfil((int)Utilities.Utilities.Modulos.Articulos, "Actualizar")]
        public ActionResult EditVP(VariacionPermitida variacion)
        {

            if (ModelState.IsValid)
            {
                variacion.MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name;
                variacion.MFechaHora = DateTime.Now;
                db.Entry(variacion).State = EntityState.Modified;
                db.SaveChanges();
                Metodos.RegistrarLog(new Log
                {
                    Modulo = "VariacionPermitida",
                    Accion = "Actualizar",
                    Detalle = "Variacion modificado: V-MINIMO:" + variacion.ValMin + " V-MAXIMO:" + variacion.ValMax
                });
                var TablaVariaciones = new List<object[]>();
                var ListaVariaciones = db.VariacionPermitida.ToList();
                foreach (var Variacion in ListaVariaciones)
                {
                    TablaVariaciones.Add(new object[]
                    {
                        Variacion.ValMin,
                        Variacion.ValMax,
                        Variacion.VariacionP,
                        (db.VARValor.Where(v => v.VARClave == "TVARIACION" && v.VAVClave == Variacion.Tipo).FirstOrDefault()).Descripcion,
                        "<a class='modal-link btn btn-sm btn-primary' href='/Rangos/EditViewVP/" + Variacion.VAPId + "'>Editar</a>",
                    });
                }
                var respuesta = new Dictionary<string, object>()
                {
                    { "Correcto", true },
                    { "TablaVariaciones", TablaVariaciones },
                };
                return Json(respuesta);
            }
            return Json(false);
        }

        // ::::::::::::::: RANGO TORQUE VACIO :::::::::::::::
        // CREACION DE REGISTROS
        //[VerificarPerfil((int)Utilities.Utilities.Modulos.Articulos, "Insertar")]
        public ActionResult CreateViewRTV()
        {
            ViewBag.EstatusList = Helpers.ComboLista("ESTATUS");
            ViewBag.BasesList = Helpers.ComboLista("BASEFRA");
            return PartialView("_CreateRTV");
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[VerificarPerfil((int)Utilities.Utilities.Modulos.Articulos, "Insertar")]
        public ActionResult SaveRTV(RangoTorqueVacio rangotorque)
        {

            if (ModelState.IsValid)
            {
                rangotorque.MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name;
                rangotorque.MFechaHora = DateTime.Now;
                db.RangoTorqueVacio.Add(rangotorque);
                db.SaveChanges();
                Metodos.RegistrarLog(new Log
                {
                    Modulo = "RangoTorqueVacio",
                    Accion = "Insertar",
                    Detalle = "Rango Torque insertado: BASE:" + rangotorque.Base + " VALOR TORQUE:" + rangotorque.ValorTorque
                });
                var TablaRangosV = new List<object[]>();
                var ListaRangosV = db.RangoTorqueVacio.ToList();
                foreach (var Variacion in ListaRangosV)
                {
                    TablaRangosV.Add(new object[]
                    {
                        (db.VARValor.Where(v => v.VARClave == "BASEFRA" && v.VAVClave == Variacion.Base).FirstOrDefault()).Descripcion,
                        Variacion.ValorTorque,
                        Variacion.TamGrano,
                        Variacion.Resultado,
                        (db.VARValor.Where(v => v.VARClave == "ESTATUS" && v.VAVClave == Variacion.Estatus).FirstOrDefault()).Descripcion,
                        "<a class='modal-link btn btn-sm btn-primary' href='/Rangos/EditViewRTV/" + Variacion.RTVId + "'>Editar</a>",
                    });
                }
                var respuesta = new Dictionary<string, object>()
                {
                    { "Correcto", true },
                    { "TablaRangosV", TablaRangosV },
                };
                return Json(respuesta);
            }
            return Json(false);
        }

        // EDICION DE REGISTROS
        //[VerificarPerfil((int)Utilities.Utilities.Modulos.Articulos, "Actualizar")]
        public ActionResult EditViewRTV(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RangoTorqueVacio variaciones = db.RangoTorqueVacio.Find(id);
            if (variaciones == null)
            {
                return HttpNotFound();
            }
            ViewBag.EstatusList = Helpers.ComboLista("ESTATUS");
            ViewBag.BasesList = Helpers.ComboLista("BASEFRA");
            return PartialView("_EditRTV", variaciones);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[VerificarPerfil((int)Utilities.Utilities.Modulos.Articulos, "Actualizar")]
        public ActionResult EditRTV(RangoTorqueVacio rangotorque)
        {

            if (ModelState.IsValid)
            {
                rangotorque.MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name;
                rangotorque.MFechaHora = DateTime.Now;
                db.Entry(rangotorque).State = EntityState.Modified;
                db.SaveChanges();
                Metodos.RegistrarLog(new Log
                {
                    Modulo = "RangoTorqueVacio",
                    Accion = "Actualizar",
                    Detalle = "Rango Torque modificado: BASE:" + rangotorque.Base + " VALOR TORQUE:" + rangotorque.ValorTorque
                });
                var TablaRangosV = new List<object[]>();
                var ListaRangosV = db.RangoTorqueVacio.ToList();
                foreach (var Variacion in ListaRangosV)
                {
                    TablaRangosV.Add(new object[]
                    {
                        (db.VARValor.Where(v => v.VARClave == "BASEFRA" && v.VAVClave == Variacion.Base).FirstOrDefault()).Descripcion,
                        Variacion.ValorTorque,
                        Variacion.TamGrano,
                        Variacion.Resultado,
                        (db.VARValor.Where(v => v.VARClave == "ESTATUS" && v.VAVClave == Variacion.Estatus).FirstOrDefault()).Descripcion,
                        "<a class='modal-link btn btn-sm btn-primary' href='/Rangos/EditViewRTV/" + Variacion.RTVId + "'>Editar</a>",
                    });
                }
                var respuesta = new Dictionary<string, object>()
                {
                    { "Correcto", true },
                    { "TablaRangosV", TablaRangosV },
                };
                return Json(respuesta);
            }
            return Json(false);
        }
    }
}