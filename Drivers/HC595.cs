using System;
using Windows.Devices.Spi;

namespace GHIElectronics.UWP.LowLevelDrivers {
    public class HC595 : IDisposable {
        private SpiDevice device;
        private byte[] buffer;
        private bool disposed;

        public static SpiConnectionSettings GetConnectionSettings(int chipSelect) => new SpiConnectionSettings(chipSelect) { ClockFrequency = 1000000, DataBitLength = 8, SharingMode = SpiSharingMode.Shared, Mode = SpiMode.Mode0 };

        public void Dispose() => this.Dispose(true);

        public HC595(SpiDevice device) {
            this.device = device;
            this.buffer = new byte[1];
            this.disposed = false;
        }

        public void SetAll(bool state) {
            if (this.disposed) throw new ObjectDisposedException(nameof(HC595));

            this.buffer[0] = (byte)(state ? 0xFF : 0x00);

            this.device.Write(this.buffer);
        }

        public void SetPin(int pin, bool state) {
            if (this.disposed) throw new ObjectDisposedException(nameof(HC595));
            if (pin < 0 || pin > 7) throw new ArgumentOutOfRangeException(nameof(pin));

            var mask = 1 << pin;

            this.buffer[0] = (byte)(state ? this.buffer[0] | mask : this.buffer[0] & ~mask);

            this.device.Write(this.buffer);
        }

        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                if (disposing) {
                    this.device.Dispose();
                }

                this.disposed = true;
            }
        }
    }
}
