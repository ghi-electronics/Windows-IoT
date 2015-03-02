using GHI.Athens.Gadgeteer;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace GHI.Athens.Modules {
	public class LEDStrip : Module {
		public override string Name { get; } = "LED Strip";
		public override string Manufacturer { get; } = "GHI Electronics, LLC";

		private GpioOutputPin outputPin;

		protected async override Task Initialize(Socket parentSocket) {
			this.outputPin = await SocketInterfaces.CreateDigitalOutputAsync(parentSocket, SocketPinNumber.Six, GpioPinValue.Low, GpioSharingMode.Exclusive);
		}

		public void TurnAllOn() {
			this.outputPin.Value = GpioPinValue.High;
		}

		public void TurnAllOff() {
			this.outputPin.Value = GpioPinValue.Low;
		}
	}
}