//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ICA.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class EvidenciaEquipo
    {
        public int AEVId { get; set; }
        public int EVEId { get; set; }
        public string EvidenciaDir { get; set; }
        public string Nombre { get; set; }
        public System.DateTime MFechaHora { get; set; }
        public string MUsuarioId { get; set; }
    
        public virtual EvEquipo EvEquipo { get; set; }
        public virtual EvEquipo EvEquipo1 { get; set; }
    }
}