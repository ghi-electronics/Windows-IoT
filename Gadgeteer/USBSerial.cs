using GHIElectronics.UWP.GadgeteerCore;
using System;
using System.Threading.Tasks;
using Windows.Devices.SerialCommunication;

using GSI = GHIElectronics.UWP.GadgeteerCore.SocketInterfaces;

namespace GHIElectronics.UWP.Gadgeteer.Modules {
    public class USBSerial : Module {
        public override string Name => "USBSerial";
        public override string Manufacturer => "GHI Electronics, LLC";

        private ISocket parentSocket;
        private GSI.SerialDevice port;

        public GSI.SerialDevice Port {
            get {
                if (this.port == null) throw new InvalidOperationException("You must call Configure first.");

                return this.port;
            }
        }

        protected override Task Initialize(ISocket parentSocket) {
            this.parentSocket = parentSocket;

            return Task.CompletedTask;
        }

        public async Task ConfigureAsync() {
            await this.ConfigureAsync(9600, SerialParity.None, SerialStopBitCount.One, 8, SerialHandshake.None);
        }

        public async Task ConfigureAsync(uint baudRate, SerialParity parity, SerialStopBitCount stopBits, ushort dataBits, SerialHandshake flowControl) {
            this.port = await parentSocket.CreateSerialDeviceAsync();
            this.port.BaudRate = baudRate;
            this.port.DataBits = dataBits;
            this.port.Parity = parity;
            this.port.StopBits = stopBits;
            this.port.Handshake = flowControl;
        }
    }
}