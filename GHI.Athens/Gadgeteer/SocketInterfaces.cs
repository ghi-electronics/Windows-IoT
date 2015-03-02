using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Devices.I2C;
using Windows.Foundation;

namespace GHI.Athens.Gadgeteer.SocketInterfaces {
	public delegate Task<DigitalInput> DigitalInputCreator(Socket socket, SocketPinNumber pinNumber, GpioInputDriveMode driveMode);
	public delegate Task<DigitalOutput> DigitalOutputCreator(Socket socket, SocketPinNumber pinNumber, bool initialValue);
	public delegate Task<DigitalInterrupt> DigitalInterruptCreator(Socket socket, SocketPinNumber pinNumber, GpioInterruptType interruptType, GpioInputDriveMode driveMode);
	public delegate Task<AnalogInput> AnalogInputCreator(Socket socket, SocketPinNumber pinNumber);
	public delegate Task<I2CDevice> I2CDeviceCreator(Socket socket);

	public abstract class DigitalOutput {
		public abstract void Write(bool value);
		public abstract bool Read();

		public bool Value {
			get {
				return this.Read();
			}
			set {
				this.Write(value);
			}
		}
	}

	public abstract class DigitalInput {
		public abstract bool Read();

		public bool Value {
			get {
				return this.Read();
			}
		}

		public GpioInputDriveMode DriveMode { get; set; }
	}

	public abstract class DigitalInterrupt : DigitalInput {
		public GpioInterruptType InterruptType { get; set; }

		public event TypedEventHandler<DigitalInterrupt, GpioInterruptEventArgs> Interrupt;

		protected void OnInterrupt(GpioInterruptEventArgs e) {
			this.Interrupt?.Invoke(this, e);
		}
	}

	public abstract class AnalogInput {
		public abstract double MaxVoltage { get; }

		public abstract double ReadVoltage();

		public double ReadProportion() {
			return this.ReadVoltage() / this.MaxVoltage;
		}
	}

	public abstract class I2CDevice {
		public abstract I2CTransferStatus WriteRead(byte[] writeBuffer, byte[] readBuffer, out uint transferred);

		public I2CTransferStatus WriteRead(byte[] writeBuffer, byte[] readBuffer) {
			uint transferred;

			return this.WriteRead(writeBuffer, readBuffer, out transferred);
		}

		public I2CTransferStatus Write(byte[] buffer) {
			uint transferred;

			return this.Write(buffer, out transferred);
		}

		public I2CTransferStatus Write(byte[] buffer, out uint transferred) {
			return this.WriteRead(buffer, new byte[0], out transferred);
		}

		public I2CTransferStatus Read(byte[] buffer) {
			uint transferred;

			return this.Read(buffer, out transferred);
		}

		public I2CTransferStatus Read(byte[] buffer, out uint transferred) {
			return this.WriteRead(new byte[0], buffer, out transferred);
		}
	}
}