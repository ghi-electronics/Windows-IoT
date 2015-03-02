using GHI.Athens.Gadgeteer;
using GHI.Athens.Gadgeteer.SocketInterfaces;
using System.Threading.Tasks;

namespace GHI.Athens.Modules {
	public class LightSense : Module {
		public override string Name { get; } = "LightSense";
		public override string Manufacturer { get; } = "GHI Electronics, LLC";

		private AnalogInput inputPin;

		protected async override Task Initialize(Socket parentSocket) {
			this.inputPin = await parentSocket.CreateAnalogInputAsync(SocketPinNumber.Three);
		}

		public double GetReading() {
			return this.inputPin.ReadProportion();
		}
	}
}