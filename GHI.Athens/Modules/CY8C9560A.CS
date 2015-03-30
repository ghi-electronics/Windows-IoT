using GHI.Athens.Gadgeteer;
using GHI.Athens.Gadgeteer.SocketInterfaces;
using System;
using System.Threading.Tasks;

namespace GHI.Athens.Modules {
	public class CY8C9560A {
		private enum Register : byte {
			InputPort0 = 0x00,
			OutputPort0 = 0x08,
			InterruptPort0 = 0x10,
			PortSelect = 0x18,
			InterruptMask = 0x19,
			SelectPwmForOutput = 0x1A,
			PinDirection = 0x1C,
			PinPullUp = 0x1D,
			PinPullDown = 0x1E,
			PinStrongDrive = 0x21,
			PinHighImpedance = 0x23,
			PwmSelect = 0x28,
			ConfigPwm = 0x29,
			PeriodPwm = 0x2A,
			PulseWidthPwm = 0x2B
		}

		private enum PwmClockSource {
			KHz32 = 0x00,
			MHz24 = 0x01,
			MHz1 = 0x02,
			KHz94 = 0x03,
			Hz367 = 0x04,
		}

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
			this.WriteRegister(Register.PortSelect, pin.Port);

			this.OrRegister(Register.PinStrongDrive, pin.Mask);
			this.OrRegister(Register.InterruptMask, pin.Mask);
			this.OrRegister(Register.SelectPwmForOutput, pin.Mask);

			this.WriteDigital(pin, true);

			double period;
			PwmClockSource clockSource;

			if (frequency <= 1.45) {
				throw new ArgumentOutOfRangeException(nameof(frequency), "The frequency is too low.");
			}
			else if (frequency <= 125.5) {
				period = 367.6 / frequency;
				clockSource = PwmClockSource.Hz367;
			}
			else if (frequency <= 367.7) {
				period = 32000.0 / frequency;
				clockSource = PwmClockSource.KHz32;
			}
			else if (frequency <= 5882.4) {
				period = 93750.0 / frequency;
				clockSource = PwmClockSource.KHz94;
			}
			else if (frequency <= 94117.7) {
				period = 1500000.0 / frequency;
				clockSource = PwmClockSource.MHz1;
			}
			else if (frequency <= 12000000.0) {
				period = 24000000.0 / frequency;
				clockSource = PwmClockSource.MHz24;
			}
			else {
				throw new ArgumentOutOfRangeException(nameof(frequency), "The frequency is too high.");
			}

			this.WriteRegister(Register.PwmSelect, (byte)(pin.PinNumber + (pin.Port == 7 ? 8 : 0)));
			this.WriteRegister(Register.ConfigPwm, (byte)clockSource);
			this.WriteRegister(Register.PeriodPwm, (byte)period);
			this.WriteRegister(Register.PulseWidthPwm, (byte)(period * dutyCycle));
		}

		public void SetOutput(Pin pin) {
			this.WriteRegister(Register.PortSelect, pin.Port);

			this.AndRegister(Register.SelectPwmForOutput, ~pin.Mask);
			this.AndRegister(Register.PinDirection, ~pin.Mask);
			this.OrRegister(Register.PinStrongDrive, pin.Mask);
			this.OrRegister(Register.InterruptMask, pin.Mask);
		}

		public void SetInput(Pin pin) {
			this.WriteRegister(Register.PortSelect, pin.Port);

			this.AndRegister(Register.SelectPwmForOutput, ~pin.Mask);
			this.OrRegister(Register.PinDirection, pin.Mask);
			this.OrRegister(Register.PinHighImpedance, pin.Mask);
			this.OrRegister(Register.InterruptMask, pin.Mask);
		}

		public bool ReadDigital(Pin pin) {
			return (this.i2c.ReadRegister((byte)(Register.InputPort0 + pin.Port)) & pin.Mask) != 0;
		}

		public void WriteDigital(Pin pin, bool value) {
			if (value) {
				this.OrRegister(Register.OutputPort0 + pin.Port, pin.Mask);
			}
			else {
				this.AndRegister(Register.OutputPort0 + pin.Port, ~pin.Mask);
			}
		}

		private byte ReadRegister(Register register) {
			return this.i2c.ReadRegister((byte)register);
		}

		private void WriteRegister(Register register, int value) {
			this.i2c.WriteRegister((byte)register, (byte)value);
		}

		private void OrRegister(Register register, int mask) {
			this.WriteRegister(register, (byte)(this.ReadRegister(register) | mask));
		}

		private void AndRegister(Register register, int mask) {
			this.WriteRegister(register, (byte)(this.ReadRegister(register) & mask));
		}
	}
}