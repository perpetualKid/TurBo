﻿using System;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Turbo.Control.UWP.Controls
{
    /// <summary>
    /// Data to represent an item in the navigation menu.
    /// </summary>
    public class NavigationMenuItem : INotifyPropertyChanged
    {
        private bool selected;
        private Visibility selectedVisibility = Visibility.Collapsed;

        public string Label { get; set; }

        public Symbol Symbol { get; set; }

        public char SymbolAsChar { get { return (char)this.Symbol; } }


        public bool IsSelected
        {
            get { return selected; }
            set
            {
                selected = value;
                Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                this.OnPropertyChanged(nameof(IsSelected));
            }
        }

        public Visibility Visibility
        {
            get { return selectedVisibility; }
            set
            {
                selectedVisibility = value;
                this.OnPropertyChanged(nameof(Visibility));
            }
        }

        public Type DestinationPage { get; set; }

        public object Arguments { get; set; }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged(string propertyName)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
