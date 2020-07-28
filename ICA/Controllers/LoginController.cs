using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ICA.Models;
using ICA.Utilities;

namespace ICA.Controllers
{
    public class LoginController : Controller
    {
        private ICAEntities db = new ICAEntities();

        // GET: Login
        public ActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Index(Usuario usuario)
        {   
            if (FormsAuthentication.Authenticate(usuario.Usuario1, usuario.Contrasena))
            {
                FormsAuthentication.SetAuthCookie(usuario.Usuario1, false);
                Metodos.RegistrarLog(new Log
                {
                    Modulo = "Acceso",
                    Accion = "Ingresar",
                    Detalle = "Acceso exitoso"
                });
                return RedirectToAction("Index", "Home");
            }
            var us = db.Usuario.Where(u => u.Usuario1 == usuario.Usuario1 && u.Estatus == "A").Any();
            if (us)
            {
               ICA.ServiceReferenceUsuarios.LoginClient servicio = new ICA.ServiceReferenceUsuarios.LoginClient();
                var respuesta = servicio.Autenticar(usuario.Usuario1, usuario.Contrasena);
                if (respuesta.EsValido)
                {
                    FormsAuthentication.SetAuthCookie(usuario.Usuario1, false);
                    Metodos.RegistrarLog(new Log
                    {
                        Modulo = "Acceso",
                        Accion = "Ingresar",
                        Detalle = "Acceso exitoso"
                    });
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.Mensaje = "El usuario no tiene permitido ingresar a la aplicación, favor de contactar al administrador.";
            ViewBag.Usuario = usuario.Usuario1;
            Metodos.RegistrarLog(new Log
            {
                Modulo = "Acceso",
                Accion = "Ingresar",
                Detalle = "Acceso fallido",
                MUsuarioId = usuario.Usuario1
            });

            return View(usuario);
        }

        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Login");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public string PruebaLeer()
        {
            try
            {
                string Msg = "";
                var Pruebas = db.Prueba;
                foreach(var Prueba in Pruebas)
                {
                    Msg += Prueba.Id.ToString() + ", " + Prueba.Nombre + ", " + Prueba.Apellido;
                }
                return Msg;
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        } 
    }
}