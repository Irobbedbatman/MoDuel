using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDuel.Tools {
    public struct ClientRequest {
        public readonly string RequestId;
        public readonly object[] Arguments;

        public ClientRequest(string animId) {
            RequestId = animId;
            Arguments = new object[0];
        }

        public ClientRequest(string animId, object[] args) {
            RequestId = animId;
            Arguments = args;
        }
    }
}
