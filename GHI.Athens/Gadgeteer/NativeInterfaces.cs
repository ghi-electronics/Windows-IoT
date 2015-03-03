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
	}

	internal class DigitalInputOutput : SocketInterfaces.DigitalInputOutput {
		private GpioPinInfo pinInfo;
		private GpioInputPin input;
		private GpioOutputPin output;

		internal DigitalInputOutput(GpioPinInfo pinInfo, GpioInputDriveMode driveMode, bool isOutput, bool initialOutputValue) {
			this.pinInfo = pinInfo;

			this.DriveMode = driveMode;

			if (isOutput) {
				this.Write(initialOutputValue);
			}
			else {
				this.Read();
			}
		}

		public override bool Read() {
			if (this.input == null) {
				this.output.Dispose();
				this.output = null;

				this.pinInfo.TryOpenInput(GpioSharingMode.Exclusive, this.DriveMode, out this.input);
			}

			return this.input.Value == GpioPinValue.High;
		}

		public override void Write(bool value) {
			if (this.output == null) {
				this.input.Dispose();
				this.input = null;

				this.pinInfo.TryOpenOutput(value ? GpioPinValue.High : GpioPinValue.Low, GpioSharingMode.Exclusive, out this.output);
			}
			else {
				this.output.Value = value ? GpioPinValue.High : GpioPinValue.Low;
			}
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