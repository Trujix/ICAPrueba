using ICA.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ICA.Utilities
{
    public class Helpers
    {
        public static List<VARValor> IndicadoresValores(string id)
        {
            ICAEntities db = new ICAEntities();
            ValorReferencia valorReferencia = db.ValorReferencia.Find(id);
            return valorReferencia.VARValor.Where(v => v.Estatus == "A").ToList();
        }

        internal static List<SelectListItem> ComboLista(string VARClave)
        {
            return IndicadoresValores(VARClave).Select(i => new SelectListItem
            {
                Value = i.VAVClave,
                Text = i.Descripcion
            }).ToList();
        }

        public static string ValorDescripcion(string VARClave, string VAVClave)
        {
            ICAEntities db = new ICAEntities();
            var valor = db.VARValor.Where(v => v.VARClave == VARClave && v.VAVClave == VAVClave).FirstOrDefault();
            return valor == null ? "" : valor.Descripcion;
        }

        public static bool TieneAcceso(int MODId, string Accion)
        {
            string usuario = HttpContext.Current.User.Identity.Name;

            bool isAuthorised = usuario == System.Web.Configuration.WebConfigurationManager.AppSettings["SuperUsuario"];

            if (!isAuthorised)
            {
                try
                {
                    ICAEntities db = new ICAEntities();
                    Usuario datos = db.Usuario.Find(usuario);
                    Perfil perfil = datos.UsuarioPerfil.First().Perfil;
                    switch (Accion)
                    {
                        case "Leer":
                            return perfil.ModuloPerfil.Where(m => m.MODId == MODId).FirstOrDefault().Leer == 1;
                        case "Insertar":
                            return perfil.ModuloPerfil.Where(m => m.MODId == MODId).FirstOrDefault().Insertar == 1;
                        case "Actualizar":
                            return perfil.ModuloPerfil.Where(m => m.MODId == MODId).FirstOrDefault().Actualizar == 1;
                        case "Borrar":
                            return perfil.ModuloPerfil.Where(m => m.MODId == MODId).FirstOrDefault().Borrar == 1;
                        default:
                            break;
                    }
                }
                catch (Exception)
                {
                }
            }

            return isAuthorised;
        }

        internal static List<SelectListItem> ComboDecision()
        {
            List<SelectListItem> lista = new List<SelectListItem>();

            lista.Add(new SelectListItem

            {
                Value = true.ToString(),
                Text = "Si"
            });

            lista.Add(new SelectListItem

            {
                Value = false.ToString(),
                Text = "No"
            });

            return lista;
        }
    }
}