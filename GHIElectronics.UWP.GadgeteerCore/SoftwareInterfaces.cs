using GHIElectronics.UWP.GadgeteerCore.SocketInterfaces;
using System;
using System.Threading.Tasks;

namespace GHIElectronics.UWP.GadgeteerCore.SoftwareInterfaces {
    internal class I2cDevice : SocketInterfaces.I2cDevice {
        private byte readAddress;
        private byte writeAddress;
        private DigitalIO sda;
        private DigitalIO scl;
        private bool start;

        internal I2cDevice(DigitalIO sda, DigitalIO scl, Windows.Devices.I2c.I2cConnectionSettings settings) {
            this.sda = sda;
            this.scl = scl;
            this.start = false;
            this.writeAddress = (byte)(settings.SlaveAddress << 1);
            this.readAddress = (byte)((settings.SlaveAddress << 1) | 1);
        }

        public override void Read(byte[] buffer) {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            this.Read(buffer, true, true);

            this.ReleaseScl();
            this.ReleaseSda();
        }

        public override void Write(byte[] buffer) {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            this.Write(buffer, true, true);

            this.ReleaseScl();
            this.ReleaseSda();
        }

        public override void WriteThenRead(byte[] writeBuffer, byte[] readBuffer) {
            if (readBuffer == null) throw new ArgumentNullException(nameof(readBuffer));
            if (writeBuffer == null) throw new ArgumentNullException(nameof(writeBuffer));

            this.Write(writeBuffer, true, false);
            this.Read(readBuffer, true, true);

            this.ReleaseScl();
            this.ReleaseSda();
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

        private void Write(byte[] buffer, bool sendStart, bool sendStop) {
            if (!this.Transmit(sendStart, buffer.Length == 0, this.writeAddress))
                return;

            for (var i = 0; i < buffer.Length; i++)
                if (!this.Transmit(false, i == buffer.Length - 1 ? sendStop : false, buffer[i]))
                    return;
        }

        private void Read(byte[] buffer, bool sendStart, bool sendStop) {
            if (!this.Transmit(sendStart, buffer.Length == 0, this.readAddress))
                return;

            for (var i = 0; i < buffer.Length; i++)
                buffer[i] = this.Receive(i < buffer.Length - 1, i == buffer.Length - 1 ? sendStop : false);
        }
    }

    internal class SpiDevice : SocketInterfaces.SpiDevice {
        private DigitalIO chipSelect;
        private DigitalIO masterOut;
        private DigitalIO masterIn;
        private DigitalIO clock;
        private bool captureOnRisingEdge;
        private bool clockActiveState;

        internal SpiDevice(DigitalIO chipSelect, DigitalIO masterOut, DigitalIO masterIn, DigitalIO clock, Windows.Devices.Spi.SpiConnectionSettings settings) {
            if (settings.DataBitLength != 8) throw new NotSupportedException("Only 8 data bits are supported.");

            this.chipSelect = chipSelect;
            this.masterOut = masterOut;
            this.masterIn = masterIn;
            this.clock = clock;

            this.clockActiveState = (((int)settings.Mode) & 0x02) == 0;
            this.captureOnRisingEdge = ((((int)settings.Mode) & 0x01) == 0);

            this.clock.Write(!this.clockActiveState);
            this.masterIn.Read();
            this.masterOut.Write(true);
            this.chipSelect.Write(true);
        }

        public override void Write(byte[] buffer) {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            this.WriteRead(buffer, null, true);
        }

        public override void Read(byte[] buffer) {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            this.WriteRead(null, buffer, true);
        }

        public override void WriteThenRead(byte[] writeBuffer, byte[] readBuffer) {
            if (readBuffer == null) throw new ArgumentNullException(nameof(readBuffer));
            if (writeBuffer == null) throw new ArgumentNullException(nameof(writeBuffer));

            this.WriteRead(writeBuffer, null, false);
            this.WriteRead(null, readBuffer, true);
        }

        public override void WriteAndRead(byte[] writeBuffer, byte[] readBuffer) {
            if (readBuffer == null) throw new ArgumentNullException(nameof(readBuffer));
            if (writeBuffer == null) throw new ArgumentNullException(nameof(writeBuffer));

            this.WriteRead(writeBuffer, readBuffer, true);
        }

        private void WriteRead(byte[] writeBuffer, byte[] readBuffer, bool deselectAfter) {
            if (writeBuffer == null && readBuffer == null) throw new InvalidOperationException($"At least one of {nameof(writeBuffer)} and {nameof(readBuffer)} must not be null.");

            var writeLength = writeBuffer?.Length ?? 0;
            var readLength = readBuffer?.Length ?? 0;

            if (readBuffer != null)
                Array.Clear(readBuffer, 0, readLength);

            this.clock.Write(!this.clockActiveState);
            this.chipSelect.Write(false);

            for (var i = 0; i < Math.Max(readLength, writeLength); i++) {
                byte mask = 0x80;
                var w = i < writeLength && writeBuffer != null ? writeBuffer[i] : (byte)0;
                var r = false;

                for (var j = 0; j < 8; j++) {
                    if (this.captureOnRisingEdge) {
                        this.clock.Write(!this.clockActiveState);

                        this.masterOut.Write((w & mask) != 0);
                        r = this.masterIn.Read();

                        this.clock.Write(this.clockActiveState);
                    }
                    else {
                        this.clock.Write(this.clockActiveState);

                        this.masterOut.Write((w & mask) != 0);
                        r = this.masterIn.Read();

                        this.clock.Write(!this.clockActiveState);
                    }

                    if (i < readLength && readBuffer != null && r)
                        readBuffer[i] |= mask;

                    mask >>= 1;
                }
            }

            this.clock.Write(!this.clockActiveState);

            if (deselectAfter)
                this.chipSelect.Write(true);
        }
    }
}