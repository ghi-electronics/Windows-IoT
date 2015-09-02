using GHIElectronics.UWP.Gadgeteer.SocketInterfaces;
using System.Threading.Tasks;

namespace GHIElectronics.UWP.Gadgeteer.Modules {
	public class LightSense : Module {
		public override string Name => "LightSense";
		public override string Manufacturer => "GHI Electronics, LLC";

		private AnalogIO input;

		protected async override Task Initialize(ISocket parentSocket) {
			this.input = await parentSocket.CreateAnalogIOAsync(SocketPinNumber.Three);
		}

		public double GetReading() {
			return this.input.ReadProportion();
		}
	}
}