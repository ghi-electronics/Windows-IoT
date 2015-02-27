using GHI.Athens.Gadgeteer;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace GHI.Athens.Modules {
	public class Button : Module {
		public override string Name { get; } = "Button";
		public override string Manufacturer { get; } = "GHI Electronics, LLC";

		private GpioInputPin inputPin;

		private Button() {

		}

		public async static Task<Button> Create(Socket socket) {
			var button = new Button();

			button.inputPin = await SocketInterfaces.CreateDigitalInputAsync(socket, SocketPinNumber.Three, GpioSharingMode.Exclusive, GpioInputDriveMode.HighImpedance);

			return button;
		}

		public bool IsPressed() {
			return this.inputPin.Value == GpioPinValue.Low;
		}
	}
}