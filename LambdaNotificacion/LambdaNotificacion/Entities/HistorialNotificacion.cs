using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LambdaNotificacion.Entities {
    internal class HistorialNotificacion {
        public long? Id { get; set; }

        public required long IdNotificacion { get; set; }

        public required DateTimeOffset FechaNotificacion { get; set; }

        public required short Estado { get; set; }
    }
}
