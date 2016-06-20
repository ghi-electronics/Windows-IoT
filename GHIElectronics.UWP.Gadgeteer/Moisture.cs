using GHIElectronics.UWP.GadgeteerCore;
using System.Threading.Tasks;

using GSI = GHIElectronics.UWP.GadgeteerCore.SocketInterfaces;

namespace GHIElectronics.UWP.Gadgeteer.Modules {
	public class Moisture : Module {
		public override string Name => "Moisture";
		public override string Manufacturer => "GHI Electronics, LLC";

		private GSI.AnalogIO input;
		private GSI.DigitalIO enable;

		protected async override Task Initialize(ISocket parentSocket) {
			this.input = await parentSocket.CreateAnalogIOAsync(SocketPinNumber.Three);
			this.enable = await parentSocket.CreateDigitalIOAsync(SocketPinNumber.Six, true);
		}

		public double GetReading() {
			return this.input.ReadProportion() * 1.6;
		}
	}
}