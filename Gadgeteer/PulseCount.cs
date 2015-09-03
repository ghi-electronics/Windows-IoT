using GHIElectronics.UWP.GadgeteerCore;
using System.Threading.Tasks;

using GSI = GHIElectronics.UWP.GadgeteerCore.SocketInterfaces;

namespace GHIElectronics.UWP.Gadgeteer.Modules {
	public class PulseCount : RotaryH1 {
		public override string Name => "PulseCount";

		public async Task<GSI.DigitalIO> CreateInput() {
			return await this.socket.CreateDigitalIOAsync(SocketPinNumber.Three);
		}
	}
}