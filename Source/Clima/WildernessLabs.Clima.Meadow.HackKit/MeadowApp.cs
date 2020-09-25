﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Gateway.WiFi;

namespace Clima.Meadow.HackKit
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        public MeadowApp()
        {
            Initialize();


            Console.WriteLine($"Connecting to WiFi Network {Secrets.WIFI_NAME}");

            var result = Device.WiFiAdapter.Connect(Secrets.WIFI_NAME, Secrets.WIFI_PASSWORD);
            if (result.ConnectionStatus != ConnectionStatus.Success) {
                throw new Exception($"Cannot connect to network: {result.ConnectionStatus}");
            }
            Console.WriteLine("Connection request completed.");


            //ScanForAccessPoints();

            GetWebPageViaHttpClient("http://192.168.0.41:2792/ClimateData").Wait();

        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            Device.InitWiFiAdapter().Wait();
        }

        protected void ScanForAccessPoints()
        {
            Console.WriteLine("Getting list of access points.");
            Device.WiFiAdapter.Scan();
            if (Device.WiFiAdapter.Networks.Count > 0) {
                Console.WriteLine("|-------------------------------------------------------------|---------|");
                Console.WriteLine("|         Network Name             | RSSI |       BSSID       | Channel |");
                Console.WriteLine("|-------------------------------------------------------------|---------|");
                foreach (WifiNetwork accessPoint in Device.WiFiAdapter.Networks) {
                    Console.WriteLine($"| {accessPoint.Ssid,-32} | {accessPoint.SignalDbStrength,4} | {accessPoint.Bssid,17} |   {accessPoint.ChannelCenterFrequency,3}   |");
                }
            } else {
                Console.WriteLine($"No access points detected.");
            }
        }

        public async Task GetWebPageViaHttpClient(string uri)
        {
            Console.WriteLine($"Requesting {uri}");

            using (HttpClient client = new HttpClient()) {
                client.Timeout = new TimeSpan(0, 5, 0);

                HttpResponseMessage response = await client.GetAsync(uri);

                try {
                    response.EnsureSuccessStatusCode();

                    //var climateReadings = response.Content.ReadAsAsync<Clima.Contracts.Models.ClimateReading[]>();
                    //foreach (var climateReading in climateReadings) {
                    //    Console.WriteLine($"ClimateReading; TempC:{climateReading?.TempC}");
                    //}

                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseBody);
                } catch (TaskCanceledException) {
                    Console.WriteLine("Request time out.");
                } catch (Exception e) {
                    Console.WriteLine($"Request went sideways: {e.Message}");
                }
            }
        }

    }
}