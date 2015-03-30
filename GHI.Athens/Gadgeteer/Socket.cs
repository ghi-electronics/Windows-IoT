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
		B,
		C,
		D,
		E,
		F,
		G,
		H,
		I,
		K,
		O,
		P,
		R,
		S,
		T,
		U,
		X,
		Y,
		Z
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

		public DigitalIOCreator DigitalIOCreator { get; set; }
		public AnalogIOCreator AnalogIOCreator { get; set; }
		public PwmOutputCreator PwmOutputCreator { get; set; }
		public I2CDeviceCreator I2CDeviceCreator { get; set; }
		public SpiDeviceCreator SpiDeviceCreator { get; set; }
		public SerialDeviceCreator SerialDeviceCreator { get; set; }
		public CanDeviceCreator CanDeviceCreator { get; set; }

		private async Task<GpioPin> CreatePin(int pinNumber) {
			return (await GpioController.GetDefaultAsync()).OpenPin(pinNumber);
		}

		public async Task<DigitalIO> CreateDigitalIOAsync(SocketPinNumber pinNumber) {
			this.EnsureTypeIsSupported((int)pinNumber <= 5 ? SocketType.X : SocketType.Y);

			DigitalIO result;

			if (this.DigitalIOCreator != null) {
				result = await this.DigitalIOCreator(this, pinNumber);
			}
			else {
				result = new NativeInterfaces.DigitalIO(await this.CreatePin(this.Pins[pinNumber]));
			}

			result.DriveMode = GpioPinDriveMode.Input;

			return result;
		}

		public async Task<DigitalIO> CreateDigitalIOAsync(SocketPinNumber pinNumber, GpioPinEdge interruptType) {
			var result = await this.CreateDigitalIOAsync(pinNumber);

			result.InterruptType = interruptType;

			return result;
		}

		public async Task<DigitalIO> CreateDigitalIOAsync(SocketPinNumber pinNumber, bool initialValue) {
			var result = await this.CreateDigitalIOAsync(pinNumber);

			result.DriveMode = GpioPinDriveMode.Output;
			result.Value = initialValue;

			return result;
		}

		public async Task<AnalogIO> CreateAnalogIOAsync(SocketPinNumber pinNumber) {
			this.EnsureTypeIsSupported(SocketType.A);

			AnalogIO result;

			if (this.AnalogIOCreator != null) {
				result = await this.AnalogIOCreator(this, pinNumber);
			}
			else {
				result = new NativeInterfaces.AnalogIO();
			}

			result.DriveMode = GpioPinDriveMode.Input;

			return result;
		}

		public async Task<AnalogIO> CreateAnalogIOAsync(SocketPinNumber pinNumber, double initialVoltage) {
			var result = await this.CreateAnalogIOAsync(pinNumber);

			result.DriveMode = GpioPinDriveMode.Output;
			result.Voltage = initialVoltage;

			return result;
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

			var sda = await this.CreateDigitalIOAsync(sdaPinNumber);
			var scl = await this.CreateDigitalIOAsync(sclPinNumber);

			return new SoftwareInterfaces.I2CDevice(sda, scl, connectionSettings);
		}

		public async Task<SpiDevice> CreateSpiDeviceAsync(Windows.Devices.Spi.SpiConnectionSettings connectionSettings) {
			if (this.IsTypeSupported(SocketType.Y) && ! this.IsTypeSupported(SocketType.S))
				return await this.CreateSpiDeviceAsync(connectionSettings, SocketPinNumber.Six, SocketPinNumber.Seven, SocketPinNumber.Eight, SocketPinNumber.Nine);

			this.EnsureTypeIsSupported(SocketType.S);

			if (this.SpiDeviceCreator != null)
				return await this.SpiDeviceCreator(this);

			var infos = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(Windows.Devices.Spi.SpiBus.GetDeviceSelector(this.NativeSpiDeviceId));
			var device = await Windows.Devices.Spi.SpiBus.CreateDeviceAsync(infos[0].Id, connectionSettings);

			return new NativeInterfaces.SpiDevice(device);
		}

		public async Task<SpiDevice> CreateSpiDeviceAsync(Windows.Devices.Spi.SpiConnectionSettings connectionSettings, SocketPinNumber chipSelectPinNumber, SocketPinNumber masterOutPinNumber, SocketPinNumber masterInPinNumber, SocketPinNumber clockPinNumber) {
			if (chipSelectPinNumber == SocketPinNumber.Six && masterOutPinNumber == SocketPinNumber.Seven && masterInPinNumber == SocketPinNumber.Eight && clockPinNumber == SocketPinNumber.Nine && this.IsTypeSupported(SocketType.S))
				return await this.CreateSpiDeviceAsync(connectionSettings);

			var chipSelect = await this.CreateDigitalIOAsync(chipSelectPinNumber);
			var masterOut = await this.CreateDigitalIOAsync(masterOutPinNumber);
			var masterIn = await this.CreateDigitalIOAsync(masterInPinNumber);
			var clock = await this.CreateDigitalIOAsync(clockPinNumber);

			return new SoftwareInterfaces.SpiDevice(chipSelect, masterOut, masterIn, clock, connectionSettings);
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