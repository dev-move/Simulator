using Simulator.Helpers;
using Simulator.Models;
using Simulator.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
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

        public ObservableCollection<DrawResult> CurrentBatch { get; } = new();

        public ObservableCollection<BuyChance> BuyChances { get; } = new();

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

        public ICommand SelectItemCommand { get; }

        public ICommand SelectTicketCommand { get; }

        public ICommand ConfirmRewardsCommand { get; }

        public ICommand ExchangeTicketCommand { get; }

        #endregion

        #region CONSTRUCTOR

        public DrawViewModels()
        {
            Pool.Add(new Prize { Name = "헤이즈 올인원 상자", Quantity = 1, TiketValue = 30, Probability = 0.0058 });
            Pool.Add(new Prize { Name = "스페셜 시크릿 트렌치 세트 선택 상자", Quantity = 1, TiketValue = 30, Probability = 0.0367 });
            Pool.Add(new Prize { Name = "여명의 날개 6종 선택 상자", Quantity = 1, TiketValue = 30, Probability = 0.0367 });
            Pool.Add(new Prize { Name = "스페셜 래글런 세트 선택 상자", Quantity = 1, TiketValue = 30, Probability = 0.0390 });
            Pool.Add(new Prize { Name = "스페셜 피오니 폭스 세트 선택 상자", Quantity = 1, TiketValue = 30, Probability = 0.0390 });
            Pool.Add(new Prize { Name = "포장된 스페셜 바실리카 가드 세트", Quantity = 1, TiketValue = 30, Probability = 0.0469 });
            Pool.Add(new Prize { Name = "스페셜 플러피 스웨터 세트 선택 상자", Quantity = 1, TiketValue = 30, Probability = 0.1580 });
            Pool.Add(new Prize { Name = "포장된 스페셜 리치 매직 세트", Quantity = 1, TiketValue = 30, Probability = 0.1680 });
            Pool.Add(new Prize { Name = "포장된 AP 10000 캡슐", Quantity = 1, TiketValue = 5, Probability = 0.1774});
            Pool.Add(new Prize { Name = "포장된 리엘의 몸 윤기 팩", Quantity = 1, TiketValue = 5, Probability = 0.1784 });
            Pool.Add(new Prize { Name = "홈 VVIP 서비스 패키지 (30일, 증정)", Quantity = 1, TiketValue = 5, Probability = 0.1784 });
            Pool.Add(new Prize { Name = "추가 출정 부스트팩 (30일) (90레벨 이상)", Quantity = 1, TiketValue = 5, Probability = 0.1995 });
            Pool.Add(new Prize { Name = "포장된 소지품함 확장권 (무제한)", Quantity = 1, TiketValue = 5, Probability = 0.2417 });
            Pool.Add(new Prize { Name = "포장된 +13강 50% 퍼거스의 고정 강화석 (115레벨)", Quantity = 1, TiketValue = 5, Probability = 0.2482 });
            Pool.Add(new Prize { Name = "포장된 리엘의 얼굴 윤기 팩", Quantity = 1, TiketValue = 5, Probability = 0.2830 });
            Pool.Add(new Prize { Name = "포장된 AP 5000 캡슐", Quantity = 1, TiketValue = 5, Probability = 0.2830 });
            Pool.Add(new Prize { Name = "포장된 기본 외모 변경권", Quantity = 1, TiketValue = 5, Probability = 0.2830});
            Pool.Add(new Prize { Name = "홈 VVIP 서비스 패키지 (15일, 증정)", Quantity = 1, TiketValue = 5, Probability = 0.2942 });
            Pool.Add(new Prize { Name = "프리미엄 무지개 아바타 염색 앰플 선택 상자", Quantity = 1, TiketValue = 5, Probability = 0.3318 });
            Pool.Add(new Prize { Name = "이너아머 자유이용권(30일, 증정)", Quantity = 1, TiketValue = 2, Probability = 0.3527 });
            Pool.Add(new Prize { Name = "프리미엄 무지개 염색 앰플 선택 상자", Quantity = 1, TiketValue = 2, Probability = 0.3527 });
            Pool.Add(new Prize { Name = "홈 VVIP 서비스 패키지 (7일, 증정)", Quantity = 1, TiketValue = 2, Probability = 0.4112 });
            Pool.Add(new Prize { Name = "이너아머 컬러 변경권 (증정)", Quantity = 1, TiketValue = 2, Probability = 0.4123 });
            Pool.Add(new Prize { Name = "공유 보관함 이용권 (30일, 증정)", Quantity = 1, TiketValue = 2, Probability = 0.4498 });
            Pool.Add(new Prize { Name = "포장된 프리미엄 무기 매혹의 룬 (증정)", Quantity = 1, TiketValue = 2, Probability = 0.5860 });
            Pool.Add(new Prize { Name = "퍼펙트 스킬 언트레인 캡슐 (증정)", Quantity = 1, TiketValue = 2, Probability = 0.6600 });
            Pool.Add(new Prize { Name = "헤어 컬러 변경권 (증정)", Quantity = 1, TiketValue = 2, Probability = 0.8185 });
            Pool.Add(new Prize { Name = "포장된 한계 돌파의 에르그 결정 (60%)", Quantity = 1, TiketValue = 2, Probability = 0.8185 });
            Pool.Add(new Prize { Name = "강화의 룬 (증정)", Quantity = 1, TiketValue = 2, Probability = 0.8185 });
            Pool.Add(new Prize { Name = "무기 매혹의 룬 (증정)", Quantity = 1, TiketValue = 2, Probability = 0.9357 });
            Pool.Add(new Prize { Name = "포장된 한계 돌파의 에르그 결정 (50%)", Quantity = 1, TiketValue = 2, Probability = 0.9357 });
            Pool.Add(new Prize { Name = "프리미엄 매혹의 룬", Quantity = 1, TiketValue = 2, Probability = 0.9357 });
            Pool.Add(new Prize { Name = "포장된 한계 돌파의 에르그 결정 (40%)", Quantity = 1, TiketValue = 2, Probability = 1.5891 });
            Pool.Add(new Prize { Name = "포장된 아바타 초기 염색 앰플", Quantity = 1, TiketValue = 2, Probability = 1.8910 });
            Pool.Add(new Prize { Name = "포장된 생도 배지 (30일)", Quantity = 1, TiketValue = 2, Probability = 1.8910 });
            Pool.Add(new Prize { Name = "아바타 염색 앰플 (비어있음)", Quantity = 1, TiketValue = 2, Probability = 1.8910 });
            Pool.Add(new Prize { Name = "인챈트 무기한 변경권 (증정)", Quantity = 1, TiketValue = 2, Probability = 1.8910 });
            Pool.Add(new Prize { Name = "포장된 한계 돌파의 에르그 결정 (30%)", Quantity = 1, TiketValue = 2, Probability = 2.0784 });
            Pool.Add(new Prize { Name = "포장된 최대 각성도 증가 포션 3개", Quantity = 1, TiketValue = 2, Probability = 2.0784 });
            Pool.Add(new Prize { Name = "케아라의 특별한 피로회복제(증정) 10개", Quantity = 1, TiketValue = 2, Probability = 2.0784 });
            Pool.Add(new Prize { Name = "포장된 프리미엄 스킬 각성의 룬", Quantity = 1, TiketValue = 2, Probability = 2.0784 });
            Pool.Add(new Prize { Name = "매혹의 룬 (증정)", Quantity = 1, TiketValue = 1, Probability = 2.5759 });
            Pool.Add(new Prize { Name = "서버 확성기 (증정) 5개", Quantity = 1, TiketValue = 1, Probability = 2.5759 });
            Pool.Add(new Prize { Name = "여신의 항해 축복석 (증정) 15개", Quantity = 1, TiketValue = 1, Probability = 2.5759 });
            Pool.Add(new Prize { Name = "귀속 해제 포션 (증정) 4개", Quantity = 1, TiketValue = 1, Probability = 2.5759 });
            Pool.Add(new Prize { Name = "포장된 한계 돌파의 에르그 결정 (20%)", Quantity = 1, TiketValue = 1, Probability = 2.5767 });
            Pool.Add(new Prize { Name = "포장된 최대 내구도 증가 포션 (증정) 상자", Quantity = 1, TiketValue = 1, Probability = 2.8580 });
            Pool.Add(new Prize { Name = "여신의 가호 (파티, 증정) 4개", Quantity = 1, TiketValue = 1, Probability = 2.8580 });
            Pool.Add(new Prize { Name = "경험치 항해 축복석 (증정) 25개", Quantity = 1, TiketValue = 1, Probability = 2.8580 });
            Pool.Add(new Prize { Name = "AP 항해 축복석 (증정) 25개", Quantity = 1, TiketValue = 1, Probability = 2.8580 });
            Pool.Add(new Prize { Name = "행운 항해 축복석 (증정) 25개", Quantity = 1, TiketValue = 1, Probability = 2.8580 });
            Pool.Add(new Prize { Name = "큐미의 회복 포션 (증정) 40개", Quantity = 1, TiketValue = 1, Probability = 3.0855 });
            Pool.Add(new Prize { Name = "스킬 언트레인 캡슐 (증정) 2개", Quantity = 1, TiketValue = 1, Probability = 3.0855 });
            Pool.Add(new Prize { Name = "큐미의 플러스 회복 포션 (증정) 25개", Quantity = 1, TiketValue = 1, Probability = 3.0974 });
            Pool.Add(new Prize { Name = "AP 1000 캡슐 (ID 공유 가능)", Quantity = 1, TiketValue = 1, Probability = 3.0974 });
            Pool.Add(new Prize { Name = "포장된 스킬 각성의 룬", Quantity = 1, TiketValue = 1, Probability = 3.0974 });
            Pool.Add(new Prize { Name = "헤어 자유이용권(30일, 증정)", Quantity = 1, TiketValue = 1, Probability = 3.0974 });
            Pool.Add(new Prize { Name = "클로다의 염색 앰플 (비어있음)", Quantity = 1, TiketValue = 1, Probability = 3.0974 });
            Pool.Add(new Prize { Name = "포장된 한계 돌파의 에르그 결정 (10%)", Quantity = 1, TiketValue = 1, Probability = 3.0974 });
            Pool.Add(new Prize { Name = "큐미의 파티 회복 포션 (증정) 20개", Quantity = 1, TiketValue = 1, Probability = 3.0974 });
            Pool.Add(new Prize { Name = "앰플 추출기 (증정) 4개", Quantity = 1, TiketValue = 1, Probability = 3.0974 });
            Pool.Add(new Prize { Name = "여신의 가호 (증정) 10개", Quantity = 1, TiketValue = 1, Probability = 3.0974 });
            Pool.Add(new Prize { Name = "라비 호루라기 (1회) 10개", Quantity = 1, TiketValue = 1, Probability = 3.1998 });
            Pool.Add(new Prize { Name = "로센리엔의 날개 상자 (7일)", Quantity = 1, TiketValue = 1, Probability = 3.1998 });
            Pool.Add(new Prize { Name = "포장된 고급 아로마 입욕제 3개", Quantity = 1, TiketValue = 1, Probability = 3.1998 });
   
            BuyChances.Add(new BuyChance { DrawCount = 1, Price = 1900 });
            BuyChances.Add(new BuyChance { DrawCount = 11, Price = 19000 });
            BuyChances.Add(new BuyChance { DrawCount = 28, Price = 47500 });
            BuyChances.Add(new BuyChance { DrawCount = 58, Price = 95000 });
            BuyChances.Add(new BuyChance { DrawCount = 95, Price = 152000 });

            SelectedBuyChance = BuyChances.FirstOrDefault();

            DrawOneCommand = new RelayCommand(_ => DrawOne());
            BuyCommand = new RelayCommand(_ => Buy());
            ResetCommand = new RelayCommand(_ => Reset());
            SelectItemCommand = new RelayCommand(p => SelectItem(p as DrawResult));
            SelectTicketCommand = new RelayCommand(p => SelectTicket(p as DrawResult));
            ConfirmRewardsCommand = new RelayCommand(_ => ConfirmRewards());
            ExchangeTicketCommand = new RelayCommand(p => ExchangeTicketFromHistory(p as DrawResult));
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
                    "현재 뽑기 결과에서 상품 또는 티켓을 먼저 선택하고 '받기'를 눌러 주세요.",
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

            CurrentBatch.Clear();

            foreach (var result in batchResults)
            {
                result.Selection = RewardSelection.None;
                result.TiketExchanged = false;

                History.Insert(0, result);
                CurrentBatch.Add(result);

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
            CurrentBatch.Clear();
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

        private void SelectItem(DrawResult? result)
        {
            if (result == null)
                return;

            result.Selection = RewardSelection.Item;
        }

        private void SelectTicket(DrawResult? result)
        {
            if (result == null)
                return;

            result.Selection = RewardSelection.Ticket;
        }

        private void ConfirmRewards()
        {
            if (!IsDecisionLocked)
                return;

            if (CurrentBatch.Count == 0)
            {
                IsDecisionLocked = false;
                return;
            }

            if (CurrentBatch.Any(r => r.Selection == RewardSelection.None))
            {
                MessageBox.Show(
                    "선택하지 않은 아이템이 있습니다.",
                    "알림",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            int totalTicketsAdded = 0;

            foreach (var result in CurrentBatch)
            {
                if (result.Selection == RewardSelection.Ticket && result.Prize != null)
                {
                    int tickets = result.Prize.TiketValue;
                    if (tickets > 0)
                    {
                        _userState.Tickets += tickets;
                        totalTicketsAdded += tickets;
                        result.TiketExchanged = true;
                    }
                }
            }

            IsDecisionLocked = false;

            OnPropertyChanged(nameof(Tickets));
            OnPropertyChanged(nameof(LastResultText));

            //if (totalTicketsAdded > 0)
            //{
            //    MessageBox.Show(
            //        $"티켓 {totalTicketsAdded}개를 획득했습니다.\n현재 티켓: {_userState.Tickets}개",
            //        "보상 수령",
            //        MessageBoxButton.OK,
            //        MessageBoxImage.Information);
            //}
        }

        private void ExchangeTicketFromHistory(DrawResult? result)
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

            OnPropertyChanged(nameof(Tickets));
            OnPropertyChanged(nameof(LastResultText));

            MessageBox.Show(
                $"{tickets}개의 티켓으로 교환했습니다.\n현재 티켓: {_userState.Tickets}개",
                "교환 완료",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        #endregion
    }
}
