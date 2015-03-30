using GHI.Athens.Gadgeteer;
using GHI.Athens.Gadgeteer.SocketInterfaces;
using System;
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
		private static byte PinPullUpRegister { get; } = 0x1D;
		private static byte PinPullDownRegister { get; } = 0x1E;
		private static byte PinStrongDriveRegister { get; } = 0x21;
		private static byte PinHighImpedanceRegister { get; } = 0x23;
		private static byte PwmSelectRegister { get; } = 0x28;
		private static byte PwmConfigRegister { get; } = 0x29;
		private static byte PwmPeriodRegister { get; } = 0x2A;
		private static byte PwmPulseWidthRegister { get; } = 0x2B;

		private static byte PwmClockSource32KHz { get; } = 0x00;
		private static byte PwmClockSource24MHz { get; } = 0x01;
		private static byte PwmClockSource1MHz { get; } = 0x02;
		private static byte PwmClockSource94KHz { get; } = 0x03;
		private static byte PwmClockSource367Hz { get; } = 0x04;

		private I2CDevice i2c;

		public struct Pin {
			public byte Port { get; set; }
			public byte PinNumber { get; set; }
			public byte Mask { get { return (byte)(1 << this.PinNumber); } }
		}

		public async Task Initialize(Socket socket) {
			this.i2c = await socket.CreateI2CDeviceAsync(new Windows.Devices.I2C.I2CConnectionSettings(0x20));
		}

		public void SetPwm(Pin pin, double frequency, double dutyCycle) {
			this.i2c.WriteRegister(CY8C9560A.PortSelectRegister, pin.Port);

			var val = this.i2c.ReadRegister(CY8C9560A.PinStrongDriveRegister);
			this.i2c.WriteRegister(CY8C9560A.PinStrongDriveRegister, (byte)(val | pin.Mask));

			val = this.i2c.ReadRegister(CY8C9560A.InterruptMaskRegister);
			this.i2c.WriteRegister(CY8C9560A.InterruptMaskRegister, (byte)(val | pin.Mask));
			
			this.WriteDigital(pin, true);

			val = this.i2c.ReadRegister(CY8C9560A.PwmEnableRegister);
			this.i2c.WriteRegister(CY8C9560A.PwmEnableRegister, (byte)(val | pin.Mask));
			
			var period = 0.0;
			byte clockSource = 0;

			if (frequency <= 1.45) {
				throw new ArgumentOutOfRangeException("frequency", "The frequency is too low.");
			}
			else if (frequency <= 125.5) {
				period = 367.6 / frequency;
				clockSource = CY8C9560A.PwmClockSource367Hz;
			}
			else if (frequency <= 367.7) {
				period = 32000.0 / frequency;
				clockSource = CY8C9560A.PwmClockSource32KHz;
			}
			else if (frequency <= 5882.4) {
				period = 93750.0 / frequency;
				clockSource = CY8C9560A.PwmClockSource94KHz;
			}
			else if (frequency <= 94117.7) {
				period = 1500000.0 / frequency;
				clockSource = CY8C9560A.PwmClockSource1MHz;
			}
			else if (frequency <= 12000000.0) {
				period = 24000000.0 / frequency;
				clockSource = CY8C9560A.PwmClockSource1MHz;
			}
			else {
				throw new ArgumentOutOfRangeException(nameof(frequency), "The frequency is too high.");
			}

			this.i2c.WriteRegister(CY8C9560A.PwmSelectRegister, (byte)(pin.PinNumber + (pin.Port == 7 ? 8 : 0)));
			this.i2c.WriteRegister(CY8C9560A.PwmConfigRegister, clockSource);
			this.i2c.WriteRegister(CY8C9560A.PwmPeriodRegister, (byte)period);
			this.i2c.WriteRegister(CY8C9560A.PwmPulseWidthRegister, (byte)(period * dutyCycle));
		}

		public void SetOutput(Pin pin) {
			this.i2c.WriteRegister(CY8C9560A.PortSelectRegister, pin.Port);

			var mask = pin.Mask;

			var val = this.i2c.ReadRegister(CY8C9560A.PwmEnableRegister);
			this.i2c.WriteRegister(CY8C9560A.PwmEnableRegister, (byte)(val & ~mask));

			val = this.i2c.ReadRegister(CY8C9560A.PinDirectionRegister);
			this.i2c.WriteRegister(CY8C9560A.PinDirectionRegister, (byte)(val & ~mask));

			val = this.i2c.ReadRegister(CY8C9560A.PinStrongDriveRegister);
			this.i2c.WriteRegister(CY8C9560A.PinStrongDriveRegister, (byte)(val | mask));

			val = this.i2c.ReadRegister(CY8C9560A.InterruptMaskRegister);
			this.i2c.WriteRegister(CY8C9560A.InterruptMaskRegister, (byte)(val | mask));
		}

		public void SetInput(Pin pin, GpioPinDriveMode driveMode) {
			this.i2c.WriteRegister(CY8C9560A.PortSelectRegister, pin.Port);

			var mask = pin.Mask;

			var val = this.i2c.ReadRegister(CY8C9560A.PwmEnableRegister);
			this.i2c.WriteRegister(CY8C9560A.PwmEnableRegister, (byte)(val & ~mask));

			val = this.i2c.ReadRegister(CY8C9560A.PinDirectionRegister);
			this.i2c.WriteRegister(CY8C9560A.PinDirectionRegister, (byte)(val | mask));

			//byte resistorRegister = 0;
			//
			//switch (driveMode) {
			//	case GpioInputDriveMode.HighImpedance: resistorRegister = CY8C9560A.PinHighImpedanceRegister; break;
			//	case GpioInputDriveMode.PullDown: resistorRegister = CY8C9560A.PinPullDownRegister; break;
			//	case GpioInputDriveMode.PullUp: resistorRegister = CY8C9560A.PinPullUpRegister; break;
			//}

			val = this.i2c.ReadRegister(CY8C9560A.PinHighImpedanceRegister);
			this.i2c.WriteRegister(CY8C9560A.PinHighImpedanceRegister, (byte)(val | mask));

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