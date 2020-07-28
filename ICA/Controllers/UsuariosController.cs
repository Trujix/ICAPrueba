using ICA.Models;
using ICA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Data.Entity;

namespace ICA.Controllers
{
    [Autorizar]
    public class UsuariosController : Controller
    {
        private ICAEntities db = new ICAEntities();

        // GET: Usuarios
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Usuarios, "Leer")]
        public ActionResult Index()
        {
            return View(db.Usuario.ToList());
        }

        // GET: Usuarios/Create
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Usuarios, "Insertar")]
        public ActionResult Create()
        {
            ViewBag.EstatusList = Helpers.ComboLista("ESTATUS");
            ICA.ServiceReferenceUsuarios.LoginClient servicio = new ICA.ServiceReferenceUsuarios.LoginClient();
            var usuarios = servicio.ObtenerTodosUsuarios();
            ViewBag.Usuarios = usuarios;
            List<Perfil> perfiles = db.Perfil.Where(m => m.Estatus == "A").ToList();
            ViewBag.Perfiles = perfiles.Select(i => new SelectListItem
            {
                Value = i.PERId.ToString(),
                Text = i.Descripcion
            }).ToList();

            return PartialView("_Create");
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Usuarios, "Insertar")]
        public ActionResult Create(Usuario usuario)
        {
            usuario.UsuarioPerfil.First().MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name;
            usuario.UsuarioPerfil.First().MFechaHora = DateTime.Now;
            usuario.MFechaHora = DateTime.Now;
            usuario.MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name;

            if (ModelState.IsValid)
            {

                if (db.Usuario.Where(u => u.Usuario1.Equals(usuario.Usuario1)).Count() == 0)
                {
                    db.Usuario.Add(usuario);
                    db.SaveChanges();
                    Metodos.RegistrarLog(new Log
                    {
                        Modulo = "Usuarios",
                        Accion = "Insertar",
                        Detalle = "Usuario insertado: " + usuario.Usuario1
                    });
                    TempData["MensajeClase"] = "alert-success";
                    TempData["Mensaje"] = "Cambios guardados correctamente.";
                    return Json(true);
                }
                else
                {
                    TempData["MensajeClase"] = "alert-danger";
                    TempData["Mensaje"] = "El usuario que intenta registrar ya se encuentra registrado.";
                    return Json(false);
                }
                
            }
            TempData["MensajeClase"] = "alert-danger";
            TempData["Mensaje"] = "Hubo un error al intentar guardar los cambios, porfavor intente nuevamente.";
            return Json(false);
        }

        // GET: Usuarios/Edit/5
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Usuarios, "Actualizar")]
        public ActionResult Edit(string id)
        {
            Usuario usuario = db.Usuario.Find(id);
            if (usuario == null)
            {
                return HttpNotFound();
            }
            ViewBag.EstatusList = Helpers.ComboLista("ESTATUS");
            List<Perfil> perfiles = db.Perfil.Where(m => m.Estatus == "A").ToList();
            ViewBag.Perfiles = perfiles.Select(i => new SelectListItem
            {
                Value = i.PERId.ToString(),
                Text = i.Descripcion
            }).ToList();

            return PartialView("_Edit", usuario);
        }

        // POST: Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Usuarios, "Actualizar")]
        public ActionResult Edit(Usuario usuario, UsuarioPerfil perfil)
        {
            usuario.MFechaHora = DateTime.Now;
            usuario.MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name;
            perfil.MFechaHora = DateTime.Now;
            perfil.MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name;
            perfil.Estatus = "A";
            if (ModelState.IsValid)
            {
                db.Entry(usuario).State = EntityState.Modified;
                db.SaveChanges();
                var usuper = db.UsuarioPerfil.Where(up => up.PERId == perfil.PERId && up.Usuario == perfil.Usuario).Any();
                if (usuper)
                {
                    db.Entry(perfil).State = EntityState.Modified;
                }
                else
                {
                    db.Entry(db.UsuarioPerfil.Where(up => up.Usuario == perfil.Usuario).First()).State = EntityState.Deleted;
                    db.UsuarioPerfil.Add(perfil);
                }
                db.SaveChanges();
                Metodos.RegistrarLog(new Log
                {
                    Modulo = "Usuarios",
                    Accion = "Actualizar",
                    Detalle = "Usuario modificado: " + usuario.Usuario1
                });
                TempData["MensajeClase"] = "alert-success";
                TempData["Mensaje"] = "Cambios guardados correctamente.";
                return Json(true);
            }
            TempData["MensajeClase"] = "alert-danger";
            TempData["Mensaje"] = "Hubo un error al intentar guardar los cambios, porfavor intente nuevamente.";
            return Json(true);
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