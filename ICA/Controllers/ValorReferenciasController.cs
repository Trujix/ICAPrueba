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
    public class ValorReferenciasController : Controller
    {
        private ICAEntities db = new ICAEntities();

        // GET: ValorReferencias
        [VerificarPerfil((int)Utilities.Utilities.Modulos.ValorReferencia, "Leer")]
        public ActionResult Index()
        {
            return View(db.ValorReferencia.ToList());
        }

        // GET: ValorReferencias/Details/5
        [VerificarPerfil((int)Utilities.Utilities.Modulos.ValorReferencia, "Leer")]
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ValorReferencia valorReferencia = db.ValorReferencia.Find(id);
            if (valorReferencia == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Valores", valorReferencia);
        }

        // GET: ValorReferencias/Create
        [VerificarPerfil((int)Utilities.Utilities.Modulos.ValorReferencia, "Insertar")]
        public ActionResult Create(string id)
        {
            ViewBag.EstatusList = Helpers.ComboLista("ESTATUS");

            ValorReferencia valorReferencia = db.ValorReferencia.Find(id);
            return PartialView("_CrearValor", valorReferencia);
        }

        // POST: ValorReferencias/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [VerificarPerfil((int)Utilities.Utilities.Modulos.ValorReferencia, "Insertar")]
        public ActionResult Create(string id, [Bind(Include = "VARClave,VAVClave,Descripcion,Estatus,MFechaHora,MUsuarioId")] VARValor valor)
        {
            ValorReferencia valorReferencia = db.ValorReferencia.Find(id);
            valor.MFechaHora = DateTime.Now;
            valor.MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name;
            if (ModelState.IsValid)
            {
                db.VARValor.Add(valor);
                db.SaveChanges();
                Metodos.RegistrarLog(new Log
                {
                    Modulo = "ValorReferencia",
                    Accion = "Insertar",
                    Detalle = "ValorCreado: " + valor.VARClave + " " + valor.Descripcion
                });
                TempData["MensajeClase"] = "alert-success";
                TempData["Mensaje"] = "Cambios guardados correctamente.";
            }
            else
            {
                TempData["MensajeClase"] = "alert-danger";
                TempData["Mensaje"] = "Hubo un error al intentar guardar los cambios, porfavor intente nuevamente.";
            }

            return PartialView("_Valores", valorReferencia);
        }

        // GET: ValorReferencias/Edit/5
        [VerificarPerfil((int)Utilities.Utilities.Modulos.ValorReferencia, "Actualizar")]
        public ActionResult Edit(string VARClave, string VAVClave)
        {
            if (VARClave == null || VAVClave == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VARValor valor = db.VARValor.Where(v => v.VAVClave == VAVClave && v.VARClave == VARClave).FirstOrDefault();
            if (valor == null)
            {
                return HttpNotFound();
            }
            ViewBag.EstatusList = Helpers.ComboLista("ESTATUS");

            return PartialView("_EditarValor", valor);
        }

        // POST: ValorReferencias/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [VerificarPerfil((int)Utilities.Utilities.Modulos.ValorReferencia, "Actualizar")]
        public ActionResult Edit(string id, [Bind(Include = "VARClave,VAVClave,Descripcion,Estatus,MFechaHora,MUsuarioId")] VARValor valor)
        {
            valor.MFechaHora = DateTime.Now;
            valor.MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name;
            if (ModelState.IsValid)
            {
                db.Entry(valor).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                Metodos.RegistrarLog(new Log
                {
                    Modulo = "ValorReferencia",
                    Accion = "Actualizar",
                    Detalle = "ValorModificado: " + valor.VARClave + " " + valor.Descripcion
                });
                TempData["MensajeClase"] = "alert-success";
                TempData["Mensaje"] = "Cambios guardados correctamente.";
            }
            else
            {
                TempData["MensajeClase"] = "alert-danger";
                TempData["Mensaje"] = "Hubo un error al intentar guardar los cambios, porfavor intente nuevamente.";
            }
            ValorReferencia valorReferencia = db.ValorReferencia.Find(id);
            return PartialView("_Valores", valorReferencia);
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