using GHI.Athens.Gadgeteer;
using GHI.Athens.Gadgeteer.SocketInterfaces;
using System.Threading.Tasks;

namespace GHI.Athens.Modules {
	public class Moisture : Module {
		public override string Name { get; } = "Moisture";
		public override string Manufacturer { get; } = "GHI Electronics, LLC";

		private AnalogIO input;
		private DigitalIO enable;

		protected async override Task Initialize(ISocket parentSocket) {
			this.input = await parentSocket.CreateAnalogIOAsync(SocketPinNumber.Three);
			this.enable = await parentSocket.CreateDigitalIOAsync(SocketPinNumber.Six, true);
		}

		public double GetReading() {
			return this.input.ReadProportion() / 1.6;
		}
	}
}