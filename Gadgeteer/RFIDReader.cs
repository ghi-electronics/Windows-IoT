using System.Text;
using System.Threading.Tasks;
using GHIElectronics.UWP.GadgeteerCore;
using Windows.Devices.SerialCommunication;

using GSI = GHIElectronics.UWP.GadgeteerCore.SocketInterfaces;

namespace GHIElectronics.UWP.Gadgeteer.Modules {
    public class RFIDReader : Module {
        public override string Name => "RFID Reader";
        public override string Manufacturer => "GHI Electronics, LLC";

        private static int MessageLength => 13;

        private GSI.SerialDevice port;
        private byte[] buffer;

        protected async override Task Initialize(ISocket parentSocket) {
            this.buffer = new byte[RFIDReader.MessageLength];

            this.port = await parentSocket.CreateSerialDeviceAsync();
            this.port.BaudRate = 9600;
            this.port.DataBits = 8;
            this.port.Parity = SerialParity.None;
            this.port.StopBits = SerialStopBitCount.Two;
            this.port.Handshake = SerialHandshake.None;
        }

        private int AsciiTonumber(byte upper, byte lower) {
            var high = upper - 48 - (upper >= 'A' ? 7 : 0);
            var low = lower - 48 - (lower >= 'A' ? 7 : 0);

            return (high << 4) | low;
        }

        public string ReadId() {
            var read = 0;
            while (read != RFIDReader.MessageLength)
                read += this.port.ReadByte();

            var checksum = 0;
            for (int i = 1; i < 10; i += 2)
                checksum ^= this.AsciiTonumber(this.buffer[i], this.buffer[i + 1]);

            if (this.buffer[0] == 0x02 && this.buffer[12] == 0x03 && checksum == this.buffer[11]) {
                return new string(Encoding.UTF8.GetChars(this.buffer, 1, 10));
            }
            else {
                return null;
            }
        }
    }
}