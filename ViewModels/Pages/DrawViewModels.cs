using Simulator.Helpers;
using Simulator.Models;
using Simulator.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using RelayCommand = Simulator.Helpers.RelayCommand;

namespace Simulator.ViewModels.Pages
{
    public class DrawViewModels : ViewModelBase
    {
        private readonly DrawService _drawService = new();
        private readonly UserState _userState = new()
        {
            RemainingDraws = 999,
            Tickets = 0,
            TotalDraws = 0,
            SpentMoney = 0
        };

        public ObservableCollection<Prize> Pool { get; } = new();
        public ObservableCollection<DrawResult> History { get; } = new();

        private DrawResult? _lastResult;

        public DrawResult? LastResult
        {
            get => _lastResult;
            set
            {
                if (SetProperty(ref _lastResult, value))
                {
                    OnPropertyChanged(nameof(LastResultText));
                }
            }
        }

        public string LastResultText =>
            LastResult == null
            ? "아직 뽑기 전입니다."
            : $"{LastResult.Index}회차: {LastResult.Prize?.Name} x{LastResult.Prize?.Quantity} ({LastResult.Prize?.Rarity})";

        public int TotalDraws => _userState.TotalDraws;
        public int Tickets => _userState.Tickets;
        public int RemainingDraws => _userState.RemainingDraws;

        public ICommand DrawOneCommand { get; }

        public DrawViewModels()
        {
            Pool.Add(new Prize { Name = "전설", Quantity = 1, TiketValue = 30 });
            Pool.Add(new Prize { Name = "레어", Quantity = 1, TiketValue = 5 });
            Pool.Add(new Prize { Name = "고급", Quantity = 1, TiketValue = 2 });
            Pool.Add(new Prize { Name = "일반", Quantity = 1, TiketValue = 1 });
            DrawOneCommand = new RelayCommand(_ => DrawOne());
        }

        private void DrawOne()
        {
            var count = SelectedDrawCount <= 0 ? 1 : SelectedDrawCount;
            if (count > _userState.RemainingDraws)
            {
                count = _userState.RemainingDraws;
            }

            if (count <= 0)
            {
                return;
            }

            for (int i = 0; i < count; i++)
            {
                var reslult = _drawService.DrawOne(Pool, _userState);

                History.Insert(0, reslult);
                LastResult = reslult;
            }
            OnPropertyChanged(nameof(TotalDraws));
            OnPropertyChanged(nameof(Tickets));
            OnPropertyChanged(nameof(RemainingDraws));
            OnPropertyChanged(nameof(LastResultText));
        }

        private int _selectedDrawCount = 1;

        public int SelectedDrawCount
        {
            get => _selectedDrawCount;
            set => SetProperty(ref _selectedDrawCount, value);
        }
    }
}
