using GHI.Athens.Gadgeteer;
using GHI.Athens.Gadgeteer.SocketInterfaces;
using System.Threading.Tasks;

namespace GHI.Athens.Modules {
	public class PulseCount : RotaryH1 {
		public override string Name { get; } = "PulseCount";

		public async Task<DigitalInput> CreateInput(Windows.Devices.Gpio.GpioPinDriveMode driveMode) {
			return await this.socket.CreateDigitalInputAsync(SocketPinNumber.Three, driveMode);
		}
	}
}