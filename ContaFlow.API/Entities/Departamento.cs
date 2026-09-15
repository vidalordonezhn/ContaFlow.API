using System.Collections.Generic;

namespace ContaFlow.API.Entities
{
    public class Departamento
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty; // "01", "02", ... "18"
        public string Nombre { get; set; } = string.Empty; // "Francisco Morazán", "Cortés", etc.
        public string? Cabecera { get; set; }

        public ICollection<Municipio> Municipios { get; set; } = new List<Municipio>();
    }
}
