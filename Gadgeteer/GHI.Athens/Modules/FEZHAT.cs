using GHI.Athens.Gadgeteer;
using System.Threading.Tasks;

namespace GHI.Athens.Modules {
	public class FEZHAT : Module {
		public override string Name { get; } = "FEZHAT";
		public override string Manufacturer { get; } = "GHI Electronics, LLC";
		public override uint RequiredSockets { get; } = 0;

		protected async override Task Initialize() {
			Socket socket;

			socket = this.CreateSocket(1);
			socket.AddSupportedTypes(SocketType.Y);
            socket.SetNativePin(SocketPinNumber.Eight, 27);
            socket.SetNativePin(SocketPinNumber.Nine, 22);
        }
	}
}