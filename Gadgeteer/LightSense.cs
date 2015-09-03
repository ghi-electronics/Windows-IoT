using GHIElectronics.UWP.GadgeteerCore;
using System.Threading.Tasks;

using GSI = GHIElectronics.UWP.GadgeteerCore.SocketInterfaces;

namespace GHIElectronics.UWP.Gadgeteer.Modules {
	public class LightSense : Module {
		public override string Name => "LightSense";
		public override string Manufacturer => "GHI Electronics, LLC";

		private GSI.AnalogIO input;

		protected async override Task Initialize(ISocket parentSocket) {
			this.input = await parentSocket.CreateAnalogIOAsync(SocketPinNumber.Three);
		}

		public double GetReading() {
			return this.input.ReadProportion();
		}
	}
}