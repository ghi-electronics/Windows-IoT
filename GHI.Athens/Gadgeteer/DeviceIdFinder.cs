using System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2C;

namespace GHI.Athens.Gadgeteer {
	public static class DeviceIdFinder {
		public static async Task<string> GetGpioIdAsync(string friendlyName) {
			var info = await DeviceInformation.FindAllAsync(GpioController.GetDeviceSelector(friendlyName), null);

			return info[0].Id;
		}

		public static async Task<string> GetI2CIdAsync() {
			var info = await DeviceInformation.FindAllAsync(I2CDevice.GetDeviceSelector());

			return info[2].Id;
		}
	}
}