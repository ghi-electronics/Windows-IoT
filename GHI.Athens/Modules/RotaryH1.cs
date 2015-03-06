using GHI.Athens.Gadgeteer;
using GHI.Athens.Gadgeteer.SocketInterfaces;
using System;
using System.Threading.Tasks;

namespace GHI.Athens.Modules {
	public class RotaryH1 : Module {
		private byte[] write1;
		private byte[] write2;
		private byte[] read5;

		private SpiDevice spi;
		private DigitalOutput enable;
		private CountMode mode;

		protected Socket socket;

		public override string Name { get; } = "RotaryH1";
		public override string Manufacturer { get; } = "GHI Electronics, LLC";

		protected async override Task Initialize(Socket parentSocket) {
			this.socket = parentSocket;

			this.write1 = new byte[1];
			this.write2 = new byte[2];
			this.read5 = new byte[5];

			this.spi = await this.socket.CreateSpiDeviceAsync(new SpiConfiguration() { ClockRate = 1000, SlaveSelectActiveHigh = false, ClockIdleHigh = false, ClockSampleOnRising = true }, SocketPinNumber.Six);
			this.enable = await this.socket.CreateDigitalOutputAsync(SocketPinNumber.Five, true);

			this.Write(Command.Clear, Register.Mode0);
			this.Write(Command.Clear, Register.Mode1);
			this.Write(Command.Clear, Register.Status);
			this.Write(Command.Clear, Register.Counter);
			this.Write(Command.Load, Register.Output);

			this.Mode = CountMode.Quad1;

			this.Write(Command.Write, Register.Mode1, Mode1.FourByte | Mode1.EnableCount);
		}

		public int GetCount() {
			this.Write(Command.Load, Register.Output);

			return this.Read4(Command.Read, Register.Output);
		}

		public void ResetCount() {
			this.Write(Command.Clear, Register.Counter);
		}

		public CountMode Mode {
			get {
				return this.mode;
			}
			set {
				if (this.mode == value)
					return;

				this.mode = value;

				var command = Mode0.FreeRunning | Mode0.DisableIndex | Mode0.FilterClockDivisionTwo;

				switch (this.mode) {
					case CountMode.None: command |= Mode0.Quad0; break;
					case CountMode.Quad1: command |= Mode0.Quad1; break;
					case CountMode.Quad2: command |= Mode0.Quad2; break;
					case CountMode.Quad4: command |= Mode0.Quad4; break;
				}

				this.Write(Command.Write, Register.Mode0, command);
			}
		}

		private int Read4(Command command, Register register) {
			this.write1[0] = (byte)((byte)command | (byte)register);

			this.spi.WriteRead(this.write1, this.read5);

			return (this.read5[1] << 24) + (this.read5[2] << 16) + (this.read5[3] << 8) + this.read5[4];
		}

		private void Write(Command command, Register register) {
			this.write1[0] = (byte)((byte)command | (byte)register);

			this.spi.WriteRead(this.write1, null);
		}

		private void Write(Command command, Register register, Mode0 mode) {
			this.write2[0] = (byte)((byte)command | (byte)register);
			this.write2[1] = (byte)mode;

			this.spi.WriteRead(this.write2, null);
		}

		private void Write(Command command, Register register, Mode1 mode) {
			this.write2[0] = (byte)((byte)command | (byte)register);
			this.write2[1] = (byte)mode;

			this.spi.WriteRead(this.write2, null);
		}

		public enum Direction : byte {
			CounterClockwise,
			Clockwise
		}

		private enum Command {
			Clear = 0x00,
			Read = 0x40,
			Write = 0x80,
			Load = 0xC0,
		}

		private enum Register {
			Mode0 = 0x08,
			Mode1 = 0x10,
			Input = 0x18,
			Counter = 0x20,
			Output = 0x28,
			Status = 0x30,
		}

		[Flags]
		private enum Mode0 {
			Quad0 = 0x00,
			Quad1 = 0x01,
			Quad2 = 0x02,
			Quad4 = 0x03,
			FreeRunning = 0x00,
			SingleCycleCount = 0x04,
			Range = 0x08,
			ModuloN = 0x0C,
			DisableIndex = 0x00,
			IndexAsLoadCount = 0x10,
			IndexAsResetCount = 0x20,
			IndexAsLoadOutput = 0x30,
			AsynchronousIndex = 0x00,
			SynchronousIndex = 0x40,
			FilterClockDivisionOne = 0x00,
			FilterClockDivisionTwo = 0x80,
		}

		[Flags]
		private enum Mode1 {
			FourByte = 0x00,
			ThreeByte = 0x01,
			TwoByte = 0x02,
			OneByte = 0x03,
			EnableCount = 0x00,
			DisableCount = 0x04,
			FlagIndex = 0x10,
			FlagCompare = 0x20,
			FlagBorrow = 0x40,
			FlagCarry = 0x80
		}

		public enum CountMode {
			None,
			Quad1,
			Quad2,
			Quad4
		}
	}
}