using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDuel.Animation {
    public struct AnimationData {
        public readonly string AnimId;
        public readonly object[] Arguments;

        public AnimationData(string animId) {
            AnimId = animId;
            Arguments = new object[0];
        }

        public AnimationData(string animId, object[] args) {
            AnimId = animId;
            Arguments = args;
        }
    }
}
