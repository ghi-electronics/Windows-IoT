using System;
using Windows.Storage.Streams;
using WD = Windows.Devices;

namespace GHI.Athens.Gadgeteer.NativeInterfaces {
	internal class DigitalIO : SocketInterfaces.DigitalIO {
		private WD.Gpio.GpioPin pin;

		internal DigitalIO(WD.Gpio.GpioPin pin) {
			this.pin = pin;
		}

		private void OnInterrupt(WD.Gpio.GpioPin sender, WD.Gpio.GpioPinValueChangedEventArgs e) {
			this.OnValueChanged(e.Edge == WD.Gpio.GpioPinEdge.RisingEdge);
		}

		protected override void AddInterrupt() {
			this.pin.ValueChanged += this.OnInterrupt;
		}

		protected override void RemoveInterrupt() {
			this.pin.ValueChanged -= this.OnInterrupt;
		}

		protected override bool ReadInternal() {
			return this.pin.Read() == WD.Gpio.GpioPinValue.High;
		}

		protected override void WriteInternal(bool value) {
			this.pin.Write(value ? WD.Gpio.GpioPinValue.High : WD.Gpio.GpioPinValue.Low);
		}

		public override WD.Gpio.GpioPinDriveMode DriveMode {
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
		public override WD.Gpio.GpioPinDriveMode DriveMode { get; set; }

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
		private WD.I2C.I2CDevice device;

        internal I2CDevice(WD.I2C.I2CDevice device) {
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
		private WD.Spi.SpiDevice device;

		internal SpiDevice(WD.Spi.SpiDevice device) {
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
		private WD.SerialCommunication.SerialDevice device;
		private DataWriter writer;
		private DataReader reader;

		public override string PortName { get { return this.device.PortName; } }
		public override uint BaudRate { get { return this.device.BaudRate; } set { this.device.BaudRate = value; } }
		public override ushort DataBits { get { return this.device.DataBits; } set { this.device.DataBits = value; } }
		public override WD.SerialCommunication.SerialHandshake Handshake { get { return this.device.Handshake; } set { this.device.Handshake = value; } }
		public override WD.SerialCommunication.SerialParity Parity { get { return this.device.Parity; } set { this.device.Parity = value; } }
		public override WD.SerialCommunication.SerialStopBitCount StopBits { get { return this.device.StopBits; } set { this.device.StopBits = value; } }

		internal SerialDevice(WD.SerialCommunication.SerialDevice device) {
			this.device = device;
			this.writer = new DataWriter(this.device.OutputStream);
			this.reader = new DataReader(this.device.InputStream);
		}

		public override void Write(byte[] buffer) {
			this.writer.WriteBytes(buffer);
		}

		public override void Read(byte[] buffer) {
			this.reader.ReadBytes(buffer);
		}
	}

	internal class CanDevice : SocketInterfaces.CanDevice {

	}
}