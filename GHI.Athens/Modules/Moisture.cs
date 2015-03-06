using GHI.Athens.Gadgeteer;
using GHI.Athens.Gadgeteer.SocketInterfaces;
using System.Threading.Tasks;

namespace GHI.Athens.Modules {
	public class Moisture : Module {
		public override string Name { get; } = "Moisture";
		public override string Manufacturer { get; } = "GHI Electronics, LLC";

		private AnalogInput input;
		private DigitalOutput enable;

		protected async override Task Initialize(Socket parentSocket) {
			this.input = await parentSocket.CreateAnalogInputAsync(SocketPinNumber.Three);
			this.enable = await parentSocket.CreateDigitalOutputAsync(SocketPinNumber.Six, true);
		}

		public double GetReading() {
			return this.input.ReadProportion() / 1.6;
		}
	}
}