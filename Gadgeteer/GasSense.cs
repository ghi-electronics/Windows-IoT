using GHIElectronics.UWP.GadgeteerCore;
using System.Threading.Tasks;

using GSI = GHIElectronics.UWP.GadgeteerCore.SocketInterfaces;

namespace GHIElectronics.UWP.Gadgeteer.Modules {
	public class GasSense : Module {
		private double offsetX;
		private double offsetY;
		private GSI.AnalogIO input;
		private GSI.DigitalIO enable;

		public override string Name => "GasSense";
		public override string Manufacturer => "GHI Electronics, LLC";

		protected async override Task Initialize(ISocket parentSocket) {
			this.input = await parentSocket.CreateAnalogIOAsync(SocketPinNumber.Three);
			this.enable = await parentSocket.CreateDigitalIOAsync(SocketPinNumber.Four, false);
		}

        public bool HeatingElementEnabled {
            get {
                return this.enable.Read();
            }

            set {
                this.enable.Write(value);
            }
        }

        public double ReadVoltage() => this.input.ReadVoltage();
        public double ReadProportion() => this.input.ReadProportion();
    }
}