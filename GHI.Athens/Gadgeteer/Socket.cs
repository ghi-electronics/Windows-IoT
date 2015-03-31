using GHI.Athens.Gadgeteer.SocketInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WD = Windows.Devices;

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

	public interface ISocket {
		uint Number { get; }

		IReadOnlyCollection<SocketType> SupportedTypes { get; }
		IReadOnlyDictionary<SocketPinNumber, int> Pins { get; }

		string NativeI2CDeviceId { get; }
		string NativeSpiDeviceId { get; }
		string NativeSerialDeviceId { get; }
		string NativeCanDeviceId { get; }

		void EnsureTypeIsSupported(SocketType type);
		bool IsTypeSupported(SocketType type);

		Task<DigitalIO> CreateDigitalIOAsync(SocketPinNumber pinNumber);
		Task<DigitalIO> CreateDigitalIOAsync(SocketPinNumber pinNumber, WD.Gpio.GpioPinEdge interruptType);
		Task<DigitalIO> CreateDigitalIOAsync(SocketPinNumber pinNumber, bool initialValue);
		Task<AnalogIO> CreateAnalogIOAsync(SocketPinNumber pinNumber);
		Task<AnalogIO> CreateAnalogIOAsync(SocketPinNumber pinNumber, double initialVoltage);
		Task<PwmOutput> CreatePwmOutputAsync(SocketPinNumber pinNumber);
		Task<I2CDevice> CreateI2CDeviceAsync(WD.I2C.I2CConnectionSettings connectionSettings);
		Task<I2CDevice> CreateI2CDeviceAsync(WD.I2C.I2CConnectionSettings connectionSettings, SocketPinNumber sdaPinNumber, SocketPinNumber sclPinNumber);
		Task<SpiDevice> CreateSpiDeviceAsync(WD.Spi.SpiConnectionSettings connectionSettings);
		Task<SpiDevice> CreateSpiDeviceAsync(WD.Spi.SpiConnectionSettings connectionSettings, SocketPinNumber chipSelectPinNumber, SocketPinNumber masterOutPinNumber, SocketPinNumber masterInPinNumber, SocketPinNumber clockPinNumber);
		Task<SerialDevice> CreateSerialDeviceAsync();
		Task<CanDevice> CreateCanDeviceAsync();
	}

	public sealed class Socket : ISocket {
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

		private async Task<WD.Gpio.GpioPin> CreatePin(int pinNumber) {
			return (await WD.Gpio.GpioController.GetDefaultAsync()).OpenPin(pinNumber);
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

			result.DriveMode = WD.Gpio.GpioPinDriveMode.Input;

			return result;
		}

		public async Task<DigitalIO> CreateDigitalIOAsync(SocketPinNumber pinNumber, WD.Gpio.GpioPinEdge interruptType) {
			var result = await this.CreateDigitalIOAsync(pinNumber);

			result.InterruptType = interruptType;

			return result;
		}

		public async Task<DigitalIO> CreateDigitalIOAsync(SocketPinNumber pinNumber, bool initialValue) {
			var result = await this.CreateDigitalIOAsync(pinNumber);

			result.DriveMode = WD.Gpio.GpioPinDriveMode.Output;
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

			result.DriveMode = WD.Gpio.GpioPinDriveMode.Input;

			return result;
		}

		public async Task<AnalogIO> CreateAnalogIOAsync(SocketPinNumber pinNumber, double initialVoltage) {
			var result = await this.CreateAnalogIOAsync(pinNumber);

			result.DriveMode = WD.Gpio.GpioPinDriveMode.Output;
			result.Voltage = initialVoltage;

			return result;
		}

		public async Task<PwmOutput> CreatePwmOutputAsync(SocketPinNumber pinNumber) {
			this.EnsureTypeIsSupported(SocketType.P);

			if (this.PwmOutputCreator != null)
				return await this.PwmOutputCreator(this, pinNumber);

			return new NativeInterfaces.PwmOutput();
		}

		public async Task<I2CDevice> CreateI2CDeviceAsync(WD.I2C.I2CConnectionSettings connectionSettings) {
			if (this.IsTypeSupported(SocketType.Y) && !this.IsTypeSupported(SocketType.I))
				return await this.CreateI2CDeviceAsync(connectionSettings, SocketPinNumber.Eight, SocketPinNumber.Nine);

			this.EnsureTypeIsSupported(SocketType.I);

			if (this.I2CDeviceCreator != null)
				return await this.I2CDeviceCreator(this);

			var infos = await WD.Enumeration.DeviceInformation.FindAllAsync(WD.I2C.I2CBus.GetDeviceSelector(this.NativeI2CDeviceId));
			var device = await WD.I2C.I2CBus.CreateDeviceAsync(infos[0].Id, connectionSettings);

			return new NativeInterfaces.I2CDevice(device);
		}

		public async Task<I2CDevice> CreateI2CDeviceAsync(WD.I2C.I2CConnectionSettings connectionSettings, SocketPinNumber sdaPinNumber, SocketPinNumber sclPinNumber) {
			if (sdaPinNumber == SocketPinNumber.Eight && sclPinNumber == SocketPinNumber.Nine && this.IsTypeSupported(SocketType.I))
				return await this.CreateI2CDeviceAsync(connectionSettings);

			var sda = await this.CreateDigitalIOAsync(sdaPinNumber);
			var scl = await this.CreateDigitalIOAsync(sclPinNumber);

			return new SoftwareInterfaces.I2CDevice(sda, scl, connectionSettings);
		}

		public async Task<SpiDevice> CreateSpiDeviceAsync(WD.Spi.SpiConnectionSettings connectionSettings) {
			if (this.IsTypeSupported(SocketType.Y) && ! this.IsTypeSupported(SocketType.S))
				return await this.CreateSpiDeviceAsync(connectionSettings, SocketPinNumber.Six, SocketPinNumber.Seven, SocketPinNumber.Eight, SocketPinNumber.Nine);

			this.EnsureTypeIsSupported(SocketType.S);

			if (this.SpiDeviceCreator != null)
				return await this.SpiDeviceCreator(this);

			var infos = await WD.Enumeration.DeviceInformation.FindAllAsync(WD.Spi.SpiBus.GetDeviceSelector(this.NativeSpiDeviceId));
			var device = await WD.Spi.SpiBus.CreateDeviceAsync(infos[0].Id, connectionSettings);

			return new NativeInterfaces.SpiDevice(device);
		}

		public async Task<SpiDevice> CreateSpiDeviceAsync(WD.Spi.SpiConnectionSettings connectionSettings, SocketPinNumber chipSelectPinNumber, SocketPinNumber masterOutPinNumber, SocketPinNumber masterInPinNumber, SocketPinNumber clockPinNumber) {
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

			var infos = await WD.Enumeration.DeviceInformation.FindAllAsync(WD.SerialCommunication.SerialDevice.GetDeviceSelector(this.NativeSerialDeviceId));
			var device = await WD.SerialCommunication.SerialDevice.FromIdAsync(infos[0].Id);

			return new NativeInterfaces.SerialDevice(device);
		}

		public async Task<CanDevice> CreateCanDeviceAsync() {
			this.EnsureTypeIsSupported(SocketType.C);

			if (this.CanDeviceCreator != null)
				return await this.CanDeviceCreator(this);

			return new NativeInterfaces.CanDevice();
		}
	}
}