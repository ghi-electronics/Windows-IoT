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
		X,
		Y,
		I,
		A,
		P,
		S,
		U
	}

	public sealed class Socket {
		private Dictionary<SocketPinNumber, GpioPinDefinition> pinDefinitions;
		private HashSet<SocketType> supportedTypes;

		internal Socket(uint socketNumber) {
			this.pinDefinitions = new Dictionary<SocketPinNumber, GpioPinDefinition>();
			this.supportedTypes = new HashSet<SocketType>();

			this.Number = socketNumber;
        }

		public void EnsureTypeIsSupported(SocketType type) {
			if (!this.IsTypeSupported(type))
				throw new UnsupportedSocketTypeException();
		}

		public bool IsTypeSupported(SocketType type) {
			return (type == SocketType.X ? this.supportedTypes.Contains(SocketType.Y) : false) || this.supportedTypes.Contains(type);
		}

		public void AddSupportedTypes(params SocketType[] types) {
			foreach (var t in types)
				this.supportedTypes.Add(t);
		}

		public void AddGpioPinDefinition(SocketPinNumber pinNumber, GpioPinDefinition pinDefinition) {
			this.pinDefinitions[pinNumber] = pinDefinition;
		}

		public IReadOnlyDictionary<SocketPinNumber, GpioPinDefinition> GpioPinDefinitions { get { return this.pinDefinitions; } }
		public IReadOnlyList<SocketType> SupportedTypes { get { return this.supportedTypes.ToList(); } }

		public string NativeI2CDeviceId { get; set; }
		public string NativeSpiDeviceId { get; set; }
		public string NativeSerialDeviceId { get; set; }

		public uint Number { get; }

		public DigitalInputCreator DigitalInputCreator { get; set; }
		public DigitalOutputCreator DigitalOutputCreator { get; set; }
		public DigitalInterruptCreator DigitalInterruptCreator { get; set; }
		public AnalogInputCreator AnalogInputCreator { get; set; }
		public I2CDeviceCreator I2CDeviceCreator { get; set; }

		private async Task<GpioPinInfo> GetPinInfo(SocketPinNumber pinNumber) {
			var gpioDefinition = this.GpioPinDefinitions[pinNumber];
			var controller = await GpioController.FromIdAsync(gpioDefinition.ControllerDeviceId);

			GpioPinInfo pinInfo;

			if (!controller.Pins.TryGetValue(gpioDefinition.PinNumber, out pinInfo))
				throw new SocketInterfaceCreationException("Error when querying the pin.");

			return pinInfo;
		}

		public async Task<DigitalInput> CreateDigitalInputAsync(SocketPinNumber pinNumber, GpioInputDriveMode driveMode) {
			this.EnsureTypeIsSupported((int)pinNumber <= 5 ? SocketType.X : SocketType.Y);

			if (this.DigitalInputCreator != null)
				return await this.DigitalInputCreator(this, pinNumber, driveMode);

			GpioInputPin pin;

			var pinInfo = await this.GetPinInfo(pinNumber);

			if (!pinInfo.Capabilities.IsInputSupported)
				throw new SocketInterfaceCreationException("The given pin does not support this mode.");

			if (pinInfo.TryOpenInput(GpioSharingMode.Shared, driveMode, out pin) != GpioOpenStatus.Success)
				throw new SocketInterfaceCreationException("The pin could not be created.");

			return new NativeInterfaces.DigitalInput(pin);
		}

		public async Task<DigitalOutput> CreateDigitalOutputAsync(SocketPinNumber pinNumber, bool initialValue) {
			this.EnsureTypeIsSupported((int)pinNumber <= 5 ? SocketType.X : SocketType.Y);

			if (this.DigitalInputCreator != null)
				return await this.DigitalOutputCreator(this, pinNumber, initialValue);

			GpioOutputPin pin;

			var pinInfo = await this.GetPinInfo(pinNumber);

			if (!pinInfo.Capabilities.IsOutputSupported)
				throw new SocketInterfaceCreationException("The given pin does not support this mode.");

			if (pinInfo.TryOpenOutput(initialValue ? GpioPinValue.High : GpioPinValue.Low, GpioSharingMode.Shared, out pin) != GpioOpenStatus.Success)
				throw new SocketInterfaceCreationException("The pin could not be created.");

			return new NativeInterfaces.DigitalOutput(pin);
		}

		public async Task<DigitalInterrupt> CreateDigitalInterruptAsync(SocketPinNumber pinNumber, GpioInterruptType interruptType, GpioInputDriveMode driveMode) {
			this.EnsureTypeIsSupported((int)pinNumber <= 5 ? SocketType.X : SocketType.Y);

			if (this.DigitalInputCreator != null)
				return await this.DigitalInterruptCreator(this, pinNumber, interruptType, driveMode);

			GpioInterruptPin pin;

			var pinInfo = await this.GetPinInfo(pinNumber);

			if (!pinInfo.Capabilities.IsInterruptSupported)
				throw new SocketInterfaceCreationException("The given pin does not support this mode.");

			if (pinInfo.TryOpenInterrupt(interruptType, GpioSharingMode.Shared, driveMode, out pin) != GpioOpenStatus.Success)
				throw new SocketInterfaceCreationException("The pin could not be created.");

			return new NativeInterfaces.DigitalInterrupt(pin);
		}

		public async Task<AnalogInput> CreateAnalogInputAsync(SocketPinNumber pinNumber) {
			this.EnsureTypeIsSupported(SocketType.A);

			if (this.AnalogInputCreator != null)
				return await this.AnalogInputCreator(this, pinNumber);

			throw new NotSupportedException();
		}

		public async Task<I2CDevice> CreateI2CDeviceAsync(Windows.Devices.I2C.I2CConnectionSettings connectionSettings) {
			this.EnsureTypeIsSupported(SocketType.I);

			if (this.I2CDeviceCreator != null)
				return await this.I2CDeviceCreator(this);

			var device = await Windows.Devices.I2C.I2CDevice.CreateDeviceAsync(this.NativeI2CDeviceId, connectionSettings);

			return new NativeInterfaces.I2CDevice(device);
		}
	}
}