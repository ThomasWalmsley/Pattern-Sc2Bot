using System.Collections.Generic;
using SC2APIProtocol;

namespace Bot {
    public interface Bot {
        IEnumerable<SC2APIProtocol.Action> OnFrame();
    }
}