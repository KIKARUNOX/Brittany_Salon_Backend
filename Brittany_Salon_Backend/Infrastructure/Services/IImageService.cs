namespace Brittany_Salon_Backend.Infrastructure.Services
{
    public interface IImageService
    {
        /// <summary>
        /// Procesa y guarda una imagen de empleado, convirtiéndola a WebP
        /// </summary>
        /// <param name="base64Image">Imagen en formato Base64</param>
        /// <param name="employeeId">ID del empleado para generar nombre único</param>
        /// <returns>URL relativa de la imagen guardada</returns>
        Task<string> ProcessAndSaveEmployeeImageAsync(string base64Image, int employeeId);

        /// <summary>
        /// Elimina una imagen de empleado del sistema de archivos
        /// </summary>
        /// <param name="imageUrl">URL relativa de la imagen a eliminar</param>
        /// <returns>True si se eliminó correctamente</returns>
        bool DeleteEmployeeImage(string imageUrl);
    }
}
