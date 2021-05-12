using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDuel.Tools {
    public struct ClientRequest {
        public readonly string RequestId;
        public readonly DynValue[] Arguments;

        public ClientRequest(string requestId) {
            RequestId = requestId;
            Arguments = new DynValue[0];
        }

        public ClientRequest(string requestId, DynValue[] args) {
            RequestId = requestId;
            Arguments = args;
        }
    }
}
