using GHI.Athens.Gadgeteer;
using GHI.Athens.Modules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2C;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace GHI.Athens.Demo {
	public sealed partial class MainPage : Windows.UI.Xaml.Controls.Page {
		private DispatcherTimer timer;
		private TheProfessor mainboard;
		private Button button;
		private LEDStrip ledStrip;
		private LightSense lightSense;

		public MainPage() {
			this.InitializeComponent();

			Task.Run(async () => this.mainboard = await Module.Create<TheProfessor>())
				.ContinueWith(async t => this.button = await Module.Create<Button>(this.mainboard.GetProvidedSocket(1)))
				.ContinueWith(async t => this.ledStrip = await Module.Create<LEDStrip>(this.mainboard.GetProvidedSocket(2)))
				.ContinueWith(async t => this.lightSense = await Module.Create<LightSense>(this.mainboard.GetProvidedSocket(3)))
				.ContinueWith(t => this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, this.ProgramStarted));
		}

		private void ProgramStarted() {
			this.timer = new DispatcherTimer();
			this.timer.Interval = TimeSpan.FromMilliseconds(100);
			this.timer.Tick += this.Timer_Tick;
			this.timer.Start();
        }

		private void Timer_Tick(object sender, object e) {
			if (this.button.IsPressed())
				this.ledStrip.TurnAllOn();
			else
				this.ledStrip.TurnAllOff();

			Debug.WriteLine($"{this.lightSense.GetReading():N2}");
		}
	}

	public class TempHumidSI70 {
		private const byte MEASURE_HUMIDITY_HOLD = 0xE5;
		private const byte READ_TEMP_FROM_PREVIOUS = 0xE0;
		private const byte I2C_ADDRESS = 0x40;

		private I2CDevice i2c;
		private byte[] writeBuffer1;
		private byte[] writeBuffer2;
		private byte[] readBuffer1;
		private byte[] readBuffer2;

		/// <summary>Constructs a new instance.</summary>
		/// <param name="socketNumber">The socket that this module is plugged in to.</param>
		public TempHumidSI70() {
			this.i2c = null;

			this.writeBuffer1 = new byte[1] { TempHumidSI70.MEASURE_HUMIDITY_HOLD };
			this.writeBuffer2 = new byte[1] { TempHumidSI70.READ_TEMP_FROM_PREVIOUS };
			this.readBuffer1 = new byte[2];
			this.readBuffer2 = new byte[2];
		}

		/// <summary>
		/// Obtains a single measurement.
		/// </summary>
		/// <returns>The measurement.</returns>
		public async Task<Measurement> TakeMeasurement() {
			if (this.i2c == null) {
				var settings = new I2CConnectionSettings(TempHumidSI70.I2C_ADDRESS, I2CBusSpeed.StandardMode, I2CAddressingMode.SevenBit);
				var deviceId = I2CDevice.GetDeviceSelector();
				var deviceInfos = await DeviceInformation.FindAllAsync(deviceId);

				this.i2c = await I2CDevice.CreateDeviceAsync(deviceInfos[2].Id, settings);
			}

			if (this.i2c.TryWriteRead(this.writeBuffer1, this.readBuffer1) != I2CTransferStatus.Success) return null;
			if (this.i2c.TryWriteRead(this.writeBuffer2, this.readBuffer2) != I2CTransferStatus.Success) return null;

			int rawRH = this.readBuffer1[0] << 8 | this.readBuffer1[1];
			int rawTemp = this.readBuffer2[0] << 8 | this.readBuffer2[1];

			double temperature = 175.72 * rawTemp / 65536.0 - 46.85;
			double relativeHumidity = 125.0 * rawRH / 65536.0 - 6.0;

			if (relativeHumidity < 0.0)
				relativeHumidity = 0.0;

			if (relativeHumidity > 100.0)
				relativeHumidity = 100.0;

			return new Measurement(temperature, relativeHumidity);
		}

		/// <summary>
		/// Result of a measurement.
		/// </summary>
		public class Measurement {
			/// <summary>
			/// The measured temperature in degrees Celsius.
			/// </summary>
			public double Temperature { get; private set; }

			/// <summary>
			/// The measured temperature in degrees Fahrenheit.
			/// </summary>
			public double TemperatureFahrenheit { get; private set; }

			/// <summary>
			/// The measured relative humidity.
			/// </summary>
			public double RelativeHumidity { get; private set; }

			/// <summary>
			/// Provides a string representation of the instance.
			/// </summary>
			/// <returns>A string describing the values contained in the object.</returns>
			public override string ToString() {
				return this.Temperature.ToString("F1") + " degrees Celsius, " + this.RelativeHumidity.ToString("F1") + "% relative humidity.";
			}

			internal Measurement(double temperature, double relativeHumidity) {
				this.RelativeHumidity = relativeHumidity;
				this.Temperature = temperature;
				this.TemperatureFahrenheit = temperature * 1.8 + 32.0;
			}
		}
	}

	public class IO60P16 {
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
		private GpioInterruptPin interrupt;
		private List<InterruptRegistraton> interruptHandlers;
		private byte[] write2;
		private byte[] write1;
		private byte[] read1;
		private byte[] pwms;

		public delegate void InterruptHandler(bool state);

		private class InterruptRegistraton {
			public InterruptMode mode;
			public InterruptHandler handler;
			public byte pin;
		}

		public enum IOState {
			InputInterrupt,
			Input,
			Output,
			Pwm
		}

		public enum ResistorMode {
			PullUp = IO60P16.PIN_PULL_UP,
			PullDown = IO60P16.PIN_PULL_DOWN,
			Floating = IO60P16.PIN_HIGH_IMPEDENCE,
		}

		public enum InterruptMode {
			RisingAndFallingEdge,
			RisingEdge,
			FallingEdge
		}

		private byte GetPort(byte pin) {
			return (byte)(pin >> 4);
		}

		private byte GetMask(byte pin) {
			return (byte)(1 << (pin & 0x0F));
		}

		private void WriteRegister(byte register, byte value) {
			lock (this.io60Chip) {
				write2[0] = register;
				write2[1] = value;
				this.io60Chip.TryWrite(write2);
			}
		}

		private byte ReadRegister(byte register) {
			byte result;

			lock (this.io60Chip) {
				write1[0] = register;
				this.io60Chip.TryWriteRead(write1, read1);
				result = read1[0];
			}

			return result;
		}

		private byte[] ReadRegisters(byte register, uint count) {
			byte[] result = new byte[count];

			lock (this.io60Chip) {
				write1[0] = register;
				this.io60Chip.TryWriteRead(write1, result);
			}

			return result;
		}

		private void OnInterrupt(object sender, bool value) {
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
							if ((reg.mode == InterruptMode.RisingEdge && val) || (reg.mode == InterruptMode.FallingEdge && !val) || reg.mode == InterruptMode.RisingAndFallingEdge)
								reg.handler(val);
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

			//this.interrupt = GTI.InterruptInputFactory.Create(socket, Socket.Pin.Three, GTI.GlitchFilterMode.On, GTI.ResistorMode.Disabled, GTI.InterruptMode.RisingEdge, null);
			//this.interrupt.Interrupt += this.OnInterrupt;
		}

		public void Initialize() {
			var settings = new I2CConnectionSettings(0x20, I2CBusSpeed.StandardMode, I2CAddressingMode.SevenBit);
			var deviceId = I2CDevice.GetDeviceSelector();

			var t1 = DeviceInformation.FindAllAsync(deviceId);
			t1.AsTask().Wait();
			var deviceInfos = t1.GetResults();

			var t2 = I2CDevice.CreateDeviceAsync(deviceInfos[2].Id, settings);
			t2.AsTask().Wait();
			this.io60Chip = t2.GetResults();
		}

		public void RegisterInterruptHandler(byte pin, InterruptMode mode, InterruptHandler handler) {
			InterruptRegistraton reg = new InterruptRegistraton();
			reg.handler = handler;
			reg.mode = mode;
			reg.pin = pin;

			lock (this.interruptHandlers)
				this.interruptHandlers.Add(reg);
		}

		public void SetIOMode(byte pin, IOState state, ResistorMode resistorMode) {
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

					val = this.ReadRegister((byte)resistorMode);
					this.WriteRegister((byte)resistorMode, (byte)(val | mask));
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