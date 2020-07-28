using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ICA.Models
{
    public class ArticuloEnviar
    {
        public int ARTId { get; set; }
        public string SKU { get; set; }
        public string Nombre { get; set; }
        public string Estatus { get; set; }
        public string TipoProducto { get; set; }
        public string ClasificacionICA { get; set; }
        public System.DateTime MFechaHora { get; set; }
        public string MUsuarioId { get; set; }
        public List<Criterio> Criterios { get; set; }
    }
}