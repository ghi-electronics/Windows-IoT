using System.Threading.Tasks;
using GHIElectronics.UWP.GadgeteerCore;
using Windows.Devices.I2c;
using Windows.Devices.Spi;

using GSI = GHIElectronics.UWP.GadgeteerCore.SocketInterfaces;

namespace GHIElectronics.UWP.Gadgeteer.Modules {
    public class Breakout : Module {
        public override string Name => "Breakout";
        public override string Manufacturer => "GHI Electronics, LLC";

        private ISocket socket;

        protected override Task Initialize(ISocket parentSocket) {
            this.socket = parentSocket;

            return Task.CompletedTask;
        }

        public async Task<GSI.DigitalIO> CreateDigitalIOAsync(SocketPinNumber pinNumber) => await this.socket.CreateDigitalIOAsync(pinNumber);
        public async Task<GSI.DigitalIO> CreateDigitalIOAsync(SocketPinNumber pinNumber, bool initialValue) => await this.socket.CreateDigitalIOAsync(pinNumber, initialValue);
        public async Task<GSI.AnalogIO> CreateAnalogIOAsync(SocketPinNumber pinNumber) => await this.socket.CreateAnalogIOAsync(pinNumber);
        public async Task<GSI.AnalogIO> CreateAnalogIOAsync(SocketPinNumber pinNumber, double initialVoltage) => await this.socket.CreateAnalogIOAsync(pinNumber, initialVoltage);
        public async Task<GSI.PwmOutput> CreatePwmOutputAsync(SocketPinNumber pinNumber) => await this.socket.CreatePwmOutputAsync(pinNumber);
        public async Task<GSI.I2cDevice> CreateI2cDeviceAsync(I2cConnectionSettings settings) => await this.socket.CreateI2cDeviceAsync(settings);
        public async Task<GSI.SpiDevice> CreateSpiDeviceAsync(SpiConnectionSettings settings) => await this.socket.CreateSpiDeviceAsync(settings);
        public async Task<GSI.SerialDevice> CreateSerialDeviceAsync() => await this.socket.CreateSerialDeviceAsync();
    }
}