using GHI.Athens.Gadgeteer;
using GHI.Athens.Gadgeteer.SocketInterfaces;
using System.Threading.Tasks;

namespace GHI.Athens.Modules {
	public class LEDStrip : Module {
		public override string Name { get; } = "LED Strip";
		public override string Manufacturer { get; } = "GHI Electronics, LLC";

		private DigitalOutput outputPin;

		protected async override Task Initialize(Socket parentSocket) {
			this.outputPin = await parentSocket.CreateDigitalOutputAsync(SocketPinNumber.Six, false);
		}

		public void TurnAllOn() {
			this.outputPin.Value = true;
		}

		public void TurnAllOff() {
			this.outputPin.Value = false;
		}
	}
}