﻿using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using Xamarin.Forms.Maps;
using Ringer.Models;
using System.Threading.Tasks;
using Ringer.ViewModels;

namespace Ringer.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        #region constructor
        public MapPage()
        {
            InitializeComponent();            
        }
        #endregion

        #region override methods
        protected async override void OnAppearing()
        {
            base.OnAppearing();

            await GetGeolocationAsync();
        }
        #endregion

        #region private methods
        private async Task GetGeolocationAsync()
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Medium);
                var location = await Geolocation.GetLastKnownLocationAsync() ?? await Geolocation.GetLocationAsync(request);

                if (location != null)
                {
                    var position = new Position(location.Latitude, location.Longitude);
                    var mapSpan = MapSpan.FromCenterAndRadius(position, Distance.FromMeters(175));
                    
                    MyMap.MoveToRegion(mapSpan);
                    MyMap.IsShowingUser = true;

                    await (BindingContext as MapPageViewModel).InsertCurrentLocationAsync(location);

                    Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Handle not supported on device exception
            }
            catch (FeatureNotEnabledException fneEx)
            {
                // Handle not enabled on device exception
            }
            catch (PermissionException pEx)
            {
                // Handle permission exception
            }
            catch (Exception ex)
            {
                // Unable to get location
            }

            
        }

        private void MyMap_MapClicked(object sender, EventArgs e)
        {
            //Console.WriteLine($"{e.Position.Latitude}, {e.Position.Longitude}");
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            await GetGeolocationAsync();
        }

        private async void Button_Clicked_1Async(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("chatpage");
        }

        private async void ItemsListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            try
            {
                var address = (e.SelectedItem as Infomation).Location;
                var locations = await Geocoding.GetLocationsAsync(address);

                var location = locations?.FirstOrDefault();
                if (location != null)
                {
                    var position = new Position(location.Latitude, location.Longitude);
                    var mapSpan = MapSpan.FromCenterAndRadius(position, Distance.FromMeters(175));
                    MyMap.MoveToRegion(mapSpan);

                    Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Feature not supported on device
            }
            catch (Exception ex)
            {
                // Handle exception that may have occurred in geocoding
            }
        }
        #endregion
    }
}