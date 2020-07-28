using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using ICA.Models;
using Newtonsoft.Json;

namespace ICA.Utilities
{
    public class Metodos
    {
        public static bool RegistrarLog(Log log)
        {
            log.MUsuarioId = log.MUsuarioId == null ? HttpContext.Current.User.Identity.Name : log.MUsuarioId;
            log.MFechaHora = DateTime.Now;
            ICAEntities db = new ICAEntities();
            db.Log.Add(log);
            db.SaveChanges();
            return true;
        }

        public static string[] PeticionICA(string direccion, string data)
        {
            HttpWebRequest request = WebRequest.Create("https://icaservice.mikiosko.mx/api/" + direccion) as HttpWebRequest;
            request.Method = "POST";
            request.ContentType = "application/json";
            string credidentials = "SisICA" + ":" + "SisICA2020";
            var authorization = Convert.ToBase64String(Encoding.Default.GetBytes(credidentials));
            request.Headers["Authorization"] = "Basic " + authorization;
            request.ContentLength = 0;
            request.Expect = "application/json";

            if (!String.IsNullOrEmpty(data))
            {
                // Create POST data and convert it to a byte array.  
                byte[] byteArray = Encoding.UTF8.GetBytes(data);

                // Set the ContentType property of the WebRequest.  
                request.ContentType = "application/x-www-form-urlencoded";
                // Set the ContentLength property of the WebRequest.  
                request.ContentLength = byteArray.Length;

                // Get the request stream.  
                Stream dataStream = request.GetRequestStream();
                // Write the data to the request stream.  
                dataStream.Write(byteArray, 0, byteArray.Length);
                // Close the Stream object.  
                dataStream.Close();
            }

            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                string resp = reader.ReadToEnd();
                string[] arrayObj = resp.Split('}');
                int i;
                for (i = 0; i < arrayObj.Count(); i++)
                {
                    arrayObj[i] = arrayObj[i].Substring(1, arrayObj[i].Length - 1);
                    arrayObj[i] = arrayObj[i] + "}";
                }
                arrayObj[arrayObj.Length - 1] = "";
                return arrayObj;
            }
        }

        public static List<TiendaZona> convertirStringATiendaZona(string[] arrayObj)
        {
            List<TiendaZona> tiendasZonas = new List<TiendaZona>();
            foreach (var item in arrayObj)
            {
                tiendasZonas.Add(JsonConvert.DeserializeObject<TiendaZona>(item.Replace("\\", "")));
            }
            tiendasZonas.RemoveAt(tiendasZonas.Count() - 1);
            return tiendasZonas;
        }

        public static List<Subzona> convertirStringASubzona(string[] arrayObj)
        {
            List<Subzona> subzonas = new List<Subzona>();
            foreach (var item in arrayObj)
            {
                subzonas.Add(JsonConvert.DeserializeObject<Subzona>(item.Replace("\\", "")));
            }
            subzonas.RemoveAt(subzonas.Count() - 1);
            return subzonas;
        }

        public static List<TiendaCalibracion> convertirStringACalibracionTiendas(string[] arrayObj)
        {
            List<TiendaCalibracion> tiendas = new List<TiendaCalibracion>();
            foreach (var item in arrayObj)
            {
                tiendas.Add(JsonConvert.DeserializeObject<TiendaCalibracion>(item.Replace("\\", "")));
            }
            tiendas.RemoveAt(tiendas.Count() - 1);
            return tiendas;
        }

        public static List<TiendaServicio> convertirStringATiendas(string[] arrayObj)
        {
            List<TiendaServicio> tiendas = new List<TiendaServicio>();
            foreach (var item in arrayObj)
            {
                tiendas.Add(JsonConvert.DeserializeObject<TiendaServicio>(item.Replace("\\", "")));
            }
            tiendas.RemoveAt(tiendas.Count() - 1);
            return tiendas;
        }
    }
}