using GHIElectronics.UWP.GadgeteerCore;
using System.Threading.Tasks;

using GSI = GHIElectronics.UWP.GadgeteerCore.SocketInterfaces;

namespace GHIElectronics.UWP.Gadgeteer.Modules {
    public class RelayX1 : Module {
        public override string Name => "Relay X1";
        public override string Manufacturer => "GHI Electronics, LLC";

        private GSI.DigitalIO enable;

        protected async override Task Initialize(ISocket parentSocket) {
            this.enable = await parentSocket.CreateDigitalIOAsync(SocketPinNumber.Five, false);
        }

        public bool Enabled {
            get {
                return this.enable.Read();
            }

            set {
                this.enable.Write(value);
            }
        }

        public void TurnOn() => this.Enabled = true;
        public void TurnOff() => this.Enabled = false;
    }
}