using GHI.Athens.Gadgeteer.SocketInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace GHI.Athens.Gadgeteer {
	public enum SocketPinNumber {
		Three = 3,
		Four,
		Five,
		Six,
		Seven,
		Eight,
		Nine
	}

	public enum SocketType {
		A,
		C,
		I,
		O,
		P,
		S,
		U,
		X,
		Y
	}

	public sealed class Socket {
		private Dictionary<SocketPinNumber, GpioPinDefinition> nativePinDefinitions;
		private HashSet<SocketType> supportedTypes;

		internal Socket(uint socketNumber) {
			this.nativePinDefinitions = new Dictionary<SocketPinNumber, GpioPinDefinition>();
			this.supportedTypes = new HashSet<SocketType>();

			this.Number = socketNumber;
		}

		public void EnsureTypeIsSupported(SocketType type) {
			if (!this.IsTypeSupported(type))
				throw new UnsupportedSocketTypeException($"{type} is not supported on this socket.");
        }

		public bool IsTypeSupported(SocketType type) {
			return (type == SocketType.X ? this.supportedTypes.Contains(SocketType.Y) : false) || this.supportedTypes.Contains(type);
		}

		public void AddSupportedTypes(params SocketType[] types) {
			foreach (var t in types)
				this.supportedTypes.Add(t);
		}

		public void AddGpioPinDefinition(SocketPinNumber pinNumber, GpioPinDefinition pinDefinition) {
			this.nativePinDefinitions[pinNumber] = pinDefinition;
		}

		public IReadOnlyList<SocketType> SupportedTypes { get { return this.supportedTypes.ToList(); } }

		public string NativeI2CDeviceId { get; set; }
		public string NativeSpiDeviceId { get; set; }
		public string NativeSerialDeviceId { get; set; }
		public string NativeCanDeviceId { get; set; }

		public uint Number { get; }

		public DigitalInputCreator DigitalInputCreator { get; set; }
		public DigitalOutputCreator DigitalOutputCreator { get; set; }
		public DigitalInterruptCreator DigitalInterruptCreator { get; set; }
		public DigitalInputOutputCreator DigitalInputOutputCreator { get; set; }
		public AnalogInputCreator AnalogInputCreator { get; set; }
		public AnalogOutputCreator AnalogOutputCreator { get; set; }
		public PwmOutputCreator PwmOutputCreator { get; set; }
		public I2CDeviceCreator I2CDeviceCreator { get; set; }
		public SpiDeviceCreator SpiDeviceCreator { get; set; }
		public SerialDeviceCreator SerialDeviceCreator { get; set; }
		public CanDeviceCreator CanDeviceCreator { get; set; }

		private async Task<GpioPinInfo> GetPinInfo(SocketPinNumber pinNumber) {
			var gpioDefinition = this.nativePinDefinitions[pinNumber];
			var controller = await GpioController.FromIdAsync(gpioDefinition.ControllerDeviceId);

			GpioPinInfo pinInfo;

			if (!controller.Pins.TryGetValue(gpioDefinition.PinNumber, out pinInfo))
				throw new SocketInterfaceCreationException("Error when querying the native pin controller.");

			return pinInfo;
		}

		public async Task<DigitalInput> CreateDigitalInputAsync(SocketPinNumber pinNumber, GpioInputDriveMode driveMode) {
			this.EnsureTypeIsSupported((int)pinNumber <= 5 ? SocketType.X : SocketType.Y);

			if (this.DigitalInputCreator != null)
				return await this.DigitalInputCreator(this, pinNumber, driveMode);

			GpioInputPin pin;

			var pinInfo = await this.GetPinInfo(pinNumber);

			if (!pinInfo.Capabilities.IsInputSupported)
				throw new UnsupportedPinModeException();

			if (pinInfo.TryOpenInput(GpioSharingMode.Shared, driveMode, out pin) != GpioOpenStatus.Success)
				throw new SocketInterfaceCreationException("The pin could not be opened.");

			return new NativeInterfaces.DigitalInput(pin);
		}

		public async Task<DigitalOutput> CreateDigitalOutputAsync(SocketPinNumber pinNumber, bool initialValue) {
			this.EnsureTypeIsSupported((int)pinNumber <= 5 ? SocketType.X : SocketType.Y);

			if (this.DigitalInputCreator != null)
				return await this.DigitalOutputCreator(this, pinNumber, initialValue);

			GpioOutputPin pin;

			var pinInfo = await this.GetPinInfo(pinNumber);

			if (!pinInfo.Capabilities.IsOutputSupported)
				throw new UnsupportedPinModeException();

			if (pinInfo.TryOpenOutput(initialValue ? GpioPinValue.High : GpioPinValue.Low, GpioSharingMode.Shared, out pin) != GpioOpenStatus.Success)
				throw new SocketInterfaceCreationException("The pin could not be opened.");

			return new NativeInterfaces.DigitalOutput(pin);
		}

		public async Task<DigitalInterrupt> CreateDigitalInterruptAsync(SocketPinNumber pinNumber, GpioInterruptType interruptType, GpioInputDriveMode driveMode) {
			this.EnsureTypeIsSupported((int)pinNumber <= 5 ? SocketType.X : SocketType.Y);

			if (this.DigitalInputCreator != null)
				return await this.DigitalInterruptCreator(this, pinNumber, interruptType, driveMode);

			GpioInterruptPin pin;

			var pinInfo = await this.GetPinInfo(pinNumber);

			if (!pinInfo.Capabilities.IsInterruptSupported)
				throw new UnsupportedPinModeException();

			if (pinInfo.TryOpenInterrupt(interruptType, GpioSharingMode.Shared, driveMode, out pin) != GpioOpenStatus.Success)
				throw new SocketInterfaceCreationException("The pin could not be opened.");

			return new NativeInterfaces.DigitalInterrupt(pin);
		}

		public async Task<DigitalInputOutput> CreateDigitalInputOutputAsync(SocketPinNumber pinNumber, GpioInputDriveMode driveMode) {
			this.EnsureTypeIsSupported((int)pinNumber <= 5 ? SocketType.X : SocketType.Y);

			if (this.DigitalInputOutputCreator != null)
				return await this.DigitalInputOutputCreator(this, pinNumber, DigitalInputOutputMode.Input, driveMode, false);

			var pinInfo = await this.GetPinInfo(pinNumber);

			if (!pinInfo.Capabilities.IsOutputSupported || !pinInfo.Capabilities.IsInputSupported)
				throw new UnsupportedPinModeException();

			return new NativeInterfaces.DigitalInputOutput(pinInfo, DigitalInputOutputMode.Input, driveMode, false);
		}

		public async Task<DigitalInputOutput> CreateDigitalInputOutputAsync(SocketPinNumber pinNumber, bool initialOutputValue) {
			this.EnsureTypeIsSupported((int)pinNumber <= 5 ? SocketType.X : SocketType.Y);

			if (this.DigitalInputOutputCreator != null)
				return await this.DigitalInputOutputCreator(this, pinNumber, DigitalInputOutputMode.Output, GpioInputDriveMode.HighImpedance, initialOutputValue);

			var pinInfo = await this.GetPinInfo(pinNumber);

			if (!pinInfo.Capabilities.IsOutputSupported || !pinInfo.Capabilities.IsInputSupported)
				throw new UnsupportedPinModeException();

			return new NativeInterfaces.DigitalInputOutput(pinInfo, DigitalInputOutputMode.Output, GpioInputDriveMode.HighImpedance, initialOutputValue);
		}

		public async Task<AnalogInput> CreateAnalogInputAsync(SocketPinNumber pinNumber) {
			this.EnsureTypeIsSupported(SocketType.A);

			if (this.AnalogInputCreator != null)
				return await this.AnalogInputCreator(this, pinNumber);

			return new NativeInterfaces.AnalogInput();
		}

		public async Task<AnalogOutput> CreateAnalogOutputAsync(SocketPinNumber pinNumber, double initialValue) {
			this.EnsureTypeIsSupported(SocketType.O);

			if (this.AnalogOutputCreator != null)
				return await this.AnalogOutputCreator(this, pinNumber, initialValue);

			return new NativeInterfaces.AnalogOutput();
		}

		public async Task<PwmOutput> CreatePwmOutputAsync(SocketPinNumber pinNumber) {
			this.EnsureTypeIsSupported(SocketType.P);

			if (this.PwmOutputCreator != null)
				return await this.PwmOutputCreator(this, pinNumber);

			return new NativeInterfaces.PwmOutput();
		}

		public async Task<I2CDevice> CreateI2CDeviceAsync(Windows.Devices.I2C.I2CConnectionSettings connectionSettings) {
			if (this.IsTypeSupported(SocketType.Y) && !this.IsTypeSupported(SocketType.I))
				return await this.CreateI2CDeviceAsync(connectionSettings, SocketPinNumber.Eight, SocketPinNumber.Nine);

			this.EnsureTypeIsSupported(SocketType.I);

			if (this.I2CDeviceCreator != null)
				return await this.I2CDeviceCreator(this);

			var device = await Windows.Devices.I2C.I2CDevice.CreateDeviceAsync(this.NativeI2CDeviceId, connectionSettings);

			return new NativeInterfaces.I2CDevice(device);
		}

		public async Task<I2CDevice> CreateI2CDeviceAsync(Windows.Devices.I2C.I2CConnectionSettings connectionSettings, SocketPinNumber sdaPinNumber, SocketPinNumber sclPinNumber) {
			if (sdaPinNumber == SocketPinNumber.Eight && sclPinNumber == SocketPinNumber.Nine && this.IsTypeSupported(SocketType.I))
				return await this.CreateI2CDeviceAsync(connectionSettings);

			var sda = await this.CreateDigitalInputOutputAsync(sdaPinNumber, GpioInputDriveMode.PullUp);
			var scl = await this.CreateDigitalInputOutputAsync(sclPinNumber, GpioInputDriveMode.PullUp);

			return new SoftwareInterfaces.I2CDevice(sda, scl, connectionSettings);
		}

		public async Task<SpiDevice> CreateSpiDeviceAsync(SpiConfiguration configuration, SocketPinNumber slaveSelectPinNumber) {
			if (this.IsTypeSupported(SocketType.Y) && ! this.IsTypeSupported(SocketType.S))
				return await this.CreateSpiDeviceAsync(configuration, SocketPinNumber.Six, SocketPinNumber.Seven, SocketPinNumber.Eight, SocketPinNumber.Nine);

			this.EnsureTypeIsSupported(SocketType.S);

			if (this.SpiDeviceCreator != null)
				return await this.SpiDeviceCreator(this);

			var slaveSelect = await this.CreateDigitalOutputAsync(slaveSelectPinNumber, !configuration.SlaveSelectActiveHigh);

			return new NativeInterfaces.SpiDevice(configuration, slaveSelect);
		}

		public async Task<SpiDevice> CreateSpiDeviceAsync(SpiConfiguration configuration, SocketPinNumber slaveSelectPinNumber, SocketPinNumber masterOutPinNumber, SocketPinNumber masterInPinNumber, SocketPinNumber clockPinNumber) {
			if (slaveSelectPinNumber == SocketPinNumber.Six && masterOutPinNumber == SocketPinNumber.Seven && masterInPinNumber == SocketPinNumber.Eight && clockPinNumber == SocketPinNumber.Nine && this.IsTypeSupported(SocketType.S))
				return await this.CreateSpiDeviceAsync(configuration, slaveSelectPinNumber);

			var slaveSelect = await this.CreateDigitalOutputAsync(slaveSelectPinNumber, !configuration.SlaveSelectActiveHigh);
			var masterOut = await this.CreateDigitalOutputAsync(masterOutPinNumber, false);
			var masterIn = await this.CreateDigitalInputAsync(masterInPinNumber, GpioInputDriveMode.HighImpedance);
			var clock = await this.CreateDigitalOutputAsync(clockPinNumber, configuration.ClockIdleHigh);

			return new SoftwareInterfaces.SpiDevice(configuration, slaveSelect, masterOut, masterIn, clock);
		}

		public async Task<SerialDevice> CreateSerialDeviceAsync() {
			this.EnsureTypeIsSupported(SocketType.U);

			if (this.SerialDeviceCreator != null)
				return await this.SerialDeviceCreator(this);

			return new NativeInterfaces.SerialDevice();
		}

		public async Task<CanDevice> CreateCanDeviceAsync() {
			this.EnsureTypeIsSupported(SocketType.C);

			if (this.CanDeviceCreator != null)
				return await this.CanDeviceCreator(this);

			return new NativeInterfaces.CanDevice();
		}
	}
}