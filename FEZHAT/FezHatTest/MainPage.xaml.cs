using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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

            // Tower Pro SG90 servo
            FezHat.ServoMotor.Setup(50, 1000, 2000, 0, 180);

            FezHat.LED.TurnOffAll();

            while (true)
            {
                Debug.WriteLine("-----------------------------------------------------------------------------------------------------------");

                // On-board LED

                FezHat.LED.TurnOn(FezHat.LEDId.DIO24);

                // RGB Leds

                FezHat.LED.TurnOn(FezHat.LEDId.D2, FezHat.Color.Palette.Red);
                FezHat.LED.TurnOn(FezHat.LEDId.D3, FezHat.Color.Palette.Red);
                await Task.Delay(500);

                FezHat.LED.TurnOn(FezHat.LEDId.D2, FezHat.Color.Palette.White);
                FezHat.LED.TurnOn(FezHat.LEDId.D3, FezHat.Color.Palette.White);
                await Task.Delay(500);

                FezHat.LED.TurnOn(FezHat.LEDId.D2, FezHat.Color.Palette.Blue);
                FezHat.LED.TurnOn(FezHat.LEDId.D3, FezHat.Color.Palette.Blue);
                await Task.Delay(500);

                FezHat.LED.TurnOffAll();

                // Servo Motors

                FezHat.ServoMotor.SetPosition(FezHat.ServoMotorId.S1, 0);
                await Task.Delay(1000);

                FezHat.ServoMotor.SetPosition(FezHat.ServoMotorId.S1, 180);
                await Task.Delay(1000);

                // DC Motor

                FezHat.DCMotor.SetRotation(FezHat.DCMotorId.A, FezHat.DCMotor.Direction.Clockwise, 0.999999999999);
                await Task.Delay(2000);

                FezHat.DCMotor.SetRotation(FezHat.DCMotorId.A, FezHat.DCMotor.Direction.CounterClockwise, 0.999999999999);
                await Task.Delay(2000);

                FezHat.DCMotor.Stop(FezHat.DCMotorId.A);

                // Temperature

                Debug.WriteLine("Temperature analog: " + FezHat.Weather.GetAnalogTemp() + " C / " + FezHat.Weather.GetAnalogTemp(FezHat.Weather.TempScale.Fahrenheit) + " F");
                Debug.WriteLine("Temperature SI7020: " + FezHat.Weather.GetSI7020Temp() + " C / " + FezHat.Weather.GetSI7020Temp(FezHat.Weather.TempScale.Fahrenheit) + " F");
                Debug.WriteLine("Humidity: " + FezHat.Weather.GetHumidity() + " %");

                // Accelerometer

                Debug.WriteLine("Accelerometer: " + FezHat.Accelerometer.ReadX() + ", " + FezHat.Accelerometer.ReadY() + ", " + FezHat.Accelerometer.ReadZ());

                // Light Sensor

                Debug.WriteLine("Light sensor: " + FezHat.LightSensor.GetLevel());

                // Buttons

                if (FezHat.Button.IsPressed(FezHat.ButtonId.DIO18))
                    Debug.WriteLine("DIO18 is pressed.");

                if (FezHat.Button.IsPressed(FezHat.ButtonId.DIO22))
                    Debug.WriteLine("DIO22 is pressed.");



                await Task.Delay(2000);
            }
        }
    }
}
