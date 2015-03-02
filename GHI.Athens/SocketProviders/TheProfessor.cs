using GHI.Athens.Gadgeteer;
using System.Threading.Tasks;

namespace GHI.Athens.SocketProviders {
	public class TheProfessor : Module {
		public override string Name { get; } = "The Professor";
		public override string Manufacturer { get; } = "GHI Electronics, LLC";
		protected override int RequiredSockets { get; } = 0;

		protected async override Task Initialize() {
			var s0 = await DeviceIdFinder.GetGpioIdFromFriendlyName("GPIO_S0");
			var s5 = await DeviceIdFinder.GetGpioIdFromFriendlyName("GPIO_S5");

			Socket socket;

			socket = this.AddProvidedSocket(1);
			socket.AddSupportedTypes(SocketType.S, SocketType.U, SocketType.Y);
			socket.AddGpioPinDefinition(SocketPinNumber.Three, new GpioPinDefinition(s0, 62));
			socket.AddGpioPinDefinition(SocketPinNumber.Four, new GpioPinDefinition(s0, 74));
			socket.AddGpioPinDefinition(SocketPinNumber.Five, new GpioPinDefinition(s0, 75));
			socket.AddGpioPinDefinition(SocketPinNumber.Six, new GpioPinDefinition(s0, 66));
			socket.AddGpioPinDefinition(SocketPinNumber.Seven, new GpioPinDefinition(s0, 68));
			socket.AddGpioPinDefinition(SocketPinNumber.Eight, new GpioPinDefinition(s0, 67));
			socket.AddGpioPinDefinition(SocketPinNumber.Nine, new GpioPinDefinition(s0, 69));

			socket = this.AddProvidedSocket(2);
			socket.AddSupportedTypes(SocketType.P, SocketType.U, SocketType.Y);
			socket.AddGpioPinDefinition(SocketPinNumber.Three, new GpioPinDefinition(s0, 54));
			socket.AddGpioPinDefinition(SocketPinNumber.Four, new GpioPinDefinition(s0, 71));
			socket.AddGpioPinDefinition(SocketPinNumber.Five, new GpioPinDefinition(s0, 70));
			socket.AddGpioPinDefinition(SocketPinNumber.Six, new GpioPinDefinition(s0, 72));
			socket.AddGpioPinDefinition(SocketPinNumber.Seven, new GpioPinDefinition(s0, 73));
			socket.AddGpioPinDefinition(SocketPinNumber.Eight, new GpioPinDefinition(s0, 94));
			socket.AddGpioPinDefinition(SocketPinNumber.Nine, new GpioPinDefinition(s0, 95));

			socket = this.AddProvidedSocket(3);
			socket.AddSupportedTypes(SocketType.A, SocketType.I, SocketType.Y);
			socket.AddGpioPinDefinition(SocketPinNumber.Three, new GpioPinDefinition(s5, 0));
			socket.AddGpioPinDefinition(SocketPinNumber.Four, new GpioPinDefinition(s0, 63));
			socket.AddGpioPinDefinition(SocketPinNumber.Five, new GpioPinDefinition(s5, 1));
			socket.AddGpioPinDefinition(SocketPinNumber.Six, new GpioPinDefinition(s5, 2));
			socket.AddGpioPinDefinition(SocketPinNumber.Seven, new GpioPinDefinition(s0, 65));
			socket.AddGpioPinDefinition(SocketPinNumber.Eight, new GpioPinDefinition(s0, 88));
			socket.AddGpioPinDefinition(SocketPinNumber.Nine, new GpioPinDefinition(s0, 89));
			socket.I2CDeviceId = await DeviceIdFinder.GetI2CId();
        }
	}
}