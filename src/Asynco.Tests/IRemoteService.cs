using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asynco.Tests
{
    public interface IRemoteService
    {
        void DoWork();
        Response DoWork(Request request);
        Task<Response> DoWorkAsync(Request request);
        Task DoWorkAsync();
        Response DoWork<T>(T request);
        T DoWork<T>(Request request) where T : Response;
        Task<Response> DoWorkAsync<T>(T request);
        Task<T> DoWorkAsync<T>(Request request) where T : Response;
    }
}
