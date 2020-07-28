using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ICA.Models
{
    public class RangoCriterioTabla
    {
        public int RNGId { get; set; }
        public string NombreCriterio { get; set; }
        public int CRTId { get; set; }
        public string NombreBase { get; set; }
        public string ClaveBase { get; set; }
        public double ValorMin { get; set; }
        public double ValorMax { get; set; }
    }
}