using GHI.Athens.Gadgeteer;
using GHI.Athens.Gadgeteer.SocketInterfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace GHI.Athens.Modules {
	public class HubAP5 : Module {
		private ADS7830 ads;
		private CY8C9560A cy8;
		private List<Dictionary<SocketPinNumber, CY8C9560A.Pin>> map;

		public override string Name { get; } = "Hub AP5";
		public override string Manufacturer { get; } = "GHI Electronics, LLC";

		protected async override Task Initialize(Socket parentSocket) {
			this.ads = new ADS7830();
			this.cy8 = new CY8C9560A();

			await this.ads.Initialize(parentSocket);
			await this.cy8.Initialize(parentSocket);

			this.map = this.CreatePinMap();

			Socket socket;
			for (var i = 0U; i < 8; i++) {
				socket = this.AddProvidedSocket(i + 1);
				socket.AddSupportedTypes(SocketType.Y);

				if (i < 2) {
					socket.AddSupportedTypes(SocketType.A);
				}
				else if (i > 2) {
					socket.AddSupportedTypes(SocketType.P);
				}

				socket.DigitalInputCreator = (indirectedSocket, indirectedPin, driveMode) => Task.FromResult<DigitalInput>(new IndirectedDigitalInput(this.GetPin(indirectedSocket, indirectedPin), driveMode, this.cy8));
				socket.DigitalOutputCreator = (indirectedSocket, indirectedPin, initialState) => Task.FromResult<DigitalOutput>(new IndirectedDigitalOutput(this.GetPin(indirectedSocket, indirectedPin), initialState, this.cy8));
				socket.DigitalInputOutputCreator = (indirectedSocket, indirectedPin, mode, driveMode, initialOutputValue) => Task.FromResult<DigitalInputOutput>(new IndirectedDigitalInputOutput(this.GetPin(indirectedSocket, indirectedPin), mode, driveMode, initialOutputValue, this.cy8));
				socket.AnalogInputCreator = (indirectedSocket, indirectedPin) => Task.FromResult<AnalogInput>(new IndirectedAnalogInput(indirectedPin, this.GetPin(indirectedSocket, indirectedPin), this.ads, this.cy8));
				socket.PwmOutputCreator = (indirectedSocket, indirectedPin) => Task.FromResult<PwmOutput>(new IndirectedPwmOutput(GetPin(indirectedSocket, indirectedPin), this.cy8));
			}
		}

		private List<Dictionary<SocketPinNumber, CY8C9560A.Pin>> CreatePinMap() {
			var s1 = new Dictionary<SocketPinNumber, CY8C9560A.Pin>();
			s1.Add(SocketPinNumber.Three, new CY8C9560A.Pin() { Port = 5, PinNumber = 0 });
			s1.Add(SocketPinNumber.Four, new CY8C9560A.Pin() { Port = 5, PinNumber = 1 });
			s1.Add(SocketPinNumber.Five, new CY8C9560A.Pin() { Port = 5, PinNumber = 2 });
			s1.Add(SocketPinNumber.Six, new CY8C9560A.Pin() { Port = 5, PinNumber = 3 });
			s1.Add(SocketPinNumber.Seven, new CY8C9560A.Pin() { Port = 5, PinNumber = 4 });
			s1.Add(SocketPinNumber.Eight, new CY8C9560A.Pin() { Port = 5, PinNumber = 5 });
			s1.Add(SocketPinNumber.Nine, new CY8C9560A.Pin() { Port = 5, PinNumber = 6 });

			var s2 = new Dictionary<SocketPinNumber, CY8C9560A.Pin>();
			s2.Add(SocketPinNumber.Three, new CY8C9560A.Pin() { Port = 4, PinNumber = 0 });
			s2.Add(SocketPinNumber.Four, new CY8C9560A.Pin() { Port = 4, PinNumber = 1 });
			s2.Add(SocketPinNumber.Five, new CY8C9560A.Pin() { Port = 4, PinNumber = 2 });
			s2.Add(SocketPinNumber.Six, new CY8C9560A.Pin() { Port = 4, PinNumber = 3 });
			s2.Add(SocketPinNumber.Seven, new CY8C9560A.Pin() { Port = 4, PinNumber = 4 });
			s2.Add(SocketPinNumber.Eight, new CY8C9560A.Pin() { Port = 4, PinNumber = 5 });
			s2.Add(SocketPinNumber.Nine, new CY8C9560A.Pin() { Port = 4, PinNumber = 6 });

			var s3 = new Dictionary<SocketPinNumber, CY8C9560A.Pin>();
			s3.Add(SocketPinNumber.Three, new CY8C9560A.Pin() { Port = 3, PinNumber = 0 });
			s3.Add(SocketPinNumber.Four, new CY8C9560A.Pin() { Port = 3, PinNumber = 1 });
			s3.Add(SocketPinNumber.Five, new CY8C9560A.Pin() { Port = 3, PinNumber = 2 });
			s3.Add(SocketPinNumber.Six, new CY8C9560A.Pin() { Port = 3, PinNumber = 3 });
			s3.Add(SocketPinNumber.Seven, new CY8C9560A.Pin() { Port = 3, PinNumber = 4 });
			s3.Add(SocketPinNumber.Eight, new CY8C9560A.Pin() { Port = 3, PinNumber = 5 });
			s3.Add(SocketPinNumber.Nine, new CY8C9560A.Pin() { Port = 3, PinNumber = 6 });

			var s4 = new Dictionary<SocketPinNumber, CY8C9560A.Pin>();
			s4.Add(SocketPinNumber.Three, new CY8C9560A.Pin() { Port = 2, PinNumber = 0 });
			s4.Add(SocketPinNumber.Four, new CY8C9560A.Pin() { Port = 2, PinNumber = 1 });
			s4.Add(SocketPinNumber.Five, new CY8C9560A.Pin() { Port = 2, PinNumber = 2 });
			s4.Add(SocketPinNumber.Six, new CY8C9560A.Pin() { Port = 2, PinNumber = 3 });
			s4.Add(SocketPinNumber.Seven, new CY8C9560A.Pin() { Port = 7, PinNumber = 4 });
			s4.Add(SocketPinNumber.Eight, new CY8C9560A.Pin() { Port = 7, PinNumber = 5 });
			s4.Add(SocketPinNumber.Nine, new CY8C9560A.Pin() { Port = 7, PinNumber = 6 });

			var s5 = new Dictionary<SocketPinNumber, CY8C9560A.Pin>();
			s5.Add(SocketPinNumber.Three, new CY8C9560A.Pin() { Port = 1, PinNumber = 4 });
			s5.Add(SocketPinNumber.Four, new CY8C9560A.Pin() { Port = 1, PinNumber = 5 });
			s5.Add(SocketPinNumber.Five, new CY8C9560A.Pin() { Port = 1, PinNumber = 6 });
			s5.Add(SocketPinNumber.Six, new CY8C9560A.Pin() { Port = 1, PinNumber = 7 });
			s5.Add(SocketPinNumber.Seven, new CY8C9560A.Pin() { Port = 7, PinNumber = 1 });
			s5.Add(SocketPinNumber.Eight, new CY8C9560A.Pin() { Port = 7, PinNumber = 2 });
			s5.Add(SocketPinNumber.Nine, new CY8C9560A.Pin() { Port = 7, PinNumber = 3 });

			var s6 = new Dictionary<SocketPinNumber, CY8C9560A.Pin>();
			s6.Add(SocketPinNumber.Three, new CY8C9560A.Pin() { Port = 1, PinNumber = 0 });
			s6.Add(SocketPinNumber.Four, new CY8C9560A.Pin() { Port = 1, PinNumber = 1 });
			s6.Add(SocketPinNumber.Five, new CY8C9560A.Pin() { Port = 1, PinNumber = 2 });
			s6.Add(SocketPinNumber.Six, new CY8C9560A.Pin() { Port = 1, PinNumber = 3 });
			s6.Add(SocketPinNumber.Seven, new CY8C9560A.Pin() { Port = 6, PinNumber = 6 });
			s6.Add(SocketPinNumber.Eight, new CY8C9560A.Pin() { Port = 6, PinNumber = 7 });
			s6.Add(SocketPinNumber.Nine, new CY8C9560A.Pin() { Port = 6, PinNumber = 0 });

			var s7 = new Dictionary<SocketPinNumber, CY8C9560A.Pin>();
			s7.Add(SocketPinNumber.Three, new CY8C9560A.Pin() { Port = 0, PinNumber = 4 });
			s7.Add(SocketPinNumber.Four, new CY8C9560A.Pin() { Port = 0, PinNumber = 5 });
			s7.Add(SocketPinNumber.Five, new CY8C9560A.Pin() { Port = 0, PinNumber = 6 });
			s7.Add(SocketPinNumber.Six, new CY8C9560A.Pin() { Port = 0, PinNumber = 7 });
			s7.Add(SocketPinNumber.Seven, new CY8C9560A.Pin() { Port = 6, PinNumber = 3 });
			s7.Add(SocketPinNumber.Eight, new CY8C9560A.Pin() { Port = 6, PinNumber = 4 });
			s7.Add(SocketPinNumber.Nine, new CY8C9560A.Pin() { Port = 6, PinNumber = 5 });

			var s8 = new Dictionary<SocketPinNumber, CY8C9560A.Pin>();
			s8.Add(SocketPinNumber.Three, new CY8C9560A.Pin() { Port = 0, PinNumber = 0 });
			s8.Add(SocketPinNumber.Four, new CY8C9560A.Pin() { Port = 0, PinNumber = 1 });
			s8.Add(SocketPinNumber.Five, new CY8C9560A.Pin() { Port = 0, PinNumber = 2 });
			s8.Add(SocketPinNumber.Six, new CY8C9560A.Pin() { Port = 0, PinNumber = 3 });
			s8.Add(SocketPinNumber.Seven, new CY8C9560A.Pin() { Port = 6, PinNumber = 0 });
			s8.Add(SocketPinNumber.Eight, new CY8C9560A.Pin() { Port = 6, PinNumber = 1 });
			s8.Add(SocketPinNumber.Nine, new CY8C9560A.Pin() { Port = 6, PinNumber = 2 });

			return new List<Dictionary<SocketPinNumber, CY8C9560A.Pin>>() { s1, s2, s3, s4, s5, s6, s7, s8 };
		}

		private CY8C9560A.Pin GetPin(Socket socket, SocketPinNumber pin) {
			return this.map[(int)socket.Number - 1][pin];
		}

		private class IndirectedDigitalOutput : DigitalOutput {
			private CY8C9560A cy8;
			private CY8C9560A.Pin pin;

			public IndirectedDigitalOutput(CY8C9560A.Pin pin, bool initialState, CY8C9560A cy8) {
				this.pin = pin;

				this.cy8 = cy8;
				this.cy8.SetOutput(this.pin);

				this.Write(initialState);
			}

			public override bool Read() {
				return this.cy8.ReadDigital(this.pin);
			}

			public override void Write(bool state) {
				this.cy8.WriteDigital(this.pin, state);
			}
		}

		private class IndirectedDigitalInput : DigitalInput {
			private CY8C9560A cy8;
			private CY8C9560A.Pin pin;
			private GpioInputDriveMode driveMode;

			public IndirectedDigitalInput(CY8C9560A.Pin pin, GpioInputDriveMode driveMode, CY8C9560A cy8) {
				this.pin = pin;
				this.driveMode = driveMode;

				this.cy8 = cy8;
				this.cy8.SetInput(this.pin, driveMode);
			}

			public override bool Read() {
				return this.cy8.ReadDigital(this.pin);
			}

			public override GpioInputDriveMode DriveMode {
				get {
					return this.driveMode;
				}
				set {
					throw new NotSupportedException();
				}
			}
		}

		private class IndirectedDigitalInputOutput : DigitalInputOutput {
			private CY8C9560A cy8;
			private CY8C9560A.Pin pin;
			private GpioInputDriveMode driveMode;

			public IndirectedDigitalInputOutput(CY8C9560A.Pin pin, DigitalInputOutputMode mode, GpioInputDriveMode driveMode, bool initalOutputState, CY8C9560A cy8) {
				this.cy8 = cy8;
				this.pin = pin;
				this.driveMode = driveMode;

				if (mode == DigitalInputOutputMode.Output) {
					this.Write(initalOutputState);
				}
				else {
					this.Read();
				}
			}

			public override bool Read() {
				this.Mode = DigitalInputOutputMode.Input;

				this.cy8.SetInput(this.pin, this.driveMode);

				return this.cy8.ReadDigital(this.pin);
			}

			public override void Write(bool state) {
				this.Mode = DigitalInputOutputMode.Output;

				this.cy8.SetOutput(this.pin);

				this.cy8.WriteDigital(this.pin, state);
			}

			public override GpioInputDriveMode DriveMode {
				get {
					return this.driveMode;
				}
				set {
					throw new NotSupportedException();
				}
			}
		}

		private class IndirectedAnalogInput : AnalogInput {
			private byte channel;
			private ADS7830 ads;
			private CY8C9560A cy8;

			public override double MaxVoltage { get; } = 3.3;

			public IndirectedAnalogInput(SocketPinNumber pinNumber, CY8C9560A.Pin pin, ADS7830 ads, CY8C9560A cy8) {
				this.ads = ads;

				this.cy8 = cy8;
				this.cy8.SetInput(pin, GpioInputDriveMode.HighImpedance);

				switch (pinNumber) {
					case SocketPinNumber.Three: this.channel = 0; break;
					case SocketPinNumber.Four: this.channel = 2; break;
					case SocketPinNumber.Five: this.channel = 1; break;
				}
			}

			public override double ReadVoltage() {
				return this.ads.ReadVoltage(this.channel);
			}
		}

		private class IndirectedPwmOutput : PwmOutput {
			private CY8C9560A cy8;
			private CY8C9560A.Pin pin;

			public IndirectedPwmOutput(CY8C9560A.Pin pin, CY8C9560A cy8) {
				this.cy8 = cy8;
				this.pin = pin;
			}

			protected override void SetEnabled(bool state) {
				if (state) {
					this.cy8.SetPwm(this.pin, this.Frequency, this.DutyCycle);
				}
				else {
					this.cy8.SetInput(this.pin, GpioInputDriveMode.HighImpedance);
				}
			}

			protected override void SetValues(double frequency, double dutyCycle) {
				this.cy8.SetPwm(this.pin, frequency, dutyCycle);
			}
		}
	}
}