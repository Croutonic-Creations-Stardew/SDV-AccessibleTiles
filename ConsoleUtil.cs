using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessibleTiles {
    public class ConsoleUtil {

        internal IMonitor monitor;

        public ConsoleUtil(IMonitor monitor) {
            this.monitor = monitor;
        }

        public void Debug(string text) {
            #if DEBUG
                this.monitor.Log(text, LogLevel.Debug);
            #endif
        }

    }
}
