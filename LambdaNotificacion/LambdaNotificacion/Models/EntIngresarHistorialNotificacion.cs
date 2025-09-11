using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LambdaNotificacion.Models {
    internal class EntIngresarHistorialNotificacion {
        public required long IdNotificacion { get; set; }
        public required DateTimeOffset FechaNotificacion { get; set; }
        public required short Estado { get; set; }
    }
}
