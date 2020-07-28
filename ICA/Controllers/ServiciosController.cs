using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http.Cors;
using System.Web.Mvc;
using ICA.Models;
using ICA.Utilities;

namespace ICA.Controllers
{
    //[AllowCrossSiteAttribute]
    public class ServiciosController : Controller
    {
        private ICAEntities db = new ICAEntities();

        [HttpGet]
        public ActionResult DatosIniciales()
        {
            try
            {
                var valoresAEncontrar = new[] { "CATEGOTP", "CEQUIPOPRO", "CLASIFCRIT", "CLASIFICA", "ESTATUS", "ESTATUSEV", "TEQUIPO", "TRESPUESTA", "ZONA" };
                DatosIniciales datosIniciales;
                var valores = db.ValorReferencia.Where(v => valoresAEncontrar.Contains(v.VARClave)).ToList();
                ValorReferenciaApp valorAEnviar;
                List<ValorReferenciaApp> valoresAEnviar = new List<ValorReferenciaApp>();
                foreach (var v in valores)
                {
                   
                    valorAEnviar = new ValorReferenciaApp
                    {
                        VARClave = v.VARClave,
                        Descripcion = v.Descripcion,
                        Modificable = v.Modificable,
                        Valores = v.VARValor.Where(var => var.Estatus.Equals(Utilities.Utilities.Estatus()["Activo"])).Select(var =>
                        new VARValorApp
                        {
                            VARClave = var.VARClave,
                            VAVClave = var.VAVClave,
                            Descripcion = var.Descripcion,
                            Estatus = var.Estatus
                        }).ToList()
                    };

                    valoresAEnviar.Add(valorAEnviar);
                }
                var usuarios = db.Usuario.ToList();
                usuarios = usuarios.Where(u => u.Estatus.Equals(Utilities.Utilities.Estatus()["Activo"])).ToList();
                UsuarioApp usuarioAEnviar;
                List<UsuarioApp> usuariosAEnviar = new List<UsuarioApp>();
                foreach (var us in usuarios)
                {
                    if (us.UsuarioPerfil.FirstOrDefault().Perfil.Descripcion.Equals("Auditor"))
                    {
                        usuarioAEnviar = new UsuarioApp
                        {
                            MUsuarioId = us.MUsuarioId,
                            Nombre = us.Nombre,
                            Usuario = us.Usuario1,
                            PERId = us.UsuarioPerfil.FirstOrDefault().PERId,
                            Perfil = us.UsuarioPerfil.FirstOrDefault().Perfil.Descripcion,
                            Estatus = us.Estatus

                        };
                        usuariosAEnviar.Add(usuarioAEnviar);
                    }  
                }
                datosIniciales = new DatosIniciales
                {
                    ValorReferencias = valoresAEnviar,
                    Usuarios = usuariosAEnviar
                };
                return Json(datosIniciales, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult ValidarUsuario(string usuario, string contrasena)
        {
            var us = db.Usuario.Where(u => u.Usuario1 == usuario && u.Estatus == "A").Any();
            if (us)
            {
                ICA.ServiceReferenceUsuarios.LoginClient servicio = new ICA.ServiceReferenceUsuarios.LoginClient();
                var respuesta = servicio.Autenticar(usuario, contrasena);
                if (respuesta.EsValido)
                {
                    var r = new
                    {
                        codigo = 200,
                        mensaje = "Usuario correcto"
                    };
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var r = new
                    {
                        codigo = 500,
                        mensaje = "Contraseña incorrecta"
                    };
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                var r = new
                {
                    codigo = 404,
                    mensaje = "El usuario no existe"
                };
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult ObtenerConfiguracionEvaluacion(DateTime fecha, string auditor)
        {
            List<CriterioEnviar> criteriosAEnviar = new List<CriterioEnviar>();
            var criteriosActivos = db.Criterio.Where(c => c.Estatus.Equals("A")).ToList();
            List<Valor> valoresAEnviar = new List<Valor>();
            List<Criterio> criteriosCondicionados = new List<Criterio>();
            foreach (var cr in criteriosActivos)
            {
                valoresAEnviar.Clear();
                criteriosCondicionados.Clear();
                if (cr.TipoRespuesta.Equals("L"))
                {
                    var valor = db.ValorReferencia.Where(v => v.VARClave.Equals(cr.ValorReferencia)).FirstOrDefault();
                    if(valor is null)
                    {
                        criteriosActivos.Clear();
                        break;
                    }
                    else
                    {
                        var valorReferencia = db.VARValor.Where(v => v.VARClave.Equals(valor.VARClave)).ToList();
                       
                        foreach (var v in valorReferencia)
                        {
                            var valorEnviar = new Valor
                            {
                                VAVClave = v.VAVClave,
                                Descripcion = v.Descripcion
                            };
                            valoresAEnviar.Add(valorEnviar);
                        } 
                    }
                }

                if (cr.Condicionante.Equals(1))
                {
                    criteriosCondicionados = db.Criterio.Where(c => c.CRTId_Cond.Equals(cr.CRTId) && c.Estatus.Equals("A")).ToList();
                }
                var criterioenviar = new CriterioEnviar
                {
                    CRTId = cr.CRTId,
                    Nombre = cr.Nombre,
                    Descripcion = cr.Descripcion,
                    Clasificacion = cr.Clasificacion,
                    Ev_Posterior = cr.Ev_Posterior,
                    TipoRespuesta = cr.TipoRespuesta,
                    Valores = valoresAEnviar,
                    EvidenciaObli = cr.EvidenciaObli,
                    NoTomas = cr.NoTomas,
                    TipoEquipo = cr.TipoEquipo,
                    Precargado = cr.Precargado,
                    PermitirVacio = cr.PermitirVacio,
                    Comentario = cr.Comentario,
                    Condicionante = cr.Condicionante,
                    CRTId_Cond = cr.CRTId_Cond,
                    Estatus = cr.Estatus,
                    ValorReferencia = cr.ValorReferencia,
                    EquipoProducto = cr.EquipoProducto,
                    OperacionGlobal = cr.OperacionGlobal,
                    CriteriosCondicionados = criteriosCondicionados
                };
            }

            List<ArticuloEnviar> articulosAEnviar = new List<ArticuloEnviar>();
            var articulosCriterios = db.ArticuloCriterio.Where(a => a.Estatus.Equals("A")).Select(a => a.ARTId).ToList();
            var articulos = db.Articulo.Where(a => a.Estatus.Equals("A") && articulosCriterios.Contains(a.ARTId)).ToList();
            foreach (var art in articulos)
            {
                var criteriosArt = db.ArticuloCriterio.Where(cr => cr.ARTId.Equals(art.ARTId) && cr.Estatus.Equals("A")).Select(cr => cr.CRTId).ToList();
                var criteriosSeleccionados = db.Criterio.Where(c => criteriosArt.Contains(c.CRTId) && c.Estatus.Equals("A")).ToList();
                if(criteriosCondicionados.Count > 0)
                {
                    var articuloEnviar = new ArticuloEnviar
                    {
                        ARTId = art.ARTId,
                        SKU = art.SKU,
                        Nombre = art.Nombre,
                        Estatus = art.Estatus,
                        TipoProducto = art.TipoProducto,
                        ClasificacionICA = art.ClasificacionICA,
                        Criterios = criteriosSeleccionados
                    };
                    articulosAEnviar.Add(articuloEnviar);
                }
               
            }
            var respuesta = new
            {
                criterios = criteriosAEnviar,
                articulos = articulosAEnviar,
                equipos = db.Equipo.Where(e => e.Estatus.Equals("A")).Select(e => new { e.Nombre, e.Estatus, e.EQPId }).ToList(),
                equiposCriterio = db.EquipoCriterio.Where(e => e.Estatus.Equals("A")).Select(a => new { a.CRTId, a.Criterio.Nombre, a.Estatus, a.EQPId, a.Orden }).ToList(),
                rangoCriterio = db.RangoCriterio.ToList(),
                valorPrecargadoArticulo = db.sp_ValorPreArticulo().ToList(),
                valorPrecargadoEquipo = db.sp_ValorPreEquipo().ToList(),
                evaluacion = db.Evaluacion.Where(e => e.FechaEvaluacion == fecha && e.Auditor.Equals(auditor) && e.Estatus.Equals("P")).Select(e => new { e.EVAId, e.ZonaKiosko, e.ClaveTienda, e.Zona, e.NombreTienda, e.Estatus, e.PROId, e.Auditor, FechaEvaluacion = e.FechaEvaluacion.ToString() })
            };

            return Json(respuesta, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Evaluaciones(Evaluacion[] evaluaciones)
        {
            try
            {
                foreach (var ev in evaluaciones)
                {
                    var temp = db.Evaluacion.Find(ev.EVAId);
                    if (temp != null)
                    {
                        temp.Estatus = ev.Estatus;
                        temp.FechaInicio = ev.FechaInicio;
                        temp.FechaFin = ev.FechaFin;
                        temp.MFechaHora = DateTime.Now;
                        temp.MUsuarioId = ev.MUsuarioId;
                    }

                    db.Entry(temp).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    Metodos.RegistrarLog(new Log
                    {
                        Modulo = "Evaluaciones",
                        Accion = "Actualizar",
                        Detalle = "Evaluacion modificado: " + temp.EVAId + " " + temp.Estatus
                    });

                }
            }
            catch (Exception e)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }


            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EquiposEvaluaciones(EvEquipo evaluacion, EvidenciaEquipo[] evidencias)
        {
            try
            {
                evaluacion.MFechaHora = DateTime.Now;
                db.EvEquipo.Add(evaluacion);
                db.SaveChanges();
                Metodos.RegistrarLog(new Log
                {
                    Modulo = "Evaluaciones Equipo",
                    Accion = "Insertar",
                    Detalle = "Evaluacion: " + evaluacion.EVAId + " " + evaluacion.EVEId
                });
                foreach (var evi in evidencias)
                {
                    evi.EVEId = evaluacion.EVEId;
                    evi.MFechaHora = DateTime.Now;
                    db.EvidenciaEquipo.Add(evi);
                    db.SaveChanges();
                    Metodos.RegistrarLog(new Log
                    {
                        Modulo = "Evidencia Equipo",
                        Accion = "Insertar",
                        Detalle = "Evidencia: " + evi.AEVId + " " + evi.EVEId
                    });
                }


            } catch (Exception e)
            {
                return Json(e.Message, JsonRequestBehavior.AllowGet);
            }


            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ArticulosEvaluaciones(EvArticulo evaluacion, EvidenciaArticulo[] evidencias)
        {
            try
            {
                    evaluacion.MFechaHora = DateTime.Now;
                    db.EvArticulo.Add(evaluacion);
                    db.SaveChanges();
                    Metodos.RegistrarLog(new Log
                    {
                        Modulo = "Evaluaciones Articulo",
                        Accion = "Insertar",
                        Detalle = "Evaluacion: " + evaluacion.EVAId + " " + evaluacion.EVRId
                    });

                    foreach (var ev in evidencias)
                    {
                        ev.EVRId = evaluacion.EVRId;
                        ev.MFechaHora = DateTime.Now;
                        db.EvidenciaArticulo.Add(ev);
                        db.SaveChanges();
                        Metodos.RegistrarLog(new Log
                        {
                            Modulo = "Evidencia Articulo",
                            Accion = "Insertar",
                            Detalle = "Evidencia: " + ev.AERId + " " + ev.EVRId
                        });
                    }
                
            }
            catch (Exception e)
            {
                return Json(e.Message, JsonRequestBehavior.AllowGet);
            }


            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Imagenes(String img)
        {
            try
            {
                var ruta = System.Configuration.ConfigurationManager.AppSettings.Get("RutaEvidencias");
                byte[] imageBytes = Convert.FromBase64String(img);
                String timeStamp = GetTimestamp(DateTime.Now);
                string rutaImagen = @ruta + timeStamp + ".jpg";
                
                using (Image image = Image.FromStream(new MemoryStream(imageBytes)))
                {
                    image.Save(rutaImagen, ImageFormat.Jpeg);
                    return Json(rutaImagen, JsonRequestBehavior.AllowGet); 
                }
            }
            catch (Exception e)
            {
                return Json(e.Message, JsonRequestBehavior.AllowGet);
            }

        }

        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssffff");
        }

        [HttpPost]
        public ActionResult EquiposEvidencias(EvidenciaEquipo ev)
        {
            try
            {
                    ev.MFechaHora = DateTime.Now;
                    db.EvidenciaEquipo.Add(ev);
                    db.SaveChanges();
                    Metodos.RegistrarLog(new Log
                    {
                        Modulo = "Evidencia Equipo",
                        Accion = "Insertar",
                        Detalle = "Evidencia: " + ev.AEVId + " " + ev.EVEId
                    });
                    return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
           
        }

        [HttpPost]
        public ActionResult ArticulosEvidencias(EvidenciaArticulo ev)
        {
            try
            {
                    ev.MFechaHora = DateTime.Now;
                    db.EvidenciaArticulo.Add(ev);
                    db.SaveChanges();
                    Metodos.RegistrarLog(new Log
                    {
                        Modulo = "Evidencia Articulo",
                        Accion = "Insertar",
                        Detalle = "Evidencia: " + ev.AERId + " " + ev.EVRId
                    });

                    return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }


            
        }
    }


}