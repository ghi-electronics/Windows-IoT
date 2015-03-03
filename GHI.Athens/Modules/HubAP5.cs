using GHI.Athens.Gadgeteer;
using GHI.Athens.Gadgeteer.SocketInterfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace GHI.Athens.Modules {
	public class HubAP5 : Module {
		private ADS7830 ads;
		private IO60P16 io60;
		private byte[] pinMap;

		public override string Name { get; } = "Hub AP5";
		public override string Manufacturer { get; } = "GHI Electronics, LLC";

		protected async override Task Initialize(Socket parentSocket) {
			this.ads = new ADS7830();
			this.io60 = new IO60P16();

			await this.ads.Initialize(parentSocket);
			await this.io60.Initialize(parentSocket);

			this.pinMap = new byte[56] {
				0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, //Socket 1
				0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, //Socket 2
				0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, //Socket 3
				0x20, 0x21, 0x22, 0x23, 0x74, 0x75, 0x76, //Socket 4
				0x14, 0x15, 0x16, 0x17, 0x71, 0x72, 0x73, //Socket 5
				0x10, 0x11, 0x12, 0x13, 0x66, 0x67, 0x70, //Socket 6
				0x04, 0x05, 0x06, 0x07, 0x63, 0x64, 0x65, //Socket 7
				0x00, 0x01, 0x02, 0x03, 0x60, 0x61, 0x62  //Socket 8
			};

			Socket socket;
			for (var i = 0U; i < 8; i++) {
				socket = this.AddProvidedSocket(i + 1);
				socket.AddSupportedTypes(SocketType.Y);

				if (i < 2) {
					socket.AddSupportedTypes(SocketType.A);
				}
				else if (i > 2) {
					socket.AddSupportedTypes(SocketType.P);
				}

				socket.DigitalInputCreator = (indirectedSocket, indirectedPin, resistorMode) => Task.FromResult<DigitalInput>(new IndirectedDigitalInput(this.GetPin(indirectedSocket, indirectedPin), resistorMode, this.io60));
				socket.DigitalOutputCreator = (indirectedSocket, indirectedPin, initialState) => Task.FromResult<DigitalOutput>(new IndirectedDigitalOutput(this.GetPin(indirectedSocket, indirectedPin), initialState, this.io60));
				socket.DigitalInputOutputCreator = (indirectedSocket, indirectedPin, resistorMode, isOutput, initialOutputValue) => Task.FromResult<DigitalInputOutput>(new IndirectedDigitalInputOutput(this.GetPin(indirectedSocket, indirectedPin), initialOutputValue, isOutput, resistorMode, this.io60));
				socket.DigitalInterruptCreator = (indirectedSocket, indirectedPin, interruptMode, resistorMode) => Task.FromResult<DigitalInterrupt>(new IndirectedDigitalInterrupt(this.GetPin(indirectedSocket, indirectedPin), resistorMode, interruptMode, this.io60));
				socket.AnalogInputCreator = (indirectedSocket, indirectedPin) => Task.FromResult<AnalogInput>(new IndirectedAnalogInput(indirectedPin, this.GetPin(indirectedSocket, indirectedPin), this.ads, this.io60));
				socket.PwmOutputCreator = (indirectedSocket, indirectedPin) => Task.FromResult<PwmOutput>(new IndirectedPwmOutput(this.GetPin(indirectedSocket, indirectedPin), this.io60));
			}
		}

		private byte GetPin(Socket socket, SocketPinNumber pin) {
			return this.pinMap[(socket.Number - 1) * 7 + (int)pin - 3];
		}

		private class IndirectedDigitalOutput : DigitalOutput {
			private IO60P16 io60;
			private byte pin;

			public IndirectedDigitalOutput(byte pin, bool initialState, IO60P16 io60) {
				this.io60 = io60;
				this.pin = pin;

				this.io60.SetIOMode(this.pin, IO60P16.IOState.Output, GpioInputDriveMode.HighImpedance);
				this.Write(initialState);
			}

			public override bool Read() {
				return this.io60.ReadDigital(this.pin);
			}

			public override void Write(bool state) {
				this.io60.WriteDigital(this.pin, state);
			}
		}

		private class IndirectedDigitalInput : DigitalInput {
			private IO60P16 io60;
			private byte pin;

			public IndirectedDigitalInput(byte pin, GpioInputDriveMode resistorMode, IO60P16 io60) {
				this.io60 = io60;
				this.pin = pin;

				this.io60.SetIOMode(this.pin, IO60P16.IOState.Input, resistorMode);
			}

			public override bool Read() {
				return this.io60.ReadDigital(this.pin);
			}
		}

		private class IndirectedDigitalInputOutput : DigitalInputOutput {
			private IO60P16 io60;
			private byte pin;

			public IndirectedDigitalInputOutput(byte pin, bool initialState, bool isOutput, GpioInputDriveMode resistorMode, IO60P16 io60) {
				this.io60 = io60;
				this.pin = pin;

				if (isOutput) {
					this.Write(initialState);
				}
				else {
					this.Read();
				}
			}

			public override bool Read() {
				this.io60.SetIOMode(this.pin, IO60P16.IOState.Input, this.DriveMode);
				return this.io60.ReadDigital(this.pin);
			}

			public override void Write(bool state) {
				this.io60.SetIOMode(this.pin, IO60P16.IOState.Output, this.DriveMode);
				this.io60.WriteDigital(this.pin, state);
			}
		}

		private class IndirectedDigitalInterrupt : DigitalInterrupt {
			private IO60P16 io60;
			private byte pin;

			public IndirectedDigitalInterrupt(byte pin, GpioInputDriveMode resistorMode, GpioInterruptType interruptMode, IO60P16 io60) {
				this.io60 = io60;
				this.pin = pin;

				this.io60.SetIOMode(this.pin, IO60P16.IOState.InputInterrupt, resistorMode);
				this.io60.RegisterInterruptHandler(this.pin, interruptMode, this.OnInterrupt);
			}

			public override bool Read() {
				return this.io60.ReadDigital(this.pin);
			}
		}

		private class IndirectedAnalogInput : AnalogInput {
			private byte channel;
			private ADS7830 ads;
			private IO60P16 io60;

			public override double MaxVoltage { get; } = 3.3;

			public IndirectedAnalogInput(SocketPinNumber pinNumber, byte pin, ADS7830 ads, IO60P16 io60) {
				this.ads = ads;
				this.io60 = io60;

				this.io60.SetIOMode(pin, IO60P16.IOState.Input, GpioInputDriveMode.HighImpedance);

				switch (pinNumber) {
					case SocketPinNumber.Three: this.channel = 0; break;
					case SocketPinNumber.Four: this.channel = 2; break;
					case SocketPinNumber.Five: this.channel = 1; break;
				}
			}

			public override double ReadVoltage() {
				return this.ads.ReadVoltage(this.channel);
			}
		}

		private class IndirectedPwmOutput : PwmOutput {
			private IO60P16 io60;
			private byte pin;

			public IndirectedPwmOutput(byte pin, IO60P16 io60) {
				this.io60 = io60;
				this.pin = pin;
			}

			protected override void SetEnabled(bool state) {
				if (state) {
					this.io60.SetIOMode(this.pin, IO60P16.IOState.Pwm, GpioInputDriveMode.HighImpedance);
					this.io60.SetPWM(this.pin, this.Frequency, this.DutyCycle);
				}
				else {
					this.io60.SetIOMode(this.pin, IO60P16.IOState.Input, GpioInputDriveMode.HighImpedance);
				}
			}

			protected override void SetValues(double frequency, double dutyCycle) {
				this.io60.SetPWM(this.pin, frequency, dutyCycle);
			}
		}

		private class ADS7830 {
			private static byte CmdSdSe { get; } = 0x80;
			private static byte CmdPdOff { get; } = 0x00;
			private static byte CmdPdOn { get; } = 0x04;
			private static byte Address { get; } = 0x48;

			private I2CDevice i2c;

			public async Task Initialize(Socket socket) {
				this.i2c = await socket.CreateI2CDeviceAsync(new Windows.Devices.I2C.I2CConnectionSettings(ADS7830.Address, Windows.Devices.I2C.I2CBusSpeed.StandardMode, Windows.Devices.I2C.I2CAddressingMode.SevenBit));
			}

			public double ReadVoltage(byte channel) {
				var command = new byte[] { (byte)(ADS7830.CmdSdSe | ADS7830.CmdPdOn) };
				var read = new byte[1];

				command[0] |= (byte)((channel % 2 == 0 ? channel / 2 : (channel - 1) / 2 + 4) << 4);

				if (this.i2c.WriteRead(command, read) != Windows.Devices.I2C.I2CTransferStatus.Success) return -1.0;

				return (double)read[0] / 255 * 3.3;
			}
		}

		private class IO60P16 {
			private const byte INPUT_PORT_0_REGISTER = 0x00;
			private const byte OUTPUT_PORT_0_REGISTER = 0x08;
			private const byte INTERRUPT_PORT_0_REGISTER = 0x10;
			private const byte PORT_SELECT_REGISTER = 0x18;
			private const byte INTERRUPT_MASK_REGISTER = 0x19;

			private const byte PIN_DIRECTION_REGISTER = 0x1C;
			private const byte PIN_PULL_UP = 0x1D;
			private const byte PIN_PULL_DOWN = 0x1E;
			private const byte PIN_OPEN_DRAIN_HIGH = 0x1F;
			private const byte PIN_OPEN_DRAIN_LOW = 0x20;
			private const byte PIN_STRONG_DRIVE = 0x21;
			private const byte PIN_SLOW_STRONG_DRIVE = 0x22;
			private const byte PIN_HIGH_IMPEDENCE = 0x23;

			private const byte ENABLE_PWM_REGISTER = 0x1A;
			private const byte PWM_SELECT_REGISTER = 0x28;
			private const byte PWM_CONFIG = 0x29;
			private const byte PERIOD_REGISTER = 0x2A;
			private const byte PULSE_WIDTH_REGISTER = 0x2B;

			private const byte CLOCK_SOURCE = 0x3;

			private I2CDevice io60Chip;
			private DigitalInterrupt interrupt;
			private List<InterruptRegistraton> interruptHandlers;
			private byte[] write2;
			private byte[] write1;
			private byte[] read1;
			private byte[] pwms;

			public delegate void InterruptHandler(GpioInterruptEventArgs e);

			private class InterruptRegistraton {
				public GpioInterruptType mode;
				public InterruptHandler handler;
				public byte pin;
			}

			public enum IOState {
				InputInterrupt,
				Input,
				Output,
				Pwm
			}

			private byte GetPort(byte pin) {
				return (byte)(pin >> 4);
			}

			private byte GetMask(byte pin) {
				return (byte)(1 << (pin & 0x0F));
			}

			private void WriteRegister(byte register, byte value) {
				this.write2[0] = register;
				this.write2[1] = value;

				this.io60Chip.Write(this.write2);
			}

			private byte ReadRegister(byte register) {
				this.write1[0] = register;

				this.io60Chip.WriteRead(this.write1, this.read1);

				return read1[0];
			}

			private byte[] ReadRegisters(byte register, uint count) {
				byte[] result = new byte[count];

				this.write1[0] = register;

				this.io60Chip.WriteRead(this.write1, result);

				return result;
			}

			private void OnInterrupt(DigitalInterrupt sender, GpioInterruptEventArgs e) {
				var interruptedPins = new List<int>();

				byte[] intPorts = this.ReadRegisters(IO60P16.INTERRUPT_PORT_0_REGISTER, 8);
				for (byte i = 0; i < 8; i++)
					for (int j = 1, k = 0; j <= 128; j <<= 1, k++)
						if ((intPorts[i] & j) != 0)
							interruptedPins.Add((i << 4) | k);

				foreach (int pin in interruptedPins) {
					lock (this.interruptHandlers) {
						foreach (InterruptRegistraton reg in this.interruptHandlers) {
							if (reg.pin == pin) {
								bool val = this.ReadDigital((byte)pin);
								if ((reg.mode == GpioInterruptType.RisingEdge && val) || (reg.mode == GpioInterruptType.FallingEdge && !val))
									reg.handler(null);
							}
						}
					}
				}
			}

			public IO60P16() {
				this.interruptHandlers = new List<InterruptRegistraton>();
				this.write2 = new byte[2];
				this.write1 = new byte[1];
				this.read1 = new byte[1];
				this.pwms = new byte[30] { 0x60, 0, 0x61, 1, 0x62, 2, 0x63, 3, 0x64, 4, 0x65, 5, 0x66, 6, 0x67, 7, 0x70, 8, 0x71, 9, 0x72, 10, 0x73, 11, 0x74, 12, 0x75, 13, 0x76, 14 };
			}

			public async Task Initialize(Socket socket) {
				this.io60Chip = await socket.CreateI2CDeviceAsync(new Windows.Devices.I2C.I2CConnectionSettings(0x20, Windows.Devices.I2C.I2CBusSpeed.StandardMode, Windows.Devices.I2C.I2CAddressingMode.SevenBit));
				//this.interrupt = await socket.CreateDigitalInterruptAsync(SocketPinNumber.Three, GpioInterruptType.RisingEdge, GpioInputDriveMode.HighImpedance);
				//this.interrupt.Interrupt += this.OnInterrupt;
			}

			public void RegisterInterruptHandler(byte pin, GpioInterruptType mode, InterruptHandler handler) {
				InterruptRegistraton reg = new InterruptRegistraton();
				reg.handler = handler;
				reg.mode = mode;
				reg.pin = pin;

				lock (this.interruptHandlers)
					this.interruptHandlers.Add(reg);
			}

			public void SetIOMode(byte pin, IOState state, GpioInputDriveMode resistorMode) {
				this.WriteRegister(IO60P16.PORT_SELECT_REGISTER, this.GetPort(pin));

				byte mask = this.GetMask(pin);
				byte val = this.ReadRegister(IO60P16.ENABLE_PWM_REGISTER);

				if (state == IOState.Pwm) {
					this.WriteRegister(IO60P16.ENABLE_PWM_REGISTER, (byte)(val | mask));

					this.WriteDigital(pin, true);

					byte pwm = 255;
					for (var i = 0; i < 30; i += 2)
						if (this.pwms[i] == pin)
							pwm = this.pwms[i + 1];

					this.WriteRegister(IO60P16.PWM_SELECT_REGISTER, pwm);
					this.WriteRegister(IO60P16.PWM_CONFIG, IO60P16.CLOCK_SOURCE); //93.75KHz clock

					val = this.ReadRegister(IO60P16.PIN_STRONG_DRIVE);
					this.WriteRegister(IO60P16.PIN_STRONG_DRIVE, (byte)(val | mask));
				}
				else {
					this.WriteRegister(IO60P16.ENABLE_PWM_REGISTER, (byte)(val & ~mask));
					val = this.ReadRegister(IO60P16.PIN_DIRECTION_REGISTER);

					if (state == IOState.Output) {
						this.WriteRegister(IO60P16.PIN_DIRECTION_REGISTER, (byte)(val & ~mask));

						val = this.ReadRegister(IO60P16.PIN_STRONG_DRIVE);
						this.WriteRegister(IO60P16.PIN_STRONG_DRIVE, (byte)(val | mask));
					}
					else {
						this.WriteRegister(IO60P16.PIN_DIRECTION_REGISTER, (byte)(val | mask));

						byte resistorValue = 0;

						switch (resistorMode) {
							case GpioInputDriveMode.HighImpedance: resistorValue = IO60P16.PIN_HIGH_IMPEDENCE; break;
							case GpioInputDriveMode.PullDown: resistorValue = IO60P16.PIN_PULL_DOWN; break;
							case GpioInputDriveMode.PullUp: resistorValue = IO60P16.PIN_PULL_UP; break;
						}

						val = this.ReadRegister(resistorValue);
						this.WriteRegister(resistorValue, (byte)(val | mask));
					}
				}

				val = this.ReadRegister(IO60P16.INTERRUPT_MASK_REGISTER);
				if (state == IOState.InputInterrupt)
					this.WriteRegister(IO60P16.INTERRUPT_MASK_REGISTER, (byte)(val & ~mask));
				else
					this.WriteRegister(IO60P16.INTERRUPT_MASK_REGISTER, (byte)(val | mask));
			}

			//We're using the 93.75KHz clock source because it gives a good resolution around the 1KHz frequency
			//while still allowing the user to select frequencies such as 10KHz, but with reduced duty cycle
			//resolution.
			public void SetPWM(byte pin, double frequency, double dutyCycle) {
				byte pwm = 255;
				for (var i = 0; i < 30; i += 2)
					if (this.pwms[i] == pin)
						pwm = this.pwms[i + 1];

				this.WriteRegister((byte)(IO60P16.PWM_SELECT_REGISTER), pwm); // (byte)((pin % 8) + (this.getPort(pin) - 6) * 8));

				byte period = (byte)(93750 / frequency);

				this.WriteRegister(IO60P16.PERIOD_REGISTER, period);
				this.WriteRegister((byte)(IO60P16.PULSE_WIDTH_REGISTER), (byte)(period * dutyCycle));
			}

			public bool ReadDigital(byte pin) {
				byte b = this.ReadRegister((byte)(IO60P16.INPUT_PORT_0_REGISTER + this.GetPort(pin)));

				return (b & this.GetMask(pin)) != 0;
			}

			public void WriteDigital(byte pin, bool value) {
				byte b = this.ReadRegister((byte)(IO60P16.OUTPUT_PORT_0_REGISTER + this.GetPort(pin)));

				if (value)
					b |= this.GetMask(pin);
				else
					b = (byte)(b & ~this.GetMask(pin));

				this.WriteRegister((byte)(IO60P16.OUTPUT_PORT_0_REGISTER + this.GetPort(pin)), b);
			}
		}
	}
}