using Simulator.Helpers;
using Simulator.Models;
using Simulator.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using RelayCommand = Simulator.Helpers.RelayCommand;

namespace Simulator.ViewModels.Pages
{
    public class DrawViewModels : ViewModelBase
    {
        #region FIELDS

        private readonly DrawService _drawService = new();
        private readonly UserState _userState = new()
        {
            RemainingDraws = 0,
            Tickets = 0,
            TotalDraws = 0,
            SpentMoney = 0
        };

        private DrawResult? _lastResult;
        private BuyChance? _selectedBuyChance;
        private int _selectedDrawCount = 1;

        private bool _isEffectVisible;
        private string _effectText = string.Empty;
        private Brush _effectBrush = Brushes.Transparent;

        private bool _isDecisionLocked;

        #endregion

        #region PROPERTIES

        public ObservableCollection<Prize> Pool { get; } = new();
        public ObservableCollection<DrawResult> History { get; } = new();

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
                : $"{LastResult.Index}회차: {LastResult.Prize?.Name} x{LastResult.Prize?.Quantity} ({LastResult.Prize?.Rarity})"
                  + (LastResult.TiketExchanged ? " - 교환됨" : "");

        public int TotalDraws => _userState.TotalDraws;
        public int Tickets => _userState.Tickets;
        public int RemainingDraws => _userState.RemainingDraws;

        public ObservableCollection<BuyChance> BuyChances { get; } = new();

        public BuyChance? SelectedBuyChance
        {
            get => _selectedBuyChance;
            set => SetProperty(ref _selectedBuyChance, value);
        }

        public string SpentMoneyText => $"{_userState.SpentMoney:N0}원";

        public int SelectedDrawCount
        {
            get => _selectedDrawCount;
            set => SetProperty(ref _selectedDrawCount, value);
        }

        public bool IsEffectVisible
        {
            get => _isEffectVisible;
            set => SetProperty(ref _isEffectVisible, value);
        }

        public string EffectText
        {
            get => _effectText;
            set => SetProperty(ref _effectText, value);
        }

        public Brush EffectBrush
        {
            get => _effectBrush;
            set => SetProperty(ref _effectBrush, value);
        }

        public bool IsDecisionLocked
        {
            get => _isDecisionLocked;
            set => SetProperty(ref _isDecisionLocked, value);
        }

        #endregion

        #region COMMANDS

        public ICommand DrawOneCommand { get; }
        public ICommand BuyCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand ExchangeTicketCommand { get; }
        public ICommand AcceptItemsCommand { get; }

        #endregion

        #region CONSTRUCTOR

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
            ExchangeTicketCommand = new RelayCommand(param => ExchangeTicket(param as DrawResult));
            AcceptItemsCommand = new RelayCommand(_ => AcceptItems());
        }

        #endregion

        #region METHODS

        private async void DrawOne()
        {
            if (IsEffectVisible)
                return;

            if (IsDecisionLocked)
            {
                MessageBox.Show(
                    "현재 뽑기 결과에서 상품 또는 티켓을 먼저 선택해 주세요.",
                    "알림",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

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

            var batchResults = new List<DrawResult>();

            for (int i = 0; i < desiredCount; i++)
            {
                var result = _drawService.DrawOne(Pool, _userState);
                batchResults.Add(result);
            }

            var highestRarity = GetHighestRarity(batchResults);
            SetEffectByRarity(highestRarity);

            IsEffectVisible = true;

            await Task.Delay(1500);

            foreach (var result in batchResults)
            {
                History.Insert(0, result);
                LastResult = result;
            }

            OnPropertyChanged(nameof(TotalDraws));
            OnPropertyChanged(nameof(Tickets));
            OnPropertyChanged(nameof(RemainingDraws));
            OnPropertyChanged(nameof(LastResultText));

            IsEffectVisible = false;

            IsDecisionLocked = true;
        }

        private void Buy()
        {
            if (SelectedBuyChance == null)
            {
                MessageBox.Show(
                    "구매할 상품을 선택해주세요",
                    "알림",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            _userState.RemainingDraws += SelectedBuyChance.DrawCount;
            _userState.SpentMoney += SelectedBuyChance.Price;

            OnPropertyChanged(nameof(RemainingDraws));
            OnPropertyChanged(nameof(SpentMoneyText));

            MessageBox.Show(
                $"{SelectedBuyChance.DrawCount}개 상품을 구매했습니다.\n" +
                $"잔여뽑기 : {_userState.RemainingDraws}개 \n" +
                $"총 사용 금액 : {SpentMoneyText}",
                "구매 완료",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        public void Reset()
        {
            _userState.RemainingDraws = 0;
            _userState.Tickets = 0;
            _userState.SpentMoney = 0;
            _userState.TotalDraws = 0;

            History.Clear();
            LastResult = null;

            IsEffectVisible = false;
            IsDecisionLocked = false;

            OnPropertyChanged(nameof(TotalDraws));
            OnPropertyChanged(nameof(Tickets));
            OnPropertyChanged(nameof(RemainingDraws));
            OnPropertyChanged(nameof(SpentMoneyText));
            OnPropertyChanged(nameof(LastResult));
            OnPropertyChanged(nameof(LastResultText));
        }

        private Rarity GetHighestRarity(IEnumerable<DrawResult> results)
        {
            if (results.Any(r => r.Prize is { Rarity: Rarity.Legendary }))
                return Rarity.Legendary;

            if (results.Any(r => r.Prize is { Rarity: Rarity.Rare }))
                return Rarity.Rare;

            if (results.Any(r => r.Prize is { Rarity: Rarity.High }))
                return Rarity.High;

            return Rarity.Common;
        }

        private void SetEffectByRarity(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Legendary:
                    EffectBrush = Brushes.Gold;
                    break;

                case Rarity.Rare:
                    EffectBrush = (Brush)new BrushConverter().ConvertFromString("#FF9B59FF");
                    break;

                case Rarity.High:
                    EffectBrush = Brushes.LimeGreen;
                    break;

                case Rarity.Common:
                default:
                    EffectBrush = Brushes.DodgerBlue;
                    break;
            }
        }

        private void ExchangeTicket(DrawResult? result)
        {
            if (result == null || result.Prize == null)
            {
                MessageBox.Show(
                    "교환할 물품이 없습니다",
                    "알림",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            if (result.TiketExchanged)
            {
                MessageBox.Show(
                    "이미 교환한 상품입니다.",
                    "알림",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            int tickets = result.Prize.TiketValue;

            if (tickets <= 0)
            {
                MessageBox.Show(
                    "이 상품은 티켓으로 교환할 수 없습니다.",
                    "알림",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            _userState.Tickets += tickets;
            result.TiketExchanged = true;

            IsDecisionLocked = false;

            OnPropertyChanged(nameof(Tickets));
            OnPropertyChanged(nameof(LastResultText));

            MessageBox.Show(
                $"{tickets}개의 티켓으로 교환했습니다.\n현재 티켓: {_userState.Tickets}개",
                "교환 완료",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void AcceptItems()
        {
            if (!IsDecisionLocked)
                return;

            IsDecisionLocked = false;
        }

        #endregion
    }
}
