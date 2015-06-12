using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using Windows.Devices.Gpio;

public class FezHat
{
    // PWM driver
    private static PCA9685 pca9685;

    // ADC
    private static I2cDevice adc;

    // Humidity & Temperature
    private static I2cDevice si7020;

    // Accelerometer
    private static I2cDevice mma8453q;

    // On-board LED
    private static GpioPin dio24;

    // DC Motors
    private static GpioPin dio12;
    private static GpioPin[] dcIn1 = new GpioPin[2];
    private static GpioPin[] dcIn2 = new GpioPin[2];

    // Buttons
    private static GpioPin dio18;
    private static GpioPin dio22;


    /// <summary>
    /// Initializes the FEZ Hat's features.
    /// </summary>
    /// <returns></returns>
    public static async Task Initialize()
    {
        GpioController gpioController = GpioController.GetDefault();
        DeviceInformationCollection i2cControllers = await DeviceInformation.FindAllAsync(I2cDevice.GetDeviceSelector("I2C1"));

        I2cConnectionSettings settings;

        // PWM driver
        settings = new I2cConnectionSettings(PCA9685.Address) { BusSpeed = I2cBusSpeed.StandardMode, SharingMode = I2cSharingMode.Shared };
        pca9685 = new PCA9685(await I2cDevice.FromIdAsync(i2cControllers[0].Id, settings), gpioController.OpenPin(13));

        // ADC
        settings = new I2cConnectionSettings(0x48) { BusSpeed = I2cBusSpeed.StandardMode };
        adc = await I2cDevice.FromIdAsync(i2cControllers[0].Id, settings);

        // SI7020 - Humidity & Temperature
        settings = new I2cConnectionSettings(0x40) { BusSpeed = I2cBusSpeed.StandardMode };
        si7020 = await I2cDevice.FromIdAsync(i2cControllers[0].Id, settings);

        // MMA8453Q - Accelerometer
        settings = new I2cConnectionSettings(0x1C) { BusSpeed = I2cBusSpeed.StandardMode };
        mma8453q = await I2cDevice.FromIdAsync(i2cControllers[0].Id, settings);
        mma8453q.Write(new byte[2] { 0x2A, 1 });

        // On-board RED LED
        dio24 = gpioController.OpenPin(24);
        dio24.SetDriveMode(GpioPinDriveMode.Output);

        // DC Motor

        // Set stand-by pin high
        dio12 = gpioController.OpenPin(12);
        dio12.SetDriveMode(GpioPinDriveMode.Output);
        dio12.Write(GpioPinValue.High);

        // A
        dcIn1[0] = gpioController.OpenPin(27);
        dcIn1[0].SetDriveMode(GpioPinDriveMode.Output);
        dcIn2[0] = gpioController.OpenPin(23);
        dcIn2[0].SetDriveMode(GpioPinDriveMode.Output);
        DCMotor.SetDirection(DCMotorId.A, DCMotor.Direction.Clockwise);

        // B
        dcIn1[1] = gpioController.OpenPin(6);
        dcIn1[1].SetDriveMode(GpioPinDriveMode.Output);
        dcIn2[1] = gpioController.OpenPin(5);
        dcIn2[1].SetDriveMode(GpioPinDriveMode.Output);
        DCMotor.SetDirection(DCMotorId.B, DCMotor.Direction.Clockwise);

        // Buttons
        dio18 = gpioController.OpenPin(18);
        dio22 = gpioController.OpenPin(22);
    }

    /// <summary>
    /// Provides different colors for the LED.
    /// </summary>
    public class Color
    {
        /// <summary>
        /// Palette of colors.
        /// </summary>
        public enum Palette : ushort
        {
            Black = 0,
            Blue = 31,
            Cyan = 2047,
            Gray = 21130,
            Green = 2016,
            Magneta = 63519,
            Orange = 64480,
            Red = 63488,
            Violet = 30751,
            White = 65535,
            Yellow = 65504,
        }

        /// <summary>
        /// Get the Red, Green and Blue values from a Palette color.
        /// </summary>
        /// <param name="color">Palette color.</param>
        /// <returns>The Red, Green and Blue values.</returns>
        public static byte[] RGBFromPalette(Color.Palette color)
        {
            ushort c = (ushort)color;
            byte r = (byte)(((c & 0xF800) >> 11) * 8);
            byte g = (byte)(((c & 0x7E0) >> 5) * 4);
            byte b = (byte)((c & 0x1F) * 8);

            return new byte[3] { r, g, b };
        }

        /// <summary>
        /// Get the Windows UI color from a Palette color.
        /// </summary>
        /// <param name="color">Palette color.</param>
        /// <returns>Windows UI color.</returns>
        public static Windows.UI.Color UIColorFromPalette(Color.Palette color)
        {
            byte[] rgb = RGBFromPalette(color);
            return Windows.UI.Color.FromArgb(255, rgb[0], rgb[1], rgb[2]);
        }
    }

    private class RGBLED
    {
        public int RedPin { get; set; }
        public int GreenPin { get; set; }
        public int BluePin { get; set; }

        public RGBLED(int redPin, int greenPin, int bluePin)
        {
            RedPin = redPin;
            GreenPin = greenPin;
            BluePin = bluePin;
        }
    }

    /// <summary>
    /// LED identification.
    /// </summary>
    public enum LEDId
    {
        D2 = 0,
        D3,
        DIO24
    }

    /// <summary>
    /// Controls the LEDs.
    /// </summary>
    public static class LED
    {
        private static RGBLED[] _leds = new RGBLED[2];
        private static Color.Palette _color;

        private static void Init()
        {
            if (_leds[0] == null)
            {
                _leds[0] = new RGBLED(1, 2, 0);
                _leds[1] = new RGBLED(4, 15, 3);
            }
        }

        /// <summary>
        /// Turn on an LED.
        /// </summary>
        /// <param name="id"></param>
        public static void TurnOn(LEDId id)
        {
            if (id == LEDId.DIO24)
            {
                dio24.Write(GpioPinValue.High);
            }
            else
            {
                Init();

                RGBLED led = _leds[(int)id];

                byte[] rgb = Color.RGBFromPalette(_color);
                double r = Math.Abs((rgb[0] / 255.0) - .99999),
                g = Math.Abs((rgb[1] / 255.0) - .99999),
                b = Math.Abs((rgb[2] / 255.0) - .99999);

                pca9685.SetDutyCycle(led.RedPin, r);
                pca9685.SetDutyCycle(led.GreenPin, g);
                pca9685.SetDutyCycle(led.BluePin, b);
            }
        }

        /// <summary>
        /// Turn on an LED with a specified color.
        /// </summary>
        /// <param name="id">LED Identifcation</param>
        /// <param name="color">Color to use.</param>
        public static void TurnOn(LEDId id, Color.Palette color)
        {
            _color = color;
            TurnOn(id);
        }

        /// <summary>
        /// Turn off all LEDs.
        /// </summary>
        public static void TurnOffAll()
        {
            TurnOff(LEDId.D2);
            TurnOff(LEDId.D3);
            TurnOff(LEDId.DIO24);
        }

        /// <summary>
        /// Turn off an LED.
        /// </summary>
        /// <param name="id"></param>
        public static void TurnOff(LEDId id)
        {
            if (id == LEDId.DIO24)
            {
                dio24.Write(GpioPinValue.Low);
            }
            else
            {
                Init();

                pca9685.TurnOff(_leds[(int)id].RedPin);
                pca9685.TurnOff(_leds[(int)id].GreenPin);
                pca9685.TurnOff(_leds[(int)id].BluePin);
            }
        }
    }

    // Simple class to wrap the PCA9685 driver (it also hides the servo methods).
    /// <summary>
    /// Handles the PWM driver chip.
    /// </summary>
    public static class PWM
    {
        /// <summary>
        /// Sets the PWM's frequency.
        /// </summary>
        public static int Frequency
        {
            get { return pca9685.Frequency; }
            set { pca9685.Frequency = value; }
        }

        /// <summary>
        /// Turns output enabled on or off.
        /// </summary>
        public static bool OutputEnabled
        {
            get { return pca9685.OutputEnabled; }
            set { pca9685.OutputEnabled = value; }
        }

        /// <summary>
        /// Set's the PWM's duty cycle.
        /// </summary>
        /// <param name="channel">Channel to set.</param>
        /// <param name="dutyCycle">Duty cycle to use.</param>
        public static void SetDutyCycle(int channel, double dutyCycle)
        {
            pca9685.SetDutyCycle(channel, dutyCycle);
        }

        /// <summary>
        /// Turn a channel on.
        /// </summary>
        /// <param name="channel">The channel to turn on.</param>
        public static void TurnOn(int channel)
        {
            pca9685.TurnOn(channel);
        }

        /// <summary>
        /// Turn a channel off.
        /// </summary>
        /// <param name="channel">The channel to turn off.</param>
        public static void TurnOff(int channel)
        {
            pca9685.TurnOff(channel);
        }
    }

    /// <summary>
    /// Servo motor identification.
    /// </summary>
    public enum ServoMotorId : int
    {
        // New
        //S1 = 9,
        //S2 = 10
        // Old
        S1 = 8,
        S2 = 9,
        S3 = 10
    }

    /// <summary>
    /// Controls the servo motors.
    /// </summary>
    public static class ServoMotor
    {
        private static int _freq;
        private static double _minAngle;
        private static double _maxAngle;

        /// <summary>
        /// Set the movement limits of all servo motors.
        /// </summary>
        /// <param name="frequency">Frequency (Hz)</param>
        /// <param name="minPulseWidth">Minimum pulse width.</param>
        /// <param name="maxPulseWidth">Maximum pulse width.</param>
        /// <param name="minAngle">Minimum angle.</param>
        /// <param name="maxAngle">Maximum angle.</param>
        public static void Setup(int frequency, int minPulseWidth, int maxPulseWidth, double minAngle, double maxAngle)
        {
            _freq = frequency;
            _minAngle = minAngle;
            _maxAngle = maxAngle;

            pca9685.Frequency = _freq;
            pca9685.SetServoLimits(minPulseWidth, maxPulseWidth, minAngle, maxAngle);
        }

        /// <summary>
        /// Set the servo motor position.
        /// </summary>
        /// <param name="id">Servo motor identifcation.</param>
        /// <param name="angle">The angle to use.</param>
        public static void SetPosition(ServoMotorId id, double angle)
        {
            if (angle < _minAngle || angle > _maxAngle)
                throw new Exception("angle must be a value between " + _minAngle + " and " + _maxAngle + ".");

            pca9685.Frequency = _freq;
            pca9685.SetServoPosition((int)id, angle);
        }

        /// <summary>
        /// Stop the servo motor from moving.
        /// </summary>
        /// <param name="id">Servo motor identifcation.</param>
        public static void Stop(ServoMotorId id)
        {
            pca9685.TurnOff((int)id);
        }
    }

    /// <summary>
    /// DC motor identifcation.
    /// </summary>
    public enum DCMotorId : int
    {
        A = 14,
        B = 13
    }

    /// <summary>
    /// Controls the DC motors.
    /// </summary>
    public static class DCMotor
    {
        /// <summary>
        /// Direction the motor may rotate.
        /// </summary>
        public enum Direction
        {
            Clockwise,
            CounterClockwise
        }

        /// <summary>
        /// Sets the rotation direction and speed.
        /// </summary>
        /// <param name="id">DC motor identifcation.</param>
        /// <param name="direction">Clockwise or Counter-Clockwise.</param>
        /// <param name="speed">How fast to rotate.</param>
        public static void SetRotation(DCMotorId id, Direction direction, double speed)
        {
            SetSpeed(id, speed);
            SetDirection(id, direction);
        }

        /// <summary>
        /// Sets the motor's speed.
        /// </summary>
        /// <param name="id">DC motor identifcation.</param>
        /// <param name="speed">The speed to use.</param>
        public static void SetSpeed(DCMotorId id, double speed)
        {
            pca9685.Frequency = 60;
            pca9685.SetDutyCycle((int)id, speed);
        }

        /// <summary>
        /// Set the direcation the motor rotates.
        /// </summary>
        /// <param name="id">DC motor identifcation.</param>
        /// <param name="direction">The direction to use.</param>
        public static void SetDirection(DCMotorId id, Direction direction)
        {
            int i = (id == DCMotorId.A) ? 0 : 1;

            if (direction == Direction.Clockwise)
            {
                dcIn1[i].Write(GpioPinValue.High);
                dcIn2[i].Write(GpioPinValue.Low);
            }
            else
            {
                dcIn1[i].Write(GpioPinValue.Low);
                dcIn2[i].Write(GpioPinValue.High);
            }
        }

        /// <summary>
        /// Stop a motor from rotating.
        /// </summary>
        /// <param name="id">DC motor identifcation.</param>
        public static void Stop(DCMotorId id)
        {
            pca9685.TurnOff((int)id);
        }

        /// <summary>
        /// Stop all motors from rotating.
        /// </summary>
        public static void StopAll()
        {
            Stop(DCMotorId.A);
            Stop(DCMotorId.B);
        }
    }

    /// <summary>
    /// Reads the light sensor.
    /// </summary>
    public static class LightSensor
    {
        /// <summary>
        /// Get the brightness level.
        /// </summary>
        /// <returns>The brightness level between 0 and 1; 0 being no light and 1 being full brightness.</returns>
        public static double GetLevel()
        {
            return ReadAnalogByChannel(adc, 5);
        }
    }

    /// <summary>
    /// Reads the temperature and humidity.
    /// </summary>
    public static class Weather
    {
        private static double _si7020_celsius;
        private static double _si7020_humid;

        /// <summary>
        /// What scale to use to measure the temperature.
        /// </summary>
        public enum TempScale
        {
            Celsius,
            Fahrenheit
        }

        /// <summary>
        /// Get the analog temperature in Celsius.
        /// </summary>
        /// <returns>Temperature in Celsius.</returns>
        public static double GetAnalogTemp()
        {
            return GetAnalogTemp(TempScale.Celsius);
        }

        /// <summary>
        /// Get the analog temperature.
        /// </summary>
        /// <param name="scale">Celsius or Fahrenheit.</param>
        /// <returns>The temperature in the desired scale.</returns>
        public static double GetAnalogTemp(TempScale scale)
        {
            double mv = ReadAnalogByChannel(adc, 4) * 3300;
            double celsius = (mv - 450.0) / 19.5;

            if (scale == TempScale.Celsius)
                return celsius;
            else
                return (celsius * 1.8) + 32;
        }

        /// <summary>
        /// Get the temperature from the SI7020 chip in Celsius.
        /// </summary>
        /// <returns>Temperature in Celsius.</returns>
        public static double GetSI7020Temp()
        {
            return GetSI7020Temp(TempScale.Celsius);
        }

        /// <summary>
        /// Get the temperature from the SI7020 chip.
        /// </summary>
        /// <param name="scale">Celsius or Fahrenheit.</param>
        /// <returns>The temperature in the desired scale.</returns>
        public static double GetSI7020Temp(TempScale scale)
        {
            ReadSI7020();

            if (scale == TempScale.Celsius)
                return _si7020_celsius;
            else
                return (_si7020_celsius * 1.8) + 32;
        }

        /// <summary>
        /// Get the humidity from the SI7020 chip.
        /// </summary>
        /// <returns>Humidity</returns>
        public static double GetHumidity()
        {
            ReadSI7020();

            return _si7020_humid;
        }

        private static void ReadSI7020()
        {
            byte[] writeBuffer1 = new byte[1] { 0xE5 };
            byte[] writeBuffer2 = new byte[1] { 0xE0 };
            byte[] readBuffer1 = new byte[2];
            byte[] readBuffer2 = new byte[2];

            si7020.WriteRead(writeBuffer1, readBuffer1);
            si7020.WriteRead(writeBuffer2, readBuffer2);

            int rawRH = readBuffer1[0] << 8 | readBuffer1[1];
            int rawTemp = readBuffer2[0] << 8 | readBuffer2[1];

            _si7020_celsius = 175.72 * rawTemp / 65536.0 - 46.85;
            _si7020_humid = 125.0 * rawRH / 65536.0 - 6.0;

            if (_si7020_humid < 0.0)
                _si7020_humid = 0.0;
            if (_si7020_humid > 100.0)
                _si7020_humid = 100.0;
        }
    }

    /// <summary>
    /// Reads the accelerometer.
    /// </summary>
    public static class Accelerometer
    {
        private static double ReadAxis(byte register)
        {
            byte[] data = new byte[2];
            mma8453q.WriteRead(new byte[1] { register }, data);

            double value = data[0] << 2 | data[1] >> 6;
            if ((value > 511.0))
                value -= 1024.0;
            value /= 512.0;
            return value;
        }

        /// <summary>
        /// Reads the X axis.
        /// </summary>
        /// <returns>Value representing the X axis.</returns>
        public static double ReadX()
        {
            return ReadAxis(0x1);
        }

        /// <summary>
        /// Reads the Y axis.
        /// </summary>
        /// <returns>Value representing the Y axis.</returns>
        public static double ReadY()
        {
            return ReadAxis(0x3);
        }

        /// <summary>
        /// Reads the Z axis.
        /// </summary>
        /// <returns>Value representing the Z axis.</returns>
        public static double ReadZ()
        {
            return ReadAxis(0x5);
        }
    }

    /// <summary>
    /// Button identifcation.
    /// </summary>
    public enum ButtonId
    {
        DIO18 = 0,
        DIO22
    }

    /// <summary>
    /// Handles the button interaction.
    /// </summary>
    public static class Button
    {
        private static bool _initalized = false;
        private static bool[] _pressed = new bool[] { false, false };

        private static void Initalize()
        {
            if (!_initalized)
            {
                if (dio18.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
                    dio18.SetDriveMode(GpioPinDriveMode.InputPullUp);
                else
                    dio18.SetDriveMode(GpioPinDriveMode.Input);

                dio18.DebounceTimeout = TimeSpan.FromMilliseconds(50);
                dio18.ValueChanged += Dio18_ValueChanged;

                if (dio22.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
                    dio22.SetDriveMode(GpioPinDriveMode.InputPullUp);
                else
                    dio22.SetDriveMode(GpioPinDriveMode.Input);
                
                dio22.DebounceTimeout = TimeSpan.FromMilliseconds(50);
                dio22.ValueChanged += Dio22_ValueChanged;

                _initalized = true;
            }
        }

        /// <summary>
        /// Determines whether a button is pressed.
        /// </summary>
        /// <param name="id">Button identification.</param>
        /// <returns>True if the button is pressed, otherwise false.</returns>
        public static bool IsPressed(ButtonId id)
        {
            Initalize();

            return _pressed[(int)id];
        }

        private static void Dio18_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            _pressed[0] = (args.Edge == GpioPinEdge.FallingEdge);
        }

        private static void Dio22_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            _pressed[1] = (args.Edge == GpioPinEdge.FallingEdge);
        }
    }

    /// <summary>
    /// Expansion header pins.
    /// </summary>
    public static class Expansion
    {
        public enum Pin : int
        {
            /// <summary>
            /// Digital Input/Output (channel 0)
            /// </summary>
            DIO0 = 0,
            /// <summary>
            /// Digital Input/Output (channel 1)
            /// </summary>
            DIO1 = 1,
            /// <summary>
            /// Analog In (channel 1)
            /// </summary>
            AIn1 = 1,
            /// <summary>
            /// Analog In (channel 2)
            /// </summary>
            AIn2 = 2,
            /// <summary>
            /// Analog In (channel 3)
            /// </summary>
            AIn3 = 3,
            /// <summary>
            /// Pluse-Width Modulation (channel 5)
            /// </summary>
            PWM5 = 5,
            /// <summary>
            /// Pluse-Width Modulation (channel 6)
            /// </summary>
            PWM6 = 6,
            /// <summary>
            /// Pluse-Width Modulation (channel 7)
            /// </summary>
            PWM7 = 7,
            /// <summary>
            /// Master Output, Slave Input (output from master).
            /// </summary>
            MOSI = 19,
            /// <summary>
            /// Master Input, Slave Output (output from slave).
            /// </summary>
            MISO = 21,
            /// <summary>
            /// Serial Clock (output from master).
            /// </summary>
            SCLK = 23,
            /// <summary>
            /// Chip Select (CS)
            /// </summary>
            CS = 25,
            /// <summary>
            /// Serial Data Signal
            /// </summary>
            SDA = 3,
            /// <summary>
            /// Serial Clock
            /// </summary>
            SCL = 5
        }
    }

    /// <summary>
    /// Terminal block header pins.
    /// </summary>
    public static class TerminalBlock
    {
        public enum Pin : int
        {
            /// <summary>
            /// Analog In (channel 6)
            /// </summary>
            AIn6 = 6,
            /// <summary>
            /// Analog In (channel 7)
            /// </summary>
            AIn7 = 7,
            /// <summary>
            /// Digital Input/Output (channel 16)
            /// </summary>
            DIO16 = 16,
            /// <summary>
            /// Digital Input/Output (channel 26)
            /// </summary>
            DIO26 = 26,
            /// <summary>
            /// Pluse-Width Modulation (channel 11)
            /// </summary>
            PWM11 = 11,
            /// <summary>
            /// Pluse-Width Modulation (channel 22)
            /// </summary>
            PWM12 = 22
        }
    }

    private static double ReadAnalogByChannel(I2cDevice device, int channel)
    {
        if (channel >= 8)
            throw new ArgumentOutOfRangeException("channel", "Invalid channel.");

        byte[] write = new byte[1] { (byte)(0x80 | 0x0C) };
        byte[] read = new byte[1];

        write[0] |= (byte)((channel % 2 == 0 ? channel / 2 : (channel - 1) / 2 + 4) << 4);

        device.WriteRead(write, read);
        return (double)read[0] / 255;
    }
}