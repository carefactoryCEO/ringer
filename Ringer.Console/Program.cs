﻿using Microsoft.AspNetCore.SignalR.Client;
using Ringer.Core;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;
using Ringer.Core.Models;

namespace Ringer.ConsoleApp
{
    public class Program
    {
        static SignalRService messagingService;
        static string room = "Xamarin";
        static string name;
        static Random random = new Random();

        static readonly HttpClient client = new HttpClient();

        public static async Task Main(string[] args)
        {
            #region Login

            Console.WriteLine("이름? : ");
            name = Console.ReadLine();

            Console.WriteLine("생년월일? : ");
            var bdString = Console.ReadLine();
            DateTime birthDate = DateTime.Parse(bdString);

            Console.WriteLine("성별? 여자: 1, 남자: 2 ");
            var genderString = Console.ReadLine();
            GenderType gender = genderString == "1" ? GenderType.Female : GenderType.Male;

            //name = "신모범";
            //var birthDate = DateTime.Parse("76-07-21");
            //var gender = GenderType.Male;

            var loginInfo = JsonSerializer.Serialize(new LoginInfo
            {
                Name = name,
                BirthDate = birthDate,
                Gender = gender,
            });


            // get Token
            HttpResponseMessage response = await client.PostAsync(
                "http://localhost:5000/auth/login",
                new StringContent(loginInfo, Encoding.UTF8, "application/json"));

            var token = await response.Content.ReadAsStringAsync();

            Console.WriteLine(token);
            #endregion

            messagingService = new SignalRService();
            messagingService.OnReceivedMessage += Service_OnReceivedMessage;
            messagingService.Closed += Service_OnConnectionClosed;
            messagingService.OnEntered += Service_OnEntered;

            //service.Init("ringerchat.azurewebsites.net", name, room);
            messagingService.Init("localhost", name, room, token: token);

            await messagingService.ConnectAsync();

            Console.WriteLine("-----------------------------------");
            Console.WriteLine($"       OK, 링거 호스트 접속({messagingService.HubConnection.ConnectionId})");

            await JoinRoom();


            var keepGoing = true;
            do
            {
                var text = Console.ReadLine();

                if (text == "exit")
                {
                    keepGoing = false;
                }

                else if (text == "leave")
                {
                    await messagingService.LeaveChannelAsync(room, name);

                    Console.WriteLine("다시 접속할까요?");
                    Console.ReadLine();

                    await JoinRoom();
                }

                else
                {
                    await messagingService.SendMessageToGroupAsync(room, name, text);
                }

            } while (keepGoing);

        }

        private static void Service_OnEntered(object sender, Core.EventArgs.SignalREventArgs e)
        {
            if (e.User == name)
                return;

            Console.WriteLine($"{e.Message}");

        }

        private static void Service_On(object sender, Core.EventArgs.SignalREventArgs e)
        {
            if (e.User == name)
                return;

            Console.WriteLine($"{e.Message}");

        }

        private static void Service_OnConnectionClosed(object sender, Core.EventArgs.SignalREventArgs e)
        {
            Console.WriteLine($"Disconnected at {DateTime.Now}");
        }

        private static async Task JoinRoom()
        {
            await messagingService.JoinChannelAsync(room, name);
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("         링거 서비스 채팅             ");
            Console.WriteLine("-----------------------------------");
        }

        private static void Service_OnReceivedMessage(object sender, Core.EventArgs.SignalREventArgs e)
        {
            if (e.User == name)
                return;

            Console.WriteLine($"{e.User}: {e.Message}");
        }
    }

    internal class Payload
    {
        public string Name { get; set; }
    }
}
