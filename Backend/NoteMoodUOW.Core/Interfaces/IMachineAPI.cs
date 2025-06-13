using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteMoodUOW.Core.Interfaces
{
    public interface IMachineAPI
    {
        Task<TResponse?> callFlaskAPI<TResponse>(string content, string url);
    }
}
