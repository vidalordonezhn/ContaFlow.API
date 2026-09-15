namespace ContaFlow.API.Entities
{
    public class Municipio
    {
        public int Id { get; set; }
        public int DepartamentoId { get; set; }
        public string Codigo { get; set; } = string.Empty; // "0801", "0501", etc.
        public string Nombre { get; set; } = string.Empty; // "Distrito Central", "San Pedro Sula", etc.

        public Departamento? Departamento { get; set; }
    }
}
