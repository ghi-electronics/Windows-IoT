using GHI.Athens.Gadgeteer;
using GHI.Athens.Gadgeteer.SocketInterfaces;
using System.Threading.Tasks;

namespace GHI.Athens.Modules {
	public class TempHumidSI70 : Module {
		private const byte MEASURE_HUMIDITY_HOLD = 0xE5;
		private const byte READ_TEMP_FROM_PREVIOUS = 0xE0;
		private const byte I2C_ADDRESS = 0x40;

		private I2CDevice i2c;
		private byte[] writeBuffer1;
		private byte[] writeBuffer2;
		private byte[] readBuffer1;
		private byte[] readBuffer2;

		public override string Name { get; } = "TempHumid SI70";
		public override string Manufacturer { get; } = "GHI Electronics, LLC";

		public TempHumidSI70() {
			this.writeBuffer1 = new byte[1] { TempHumidSI70.MEASURE_HUMIDITY_HOLD };
			this.writeBuffer2 = new byte[1] { TempHumidSI70.READ_TEMP_FROM_PREVIOUS };
			this.readBuffer1 = new byte[2];
			this.readBuffer2 = new byte[2];
		}

		protected async override Task Initialize(Socket parentSocket) {
			this.i2c = await parentSocket.CreateI2CDeviceAsync(SocketPinNumber.Five, SocketPinNumber.Four, new Windows.Devices.I2C.I2CConnectionSettings(TempHumidSI70.I2C_ADDRESS, Windows.Devices.I2C.I2CBusSpeed.StandardMode, Windows.Devices.I2C.I2CAddressingMode.SevenBit));
		}

		/// <summary>
		/// Obtains a single measurement.
		/// </summary>
		/// <returns>The measurement.</returns>
		public Measurement TakeMeasurement() {
			if (this.i2c.WriteRead(this.writeBuffer1, this.readBuffer1) != Windows.Devices.I2C.I2CTransferStatus.Success) return null;
			if (this.i2c.WriteRead(this.writeBuffer2, this.readBuffer2) != Windows.Devices.I2C.I2CTransferStatus.Success) return null;

			int rawRH = this.readBuffer1[0] << 8 | this.readBuffer1[1];
			int rawTemp = this.readBuffer2[0] << 8 | this.readBuffer2[1];

			double temperature = 175.72 * rawTemp / 65536.0 - 46.85;
			double relativeHumidity = 125.0 * rawRH / 65536.0 - 6.0;

			if (relativeHumidity < 0.0)
				relativeHumidity = 0.0;

			if (relativeHumidity > 100.0)
				relativeHumidity = 100.0;

			return new Measurement(temperature, relativeHumidity);
		}

		/// <summary>
		/// Result of a measurement.
		/// </summary>
		public class Measurement {
			/// <summary>
			/// The measured temperature in degrees Celsius.
			/// </summary>
			public double Temperature { get; private set; }

			/// <summary>
			/// The measured temperature in degrees Fahrenheit.
			/// </summary>
			public double TemperatureFahrenheit { get; private set; }

			/// <summary>
			/// The measured relative humidity.
			/// </summary>
			public double RelativeHumidity { get; private set; }

			/// <summary>
			/// Provides a string representation of the instance.
			/// </summary>
			/// <returns>A string describing the values contained in the object.</returns>
			public override string ToString() {
				return this.Temperature.ToString("F1") + " degrees Celsius, " + this.RelativeHumidity.ToString("F1") + "% relative humidity.";
			}

			internal Measurement(double temperature, double relativeHumidity) {
				this.RelativeHumidity = relativeHumidity;
				this.Temperature = temperature;
				this.TemperatureFahrenheit = temperature * 1.8 + 32.0;
			}
		}
	}
}