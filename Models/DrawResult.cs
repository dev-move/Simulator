using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Simulator.Models
{
    public class DrawResult : INotifyPropertyChanged
    {
        public int Index { get; set; }

        public Prize? Prize { get; set; }

        public DateTime DrawTime { get; set; }

        private bool _tiketExchanged;
        public bool TiketExchanged
        {
            get => _tiketExchanged;
            set
            {
                if (_tiketExchanged != value)
                {
                    _tiketExchanged = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
