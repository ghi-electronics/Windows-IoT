using GHI.Athens.Gadgeteer;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace GHI.Athens.Modules {
	public class LEDStrip : Module {
		public override string Name { get; } = "LED Strip";
		public override string Manufacturer { get; } = "GHI Electronics, LLC";

		private GpioOutputPin outputPin;

		private LEDStrip() {

		}

		public async static Task<LEDStrip> Create(Socket socket) {
			var module = new LEDStrip();

			module.outputPin = await SocketInterfaces.CreateDigitalOutputAsync(socket, SocketPinNumber.Six, GpioPinValue.Low, GpioSharingMode.Exclusive);

			return module;
		}

		public void TurnAllOn() {
			this.outputPin.Value = GpioPinValue.High;
		}

		public void TurnAllOff() {
			this.outputPin.Value = GpioPinValue.Low;
		}
	}
}