using PRN232_FinalProject_Client.DTO;

namespace PRN232_FinalProject_Client.Services
{
    public interface IApiService
    {
        Task<string?> LoginAsync(string email, string password);
        Task<T?> GetAsync<T>(string endpoint);
        Task<T?> PostAsync<T>(string endpoint, object data);
        Task<T?> PutAsync<T>(string endpoint, object data);
        Task<bool> DeleteAsync(string endpoint);
    }

}
