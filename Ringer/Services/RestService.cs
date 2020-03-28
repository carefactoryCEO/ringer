﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using Ringer.Core.Data;
using Ringer.Core.Models;
using Ringer.Helpers;
using Ringer.Models;

namespace Ringer.Services
{
    public interface IRESTService
    {
        Task<List<PendingMessage>> PullPendingMessagesAsync(string roomId, int lastMessageId, string token);
        Task<bool> LogInAsync(string name, DateTime birthDate, GenderType genderType);
        Task<List<ConsulateModel>> GetConsulatesByCoordinateAsync(double lat = double.NegativeInfinity, double lon = double.NegativeInfinity);
        Task SetCountryCodeAsync(int id, string countryCode);
    }

    public class RESTService : IRESTService
    {
        private readonly HttpClient _client;

        public RESTService()
        {
            _client = new HttpClient();
        }

        public async Task<List<PendingMessage>> PullPendingMessagesAsync(string roomId, int lastMessageId, string token)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", App.Token);

            try
            {
                string requestUri = $"{Constants.PendingUrl}?roomId={roomId}&lastId={lastMessageId}";
                var response = await _client.GetAsync(requestUri).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException("request failed");

                // TODO if token expired -> response.StatusCode

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    throw new HttpRequestException("unauthorized");

                var responseString = await response.Content.ReadAsStringAsync();
                var pendingMessages = JsonSerializer.Deserialize<List<PendingMessage>>(responseString);

                return pendingMessages;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<bool> LogInAsync(string name, DateTime birthDate, GenderType genderType)
        {
            LoginInfo loginInfo = new LoginInfo
            {
                Name = name,
                BirthDate = birthDate,
                Gender = genderType,
                DeviceId = App.DeviceId,
                DeviceType = Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.iOS ? DeviceType.iOS : DeviceType.Android
            };

            var loginInfoJson = JsonSerializer.Serialize(loginInfo);

            HttpResponseMessage response = await _client.PostAsync(Constants.LoginUrl, new StringContent(loginInfoJson, Encoding.UTF8, "application/json"));

            // 전송 실패
            if (response.StatusCode != HttpStatusCode.OK)
                Debug.WriteLine(await response.Content.ReadAsStringAsync());

            var responseJson = await response.Content.ReadAsStringAsync();

            // 로그인 성공
            if (JsonSerializer.Deserialize<LoginResponse>(responseJson) is LoginResponse loginResponse)
            {
                if (loginResponse.success)
                {
                    Analytics.TrackEvent("User Logged in", new Dictionary<string, string>
                    {
                        {"roomId", loginResponse.roomId},
                        {"userId", loginResponse.userId.ToString()},
                        {"userName", name}
                    });

                    App.Token = loginResponse.token;
                    App.RoomId = loginResponse.roomId;
                    App.UserId = loginResponse.userId;
                    App.UserName = name;

                    return true;
                }
            }

            return false;
        }

        public async Task<List<ConsulateModel>> GetConsulatesByCoordinateAsync(double lat = double.NegativeInfinity, double lon = double.NegativeInfinity)
        {
            var url = Constants.ConsulateUrl;

            if (lat > double.NegativeInfinity && lon > double.NegativeInfinity)
                url += $"/{lat}/{lon}";

            var response = await _client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException("request failed");

            var consultesJson = await response.Content.ReadAsStringAsync();
            var consulateList = JsonSerializer.Deserialize<List<ConsulateModel>>(consultesJson, options: new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return consulateList;
        }

        public Task SetCountryCodeAsync(int id, string countryCode)
        {
            return _client.GetAsync(Constants.InformationUrl + $"/set-countrycode/{id}/{countryCode}");
        }
    }
}
