using System;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;

public class PCA9685
{
    private I2cDevice device;
    private GpioPin outputEnable;
    private byte[] write5;
    private byte[] write2;
    private byte[] write1;
    private byte[] read1;
    private double minAngle;
    private double maxAngle;
    private double scale;
    private double offset;

    private enum Register
    {
        Mode1 = 0x00,
        Mode2 = 0x01,
        Led0OnLow = 0x06,
        Prescale = 0xFE
    }

    public static byte Address => 0x7F;

    public PCA9685(I2cDevice device, GpioPin outputEnable)
    {
        this.write5 = new byte[5];
        this.write2 = new byte[2];
        this.write1 = new byte[1];
        this.read1 = new byte[1];

        this.device = device;
        this.outputEnable = outputEnable;

        this.outputEnable.SetDriveMode(GpioPinDriveMode.Output);
        this.outputEnable.Write(GpioPinValue.Low);

        this.WriteRegister(Register.Mode1, 0x20);
        this.WriteRegister(Register.Mode2, 0x06);
    }

    //in hertz
    public int Frequency
    {
        get
        {
            return (int)(25000000 / (4096 * (this.ReadRegister(Register.Prescale) + 1)) / 0.9);
        }
        set
        {
            if (value < 40 || value > 1500) throw new ArgumentOutOfRangeException(nameof(value), "Valid range is 40 to 1500.");

            value *= 10;
            value /= 9;

            var mode = this.ReadRegister(Register.Mode1);

            this.WriteRegister(Register.Mode1, (byte)(mode | 0x10));

            this.WriteRegister(Register.Prescale, (byte)(25000000 / (4096 * value) - 1));

            this.WriteRegister(Register.Mode1, mode);
        }
    }

    public bool OutputEnabled
    {
        get
        {
            return this.outputEnable.Read() == GpioPinValue.Low;
        }
        set
        {
            this.outputEnable.Write(value ? GpioPinValue.Low : GpioPinValue.High);
        }
    }

    //pulse widths in microseconds, angles in degrees.
    public void SetServoLimits(int minPulseWidth, int maxPulseWidth, double minAngle, double maxAngle)
    {
        if (minPulseWidth < 0) throw new ArgumentOutOfRangeException(nameof(minPulseWidth));
        if (maxPulseWidth < 0) throw new ArgumentOutOfRangeException(nameof(maxPulseWidth));
        if (minAngle < 0) throw new ArgumentOutOfRangeException(nameof(minAngle));
        if (maxAngle < 0) throw new ArgumentOutOfRangeException(nameof(maxAngle));
        if (minPulseWidth >= maxPulseWidth) throw new ArgumentException(nameof(minPulseWidth));
        if (minAngle >= maxAngle) throw new ArgumentException(nameof(minAngle));

        this.minAngle = minAngle;
        this.maxAngle = maxAngle;

        var period = 1000000.0 / this.Frequency;

        minPulseWidth = (int)(minPulseWidth / period * 4096.0);
        maxPulseWidth = (int)(maxPulseWidth / period * 4096.0);

        this.scale = ((maxPulseWidth - minPulseWidth) / (maxAngle - minAngle));
        this.offset = minPulseWidth;
    }

    //position in degrees
    public void SetServoPosition(int channel, double position)
    {
        if (channel < 0 || channel > 15) throw new ArgumentOutOfRangeException(nameof(channel));
        if (position < this.minAngle || position > this.maxAngle) throw new ArgumentException(nameof(position));
        if (this.maxAngle == 0.0) throw new InvalidOperationException($"You must call {nameof(this.SetServoLimits)} first.");

        this.SetChannel((byte)channel, 0x0000, (ushort)(this.scale * position + this.offset));
    }

    public void TurnOn(int channel)
    {
        if (channel < 0 || channel > 15) throw new ArgumentOutOfRangeException(nameof(channel));

        this.SetChannel((byte)channel, 0x1000, 0x0000);
    }

    public void TurnOff(int channel)
    {
        if (channel < 0 || channel > 15) throw new ArgumentOutOfRangeException(nameof(channel));

        this.SetChannel((byte)channel, 0x0000, 0x1000);
    }

    //duty cycle between 0 and 1
    public void SetDutyCycle(int channel, double dutyCycle)
    {
        if (channel < 0 || channel > 15) throw new ArgumentOutOfRangeException(nameof(channel));
        if (dutyCycle < 0.0 || dutyCycle > 1.0) throw new ArgumentOutOfRangeException(nameof(dutyCycle));

        this.SetChannel((byte)channel, 0x0000, (ushort)(4096 * dutyCycle));
    }

    private void SetChannel(byte channel, ushort on, ushort off)
    {
        this.write5[0] = (byte)((byte)Register.Led0OnLow + channel * 4);
        this.write5[1] = (byte)on;
        this.write5[2] = (byte)(on >> 8);
        this.write5[3] = (byte)off;
        this.write5[4] = (byte)(off >> 8);

        this.device.Write(this.write5);
    }

    private void WriteRegister(Register register, byte value)
    {
        this.write2[0] = (byte)register;
        this.write2[1] = value;

        this.device.Write(this.write2);
    }

    private byte ReadRegister(Register register)
    {
        this.write1[0] = (byte)register;

        this.device.WriteRead(this.write1, this.read1);

        return this.read1[0];
    }
}