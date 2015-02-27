using System;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Devices.I2C;

namespace GHI.Athens.Gadgeteer {
	public class SocketInterfaces {
		public static GpioOutputPin pin { get; private set; }

		public static GpioInputPin CreateDigitalInput(Socket socket, SocketPinNumber pinNumber, GpioSharingMode sharingMode, GpioInputDriveMode driveMode) {
			return SocketInterfaces.CreateDigitalInputAsync(socket, pinNumber, sharingMode, driveMode).WaitForResults();
		}

		public static GpioOutputPin CreateDigitalOutput(Socket socket, SocketPinNumber pinNumber, GpioPinValue initialValue, GpioSharingMode sharingMode) {
			return SocketInterfaces.CreateDigitalOutputAsync(socket, pinNumber, initialValue, sharingMode).WaitForResults();
		}

		public static GpioInterruptPin CreateDigitalInterrupt(Socket socket, SocketPinNumber pinNumber, GpioInterruptType interruptType, GpioSharingMode sharingMode, GpioInputDriveMode driveMode) {
			return SocketInterfaces.CreateDigitalInterruptAsync(socket, pinNumber, interruptType, sharingMode, driveMode).WaitForResults();
		}

		public static I2CDevice CreateI2CDevice(Socket socket, I2CConnectionSettings connectionSettings) {
			return SocketInterfaces.CreateI2CDeviceAsync(socket, connectionSettings).WaitForResults();
		}

		public static async Task<GpioInputPin> CreateDigitalInputAsync(Socket socket, SocketPinNumber pinNumber, GpioSharingMode sharingMode, GpioInputDriveMode driveMode) {
			var gpioDefinition = socket.GpioPinDefinitions[pinNumber];
			var controller = await GpioController.FromIdAsync(gpioDefinition.ControllerDeviceId);

			GpioPinInfo pinInfo;
			GpioInputPin pin;

			if (!controller.Pins.TryGetValue(gpioDefinition.PinNumber, out pinInfo) || !pinInfo.Capabilities.IsInputSupported)
				throw new Exception();

			if (pinInfo.TryOpenInput(sharingMode, driveMode, out pin) != GpioOpenStatus.Success)
				throw new Exception();

			return pin;
		}

		public static async Task<GpioOutputPin> CreateDigitalOutputAsync(Socket socket, SocketPinNumber pinNumber, GpioPinValue initialValue, GpioSharingMode sharingMode) {
			var gpioDefinition = socket.GpioPinDefinitions[pinNumber];
			var controller = await GpioController.FromIdAsync(gpioDefinition.ControllerDeviceId);

			GpioPinInfo pinInfo;
			GpioOutputPin pin;

			if (!controller.Pins.TryGetValue(gpioDefinition.PinNumber, out pinInfo) || !pinInfo.Capabilities.IsOutputSupported)
				throw new Exception();

			if (pinInfo.TryOpenOutput(initialValue, sharingMode, out pin) != GpioOpenStatus.Success)
				throw new Exception();

			return pin;
		}

		public static async Task<GpioInterruptPin> CreateDigitalInterruptAsync(Socket socket, SocketPinNumber pinNumber, GpioInterruptType interruptType, GpioSharingMode sharingMode, GpioInputDriveMode driveMode) {
			var gpioDefinition = socket.GpioPinDefinitions[pinNumber];
			var controller = await GpioController.FromIdAsync(gpioDefinition.ControllerDeviceId);

			GpioPinInfo pinInfo;
			GpioInterruptPin pin;

			if (!controller.Pins.TryGetValue(gpioDefinition.PinNumber, out pinInfo) || !pinInfo.Capabilities.IsInterruptSupported)
				throw new Exception();

			if (pinInfo.TryOpenInterrupt(interruptType, sharingMode, driveMode, out pin) != GpioOpenStatus.Success)
				throw new Exception();

			return pin;
		}

		public static async Task<I2CDevice> CreateI2CDeviceAsync(Socket socket, I2CConnectionSettings connectionSettings) {
			return await I2CDevice.CreateDeviceAsync(socket.I2CDeviceId, connectionSettings);
		}
	}
}