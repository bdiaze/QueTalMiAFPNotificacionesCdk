using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LambdaNotificacion.TemplateModels {
    public class ResumenSemanalModel {
        public DateTime? FechaDesde { get; set; }
        
        public DateTime? FechaHasta { get; set; }

        public Dictionary<string, object?> PremioRentabilidad { get; set; } = new Dictionary<string, object?> {
            { "AFP", null },
            { "Fondo", null },
            { "Rentabilidad", null },
        };

        public Dictionary<string, decimal?> RentabilidadSemanal { get; set; } = new Dictionary<string, decimal?> {
            { "A", null },
            { "B", null },
            { "C", null },
            { "D", null },
            { "E", null },
        };

        public SortedDictionary<string, SortedDictionary<string, decimal?>> ValorCuotaFinal { get; set; } = new SortedDictionary<string, SortedDictionary<string, decimal?>> {
            { "Capital", new SortedDictionary<string, decimal?> {
                { "A",  null },
                { "B",  null },
                { "C",  null },
                { "D",  null },
                { "E",  null }
            } },
            { "Cuprum", new SortedDictionary<string, decimal?> {
                { "A",  null },
                { "B",  null },
                { "C",  null },
                { "D",  null },
                { "E",  null }
            } },
            { "Habitat", new SortedDictionary<string, decimal?> {
                { "A",  null },
                { "B",  null },
                { "C",  null },
                { "D",  null },
                { "E",  null }
            } },
            { "Modelo", new SortedDictionary<string, decimal?> {
                { "A",  null },
                { "B",  null },
                { "C",  null },
                { "D",  null },
                { "E",  null }
            } },
            { "PlanVital", new SortedDictionary<string, decimal?> {
                { "A",  null },
                { "B",  null },
                { "C",  null },
                { "D",  null },
                { "E",  null }
            } },
            { "ProVida", new SortedDictionary<string, decimal?> {
                { "A",  null },
                { "B",  null },
                { "C",  null },
                { "D",  null },
                { "E",  null }
            } },
            { "Uno", new SortedDictionary<string, decimal?> {
                { "A",  null },
                { "B",  null },
                { "C",  null },
                { "D",  null },
                { "E",  null }
            } },
        };
    }
}
