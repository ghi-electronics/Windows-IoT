using GHI.Athens.Gadgeteer;
using GHI.Athens.Gadgeteer.SocketInterfaces;
using System.Threading.Tasks;

namespace GHI.Athens.Modules {
	public class LightSense : Module {
		public override string Name { get; } = "LightSense";
		public override string Manufacturer { get; } = "GHI Electronics, LLC";

		private AnalogIO input;

		protected async override Task Initialize(Socket parentSocket) {
			this.input = await parentSocket.CreateAnalogIOAsync(SocketPinNumber.Three);
		}

		public double GetReading() {
			return this.input.ReadProportion();
		}
	}
}