namespace Brittany_Salon_Backend.Application.Exceptions
{
    /// <summary>
    /// Excepción para errores de validación de negocio
    /// </summary>
    public class ValidationException : Exception
    {
        public List<string> Errors { get; }

        public ValidationException(string message) : base(message)
        {
            Errors = [message];
        }

        public ValidationException(List<string> errors) : base("Se encontraron errores de validación.")
        {
            Errors = errors;
        }
    }

    /// <summary>
    /// Excepción para recursos duplicados
    /// </summary>
    public class DuplicateResourceException : Exception
    {
        public string Field { get; }

        public DuplicateResourceException(string field, string message) : base(message)
        {
            Field = field;
        }
    }
}
