using System.Threading.Tasks;

namespace Asynco.Tests
{
    public class RemoteService : IRemoteService
    {
        public Task<Response> DoWorkAsync(Request request)
        {
            return Task.FromResult(new Response()
            {
                Payload = request.Payload + " received!"
            });
        }

        public Task DoWorkAsync()
        {
            return Task.CompletedTask;
        }

        public void DoWork()
        {
        }

        public Response DoWork(Request request)
        {
            return new Response()
            {
                Payload = request.Payload + " received!"
            };
        }

        public Response DoWork<T>(T request)
        {
            return new Response()
            {
                Payload = "Request received!"
            };
        }

        public T DoWork<T>(Request request)
            where T : Response
        {
            return (T)new Response()
            {
                Payload = "Request received!"
            };
        }

        public Task<Response> DoWorkAsync<T>(T request)
        {
            return Task.FromResult(new Response()
            {
                Payload = "Request received!"
            });
        }

        public Task<T> DoWorkAsync<T>(Request request)
            where T : Response
        {
            return Task.FromResult((T)new Response()
            {
                Payload = "Request received!"
            });
        }
    }
}
