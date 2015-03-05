using GHI.Athens.Gadgeteer;
using GHI.Athens.Gadgeteer.SocketInterfaces;
using System.Threading.Tasks;

namespace GHI.Athens.Modules {
	public class LEDStrip : Module {
		public override string Name { get; } = "LED Strip";
		public override string Manufacturer { get; } = "GHI Electronics, LLC";

		private DigitalOutput[] outputPins;

		protected async override Task Initialize(Socket parentSocket) {
			this.outputPins = new DigitalOutput[7];

			for (var i = 0; i < 7; i++)
				this.outputPins[i] = await parentSocket.CreateDigitalOutputAsync((SocketPinNumber)(i + 3), false);
		}

		public void TurnAllOn() {
			foreach (var p in this.outputPins)
				p.SetHigh();
		}

		public void TurnAllOff() {
			foreach (var p in this.outputPins)
				p.SetLow();
		}

		public void TurnOn(uint led) {
			this.outputPins[led].SetHigh();
		}

		public void TurnOff(uint led) {
			this.outputPins[led].SetLow();
		}
	}
}