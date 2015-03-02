using GHI.Athens.Gadgeteer;
using GHI.Athens.Gadgeteer.SocketInterfaces;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace GHI.Athens.Modules {
	public class Button : Module {
		public override string Name { get; } = "Button";
		public override string Manufacturer { get; } = "GHI Electronics, LLC";

		private DigitalInput inputPin;

		protected async override Task Initialize(Socket parentSocket) {
			this.inputPin = await parentSocket.CreateDigitalInputAsync(SocketPinNumber.Three, GpioInputDriveMode.HighImpedance);
		}

		public bool IsPressed() {
			return !this.inputPin.Read();
		}
	}
}