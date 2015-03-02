using System;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Devices.I2C;

namespace GHI.Athens.Gadgeteer {
	public class SocketInterfaces {
		public class PinConfigurationException : Exception {
			public PinConfigurationException() { }
			public PinConfigurationException(string message) : base(message) { }
			public PinConfigurationException(string message, Exception inner) : base(message, inner) { }
		}

		public static GpioOutputPin pin { get; private set; }

		public static async Task<GpioInputPin> CreateDigitalInputAsync(Socket socket, SocketPinNumber pinNumber, GpioSharingMode sharingMode, GpioInputDriveMode driveMode) {
			var gpioDefinition = socket.GpioPinDefinitions[pinNumber];
			var controller = await GpioController.FromIdAsync(gpioDefinition.ControllerDeviceId);

			GpioPinInfo pinInfo;
			GpioInputPin pin;

			if (!controller.Pins.TryGetValue(gpioDefinition.PinNumber, out pinInfo) || !pinInfo.Capabilities.IsInputSupported)
				throw new PinConfigurationException("The given pin does not support this mode.");

			if (pinInfo.TryOpenInput(sharingMode, driveMode, out pin) != GpioOpenStatus.Success)
				throw new PinConfigurationException("The interface could not be created.");

			return pin;
		}

		public static async Task<GpioOutputPin> CreateDigitalOutputAsync(Socket socket, SocketPinNumber pinNumber, GpioPinValue initialValue, GpioSharingMode sharingMode) {
			var gpioDefinition = socket.GpioPinDefinitions[pinNumber];
			var controller = await GpioController.FromIdAsync(gpioDefinition.ControllerDeviceId);

			GpioPinInfo pinInfo;
			GpioOutputPin pin;

			if (!controller.Pins.TryGetValue(gpioDefinition.PinNumber, out pinInfo) || !pinInfo.Capabilities.IsOutputSupported)
				throw new PinConfigurationException("The given pin does not support this mode.");

			if (pinInfo.TryOpenOutput(initialValue, sharingMode, out pin) != GpioOpenStatus.Success)
				throw new PinConfigurationException("The interface could not be created.");

			return pin;
		}

		public static async Task<GpioInterruptPin> CreateDigitalInterruptAsync(Socket socket, SocketPinNumber pinNumber, GpioInterruptType interruptType, GpioSharingMode sharingMode, GpioInputDriveMode driveMode) {
			var gpioDefinition = socket.GpioPinDefinitions[pinNumber];
			var controller = await GpioController.FromIdAsync(gpioDefinition.ControllerDeviceId);

			GpioPinInfo pinInfo;
			GpioInterruptPin pin;

			if (!controller.Pins.TryGetValue(gpioDefinition.PinNumber, out pinInfo) || !pinInfo.Capabilities.IsInterruptSupported)
				throw new PinConfigurationException("The given pin does not support this mode.");

			if (pinInfo.TryOpenInterrupt(interruptType, sharingMode, driveMode, out pin) != GpioOpenStatus.Success)
				throw new PinConfigurationException("The interface could not be created.");

			return pin;
		}

		public static async Task<I2CDevice> CreateI2CDeviceAsync(Socket socket, I2CConnectionSettings connectionSettings) {
			return await I2CDevice.CreateDeviceAsync(socket.I2CDeviceId, connectionSettings);
		}
	}
}