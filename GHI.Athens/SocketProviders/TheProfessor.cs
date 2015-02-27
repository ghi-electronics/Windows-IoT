using GHI.Athens.Gadgeteer;

namespace GHI.Athens.SocketProviders {
	public class TheProfessor : SocketProvider {
		public override string Name { get; } = "The Professor";
		public override string Manufacturer { get; } = "GHI Electronics";

		public TheProfessor() {
			var s0 = GpioPinDefinition.GetDeviceIdFromFriendlyName("GPIO_S0");
			var s5 = GpioPinDefinition.GetDeviceIdFromFriendlyName("GPIO_S5");

			Socket socket;

			socket = this.CreateSocket(1);
			socket.AddSupportedTypes(SocketType.S, SocketType.U, SocketType.Y);
			socket.AddGpioPinDefinition(SocketPinNumber.Three, new GpioPinDefinition(s0, 62));
			socket.AddGpioPinDefinition(SocketPinNumber.Four, new GpioPinDefinition(s0, 74));
			socket.AddGpioPinDefinition(SocketPinNumber.Five, new GpioPinDefinition(s0, 75));
			socket.AddGpioPinDefinition(SocketPinNumber.Six, new GpioPinDefinition(s0, 66));
			socket.AddGpioPinDefinition(SocketPinNumber.Seven, new GpioPinDefinition(s0, 68));
			socket.AddGpioPinDefinition(SocketPinNumber.Eight, new GpioPinDefinition(s0, 67));
			socket.AddGpioPinDefinition(SocketPinNumber.Nine, new GpioPinDefinition(s0, 69));


			socket = this.CreateSocket(2);
			socket.AddSupportedTypes(SocketType.P, SocketType.U, SocketType.Y);
			socket.AddGpioPinDefinition(SocketPinNumber.Three, new GpioPinDefinition(s0, 54));
			socket.AddGpioPinDefinition(SocketPinNumber.Four, new GpioPinDefinition(s0, 71));
			socket.AddGpioPinDefinition(SocketPinNumber.Five, new GpioPinDefinition(s0, 70));
			socket.AddGpioPinDefinition(SocketPinNumber.Six, new GpioPinDefinition(s0, 72));
			socket.AddGpioPinDefinition(SocketPinNumber.Seven, new GpioPinDefinition(s0, 73));
			socket.AddGpioPinDefinition(SocketPinNumber.Eight, new GpioPinDefinition(s0, 94));
			socket.AddGpioPinDefinition(SocketPinNumber.Nine, new GpioPinDefinition(s0, 95));


			socket = this.CreateSocket(2);
			socket.AddSupportedTypes(SocketType.A, SocketType.I, SocketType.Y);
			socket.AddGpioPinDefinition(SocketPinNumber.Three, new GpioPinDefinition(s5, 0));
			socket.AddGpioPinDefinition(SocketPinNumber.Four, new GpioPinDefinition(s0, 63));
			socket.AddGpioPinDefinition(SocketPinNumber.Five, new GpioPinDefinition(s5, 1));
			socket.AddGpioPinDefinition(SocketPinNumber.Six, new GpioPinDefinition(s5, 2));
			socket.AddGpioPinDefinition(SocketPinNumber.Seven, new GpioPinDefinition(s0, 65));
			socket.AddGpioPinDefinition(SocketPinNumber.Eight, new GpioPinDefinition(s0, 88));
			socket.AddGpioPinDefinition(SocketPinNumber.Nine, new GpioPinDefinition(s0, 89));
			socket.I2CDeviceId = @"\\?\ACPI#80860F41#6#{a11ee3c6-8421-4202-a3e7-b91ff90188e4}";
		}
	}
}
