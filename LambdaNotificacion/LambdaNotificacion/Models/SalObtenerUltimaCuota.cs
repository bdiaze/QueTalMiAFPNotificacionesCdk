using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LambdaNotificacion.Models {
    internal class SalObtenerUltimaCuota {
        public required string Afp { get; set; }
        public required DateTime Fecha { get; set; }
        public required string Fondo { get; set; }
        public required decimal Valor { get; set; }
        public decimal? Comision { get; set; }
    }
}
