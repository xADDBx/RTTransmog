using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityModManagerNet;

namespace RTTransmog {
    public class Settings : UnityModManager.ModSettings {
        public int browserDetailSearchLimit = 30;
        public int browserSearchLimit = 30;
        public bool shouldShowItemsWithoutEE = false;
        public override void Save(UnityModManager.ModEntry modEntry) {
            Save(this, modEntry);
        }
    }
}
