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
            
            await FezHat.Initialize();

            FezHat.ServoMotor.Setup(50, 1000, 2000, 0, 180);
            FezHat.Led.TurnOffAll();

            await Task.Delay(2000);

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
        }
    }
}
