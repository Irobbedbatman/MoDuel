using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDuel.Animation {
    public struct AnimationData {
        public readonly string AnimId;
        public readonly string[] Arguments;

        public AnimationData(string animId) {
            AnimId = animId;
            Arguments = new string[0];
        }

        public AnimationData(string animId, string[] args) {
            AnimId = animId;
            Arguments = args;
        }
    }
}
