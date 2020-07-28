using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ICA.Models;
using ICA.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ICA.Controllers
{
    [Autorizar]
    public class ProgramacionController : Controller
    {
        private ICAEntities db = new ICAEntities();

        // GET: Programacion
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Programacion, "Leer")]
        public ActionResult Index()
        {
            ViewBag.Zonas = Helpers.ComboLista("ZONA");
            ViewBag.Auditores = db.Usuario.Where(u => u.UsuarioPerfil.FirstOrDefault().Perfil.Descripcion.Equals("Auditor")).ToList();
            return View();
        }

        [HttpGet]
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Programacion, "Leer")]
        public ActionResult ObtenerResumen(int mes, int anio)
        {
            var programacion = db.Programacion.Where(p => p.Mes == mes && p.Anio == anio).FirstOrDefault();
            if (!(programacion is null))
            {
                List<ResumenProgramacion> listaResumen = new List<ResumenProgramacion>();
                var responseZonas = Metodos.PeticionICA("subzonas", null);
                var subzonas = Metodos.convertirStringASubzona(responseZonas);
                var programacionesZona = db.ProgramacionZona.Where(pz => pz.PROId == programacion.PROId).ToList();
                foreach (var pz in programacionesZona)
                {
                    var Evaluadas = db.Evaluacion.Where(e => e.FechaFin != null && e.Zona == pz.Zona).Count();
                    var tmp = 0;
                    if(pz.Requerido != 0)
                       tmp = (Evaluadas * 100) / pz.Requerido;
                    var Avance = int.Parse(Math.Round(Double.Parse(tmp.ToString())).ToString());
                    var rp = new ResumenProgramacion
                    {
                        PROId = pz.PROId,
                        ZonaNombre =  Helpers.ValorDescripcion("ZONA",pz.Zona),
                        Zona = pz.Zona,
                        TotalTiendas = pz.Total,
                        Requerido = pz.Requerido,
                        Real = pz.Real,
                        Programadas = db.Evaluacion.Where(e => e.Estatus == "P" && e.Zona == pz.Zona && e.PROId == pz.PROId).Count(),
                        Evaluadas = Evaluadas,
                        Avance = Avance
                    };

                    listaResumen.Add(rp);
                }

                return Json(listaResumen, JsonRequestBehavior.AllowGet);
            }

            //TempData["MensajeClase"] = "alert-danger";
            //TempData["Mensaje"] = "La programación no existe";
            return Json(false, JsonRequestBehavior.AllowGet);
            
        }

        [HttpPost]
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Programacion, "Insertar")]
        public ActionResult Crear(int anio, int mes)
        {
            //Verificar que no exista esa programacion
            var existe = db.Programacion.Where(p => p.Mes == mes && p.Anio == anio).ToList();
            if(existe.Count == 0)
            {
                ValorReferencia valorReferencia = db.ValorReferencia.Find("PARAMETROS");
                var dias = int.Parse(valorReferencia.VARValor.Where(v => v.VAVClave == "DIASPROG").FirstOrDefault().Descripcion);

                //Crear programacion
                DateTime fechaMes = new DateTime(anio, mes, 1);
                DateTime fechaRangoPermitido = fechaMes.AddDays(-dias);
                DateTime hoy = DateTime.Today;
                if (hoy > fechaRangoPermitido || hoy.Month == fechaMes.Month)
                {
                    string[] meses = new string[] { "ENE", "FEB", "MAR", "ABR", "MAY", "JUN", "JUL", "AGO", "SEP", "OCT", "NOV", "DIC" };
                    var prog = guardarProgramacion(meses[mes - 1] + anio.ToString(), mes, anio);


                    var responseZonas = Metodos.PeticionICA("subzonas", null);
                    var subzonas = Metodos.convertirStringASubzona(responseZonas);

                    var responseTienda = Metodos.PeticionICA("tienda", null);
                    var tiendas = Metodos.convertirStringATiendaZona(responseTienda);

                    int[] tiendasEnZonas = new int[subzonas.Count() + 1];
                    int[] m1 = new int[subzonas.Count() + 1], m2 = new int[subzonas.Count() + 1], m3 = new int[subzonas.Count() + 1];
                    int totalTiendasGlobal = tiendas.Count();
                    int muestraGlobal = obtenerCantidadMuestra(totalTiendasGlobal);
                    List<MuestraZona> muestrasZonasDecimales = new List<MuestraZona>();
                    int acumuladoMuestrasZonas = 0;

                    //Contabiliza las tiendas por zona
                    foreach (var tienda in tiendas)
                    {
                        tiendasEnZonas[tienda.ClaveSubzona]++;
                    }

                    //Calcula la cantidad de muestra por zona
                    for (int i = 1; i < tiendasEnZonas.Length; i++)
                    {
                        double muestra = obtenerCantidadMuestraPorZona(tiendasEnZonas[i], muestraGlobal, totalTiendasGlobal);
                        var temp = new MuestraZona
                        {
                            Zona = i,
                            muestraSinRedondear = muestra,
                            muestraRedondeada = int.Parse(Math.Round(muestra).ToString()),
                            decimales = muestra - Math.Truncate(muestra),
                            tiendas = tiendasEnZonas[i]
                        };
                        muestrasZonasDecimales.Add(temp);
                        acumuladoMuestrasZonas += temp.muestraRedondeada;
                    }

                    //Obtiene la diferencia si es que hay que realizar ajuste
                    int diferencia =  ajustarMuestras(muestraGlobal, acumuladoMuestrasZonas, ref muestrasZonasDecimales);
                    int contador = 0;
                    bool ajusteHaciaArriba = false;
                    if (muestraGlobal > acumuladoMuestrasZonas)
                        ajusteHaciaArriba = true;
                    
                    foreach (var item in muestrasZonasDecimales)
                    {
                        //Si se debe realizar ajuste lo realiza
                        if (contador < diferencia)
                        {
                            if (ajusteHaciaArriba == true && item.decimales < 0.5)
                            {
                                item.muestraRedondeada = item.muestraRedondeada + 1;
                                contador++;
                            }
                            else if(ajusteHaciaArriba == false && item.decimales > 0.5)
                            {
                                    item.muestraRedondeada = item.muestraRedondeada - 1;
                                    contador++;
                            }
                        }

                        var pz = guardarProgramacionZona(prog, item.Zona.ToString(), item.tiendas, item.muestraRedondeada);
                    }

                    //Calcula las tiendas por mes segun la muestra de la zona
                    for (int i = 1; i < subzonas.Count()+1; i++)
                    {
                        var zona = muestrasZonasDecimales.Where(z => z.Zona == i).FirstOrDefault();
                        m1[i] = int.Parse(Math.Round(Double.Parse(zona.muestraRedondeada.ToString()) / 3).ToString());
                        if (zona.muestraRedondeada - m1[i] > m1[i])
                        {
                            m2[i] = m1[i];
                            m3[i] = zona.muestraRedondeada - (m1[i] + m2[i]);
                        }
                        else
                        {
                            m2[i] = zona.muestraRedondeada - m1[i];
                            m3[i] = 0;
                        }
                        if (zona.muestraRedondeada - m1[i] == 0)
                        {
                            m2[i] = 0;
                            m3[i] = 0;
                        }
                    }

                    var mes1 = fechaMes.AddMonths(-1);
                    var mes2 = fechaMes.AddMonths(-2);
                    var mes3 = fechaMes.AddMonths(-3);
                    var faltante = 0;

                    //Genera las evaluaciones por mes de cada zona
                    for (int i = 1; i < subzonas.Count()+1; i++)
                    {
                       faltante = generarMuestrasPorMes(mes1, m1, anio, i, prog, 0);
                       faltante = generarMuestrasPorMes(mes2, m2, anio, i, prog, faltante);
                       faltante =  generarMuestrasPorMes(mes3, m3, anio, i, prog, faltante);
                        var pz = db.ProgramacionZona.Find(prog.PROId, i.ToString());
                        pz.Real = pz.Requerido - faltante;
                        db.Entry(pz).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                  
                    return Json(new { estatus = true});
                }
                return Json(new { estatus = false, dias = dias });
            }
            return Json(new { estatus = false});
            
        }

        [VerificarPerfil((int)Utilities.Utilities.Modulos.Programacion, "Leer")]
        public ActionResult DetallePorZona(string zona, int PROId)
        { 
            ViewBag.Zona = Helpers.ValorDescripcion("ZONA", zona); 
            return PartialView("_Detalle", db.Evaluacion.Where(e => e.Zona.Equals(zona) && e.PROId ==PROId).ToList());
        }

        [VerificarPerfil((int)Utilities.Utilities.Modulos.Programacion, "Insertar")]
        public ActionResult DetallePorDia (int PROId, DateTime fecha, string zona, string auditor)
        {
            ViewBag.Fecha = fecha;
            //var evaluaciones = db.Evaluacion.Where(e => e.PROId == PROId && e.FechaEvaluacion == fecha && e.Auditor == auditor).Select(e => e.Zona).Distinct().ToList();
            //if (evaluaciones.Count > 1)
            //    ViewBag.NombreZona = "*";
            //else
            ViewBag.NombreZona = Helpers.ValorDescripcion("ZONA", zona);
            ViewBag.Zona = zona;
            ViewBag.Auditor = auditor;
            //&& e.Zona == zona
            
            return PartialView("_DetallePorDia", db.Evaluacion.Where(e => e.PROId == PROId && e.FechaEvaluacion == fecha && e.Auditor == auditor && e.Zona == zona).ToList());
        }

        private Programacion guardarProgramacion(string nombre, int mes, int anio)
        {
            Programacion prog = new Programacion
            {
                Nombre = nombre,
                Mes = mes,
                Anio = anio,
                Estatus = "A",
                MFechaHora = DateTime.Now,
                MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name,
            };
            db.Programacion.Add(prog);
            db.SaveChanges();
            Metodos.RegistrarLog(new Log
            {
                Modulo = "Programación",
                Accion = "Insertar",
                Detalle = "Programación insertado:" + prog.Nombre
            });

            return prog;
        }

        private ProgramacionZona guardarProgramacionZona(Programacion prog, string zona, int total, int muestra)
        {
            ProgramacionZona pz = new ProgramacionZona
            {
                Programacion = prog,
                Zona = zona,
                Total = total,
                Requerido = muestra,
                Real = 0,
                MFechaHora = DateTime.Now,
                MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name
            };

            db.ProgramacionZona.Add(pz);
            db.SaveChanges();
            Metodos.RegistrarLog(new Log
            {
                Modulo = "Programación",
                Accion = "Insertar",
                Detalle = "Programación zona insertado:" + pz.PROId + " " + zona.ToString()
            });

            return pz;
        }

        private int obtenerCantidadMuestra(int N)
        {
            double muestra = 0;
            double a =  db.ParametroMuestra.Where(m => m.Parametro == "a").Select(m => m.Valor).FirstOrDefault();
            double e = db.ParametroMuestra.Where(m => m.Parametro == "e").Select(m => m.Valor).FirstOrDefault();
            double p = db.ParametroMuestra.Where(m => m.Parametro == "p").Select(m => m.Valor).FirstOrDefault();
            double Za = db.ParametroMuestra.Where(m => m.Parametro == "Za").Select(m => m.Valor).FirstOrDefault();
            double q = db.ParametroMuestra.Where(m => m.Parametro == "q").Select(m => m.Valor).FirstOrDefault();

            muestra = (N * Math.Pow(Za, 2) * p * q) / ((Math.Pow(e, 2) * (N - 1)) + (Math.Pow(Za, 2) * p * q));

            return int.Parse(Math.Round(muestra).ToString());
        }

        private double obtenerCantidadMuestraPorZona(int cantidadTiendasZona, int muestraGlobal, int cantidadTiendasGlobal)
        {
            //return int.Parse(Math.Ceiling((double.Parse(cantidadTiendasZona.ToString()) * muestraGlobal) / cantidadTiendasGlobal).ToString());
            var muestra = (double.Parse(cantidadTiendasZona.ToString()) * double.Parse(muestraGlobal.ToString())) / double.Parse(cantidadTiendasGlobal.ToString());
            return muestra;
        }

        private int ajustarMuestras(int muestraGlobal, int acumuladoMuestrasZonas, ref List<MuestraZona> muestrasZonasDecimales)
        {
            int diferencia = 0;
            if (acumuladoMuestrasZonas < muestraGlobal)
            {
                muestrasZonasDecimales = muestrasZonasDecimales.OrderByDescending(m => m.decimales).ToList();
                diferencia = muestraGlobal - acumuladoMuestrasZonas;
            }
            else
            {
                muestrasZonasDecimales = muestrasZonasDecimales.OrderBy(m => m.decimales).ToList();
                diferencia = acumuladoMuestrasZonas - muestraGlobal;
            }

            return diferencia;
        }


        private Evaluacion GuardarEvaluacion(TiendaServicio tienda, Programacion prog)
        {
            Evaluacion evaluacion = new Evaluacion
            {
                Programacion = prog,
                ClaveTienda = tienda.ClaveTienda.ToString(),
                NombreTienda =  tienda.Nombre,
                Estatus = "NA",
                Zona = tienda.ClaveSubzona.ToString(),
                ZonaKiosko = tienda.Zona.ToString(),
                MFechaHora = DateTime.Now,
                MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name
            };

            db.Evaluacion.Add(evaluacion);
            db.SaveChanges();
            Metodos.RegistrarLog(new Log
            {
                Modulo = "Evaluación",
                Accion = "Insertar",
                Detalle = "Evaluación insertada:" + evaluacion.EVAId + " " + evaluacion.PROId + " " + evaluacion.NombreTienda
            });

            return evaluacion;
        }

        private TiendaCalibracion GetTiendaRandom(ref List<TiendaCalibracion> tiendasCalibracion)
        {
            Random random = new Random();
            int indiceTiendaElegida = random.Next(0, tiendasCalibracion.Count());
            TiendaCalibracion tiendaElegida = tiendasCalibracion.ElementAt(indiceTiendaElegida);
            tiendasCalibracion.RemoveAt(indiceTiendaElegida);
            return tiendaElegida;
        }
        
        private int generarMuestrasPorMes(DateTime fechaM, int[] cantidadMuestrasPorZona, int anio, int subzonaId, Programacion prog, int agregado)
        {
            var diasEnMes = DateTime.DaysInMonth(anio, fechaM.Month);
            var faltante = 0;
            string cadenaDiasMes = "";
            if (diasEnMes < 10)
                cadenaDiasMes = "0" + diasEnMes.ToString();
            else
                cadenaDiasMes = diasEnMes.ToString();
            string data = "idSubZona=" + subzonaId.ToString() + "&fechaInicio=" + fechaM.ToString("MM") + "/01/" + fechaM.ToString("yyyy") + "&fechaFin=" + fechaM.ToString("MM") + "/" + cadenaDiasMes + "/" + fechaM.ToString("yyyy");
            var responseTiendaCalibracion = Metodos.PeticionICA("calibracionDeEquipos", data);
            var tiendasCalibracion = Metodos.convertirStringACalibracionTiendas(responseTiendaCalibracion);
            var responseTiendas = Metodos.PeticionICA("tienda", null);
            var tiendasList = Metodos.convertirStringATiendas(responseTiendas);

            var tiendasAgregadas = 0;
            var nombreZona = Helpers.ValorDescripcion("ZONA", subzonaId.ToString());
            for (int j = 0; j < cantidadMuestrasPorZona[subzonaId] + agregado && tiendasCalibracion.Count() > 0; j++)
            {
                bool tiendaValida = false;
                TiendaCalibracion tiendaElegida = null;
                while(tiendaValida == false && tiendasCalibracion.Count() > 0)
                {
                    tiendaElegida = GetTiendaRandom(ref tiendasCalibracion);
                    
                    var existe = db.Evaluacion.Where(e => e.PROId == prog.PROId && e.ClaveTienda == tiendaElegida.ClaveTienda.ToString()).Count();
                    if (existe > 0)
                        tiendaValida = false;
                    else
                        tiendaValida = true;
                }
                if(tiendaValida == true)
                {
                    var tienda = tiendasList.Find(t => t.ClaveTienda == tiendaElegida.ClaveTienda);
                    GuardarEvaluacion(tienda, prog);
                    tiendasAgregadas++;
                }
                
            }
           
            faltante = (cantidadMuestrasPorZona[subzonaId] + agregado) - tiendasAgregadas;
            return faltante;
        }

        [HttpPost]
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Programacion, "Insertar")]
        public ActionResult ProgramarAuditor(DateTime fecha, int zona, string auditor, int PROId)
        {

            var evaluaciones = db.Evaluacion.Where(e => e.PROId == PROId && e.Zona == zona.ToString() && e.Estatus == "NA").ToList();
            if(evaluaciones.Count > 0)
            {
                ValorReferencia valorReferencia = db.ValorReferencia.Find("PARAMETROS");
                var tiendasPorDia = int.Parse(valorReferencia.VARValor.Where(v => v.VAVClave == "TIENDAXDIA").FirstOrDefault().Descripcion);

                for (int i = 0; i < tiendasPorDia; i++)
                {
                    if (evaluaciones.Count > 0)
                    {
                        var evaluacion = GetEvaluacionRandom(ref evaluaciones);
                        evaluacion.Auditor = auditor;
                        evaluacion.Estatus = "P";
                        evaluacion.FechaEvaluacion = fecha;
                        db.Entry(evaluacion).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        Metodos.RegistrarLog(new Log
                        {
                            Modulo = "Evaluación",
                            Accion = "Actualizar",
                            Detalle = "Tienda: " + evaluacion.NombreTienda + " auditor:" + evaluacion.Auditor + " programación:" + evaluacion.PROId
                        });
                    }
                    else
                        return Json(true);

                }
                return Json(true);
            }
            return Json(false);
        }

        private Evaluacion GetEvaluacionRandom(ref List<Evaluacion> evaluaciones)
        {
            Random random = new Random();
            int indiceEvaluacionElegida = random.Next(0, evaluaciones.Count());
            Evaluacion evaluacion = evaluaciones.ElementAt(indiceEvaluacionElegida);
            evaluaciones.RemoveAt(indiceEvaluacionElegida);
            return evaluacion;
        }

        [HttpGet]
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Programacion, "Leer")]
        public ActionResult ObtenerCalendario(int mes, int anio)
        {
            var programacion = db.Programacion.Where(p => p.Mes == mes && p.Anio == anio).FirstOrDefault();
            if (!(programacion is null))
            {
                DateTime fechaInicio = new DateTime(anio, mes, 1);
                var diasEnMes = DateTime.DaysInMonth(anio, mes);
                DateTime fechaFin = new DateTime(anio, mes, diasEnMes);
                var listaCalendario = db.Evaluacion.Where(e => e.FechaEvaluacion >= fechaInicio && e.FechaEvaluacion <= fechaFin && e.PROId == programacion.PROId).Select(e =>
                        new
                        {
                            Auditor = e.Auditor,
                            FechaEvaluacion = e.FechaEvaluacion.ToString(),
                            Zona = e.Zona,
                            ZonaNombre = db.VARValor.Where(v => v.VARClave == "ZONAABR" && v.VAVClave == e.Zona).FirstOrDefault().Descripcion,
                            Color = db.Evaluacion.Where(ev => ev.FechaEvaluacion == e.FechaEvaluacion  && ev.Estatus != "E").Count() > 0 ? "info": "success",
                            PROId = e.PROId
                        }).GroupBy( c => new { c.Auditor, c.FechaEvaluacion, c.Color, c.PROId, c.Zona}).ToList();

                return Json(listaCalendario, JsonRequestBehavior.AllowGet);
            }

            //TempData["MensajeClase"] = "alert-danger";
            //TempData["Mensaje"] = "La programación no existe";
            return Json(false, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Programacion, "Borrar")]
        public ActionResult eliminarProgramada(int EVAId)
        {
            var evaluacion = db.Evaluacion.Find(EVAId);
            if (evaluacion != null || evaluacion.Estatus != "E")
            {
                evaluacion.Estatus = "NA";
                evaluacion.Auditor = null;
                evaluacion.FechaEvaluacion = null;
                evaluacion.MFechaHora = DateTime.Now;
                evaluacion.MUsuarioId = System.Web.HttpContext.Current.User.Identity.Name;
                db.Entry(evaluacion).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                Metodos.RegistrarLog(new Log
                {
                    Modulo = "Evaluación",
                    Accion = "Actualizar",
                    Detalle = "Tienda: " + evaluacion.NombreTienda + " auditor:" + evaluacion.Auditor + " programación:" + evaluacion.PROId
                });
                return Json(true, JsonRequestBehavior.AllowGet);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Programacion, "Insertar")]
        public ActionResult AgregarTienda(DateTime fecha, int zona, string auditor, int PROId, string ClaveTienda)
        {
                Evaluacion evaluacion = null;
                if (string.IsNullOrEmpty(ClaveTienda))
                {
                    var evaluaciones = db.Evaluacion.Where(e => e.PROId == PROId && e.Zona == zona.ToString() && e.Estatus == "NA").ToList();
                    if (evaluaciones.Count() > 0)
                    {
                        evaluacion = GetEvaluacionRandom(ref evaluaciones);
                    }
                    else
                    {
                        return Json(new { Estatus = false, mensaje = "Ya no existen tiendas por asignar" });
                    }
                }
                else
                {
                    evaluacion = db.Evaluacion.Where(e => e.PROId == PROId && e.ClaveTienda == ClaveTienda).FirstOrDefault();
                    if(evaluacion == null)
                    {
                        var responseTiendas = Metodos.PeticionICA("tienda", null);
                        var tiendasList = Metodos.convertirStringATiendas(responseTiendas);
                        var tienda = tiendasList.Find(t => t.ClaveTienda.ToString() == ClaveTienda);
                        var prog = db.Programacion.Find(PROId);
                        evaluacion = GuardarEvaluacion(tienda, prog);
                    }
                }
                    
                      
            
                if(evaluacion != null)
                {
                    if(evaluacion.Estatus != "NA")
                        return Json(new { Estatus = false, mensaje = "La tienda ya se encuentra programada para evaluación" });

                    evaluacion.Auditor = auditor;
                    evaluacion.Estatus = "P";
                    evaluacion.FechaEvaluacion = fecha;
                    db.Entry(evaluacion).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    Metodos.RegistrarLog(new Log
                    {
                        Modulo = "Evaluación",
                        Accion = "Actualizar",
                        Detalle = "Tienda: " + evaluacion.NombreTienda + " auditor:" + evaluacion.Auditor + " programación:" + evaluacion.PROId
                    });
                    return Json(new                 {
                        ClaveTienda = evaluacion.ClaveTienda,
                        NombreTienda = evaluacion.NombreTienda,
                        Auditor = evaluacion.Auditor,
                        Fecha = evaluacion.FechaEvaluacion.ToString().Substring(0, 10),
                        Estatus = Helpers.ValorDescripcion("ESTATUSEV", evaluacion.Estatus),
                        EVAId = evaluacion.EVAId
                    });
                }
                return Json(new { Estatus = false, mensaje = "La tienda no existe"});
           
        }

        [VerificarPerfil((int)Utilities.Utilities.Modulos.Programacion, "Borrar")]
        public ActionResult Delete(int mes, int anio)
        {
            var programacion = db.Programacion.Where(p => p.Mes == mes && p.Anio == anio).FirstOrDefault();
            if (programacion == null)
            {
                var pro = new Programacion
                {
                    PROId = 0
                };
                return PartialView("_Delete", pro);
            }
            return PartialView("_Delete", programacion);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [VerificarPerfil((int)Utilities.Utilities.Modulos.Programacion, "Borrar")]
        public ActionResult Delete(Programacion programacion)
        {
            if (ModelState.IsValid)
            {
                Programacion proge = db.Programacion.Find(programacion.PROId);
                if(proge != null)
                {
                    
                    var evaluaciones = db.Evaluacion.Where(e => e.PROId == proge.PROId).ToList();
                    foreach (var evaluacion in evaluaciones)
                    {
                        var evEquipos = db.EvEquipo.Where(e => e.EVAId == evaluacion.EVAId).ToList();
                        var evArticulos = db.EvArticulo.Where(e => e.EVAId == evaluacion.EVAId).ToList();
                        var evidenciaEquipos = db.EvidenciaEquipo.Where(e => evEquipos.Select(eve => eve.EVEId).Contains(e.EVEId) ).ToList();
                        var evidenciaArticulos = db.EvidenciaArticulo.Where(e => evArticulos.Select(eva => eva.EVRId).Contains(e.EVRId)).ToList();

                        foreach (var evEquipo in evidenciaEquipos)
                        {  
                            Metodos.RegistrarLog(new Log
                            {
                                Modulo = "Evidencia Equipo",
                                Accion = "Borrar",
                                Detalle = "Evidencia Equipo borrado: " + evEquipo.Nombre
                            });
                        }
                        db.EvidenciaEquipo.RemoveRange(evidenciaEquipos);
                        foreach (var evEquipo in evEquipos)
                        {
                            Metodos.RegistrarLog(new Log
                            {
                                Modulo = "Evaluación Equipo",
                                Accion = "Borrar",
                                Detalle = "Evaluación Equipo borrado: " + evEquipo.EVAId
                            });
                        }
                        db.EvEquipo.RemoveRange(evEquipos);
                        foreach (var evArticulo in evidenciaArticulos)
                        {  
                            Metodos.RegistrarLog(new Log
                            {
                                Modulo = "Evidencia Artículo",
                                Accion = "Borrar",
                                Detalle = "Evidencia Artículo borrado: " + evArticulo.Nombre
                            });
                        }
                        db.EvidenciaArticulo.RemoveRange(evidenciaArticulos);
                        foreach (var evArticulo in evArticulos)
                        {  
                            Metodos.RegistrarLog(new Log
                            {
                                Modulo = "Evaluación Artículo",
                                Accion = "Borrar",
                                Detalle = "Evaluación Evaluación borrado: " + evArticulo.EVAId
                            });
                        }
                        db.EvArticulo.RemoveRange(evArticulos);
                        Metodos.RegistrarLog(new Log
                        {
                            Modulo = "Evaluación",
                            Accion = "Borrar",
                            Detalle = "Evaluación borrado: " + evaluacion.EVAId
                         });
                        db.Evaluacion.Remove(evaluacion);
                    }
                    var programacionesZona = db.ProgramacionZona.Where(p => p.PROId == proge.PROId).ToList();
                    foreach (var proZ in programacionesZona)
                    {
                        Metodos.RegistrarLog(new Log
                        {
                            Modulo = "ProgramaciónZona",
                            Accion = "Borrar",
                            Detalle = "ProgramaciónZona borrado: " + proZ.PROId + " " + proZ.Zona
                        });
                    }
                    db.ProgramacionZona.RemoveRange(programacionesZona);
                    Metodos.RegistrarLog(new Log
                    {
                        Modulo = "Programación",
                        Accion = "Borrar",
                        Detalle = "Programación borrado: " + proge.PROId 
                    });
                    db.Programacion.Remove(proge);
                    db.SaveChanges();
                    return Json(new { estatus = true, mensaje = "Programación eliminada con éxito" });
                }
                return Json(new { estatus = false, mensaje= "Programación no encontrada"});
            }
            return Json(new { estatus = false, mensaje = "Error al eliminar" });
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