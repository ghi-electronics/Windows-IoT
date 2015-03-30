using GHI.Athens.Gadgeteer;
using GHI.Athens.Gadgeteer.SocketInterfaces;
using System.Threading.Tasks;

namespace GHI.Athens.Modules {
	public class PulseCount : RotaryH1 {
		public override string Name { get; } = "PulseCount";

		public async Task<DigitalIO> CreateInput() {
			return await this.socket.CreateDigitalIOAsync(SocketPinNumber.Three);
		}
	}
}