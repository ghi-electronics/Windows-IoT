using GHI.Athens.Gadgeteer;
using GHI.Athens.Gadgeteer.SocketInterfaces;
using System.Threading.Tasks;
using System;
using Windows.Devices.Gpio;

namespace GHI.Athens.Modules {
	public class TheProfessor : Module {
		private ADS7830 ads;

		public override string Name { get; } = "The Professor";
		public override string Manufacturer { get; } = "GHI Electronics, LLC";
		public override uint RequiredSockets { get; } = 0;

		protected async override Task Initialize() {
			this.ads = new ADS7830();
			this.ads.A1 = true;

			Socket socket;

			socket = this.CreateSocket(1);
			socket.AddSupportedTypes(SocketType.S, SocketType.U, SocketType.Y);
			socket.SetNativePin(SocketPinNumber.Three, 3);
			socket.NativeSpiDeviceId = "";

			socket = this.CreateSocket(2);
			socket.AddSupportedTypes(SocketType.P, SocketType.U, SocketType.Y);
			socket.SetNativePin(SocketPinNumber.Three, 9);
			socket.SetNativePin(SocketPinNumber.Eight, 7);
			socket.SetNativePin(SocketPinNumber.Nine, 8);

			socket = this.CreateSocket(3);
			socket.AddSupportedTypes(SocketType.A, SocketType.I, SocketType.Y);
			socket.SetNativePin(SocketPinNumber.Three, 0);
			socket.SetNativePin(SocketPinNumber.Four, 4);
			socket.SetNativePin(SocketPinNumber.Five, 1);
			socket.SetNativePin(SocketPinNumber.Six, 2);
			socket.SetNativePin(SocketPinNumber.Seven, 5);
			socket.NativeI2CDeviceId = "I2C5";
			socket.AnalogIOCreator = (s, p) => Task.FromResult<AnalogIO>(new IndirectedAnalogIO(p, this.ads));

			await this.ads.Initialize(this.GetProvidedSocket(3));
		}

		private class IndirectedAnalogIO : AnalogIO {
			private byte channel;
			private ADS7830 ads;

			public override double MaxVoltage { get; } = 3.3;

			public IndirectedAnalogIO(SocketPinNumber pin, ADS7830 ads) {
				this.ads = ads;

				switch (pin) {
					case SocketPinNumber.Three: this.channel = 0; break;
					case SocketPinNumber.Four: this.channel = 2; break;
					case SocketPinNumber.Five: this.channel = 1; break;
				}
			}

			protected override double ReadInternal() {
				return this.ads.ReadVoltage(this.channel);
			}

			protected override void WriteInternal(double voltage) {
				throw new NotSupportedException();
			}

			public override GpioPinDriveMode DriveMode {
				get {
					return GpioPinDriveMode.Input;
				}

				set {
					if (value != GpioPinDriveMode.Input)
						throw new NotSupportedException();
				}
			}
		}
	}
}