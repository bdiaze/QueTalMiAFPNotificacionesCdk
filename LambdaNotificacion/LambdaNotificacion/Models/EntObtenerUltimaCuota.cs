using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LambdaNotificacion.Models {
    internal class EntObtenerUltimaCuota {
        public required string ListaAFPs { get; set; }
        public required string ListaFondos { get; set; }
        public required string ListaFechas { get; set; }
        public required int TipoComision { get; set; }
    }
}
