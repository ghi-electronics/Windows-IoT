using System;
using Windows.Devices.Gpio;

namespace GHI.Athens.Gadgeteer.NativeInterfaces {
	internal class DigitalInput : SocketInterfaces.DigitalInput {
		private GpioPin pin;

		internal DigitalInput(GpioPin pin, GpioPinDriveMode driveMode) {
			this.pin = pin;

			this.DriveMode = driveMode;
		}

		public override bool Read() {
			return this.pin.Read() == GpioPinValue.High;
		}

		public override GpioPinDriveMode DriveMode {
			get {
				return this.pin.GetDriveMode();
			}
			set {
				this.pin.SetDriveMode(value);
			}
		}
	}

	internal class DigitalOutput : SocketInterfaces.DigitalOutput {
		private GpioPin pin;

		internal DigitalOutput(GpioPin pin, bool initialValue) {
			this.pin = pin;

			this.Write(initialValue);
		}

		public override bool Read() {
			return this.pin.Read() == GpioPinValue.High;
		}

		public override void Write(bool value) {
			this.pin.Write(value ? GpioPinValue.High : GpioPinValue.Low);
		}
	}

	internal class DigitalInterrupt : SocketInterfaces.DigitalInterrupt {
		private GpioPin pin;

		internal DigitalInterrupt(GpioPin pin, GpioPinEdge interruptType, GpioPinDriveMode driveMode) {
			this.pin = pin;
			this.pin.ValueChanged += (a, b) => this.OnInterrupt(b);

			this.InterruptType = interruptType;
			this.DriveMode = driveMode;
		}

		public override bool Read() {
			return this.pin.Read() == GpioPinValue.High;
		}

		public override GpioPinDriveMode DriveMode {
			get {
				return this.pin.GetDriveMode();
			}
			set {
				this.pin.SetDriveMode(value);
			}
		}

		public override GpioPinEdge InterruptType {
			get {
				return this.InterruptType;
			}
			set {
				this.InterruptType = value;
			}
		}
	}

	internal class DigitalInputOutput : SocketInterfaces.DigitalInputOutput {
		private GpioPin pin;
		private GpioPinDriveMode driveMode;

		internal DigitalInputOutput(GpioPin pin, SocketInterfaces.DigitalInputOutputMode mode, GpioPinDriveMode driveMode, bool initialOutputValue) {
			this.pin = pin;
			this.driveMode = driveMode;

			this.Mode = mode;

			if (mode == SocketInterfaces.DigitalInputOutputMode.Output) {
				this.Write(initialOutputValue);
			}
			else {
				this.Read();
			}
		}

		public override bool Read() {
			return this.pin.Read() == GpioPinValue.High;
		}

		public override void Write(bool value) {
			this.pin.Write(value ? GpioPinValue.High : GpioPinValue.Low);
		}

		public override GpioPinDriveMode DriveMode {
			get {
				return this.pin.GetDriveMode();
			}
			set {
				this.pin.SetDriveMode(value);
			}
		}
	}

	internal class AnalogInput : SocketInterfaces.AnalogInput {
		public override double MaxVoltage { get; } = 3.3;

		public override double ReadVoltage() {
			throw new NotImplementedException();
		}
	}

	internal class AnalogOutput : SocketInterfaces.AnalogOutput {
		public override double MaxVoltage { get; } = 3.3;

		public override void WriteVoltage(double voltage) {
			throw new NotImplementedException();
		}
	}

	internal class PwmOutput : SocketInterfaces.PwmOutput {
		protected override void SetEnabled(bool state) {
			throw new NotImplementedException();
		}

		protected override void SetValues(double frequency, double dutyCycle) {
			throw new NotImplementedException();
		}
	}

	internal class I2CDevice : SocketInterfaces.I2CDevice {
		private Windows.Devices.I2C.I2CDevice device;

        internal I2CDevice(Windows.Devices.I2C.I2CDevice device) {
			this.device = device;
		}

		public override Windows.Devices.I2C.I2CTransferStatus Write(byte[] buffer, out uint transferred) {
			return this.device.TryWrite(buffer, out transferred);
		}

		public override Windows.Devices.I2C.I2CTransferStatus Read(byte[] buffer, out uint transferred) {
			return this.device.TryRead(buffer, out transferred);
		}

		public override Windows.Devices.I2C.I2CTransferStatus WriteRead(byte[] writeBuffer, byte[] readBuffer, out uint transferred) {
			return this.device.TryWriteRead(writeBuffer, readBuffer, out transferred);
		}
	}

	internal class SpiDevice : SocketInterfaces.SpiDevice {
		internal SpiDevice(SocketInterfaces.SpiConfiguration configuration, SocketInterfaces.DigitalOutput slaveSelect) {

		}

		public override void WriteRead(byte[] writeBuffer, byte[] readBuffer) {
			throw new NotImplementedException();
		}
	}

	internal class SerialDevice : SocketInterfaces.SerialDevice {

	}

	internal class CanDevice : SocketInterfaces.CanDevice {

	}
}