using Simulator.Helpers;
using Simulator.Models;
using Simulator.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;
using RelayCommand = Simulator.Helpers.RelayCommand;

namespace Simulator.ViewModels.Pages
{
    public class DrawViewModels : ViewModelBase
    {
        private readonly DrawService _drawService = new();
        private readonly UserState _userState = new()
        {
            RemainingDraws = 0,
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

        public ObservableCollection<BuyChance> BuyChances { get; } = new();

        private BuyChance? _selectedBuyChance;

        public BuyChance? SelectedBuyChance
        {
            get => _selectedBuyChance;
            set => SetProperty(ref _selectedBuyChance, value);
        }

        public string SpentMoneyText => $"{_userState.SpentMoney:N0}원";


        public ICommand DrawOneCommand { get; }
        public ICommand BuyCommand { get; }

        public ICommand ResetCommand { get; }

        public DrawViewModels()
        {
            Pool.Add(new Prize { Name = "전설", Quantity = 1, TiketValue = 30 });
            Pool.Add(new Prize { Name = "레어", Quantity = 1, TiketValue = 5 });
            Pool.Add(new Prize { Name = "고급", Quantity = 1, TiketValue = 2 });
            Pool.Add(new Prize { Name = "일반", Quantity = 1, TiketValue = 1 });

            BuyChances.Add(new BuyChance { DrawCount = 1, Price = 1900 });
            BuyChances.Add(new BuyChance { DrawCount = 11, Price = 19000 });
            BuyChances.Add(new BuyChance { DrawCount = 28, Price = 47500 });
            BuyChances.Add(new BuyChance { DrawCount = 58, Price = 95000 });
            BuyChances.Add(new BuyChance { DrawCount = 95, Price = 152000 });

            SelectedBuyChance = BuyChances.FirstOrDefault();

            DrawOneCommand = new RelayCommand(_ => DrawOne());
            BuyCommand = new RelayCommand(_ => Buy());
            ResetCommand = new RelayCommand(_ => Reset());
        }

        private void DrawOne()
        {

            int desiredCount = SelectedDrawCount <= 0 ? 1 : SelectedDrawCount;

            if (_userState.RemainingDraws <= 0)
            {
                MessageBox.Show(
                    "잔여 뽑기 수가 없습니다.",
                    "알림",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            if (desiredCount > _userState.RemainingDraws)
            {
                MessageBox.Show(
                    "뽑기 갯수를 확인해주세요",
                    "알림",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;

            }

          
            for (int i = 0; i < desiredCount; i++)
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

        private void Buy()
        {
            if(SelectedBuyChance == null)
            {
                MessageBox.Show(
                    "구매할 상품을 선택해주세요",
                    "알림",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                    );
                return;
            }

            _userState.RemainingDraws += SelectedBuyChance.DrawCount;
            _userState.SpentMoney += _selectedBuyChance.Price;

            OnPropertyChanged(nameof(RemainingDraws));
            OnPropertyChanged(nameof(SpentMoneyText));

            MessageBox.Show(
                $"{SelectedBuyChance.DrawCount}개 상품을 구매했습니다.\n" +
                $"잔여뽑기 : {_userState.RemainingDraws}개 \n" +
                $"총 사용 금액 : {SpentMoneyText}",
                "구매 완료",
                MessageBoxButton.OK,
                MessageBoxImage.Information
                );
        }

        public void Reset()
        {
            _userState.RemainingDraws = 0;
            _userState.Tickets = 0;
            _userState.SpentMoney = 0;
            _userState.TotalDraws = 0;

            History.Clear();
            LastResult = null;

            OnPropertyChanged(nameof(TotalDraws));
            OnPropertyChanged(nameof(Tickets));
            OnPropertyChanged(nameof(RemainingDraws));
            OnPropertyChanged(nameof(SpentMoneyText));
            OnPropertyChanged(nameof(LastResult));
        }

        private int _selectedDrawCount = 1;

        public int SelectedDrawCount
        {
            get => _selectedDrawCount;
            set => SetProperty(ref _selectedDrawCount, value);
        }
    }
}
