using System;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;

namespace GHI.Athens.Gadgeteer {
	public class GpioPinDefinition {
		public string ControllerDeviceId { get; private set; }
		public uint PinNumber { get; private set; }

		public GpioPinDefinition(string controllerDeviceId, uint pinNumber) {
			this.ControllerDeviceId = controllerDeviceId;
			this.PinNumber = pinNumber;
		}

		public static string GetDeviceIdFromFriendlyName(string friendlyName) {
			var deviceId = GpioController.GetDeviceSelector(friendlyName);
			var deviceInfosTask = DeviceInformation.FindAllAsync(deviceId, null).AsTask();
			
			deviceInfosTask.Wait();

			return deviceInfosTask.Result[0].Id;
        }
	}
}