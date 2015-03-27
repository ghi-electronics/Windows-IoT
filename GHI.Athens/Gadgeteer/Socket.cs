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
		private Dictionary<SocketPinNumber, int> nativePins;
		private HashSet<SocketType> supportedTypes;

		internal Socket(uint socketNumber) {
			this.nativePins = new Dictionary<SocketPinNumber, int>();
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

		public void SetNativePin(SocketPinNumber pinNumber, int nativePinNumber) {
			this.nativePins[pinNumber] = nativePinNumber;
		}

		public IReadOnlyCollection<SocketType> SupportedTypes { get { return this.supportedTypes.ToList(); } }
		public IReadOnlyDictionary<SocketPinNumber, int> Pins { get { return this.nativePins; } }

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

		private async Task<GpioPin> CreatePin(int pinNumber) {
			var controller = await GpioController.GetDefaultAsync();

			return controller.OpenPin(pinNumber);
		}

		public async Task<DigitalInput> CreateDigitalInputAsync(SocketPinNumber pinNumber, GpioPinDriveMode driveMode) {
			this.EnsureTypeIsSupported((int)pinNumber <= 5 ? SocketType.X : SocketType.Y);

			if (this.DigitalInputCreator != null)
				return await this.DigitalInputCreator(this, pinNumber, driveMode);

			return new NativeInterfaces.DigitalInput(await this.CreatePin(this.Pins[pinNumber]), driveMode);
		}

		public async Task<DigitalOutput> CreateDigitalOutputAsync(SocketPinNumber pinNumber, bool initialValue) {
			this.EnsureTypeIsSupported((int)pinNumber <= 5 ? SocketType.X : SocketType.Y);

			if (this.DigitalInputCreator != null)
				return await this.DigitalOutputCreator(this, pinNumber, initialValue);

			return new NativeInterfaces.DigitalOutput(await this.CreatePin(this.Pins[pinNumber]), initialValue);
		}

		public async Task<DigitalInterrupt> CreateDigitalInterruptAsync(SocketPinNumber pinNumber, GpioPinEdge interruptType, GpioPinDriveMode driveMode) {
			this.EnsureTypeIsSupported((int)pinNumber <= 5 ? SocketType.X : SocketType.Y);

			if (this.DigitalInputCreator != null)
				return await this.DigitalInterruptCreator(this, pinNumber, interruptType, driveMode);

			return new NativeInterfaces.DigitalInterrupt(await this.CreatePin(this.Pins[pinNumber]), interruptType, driveMode);
		}

		public async Task<DigitalInputOutput> CreateDigitalInputOutputAsync(SocketPinNumber pinNumber, GpioPinDriveMode driveMode) {
			this.EnsureTypeIsSupported((int)pinNumber <= 5 ? SocketType.X : SocketType.Y);

			if (this.DigitalInputOutputCreator != null)
				return await this.DigitalInputOutputCreator(this, pinNumber, DigitalInputOutputMode.Input, driveMode, false);

			return new NativeInterfaces.DigitalInputOutput(await this.CreatePin(this.Pins[pinNumber]), DigitalInputOutputMode.Input, driveMode, false);
		}

		public async Task<DigitalInputOutput> CreateDigitalInputOutputAsync(SocketPinNumber pinNumber, bool initialOutputValue) {
			this.EnsureTypeIsSupported((int)pinNumber <= 5 ? SocketType.X : SocketType.Y);

			if (this.DigitalInputOutputCreator != null)
				return await this.DigitalInputOutputCreator(this, pinNumber, DigitalInputOutputMode.Output, GpioPinDriveMode.Output, initialOutputValue);

			return new NativeInterfaces.DigitalInputOutput(await this.CreatePin(this.Pins[pinNumber]), DigitalInputOutputMode.Input, GpioPinDriveMode.Output, initialOutputValue);
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

			var infos = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(Windows.Devices.I2C.I2CBus.GetDeviceSelector(this.NativeI2CDeviceId));
			var device = await Windows.Devices.I2C.I2CBus.CreateDeviceAsync(infos[0].Id, connectionSettings);

			return new NativeInterfaces.I2CDevice(device);
		}

		public async Task<I2CDevice> CreateI2CDeviceAsync(Windows.Devices.I2C.I2CConnectionSettings connectionSettings, SocketPinNumber sdaPinNumber, SocketPinNumber sclPinNumber) {
			if (sdaPinNumber == SocketPinNumber.Eight && sclPinNumber == SocketPinNumber.Nine && this.IsTypeSupported(SocketType.I))
				return await this.CreateI2CDeviceAsync(connectionSettings);

			var sda = await this.CreateDigitalInputOutputAsync(sdaPinNumber, GpioPinDriveMode.Output);
			var scl = await this.CreateDigitalInputOutputAsync(sclPinNumber, GpioPinDriveMode.Output);

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
			var masterIn = await this.CreateDigitalInputAsync(masterInPinNumber, GpioPinDriveMode.Input);
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