namespace Ajit_Bakery.Services
{
    public interface IApiService
    {
        Task<T> GetAsync<T>(string url, string token);
        Task<T> PostAsync<T>(string url, object data);
        Task<T> PutAsync<T>(string url, object data);
        Task<T> PatchAsync<T>(string url, object data);
        Task<T> DeleteAsync<T>(string url, string token); // Update here
    }
}
