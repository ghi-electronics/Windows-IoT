using GHI.Athens.Gadgeteer;
using GHI.Athens.Gadgeteer.SocketInterfaces;
using System;
using System.Threading.Tasks;

namespace GHI.Athens.Modules {
	public class CurrentACS712 : Module {
		public override string Name { get; } = "CurrentACS712";
		public override string Manufacturer { get; } = "GHI Electronics, LLC";

		public uint SampleCount { get; set; } = 10;

		private AnalogInput input;

		protected async override Task Initialize(Socket parentSocket) {
			this.input = await parentSocket.CreateAnalogInputAsync(SocketPinNumber.Five);
		}

		public double ReadACCurrent() {
			var sum = 0.0;

			for (var i = 0; i < this.SampleCount; i++)
				sum += Math.Abs(this.input.ReadVoltage() - 2.083);

			sum /= this.SampleCount;
			sum /= 0.116;

			return sum;
		}

		public double ReadDCCurrent() {
			var sum = 0.0;

			for (var i = 0; i < this.SampleCount; i++) {
				sum += this.input.ReadVoltage();

				Task.Delay(2).Wait();
			}

			sum /= this.SampleCount;

			sum -= 2.083;
			sum /= 0.116;

			return sum;
		}
	}
}