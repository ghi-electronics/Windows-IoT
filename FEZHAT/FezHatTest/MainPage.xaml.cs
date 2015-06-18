using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FezHatTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            Initialize();
        }

        public async void Initialize()
        {
            var rotary = new RotaryH1();
            while (true)
            {
                Debug.WriteLine("Count: " + rotary.GetCount());
                await Task.Delay(2000);
            }

            /*
            await FezHat.Initialize();

            FezHat.ServoMotor.Setup(50, 1000, 2000, 0, 180);
            FezHat.Led.TurnOffAll();

            await Task.Delay(2000);

            // Test expansion pins

            FezHat.Gpio.SetDriveMode(FezHat.Gpio.Pin.DIO0, FezHat.Gpio.DriveMode.Output);
            FezHat.Gpio.SetDriveMode(FezHat.Gpio.Pin.DIO1, FezHat.Gpio.DriveMode.Output);
            FezHat.Gpio.SetDriveMode(FezHat.Gpio.Pin.DIO16, FezHat.Gpio.DriveMode.Output);
            FezHat.Gpio.SetDriveMode(FezHat.Gpio.Pin.DIO26, FezHat.Gpio.DriveMode.Output);

            var toggle = true;

            while (true)
            {
                Debug.WriteLine("---------------------------------------------------------------------------------------------");

                FezHat.Gpio.Write(FezHat.Gpio.Pin.DIO0, toggle ? FezHat.Gpio.Value.High : FezHat.Gpio.Value.Low);
                FezHat.Gpio.Write(FezHat.Gpio.Pin.DIO1, toggle ? FezHat.Gpio.Value.High : FezHat.Gpio.Value.Low);
                FezHat.Gpio.Write(FezHat.Gpio.Pin.DIO16, toggle ? FezHat.Gpio.Value.High : FezHat.Gpio.Value.Low);
                FezHat.Gpio.Write(FezHat.Gpio.Pin.DIO26, toggle ? FezHat.Gpio.Value.High : FezHat.Gpio.Value.Low);

                Debug.WriteLine("DIO0: " + FezHat.Gpio.Read(FezHat.Gpio.Pin.DIO0).ToString());
                Debug.WriteLine("DIO1: " + FezHat.Gpio.Read(FezHat.Gpio.Pin.DIO1).ToString());
                Debug.WriteLine("DIO16: " + FezHat.Gpio.Read(FezHat.Gpio.Pin.DIO16).ToString());
                Debug.WriteLine("DIO26: " + FezHat.Gpio.Read(FezHat.Gpio.Pin.DIO26).ToString());

                if (toggle)
                {
                    FezHat.Pwm.TurnOn(FezHat.Pwm.Pin.PWM5);
                    FezHat.Pwm.TurnOn(FezHat.Pwm.Pin.PWM6);
                    FezHat.Pwm.TurnOn(FezHat.Pwm.Pin.PWM7);
                    FezHat.Pwm.TurnOn(FezHat.Pwm.Pin.PWM11);
                    FezHat.Pwm.TurnOn(FezHat.Pwm.Pin.PWM12);
                }
                else
                {
                    FezHat.Pwm.TurnOff(FezHat.Pwm.Pin.PWM5);
                    FezHat.Pwm.TurnOff(FezHat.Pwm.Pin.PWM6);
                    FezHat.Pwm.TurnOff(FezHat.Pwm.Pin.PWM7);
                    FezHat.Pwm.TurnOff(FezHat.Pwm.Pin.PWM11);
                    FezHat.Pwm.TurnOff(FezHat.Pwm.Pin.PWM12);
                }

                Debug.WriteLine("AIn1: " + FezHat.Adc.Read(FezHat.Adc.Pin.AIn1).ToString("F3"));
                Debug.WriteLine("AIn2: " + FezHat.Adc.Read(FezHat.Adc.Pin.AIn2).ToString("F3"));
                Debug.WriteLine("AIn3: " + FezHat.Adc.Read(FezHat.Adc.Pin.AIn3).ToString("F3"));
                Debug.WriteLine("AIn6: " + FezHat.Adc.Read(FezHat.Adc.Pin.AIn6).ToString("F3"));
                Debug.WriteLine("AIn7: " + FezHat.Adc.Read(FezHat.Adc.Pin.AIn7).ToString("F3"));

                toggle = !toggle;
                await Task.Delay(2000);
            }

            // Test everything

            while (true)
            {
                // On-board LED

                FezHat.Led.TurnOn(FezHat.LedID.DIO24);

                // RGB Leds

                FezHat.Led.TurnOn(FezHat.LedID.D2, FezHat.Color.Palette.Red);
                FezHat.Led.TurnOn(FezHat.LedID.D3, FezHat.Color.Palette.Red);
                await Task.Delay(200);

                FezHat.Led.TurnOn(FezHat.LedID.D2, FezHat.Color.Palette.Green);
                FezHat.Led.TurnOn(FezHat.LedID.D3, FezHat.Color.Palette.Green);
                await Task.Delay(200);

                FezHat.Led.TurnOn(FezHat.LedID.D2, FezHat.Color.Palette.Blue);
                FezHat.Led.TurnOn(FezHat.LedID.D3, FezHat.Color.Palette.Blue);
                await Task.Delay(200);

                FezHat.Led.TurnOn(FezHat.LedID.D2, FezHat.Color.Palette.Yellow);
                FezHat.Led.TurnOn(FezHat.LedID.D3, FezHat.Color.Palette.Yellow);
                await Task.Delay(200);

                FezHat.Led.TurnOn(FezHat.LedID.D2, FezHat.Color.Palette.Violet);
                FezHat.Led.TurnOn(FezHat.LedID.D3, FezHat.Color.Palette.Violet);
                await Task.Delay(200);

                FezHat.Led.TurnOffAll();

                // Servo Motors

                FezHat.ServoMotor.SetPosition(FezHat.ServoMotorID.S2, 0);
                await Task.Delay(500);

                FezHat.ServoMotor.SetPosition(FezHat.ServoMotorID.S2, 180);
                await Task.Delay(500);

                // DC Motor

                FezHat.DCMotor.SetRotation(FezHat.DCMotorID.B, FezHat.DCMotor.Direction.Clockwise, 1);
                await Task.Delay(500);

                FezHat.DCMotor.SetRotation(FezHat.DCMotorID.B, FezHat.DCMotor.Direction.CounterClockwise, 1);
                await Task.Delay(500);

                FezHat.DCMotor.Stop(FezHat.DCMotorID.B);

                Debug.WriteLine("-----------------------------------------------------------------------------------------------------------");

                // Temperature

                Debug.WriteLine("Temperature analog: " + FezHat.Weather.GetTemperature() + " C / " + FezHat.Weather.GetTemperature(FezHat.Weather.TemperatureScale.Fahrenheit) + " F");

                // Accelerometer

                Debug.WriteLine("Accelerometer: " + FezHat.Accelerometer.ReadX() + ", " + FezHat.Accelerometer.ReadY() + ", " + FezHat.Accelerometer.ReadZ());

                // Light Sensor

                Debug.WriteLine("Light sensor: " + FezHat.LightSensor.GetLevel());

                // Buttons

                if (FezHat.Button.IsPressed(FezHat.ButtonID.DIO18))
                    Debug.WriteLine("DIO18 is pressed.");

                if (FezHat.Button.IsPressed(FezHat.ButtonID.DIO22))
                    Debug.WriteLine("DIO22 is pressed.");

                await Task.Delay(50);
            }
            */
        }
    }
}
