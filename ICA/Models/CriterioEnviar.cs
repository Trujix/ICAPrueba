using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ICA.Models
{
    public class CriterioEnviar
    {
        public int CRTId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Clasificacion { get; set; }
        public bool Ev_Posterior { get; set; }
        public string TipoRespuesta { get; set; }
        public bool EvidenciaObli { get; set; }
        public int NoTomas { get; set; }
        public string TipoEquipo { get; set; }
        public bool Precargado { get; set; }
        public bool PermitirVacio { get; set; }
        public bool Comentario { get; set; }
        public bool Condicionante { get; set; }
        public Nullable<int> CRTId_Cond { get; set; }
        public string Estatus { get; set; }
        public System.DateTime MFechaHora { get; set; }
        public string MUsuarioId { get; set; }
        public string ValorReferencia { get; set; }
        public string EquipoProducto { get; set; }
        public string OperacionGlobal { get; set; }
        public List<Valor> Valores { get; set; }
        public List<Criterio> CriteriosCondicionados { get; set; }
    }
}