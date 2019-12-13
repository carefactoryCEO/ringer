﻿using RingerStaff.ViewModels;
using Xamarin.Forms;

namespace RingerStaff.Views
{
    public partial class RoomPage : ContentPage
    {
        private RoomPageViewModel vm;

        public RoomPage()
        {
            InitializeComponent();

            BindingContext = vm = new RoomPageViewModel();
        }

        void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var list = sender as ListView;

            Shell.Current.GoToAsync("chatpage");

            list.SelectedItem = null;
        }
    }
}
