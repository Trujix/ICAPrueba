using ICA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ICA.Utilities
{
    public static class Utilities
    {
        public static string IsActive(this HtmlHelper html, string control, string action = "")
        {
            var routeData = html.ViewContext.RouteData;

            var routeAction = (string)routeData.Values["action"];
            var routeControl = (string)routeData.Values["controller"];

            // both must match
            var returnActive = (control == routeControl &&
                               action == routeAction) || (control == routeControl &&
                               action == "");

            return returnActive ? "active" : "";
        }

        public static string ValorDescripcion(this HtmlHelper html, string VARClave, string VAVClave)
        {
            ICAEntities db = new ICAEntities();
            var valor = db.VARValor.Where(v => v.VARClave == VARClave && v.VAVClave == VAVClave).FirstOrDefault();
            return valor == null ? "" : valor.Descripcion;
        }

        public static string DescripcionTipoProducto(this HtmlHelper html, string TPRId)
        {
            ICAEntities db = new ICAEntities();
            var valor = db.TipoProducto.Where(v => v.TPRId.ToString() == TPRId).FirstOrDefault();
            return valor == null ? "" : valor.Producto;
        }

        public static string UsuarioNombre(this HtmlHelper html)
        {
            var usuario = HttpContext.Current.User.Identity.Name;
            if (System.Web.Configuration.WebConfigurationManager.AppSettings["SuperUsuario"] == usuario)
            {
                return usuario;
            }
            else
            {
                ICAEntities db = new ICAEntities();
                Usuario datos = db.Usuario.Find(usuario);
                return datos.Nombre + " - " + datos.UsuarioPerfil.First().Perfil.Descripcion;
            }
        }

        public enum Modulos
        {
            Usuarios = 2,
            ValorReferencia = 1,
            Perfiles = 1003,
            Equipos = 2002,
            Criterios = 2003,
            Articulos = 3003,
            Asignacion = 4002,
            Parametros = 5003,
            Programacion = 5004,
            TipoProducto = 5005,
            Formula = 5006
        }

        public static Dictionary<string, string> Estatus()
        {
            Dictionary<string, string> Estatus =
            new Dictionary<string, string>();

            Estatus.Add("Activo", "A");
            Estatus.Add("Inactivo", "I");

            return Estatus;
        }


    }
}