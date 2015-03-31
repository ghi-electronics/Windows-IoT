using GHI.Athens.Gadgeteer;
using GHI.Athens.Gadgeteer.SocketInterfaces;
using System.Threading.Tasks;

namespace GHI.Athens.Modules {
	public class ADS7830 {
		private static byte CmdSdSe { get; } = 0x80;
		private static byte CmdPdOff { get; } = 0x00;
		private static byte CmdPdOn { get; } = 0x04;

		private byte Address { get { return (byte)(0x48 | (this.A0 ? 1 : 0) | (this.A1 ? 2 : 0)); } }

		public bool A0 { get; set; } = false;
		public bool A1 { get; set; } = false;

		private I2CDevice i2c;

		public async Task Initialize(ISocket socket) {
			this.i2c = await socket.CreateI2CDeviceAsync(new Windows.Devices.I2C.I2CConnectionSettings(this.Address));
		}

		public double ReadVoltage(byte channel) {
			var command = new byte[] { (byte)(ADS7830.CmdSdSe | ADS7830.CmdPdOn) };
			var read = new byte[1];

			command[0] |= (byte)((channel % 2 == 0 ? channel / 2 : (channel - 1) / 2 + 4) << 4);

			this.i2c.WriteRead(command, read);

			return (double)read[0] / 255.0 * 3.3;
		}
	}
}
