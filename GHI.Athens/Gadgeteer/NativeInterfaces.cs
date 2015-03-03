using System;
using Windows.Devices.Gpio;

namespace GHI.Athens.Gadgeteer.NativeInterfaces {
	internal class DigitalInput : SocketInterfaces.DigitalInput {
		private GpioInputPin pin;

		internal DigitalInput(GpioInputPin pin) {
			this.pin = pin;
		}

		public override bool Read() {
			return this.pin.Value == GpioPinValue.High;
		}

		public override GpioInputDriveMode DriveMode {
			get {
				return this.pin.InputDriveMode;
			}
			set {
				throw new NotSupportedException();
			}
		}
	}

	internal class DigitalOutput : SocketInterfaces.DigitalOutput {
		private GpioOutputPin pin;

		internal DigitalOutput(GpioOutputPin pin) {
			this.pin = pin;
		}

		public override bool Read() {
			return this.pin.Value == GpioPinValue.High;
		}

		public override void Write(bool value) {
			this.pin.Value = value ? GpioPinValue.High : GpioPinValue.Low;
		}
	}

	internal class DigitalInterrupt : SocketInterfaces.DigitalInterrupt {
		private GpioInterruptPin pin;

		internal DigitalInterrupt(GpioInterruptPin pin) {
			this.pin = pin;
			this.pin.InterruptRaised += (a, b) => this.OnInterrupt(b);
		}

		public override bool Read() {
			return this.pin.Value == GpioPinValue.High;
		}

		public override GpioInputDriveMode DriveMode {
			get {
				return this.pin.InputDriveMode;
			}
			set {
				throw new NotSupportedException();
			}
		}

		public override GpioInterruptType InterruptType {
			get {
				return this.pin.InterruptType;
			}
			set {
				throw new NotSupportedException();
			}
		}
	}

	internal class DigitalInputOutput : SocketInterfaces.DigitalInputOutput {
		private GpioPinInfo pinInfo;
		private GpioInputPin input;
		private GpioOutputPin output;
		private GpioInputDriveMode driveMode;

		internal DigitalInputOutput(GpioPinInfo pinInfo, GpioInputDriveMode driveMode, bool isOutput, bool initialOutputValue) {
			this.pinInfo = pinInfo;
			this.driveMode = driveMode;

			if (isOutput) {
				this.Write(initialOutputValue);
			}
			else {
				this.Read();
			}
		}

		public override bool Read() {
			if (this.input == null) {
				this.output?.Dispose();
				this.output = null;

				this.pinInfo.TryOpenInput(GpioSharingMode.Exclusive, this.driveMode, out this.input);
			}

			return this.input.Value == GpioPinValue.High;
		}

		public override void Write(bool value) {
			if (this.output == null) {
				this.input?.Dispose();
				this.input = null;

				this.pinInfo.TryOpenOutput(value ? GpioPinValue.High : GpioPinValue.Low, GpioSharingMode.Exclusive, out this.output);
			}
			else {
				this.output.Value = value ? GpioPinValue.High : GpioPinValue.Low;
			}
		}

		public override GpioInputDriveMode DriveMode {
			get {
				return this.driveMode;
			}
			set {
				this.driveMode = value;
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
}