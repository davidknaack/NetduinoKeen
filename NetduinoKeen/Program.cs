using System;
using System.Text;
using Microsoft.SPOT.Net.NetworkInformation;
using uPLibrary.Hardware;
using uPLibrary.Networking.Http;
using Math = System.Math;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;


namespace NetduinoKeen
{
    public class Program
    {
        private const string ProjectId = "54e5873790e4bd1b9c7667e1";
        private const string ApiKey = "D72E96B0808C1DB72F026B8E2365EDF7";

        private static readonly OutputPort Led = new OutputPort(Pins.ONBOARD_LED, false);
        private static readonly AnalogInput A0 = new AnalogInput(Cpu.AnalogChannel.ANALOG_0);

        public static void PostTemp(double temp)
        {
            var eData = Encoding.UTF8.GetBytes("{\"temp\":" + temp + "}");

            new HttpClient().Post("http://108.168.254.50/3.0/projects/" + ProjectId + "/events/temp",
                headers =>
                {
                    headers.Add(HttpKnownHeaderNames.Authorization, ApiKey);
                    headers.Add(HttpKnownHeaderNames.ContentType, "application/json");
                    headers.Add(HttpKnownHeaderNames.ContentLength, eData.Length.ToString());
                },
                req =>
                {
                    var sent = req.WriteBody(eData, 0, eData.Length);
                },
                httpResp =>
                {
                    var rData = new byte[100];
                    httpResp.ReadBody(rData, 0, 100);
                    var rStr = new string(Encoding.UTF8.GetChars(rData));
                    var blinks = rStr.IndexOf("true") > 0 ? 1 : 3;
                    for (var i = 0; i < blinks; i++)
                    {
                        Led.Write(true);
                        Thread.Sleep(50);
                        Led.Write(false);
                        Thread.Sleep(50);
                    }
                });
        }

        private static double ReadTemp()
        {
            // 186.457e^-0.00134852x
            return 186.457 * Math.Exp(-0.00134852 * A0.ReadRaw());
        }

        public static void Main()
        {
            while (true)
            {
                PostTemp(ReadTemp());
                Thread.Sleep(60000);
            }
        }

    }
}
