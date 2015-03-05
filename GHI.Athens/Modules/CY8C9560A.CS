using GHI.Athens.Gadgeteer;
using GHI.Athens.Gadgeteer.SocketInterfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace GHI.Athens.Modules {
	public class CY8C9560A {
		private static byte InputPortRegister { get; } = 0x00;
		private static byte OutputPortRegister { get; } = 0x08;
		private static byte InterruptPortRegister { get; } = 0x10;
		private static byte PortSelectRegister { get; } = 0x18;
		private static byte InterruptMaskRegister { get; } = 0x19;
		private static byte PinDirectionRegister { get; } = 0x1C;
		private static byte PwmEnableRegister { get; } = 0x1A;
		private static byte PwmSelectRegister { get; } = 0x28;
		private static byte PwmConfigRegister { get; } = 0x29;
		private static byte PwmPulseWidthRegister { get; } = 0x2B;
		private static byte PwmPeriodRegister { get; } = 0x2A;

		private static byte PinPullUp { get; } = 0x1D;
		private static byte PinPullDown { get; } = 0x1E;
		private static byte PinStrongDrive { get; } = 0x21;
		private static byte PinHighImpedance { get; } = 0x23;
		private static byte PwmClockSource { get; } = 0x03;

		private I2CDevice i2c;

		public struct Pin {
			public byte Port { get; set; }
			public byte PinNumber { get; set; }
			public byte Mask { get { return (byte)(1 << this.PinNumber); } }
		}

		public async Task Initialize(Socket socket) {
			this.i2c = await socket.CreateI2CDeviceAsync(new Windows.Devices.I2C.I2CConnectionSettings(0x20, Windows.Devices.I2C.I2CBusSpeed.StandardMode, Windows.Devices.I2C.I2CAddressingMode.SevenBit));
		}

		public void SetPwm(Pin pin, double frequency, double dutyCycle) {
			this.i2c.WriteRegister(CY8C9560A.PortSelectRegister, pin.Port);

			var mask = pin.Mask;

			var val = this.i2c.ReadRegister(CY8C9560A.PwmEnableRegister);
			this.i2c.WriteRegister(CY8C9560A.PwmEnableRegister, (byte)(val | mask));

			this.WriteDigital(pin, true);

			var pwm = pin.PinNumber;

			if (pin.Port == 7)
				pwm = (byte)(pin.PinNumber + 8);

			this.i2c.WriteRegister(CY8C9560A.PwmSelectRegister, pwm);
			this.i2c.WriteRegister(CY8C9560A.PwmConfigRegister, CY8C9560A.PwmClockSource);

			val = this.i2c.ReadRegister(CY8C9560A.PinStrongDrive);
			this.i2c.WriteRegister(CY8C9560A.PinStrongDrive, (byte)(val | mask));

			val = this.i2c.ReadRegister(CY8C9560A.InterruptMaskRegister);
			this.i2c.WriteRegister(CY8C9560A.InterruptMaskRegister, (byte)(val | mask));

			var period = (byte)(93750 / frequency);

			this.i2c.WriteRegister(CY8C9560A.PwmPeriodRegister, period);
			this.i2c.WriteRegister(CY8C9560A.PwmPulseWidthRegister, (byte)(period * dutyCycle));
		}

		public void SetOutput(Pin pin) {
			this.i2c.WriteRegister(CY8C9560A.PortSelectRegister, pin.Port);

			var mask = pin.Mask;

			var val = this.i2c.ReadRegister(CY8C9560A.PwmEnableRegister);
			this.i2c.WriteRegister(CY8C9560A.PwmEnableRegister, (byte)(val & ~mask));

			val = this.i2c.ReadRegister(CY8C9560A.PinDirectionRegister);
			this.i2c.WriteRegister(CY8C9560A.PinDirectionRegister, (byte)(val & ~mask));

			val = this.i2c.ReadRegister(CY8C9560A.PinStrongDrive);
			this.i2c.WriteRegister(CY8C9560A.PinStrongDrive, (byte)(val | mask));

			val = this.i2c.ReadRegister(CY8C9560A.InterruptMaskRegister);
			this.i2c.WriteRegister(CY8C9560A.InterruptMaskRegister, (byte)(val | mask));
		}

		public void SetInput(Pin pin, GpioInputDriveMode driveMode) {
			this.i2c.WriteRegister(CY8C9560A.PortSelectRegister, pin.Port);

			var mask = pin.Mask;

			var val = this.i2c.ReadRegister(CY8C9560A.PwmEnableRegister);
			this.i2c.WriteRegister(CY8C9560A.PwmEnableRegister, (byte)(val & ~mask));

			val = this.i2c.ReadRegister(CY8C9560A.PinDirectionRegister);
			this.i2c.WriteRegister(CY8C9560A.PinDirectionRegister, (byte)(val | mask));

			byte resistorValue = 0;

			switch (driveMode) {
				case GpioInputDriveMode.HighImpedance: resistorValue = CY8C9560A.PinHighImpedance; break;
				case GpioInputDriveMode.PullDown: resistorValue = CY8C9560A.PinPullDown; break;
				case GpioInputDriveMode.PullUp: resistorValue = CY8C9560A.PinPullUp; break;
			}

			val = this.i2c.ReadRegister(resistorValue);
			this.i2c.WriteRegister(resistorValue, (byte)(val | mask));

			val = this.i2c.ReadRegister(CY8C9560A.InterruptMaskRegister);
			this.i2c.WriteRegister(CY8C9560A.InterruptMaskRegister, (byte)(val | mask));
		}

		public bool ReadDigital(Pin pin) {
			return (this.i2c.ReadRegister((byte)(CY8C9560A.InputPortRegister + pin.Port)) & pin.Mask) != 0;
		}

		public void WriteDigital(Pin pin, bool value) {
			var b = this.i2c.ReadRegister((byte)(CY8C9560A.OutputPortRegister + pin.Port));

			if (value)
				b |= pin.Mask;
			else
				b = (byte)(b & ~pin.Mask);

			this.i2c.WriteRegister((byte)(CY8C9560A.OutputPortRegister + pin.Port), b);
		}
	}
}