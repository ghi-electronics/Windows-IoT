namespace GHI.Athens.Gadgeteer {
	public class GpioPinDefinition {
		public string ControllerDeviceId { get; private set; }
		public uint PinNumber { get; private set; }

		public GpioPinDefinition(string controllerDeviceId, uint pinNumber) {
			this.ControllerDeviceId = controllerDeviceId;
			this.PinNumber = pinNumber;
		}
	}
}