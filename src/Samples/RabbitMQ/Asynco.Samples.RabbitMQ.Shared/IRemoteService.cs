using System.Threading.Tasks;

namespace Asynco.Samples.RabbitMQ.Shared
{
    public interface IRemoteService
    {
        void DoWork();
        Response DoWork(Request request);
        Task<Response> DoWorkAsync(Request request);
        Task DoWorkAsync();
        Response DoWork<T>(T request) where T : Request;
        T DoWork<T>(Request request) where T : Response;
        Task<Response> DoWorkAsync<T>(T request) where T : Request;
        Task<T> DoWorkAsync<T>(Request request) where T : Response;
    }
}
