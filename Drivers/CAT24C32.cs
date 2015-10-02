using System;
using System.Threading.Tasks;
using Windows.Devices.I2c;

namespace GHIElectronics.UWP.LowLevelDrivers {
	public class CAT24C32 {
		private I2cDevice device;
		private bool disposed;
		private byte[] buffer1;
		private byte[] buffer2;
		private byte[] buffer34;

		public static byte GetAddress(bool a0, bool a1, bool a2) => (byte)(0x50 | (a0 ? 1 : 0) | (a1 ? 2 : 0) | (a2 ? 4 : 0));

		public void Dispose() => this.Dispose(true);

		public CAT24C32(I2cDevice device) {
			this.device = device;
			this.buffer1 = new byte[1];
			this.buffer2 = new byte[2];
			this.buffer34 = new byte[34];
			this.disposed = false;
		}

		protected virtual void Dispose(bool disposing) {
			if (!this.disposed) {
				if (disposing) {
					this.device.Dispose();
				}

				this.disposed = true;
			}
		}

		public void WriteByte(ushort address, byte data) {
			this.buffer1[0] = data;

			this.Write(address, this.buffer1);
		}

		public byte ReadByte(ushort address) {
			this.Read(address, this.buffer1);

			return this.buffer1[0];
		}

		public byte[] Read(ushort address, int length) {
			if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length), "Must be positive.");

			var buffer = new byte[length];

			this.Read(address, buffer);

			return buffer;
		}

		public void Read(ushort address, byte[] buffer) {
			if (this.disposed) throw new ObjectDisposedException(nameof(CAT24C32));
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (address + buffer.Length > 4096) throw new ArgumentOutOfRangeException(nameof(address), "The end address cannot exceed 4096.");

			this.SetAddress(address, this.buffer2);

			this.device.WriteRead(this.buffer2, buffer);
		}

		public void Write(ushort address, byte[] buffer) {
			if (this.disposed) throw new ObjectDisposedException(nameof(CAT24C32));
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (address + buffer.Length > 4096) throw new ArgumentOutOfRangeException(nameof(address), "The end address cannot exceed 4096.");


			for (var i = 0; i < buffer.Length / 32; i++) {
				var temp = BitConverter.GetBytes(address + i * 32);

				this.SetAddress((ushort)(address + i * 32), this.buffer34);

				Array.Copy(buffer, i * 32, this.buffer34, 2, 32);

				this.device.Write(this.buffer34);

				Task.Delay(10).Wait();
			}

			var remaining = buffer.Length % 32;
			if (remaining != 0) {
				var remainingBuffer = new byte[remaining + 2];

				this.SetAddress((ushort)(address + buffer.Length - remaining), remainingBuffer);

				Array.Copy(buffer, buffer.Length - remaining, remainingBuffer, 2, remaining);

				this.device.Write(remainingBuffer);

				Task.Delay(10).Wait();
			}
		}

		private void SetAddress(ushort address, byte[] buffer) {
			buffer[0] = (byte)((address & 0xFF00) >> 8);
			buffer[1] = (byte)((address & 0x00FF) >> 0);
		}
	}
}
