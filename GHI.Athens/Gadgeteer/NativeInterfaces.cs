using System;
using Windows.Devices.Gpio;

namespace GHI.Athens.Gadgeteer.NativeInterfaces {
	internal class DigitalIO : SocketInterfaces.DigitalIO {
		private GpioPin pin;

		internal DigitalIO(GpioPin pin) {
			this.pin = pin;
		}

		private void OnInterrupt(GpioPin sender, GpioPinValueChangedEventArgs e) {
			this.OnValueChanged(e.Edge == GpioPinEdge.RisingEdge);
		}

		protected override void AddInterrupt() {
			this.pin.ValueChanged += this.OnInterrupt;
		}

		protected override void RemoveInterrupt() {
			this.pin.ValueChanged -= this.OnInterrupt;
		}

		protected override bool ReadInternal() {
			return this.pin.Read() == GpioPinValue.High;
		}

		protected override void WriteInternal(bool value) {
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

	internal class AnalogIO : SocketInterfaces.AnalogIO {
		public override double MaxVoltage { get; } = 3.3;
		public override GpioPinDriveMode DriveMode { get; set; }

		protected override double ReadInternal() {
			throw new NotImplementedException();
		}

		protected override void WriteInternal(double voltage) {
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

		public override void Write(byte[] buffer) {
			this.device.Write(buffer);
		}

		public override void Read(byte[] buffer) {
			this.device.Read(buffer);
		}

		public override void WriteRead(byte[] writeBuffer, byte[] readBuffer) {
			this.device.WriteRead(writeBuffer, readBuffer);
		}
	}

	internal class SpiDevice : SocketInterfaces.SpiDevice {
		private Windows.Devices.Spi.SpiDevice device;

		internal SpiDevice(Windows.Devices.Spi.SpiDevice device) {
			this.device = device;
		}

		protected override void WriteRead(byte[] writeBuffer, byte[] readBuffer) {
			if (writeBuffer != null && readBuffer != null) {
				this.device.TransferFullDuplex(writeBuffer, readBuffer);
			}
			else if (writeBuffer != null) {
				this.device.Write(writeBuffer);
			}
			else if (readBuffer != null) {
				this.device.Read(readBuffer);
			}
		}
	}

	internal class SerialDevice : SocketInterfaces.SerialDevice {

	}

	internal class CanDevice : SocketInterfaces.CanDevice {

	}
}