using GHI.Athens.Gadgeteer;
using GHI.Athens.Gadgeteer.SocketInterfaces;
using System.Threading.Tasks;

namespace GHI.Athens.Modules {
	public class TempHumidSI70 : Module {
		private static byte MeasureHumidityHold { get; } = 0xE5;
		private static byte ReadTempFromPrevious { get; } = 0xE0;
		private static byte I2CAddress { get; } = 0x40;

		private I2CDevice i2c;
		private byte[] writeBuffer1;
		private byte[] writeBuffer2;
		private byte[] readBuffer1;
		private byte[] readBuffer2;

		public override string Name { get; } = "TempHumid SI70";
		public override string Manufacturer { get; } = "GHI Electronics, LLC";

		public TempHumidSI70() {
			this.writeBuffer1 = new byte[1] { TempHumidSI70.MeasureHumidityHold };
			this.writeBuffer2 = new byte[1] { TempHumidSI70.ReadTempFromPrevious };
			this.readBuffer1 = new byte[2];
			this.readBuffer2 = new byte[2];
		}

		protected async override Task Initialize(Socket parentSocket) {
			this.i2c = await parentSocket.CreateI2CDeviceAsync(new Windows.Devices.I2C.I2CConnectionSettings(TempHumidSI70.I2CAddress), SocketPinNumber.Five, SocketPinNumber.Four);
		}

		public Measurement TakeMeasurement() {
			this.i2c.WriteRead(this.writeBuffer1, this.readBuffer1);
			this.i2c.WriteRead(this.writeBuffer2, this.readBuffer2);

			var rawTemperature = this.readBuffer2[0] << 8 | this.readBuffer2[1];
			var rawHumidity = this.readBuffer1[0] << 8 | this.readBuffer1[1];

			var temperature = 175.72 * rawTemperature / 65536.0 - 46.85;
			var humidity = 125.0 * rawHumidity / 65536.0 - 6.0;

			if (humidity < 0.0)
				humidity = 0.0;

			if (humidity > 100.0)
				humidity = 100.0;

			return new Measurement() { Temperature = temperature, RelativeHumidity = humidity };
		}

		public struct Measurement {
			public double TemperatureFahrenheit { get { return this.Temperature * 1.8 + 32.0; } }
			public double Temperature { get; set; }
			public double RelativeHumidity { get; set; }

			public override string ToString() {
				return this.Temperature.ToString("F1") + " degrees Celsius, " + this.RelativeHumidity.ToString("F1") + "% relative humidity.";
			}
		}
	}
}