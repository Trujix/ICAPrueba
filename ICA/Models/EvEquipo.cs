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
    
    public partial class EvEquipo
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public EvEquipo()
        {
            this.EvidenciaEquipo = new HashSet<EvidenciaEquipo>();
        }
    
        public int EVEId { get; set; }
        public int EVAId { get; set; }
        public int EQPId { get; set; }
        public int CRTId { get; set; }
        public string Sabor { get; set; }
        public Nullable<int> NoToma { get; set; }
        public string Valor { get; set; }
        public string Comentario { get; set; }
        public System.DateTime MFechaHora { get; set; }
        public string MUsuarioId { get; set; }
    
        public virtual Criterio Criterio { get; set; }
        public virtual Equipo Equipo { get; set; }
        public virtual Evaluacion Evaluacion { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EvidenciaEquipo> EvidenciaEquipo { get; set; }
        public virtual EvidenciaEquipo EvidenciaEquipo1 { get; set; }
    }
}
