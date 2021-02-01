using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDuel.Tools {
    public struct CommandRefrence : IComparable<CommandRefrence> {

        public Player Player;
        public DateTime Time;

        public int CompareTo(CommandRefrence cmdRef) {
            int compare = Time.CompareTo(cmdRef.Time);
            if (compare != 0)
                return compare;

            return 0;
        }
    }
}
