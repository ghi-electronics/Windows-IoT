using GHI.Athens.Gadgeteer.SocketInterfaces;
using System;
using System.Threading.Tasks;

namespace GHI.Athens.Gadgeteer.SoftwareInterfaces {
	internal class I2CDevice : SocketInterfaces.I2CDevice {
		private byte readAddress;
		private byte writeAddress;
		private DigitalInputOutput sda;
		private DigitalInputOutput scl;
		private bool start;

		internal I2CDevice(DigitalInputOutput sda, DigitalInputOutput scl, Windows.Devices.I2C.I2CConnectionSettings settings) {
			if (settings.AddressingMode != Windows.Devices.I2C.I2CAddressingMode.SevenBit) throw new NotSupportedException("Only 7 bit addressing is supported.");

			this.sda = sda;
			this.scl = scl;
			this.start = false;
			this.writeAddress = (byte)(settings.SlaveAddress << 1);
			this.readAddress = (byte)((settings.SlaveAddress << 1) | 1);
		}

		public override Windows.Devices.I2C.I2CTransferStatus Read(byte[] buffer, out uint transferred) {
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));

			var result = this.Read(buffer, true, true, out transferred);

			this.ReleaseScl();
			this.ReleaseSda();

			return result;
		}

		public override Windows.Devices.I2C.I2CTransferStatus Write(byte[] buffer, out uint transferred) {
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));

			var result = this.Write(buffer, true, true, out transferred);

			this.ReleaseScl();
			this.ReleaseSda();

			return result;
		}

		public override Windows.Devices.I2C.I2CTransferStatus WriteRead(byte[] writeBuffer, byte[] readBuffer, out uint transferred) {
			if (readBuffer == null) throw new ArgumentNullException(nameof(readBuffer));
			if (writeBuffer == null) throw new ArgumentNullException(nameof(writeBuffer));

			var result = this.Write(writeBuffer, true, false, out transferred);

			if (result == Windows.Devices.I2C.I2CTransferStatus.Success) {
				var soFar = transferred;

				result = this.Read(readBuffer, true, true, out transferred);

				transferred += soFar;
			}

			this.ReleaseScl();
			this.ReleaseSda();

			return result;
		}

		private void ClearScl() {
			this.scl.Write(false);
		}

		private void ClearSda() {
			this.sda.Write(false);
		}

		private void ReleaseScl() {
			this.ReadScl();
		}

		private void ReleaseSda() {
			this.ReadSda();
		}

		private bool ReadScl() {
			return this.scl.Read();
		}

		private bool ReadSda() {
			return this.sda.Read();
		}

		private void WaitForScl() {
			while (!this.ReadScl())
				Task.Delay(1).Wait();
		}

		private bool WriteBit(bool bit) {
			if (bit)
				this.ReleaseSda();
			else
				this.ClearSda();

			this.WaitForScl();

			if (bit && !this.ReadSda())
				return false;

			this.ClearScl();

			return true;
		}

		private bool ReadBit() {
			this.ReleaseSda();

			this.WaitForScl();

			bool bit = this.ReadSda();

			this.ClearScl();

			return bit;
		}

		private bool SendStart() {
			if (this.start) {
				this.ReleaseSda();

				this.WaitForScl();
			}

			if (!this.ReadSda())
				return false;

			this.ClearSda();

			this.ClearScl();

			this.start = true;

			return true;
		}

		private bool SendStop() {
			this.ClearSda();

			this.WaitForScl();

			if (!this.ReadSda())
				return false;

			this.start = false;

			return true;
		}

		private bool Transmit(bool sendStart, bool sendStop, byte data) {
			if (sendStart)
				this.SendStart();

			for (var bit = 0; bit < 8; bit++) {
				this.WriteBit((data & 0x80) != 0);

				data <<= 1;
			}

			bool nack = this.ReadBit();

			if (sendStop)
				this.SendStop();

			return !nack;
		}

		private byte Receive(bool sendAck, bool sendStop) {
			byte d = 0;

			for (var bit = 0; bit < 8; bit++)
				d = (byte)((d << 1) | (this.ReadBit() ? 1 : 0));

			this.WriteBit(!sendAck);

			if (sendStop)
				this.SendStop();

			return d;
		}

		private Windows.Devices.I2C.I2CTransferStatus Write(byte[] buffer, bool sendStart, bool sendStop, out uint transferred) {
			transferred = 0;

			if (!this.Transmit(sendStart, buffer.Length == 0, this.writeAddress))
				return Windows.Devices.I2C.I2CTransferStatus.SlaveAddressNotAcknowledged;

			for (; transferred < buffer.Length; transferred++)
				if (!this.Transmit(false, transferred == buffer.Length - 1 ? sendStop : false, buffer[transferred]))
					return Windows.Devices.I2C.I2CTransferStatus.PartialTransfer;

			return Windows.Devices.I2C.I2CTransferStatus.Success;
		}

		private Windows.Devices.I2C.I2CTransferStatus Read(byte[] buffer, bool sendStart, bool sendStop, out uint transferred) {
			transferred = 0;

			if (!this.Transmit(sendStart, buffer.Length == 0, this.readAddress))
				return Windows.Devices.I2C.I2CTransferStatus.SlaveAddressNotAcknowledged;

			for (; transferred < buffer.Length; transferred++)
				buffer[transferred] = this.Receive(transferred < buffer.Length - 1, transferred == buffer.Length - 1 ? sendStop : false);

			return Windows.Devices.I2C.I2CTransferStatus.Success;
		}
	}

	internal class SpiDevice : SocketInterfaces.SpiDevice {
		private DigitalOutput slaveSelect;
		private DigitalOutput masterOut;
		private DigitalInput masterIn;
		private DigitalOutput clock;
		private SpiConfiguration configuration;

        internal SpiDevice(SpiConfiguration configuration, DigitalOutput slaveSelect, DigitalOutput masterOut, DigitalInput masterIn, DigitalOutput clock) {
			this.slaveSelect = slaveSelect;
			this.masterOut = masterOut;
			this.masterIn = masterIn;
			this.clock = clock;
			this.configuration = configuration;
		}

		public override void WriteRead(byte[] writeBuffer, byte[] readBuffer) {
			var writeLength = writeBuffer.Length;
			var readLength = 0;

			if (readBuffer != null) {
				readLength = readBuffer.Length;

				for (int i = 0; i < readLength; i++)
					readBuffer[i] = 0;
			}

			this.slaveSelect.Write(this.configuration.SlaveSelectActiveHigh);

			Task.Delay((int)this.configuration.SlaveSelectSetupTime).Wait();

			for (var i = 0; i < (writeLength < readLength ? readLength : writeLength); i++) {
				byte w = 0;

				if (i < writeLength)
					w = writeBuffer[i];

				byte mask = 0x80;

				for (int j = 0; j < 8; j++) {
					this.clock.Write(!this.configuration.ClockIdleHigh);

					this.masterOut.Write((w & mask) != 0);

					this.clock.Write(this.configuration.ClockIdleHigh);

					if (readBuffer != null)
						readBuffer[i] |= (this.masterIn.Read() ? mask : (byte)0x00);

					mask >>= 1;
				}

				this.masterOut.Write(false);

				this.clock.Write(this.configuration.ClockIdleHigh);
			}

			Task.Delay((int)this.configuration.SlaveSelectHoldTime).Wait();

			this.slaveSelect.Write(!this.configuration.SlaveSelectActiveHigh);
		}
	}
}