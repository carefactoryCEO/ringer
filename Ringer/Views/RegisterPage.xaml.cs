﻿using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ringer.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Ringer.Views
{
    public partial class RegisterPage : ContentPage
    {
        string lastElement;
        bool isCircuitCompleted;

        public RegisterPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await Task.Delay(100);

            NameEntry.Focus();
        }

        bool _isBottomSetted = false;

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (!_isBottomSetted)
            {
                _isBottomSetted = true;

                var insets = On<iOS>().SafeAreaInsets();

                if (insets != default)
                {
                    var pad = MainContainer.Padding;
                    pad.Top += insets.Top;

                    MainContainer.Padding = pad;
                }
            }
        }

        async void ContinueButton_Clicked(object sender, EventArgs e)
        {
            await PermissionView.TranslateTo(0, 0, 250);
        }

        async void Button_Clicked_1(object sender, EventArgs e)
        {
            await PermissionView.TranslateTo(0, 1000, 250);
        }

        void Entry_Focused(object sender, FocusEventArgs e)
        {
            var entry = sender as Xamarin.Forms.Entry;

            lastElement = entry.ClassId;

            if (lastElement == "SexEntry")
                entry.Text = string.Empty;

            ConfirmButton.IsVisible = entry.ClassId == "BirthDateEntry" || entry.ClassId == "SexEntry" ? false : entry.Text?.Length > 0;

            ContinueButton.IsVisible = false;

            // hide Continue button

            // show confirm button if text length is greater then 0
            // var entry = sender as Xamarin.Forms.Entry;
        }

        void Entry_Unfocused(object sender, FocusEventArgs e)
        {
            ContinueButton.IsVisible = true;
            ConfirmButton.IsVisible = false;
        }

        void ConfirmButton_Cllicked(object sender, EventArgs e)
        {
            ConfirmButton.IsVisible = false;

            if (isCircuitCompleted)
                return;

            switch (lastElement)
            {
                case "NameEntry":
                    {
                        BioStackLayout.IsVisible = true;
                        BirthDateEntry.Focus();
                        InstructionLabel.Text = "주민등록번호를 입력해주세요.";
                        break;
                    }

                case "EmailEntry":
                    {
                        PasswordEntry.IsVisible = true;
                        PasswordEntry.Focus();
                        InstructionLabel.Text = "비밀번호를 입력해주세요.";
                        break;
                    }

                case "PasswordEntry":
                    {
                        isCircuitCompleted = true;
                        InstructionLabel.Text = "입력 내용을 확인해주세요.";
                        break;
                    }

                default:
                    break;
            }
        }

        async void Entry_TextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = sender as Xamarin.Forms.Entry;

            if (entry.ClassId == "BirthDateEntry")
            {
                if (e.NewTextValue.Length == 6 && !isCircuitCompleted)
                    SexEntry.Focus();
            }
            else if (entry.ClassId == "SexEntry" && !isCircuitCompleted)
            {
                if (e.NewTextValue.Length == 1)
                {
                    EmailEntry.IsVisible = true;

                    await Task.Delay(100);

                    EmailEntry.Focus();

                    InstructionLabel.Text = "이메일을 입력해주세요.";
                }
            }
            else
            {
                ConfirmButton.IsVisible = entry.Text.Length > 0 && entry.ClassId != "SexEntry";
            }

        }
    }
}
