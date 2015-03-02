using GHI.Athens.Gadgeteer;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace GHI.Athens.Modules {
	public class Button : Module {
		public override string Name { get; } = "Button";
		public override string Manufacturer { get; } = "GHI Electronics, LLC";

		private GpioInputPin inputPin;

		protected async override Task Initialize(Socket parentSocket) {
			this.inputPin = await SocketInterfaces.CreateDigitalInputAsync(parentSocket, SocketPinNumber.Three, GpioSharingMode.Exclusive, GpioInputDriveMode.HighImpedance);
		}

		public bool IsPressed() {
			return this.inputPin.Value == GpioPinValue.Low;
		}
	}
}