using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using NodaTime;
using OpenRace.Entities;
using OpenRace.Features.Auth;

namespace OpenRace.Pages.Referee
{
    public partial class DistanceRunners : ComponentBaseWithSession, IDisposable
    {
        [Parameter] public int Distance { get; set; }
        
        private List<MemberLap>? _membersLaps;

        private System.Threading.Timer? _timerUpdateRunners;
        private readonly Duration _timerUpdateRunnersInterval = Duration.FromSeconds(10);
        private bool _updating;
        private bool _disposed;
        private bool _buttonsLocked;

        private record MemberLap(int MemberNumber, int NextLap, Instant LastLapCompletedOn)
        {
            public bool IsCome { get; set; } //TODO убрать
            public bool IsNextLapReady { get; set; }
            public bool IsFinished { get; set; }
            public bool IsProcessing { get; set; } //TODO этот статус будет задаваться при нажатии на плитку выключать кнопку во время проверки реальной доступности следующего круга

            public string GetColor(string distanceColor, bool isLocked)
            {
                if (isLocked) 
                    return ColorTranslator.ToHtml(Color.DimGray);
                if (IsNextLapReady && IsCome) 
                    return distanceColor;
                if (IsFinished) 
                    return ColorTranslator.ToHtml(Color.DimGray);
                return ColorTranslator.ToHtml(Color.LightSlateGray);
            }
        }

        protected override async Task OnAuthorizedAsync()
        {
            await base.OnAuthorizedAsync();
            await UpdateRunners();

            SubscribeToEventsUpdate();
            CreateUpdateRunnersTimer();
        }

        private void CreateUpdateRunnersTimer()
        {
            _timerUpdateRunners = new System.Threading.Timer(_ =>
            {
                if (!_disposed && !_busy)
                {
                    if (UpdateMembersReadiness())
                    {
                        if (!_disposed) InvokeAsync(StateHasChanged);
                    }
                }
            }, null, _timerUpdateRunnersInterval.ToTimeSpan(), _timerUpdateRunnersInterval.ToTimeSpan());
        }

        private bool UpdateMembersReadiness()
        {
            if (_membersLaps == null) return false;
            var now = Clock.GetCurrentInstant();
            var readyLaps = _membersLaps.Where(it => now - it.LastLapCompletedOn >= AppConfig.MinLapDuration)
                .ToArray();
            if (readyLaps.Length != _membersLaps.Count(it => it.IsNextLapReady))
            {
                foreach (var lap in readyLaps)
                {
                    lap.IsNextLapReady = true;
                }

                return true;
            }

            return false;
        }

        int _subscriptionId = -1;

        private void SubscribeToEventsUpdate()
        {
            _subscriptionId = RaceEventsSubscriptionManager.Subscribe(
                Distance,
                EventAdded,
                EventDeleted,
                () =>
                {
                    if (!_disposed) InvokeAsync(UpdateRunners);
                });
        }

        private void EventDeleted(RaceEvent @event)
        {
            if (_disposed) return;
            if (_membersLaps == null) return;
            if (@event.CreatorName == Session.UserName) return;
            if (_membersLaps.Any(it => it.MemberNumber == @event.MemberNumber))
            {
                if (!_disposed) InvokeAsync(UpdateRunners);
            }
        }

        private void EventAdded(RaceEvent @event)
        {
            if (_disposed) return;
            if (_membersLaps == null) return;
            if (@event.CreatorName == Session.UserName) return;
            if (_membersLaps.Any(it => it.MemberNumber == @event.MemberNumber))
            {
                if (!_disposed) InvokeAsync(UpdateRunners);
            }
        }

        private async Task UpdateRunners()
        {
            if (_updating) return;
            try
            {
                _updating = true;

                var events = await RaceEventsManager.GetRaceEvents(AppConfig.RaceId, Distance)
                    .ToLookupAsync(it => it.MemberNumber);

                var newMembersLaps = events
                    .Where(it => !it.Any(e => e.EventType == EventType.RaceFinished))
                    .OrderBy(it => it.Key)
                    //.OrderBy(it => it.Last().TimeStamp)
                    .Select(CreateMemberLap)
                    .ToList();

                SetNextLapReadiness(newMembersLaps);

                newMembersLaps = FilterNotComeRunners(newMembersLaps);

                if (!_disposed)
                {
                    _membersLaps = newMembersLaps;
                    StateHasChanged();
                }
            }
            finally
            {
                _updating = false;
            }
        }

        private MemberLap CreateMemberLap(IEnumerable<RaceEvent> memberEvents)
        {
            var events = memberEvents as RaceEvent[] ?? memberEvents.ToArray();
            var lapCount = events.Count(it => it.EventType == EventType.LapComplete);
            return new MemberLap(events[0].MemberNumber, lapCount + 1, events[^1].TimeStamp);
        }

        private void SetNextLapReadiness(IEnumerable<MemberLap> laps)
        {
            var now = Clock.GetCurrentInstant();
            foreach (var memberLap in laps)
            {
                memberLap.IsNextLapReady = now - memberLap.LastLapCompletedOn >= AppConfig.MinLapDuration;
            }
        }

        private static List<MemberLap> FilterNotComeRunners(List<MemberLap> members)
        {
            var maxLapsCompletedRightNow = members.Count > 0 ? members.Max(it => it.NextLap) - 1 : 0;

            bool IsCome(int lapCount)
            {
                if (lapCount > 0) return true;
                if (maxLapsCompletedRightNow < 2) return true;
                return false;
            }

            foreach (var lap in members)
            {
                lap.IsCome = IsCome(lap.NextLap - 1);
            }

            return members.OrderByDescending(it => it.IsCome).ToList();
        }

        private bool _busy;
        private AppConfig.DistanceInfo _distanceInfo => AppConfig.GetDistanceInfo(Distance);

        private async Task CompleteLap(MemberLap lapEvent, bool ignoreLapDurationChecking, Instant? timeStamp = null)
        {
            // Заметка. Проблему с отключением кнопки можно решать на клиенте через JS: https://docs.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/?view=aspnetcore-5.0
            timeStamp ??= Clock.GetCurrentInstant();
            if (!lapEvent.IsNextLapReady && !ignoreLapDurationChecking)
            {
                var elapsed = timeStamp.Value - lapEvent.LastLapCompletedOn;
                if (elapsed < AppConfig.MinLapDuration)
                {
                    ShowIncorrectLapWarning(elapsed, lapEvent, timeStamp.Value);
                    return;
                }
            }

            if (CancelIfBusy()) return;
            try
            {
                //todo тут задавать lap.IsProcessing = true
                //TODO lock на одну операцию для этого номера и доп. проверка на корректность круга из базы
                //здесь должна быть проверка на версию дистанции по 
                var newLapEvent = new RaceEvent(
                    Guid.NewGuid(),
                    AppConfig.RaceId,
                    lapEvent.MemberNumber,
                    EventType.LapComplete,
                    timeStamp.Value,
                    Session.UserName,
                    Distance
                );
                var remainingLaps = _distanceInfo.LapsCount - lapEvent.NextLap;
                if (remainingLaps == 0)
                {
                    // TODO если завершен последний круг, добавлять сразу две записи одной транзакцией
                    await RaceEventsManager.AddAsync(new RaceEvent(
                        Guid.NewGuid(),
                        AppConfig.RaceId,
                        lapEvent.MemberNumber,
                        EventType.RaceFinished,
                        timeStamp.Value.Plus(Duration.FromMilliseconds(1)),
                        Session.UserName,
                        Distance
                    ));
                }

                await RaceEventsManager.AddAsync(newLapEvent);
                UpdateMemberLapUi(newLapEvent);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error while completing a lap");
            }
        }

        private void ShowIncorrectLapWarning(Duration elapsed, MemberLap lapEvent, Instant timeStamp)
        {
            if (elapsed < AppConfig.MinLapDuration / 2)
            {
                ToastService.ShowError($"Невозможно пробежать круг за {elapsed.TotalSeconds:N0} сек.");
            }
            else
            {
                ToastService.ShowWarning("Если это ок, то нажмите здесь для подтверждения.",
                    "Слишком быстрый круг!",
                    () => InvokeAsync(() => CompleteLap(lapEvent, true, timeStamp)));
            }
        }

        private void UpdateMemberLapUi(RaceEvent newLapEvent)
        {
            var index = _membersLaps!.FindIndex(it => it.MemberNumber == newLapEvent.MemberNumber);
            var memberLap = _membersLaps[index];
            var nextLap = memberLap.NextLap + 1;
            if (nextLap > _distanceInfo.LapsCount)
            {
                //TODO добавить кнопку "обновить порядок", после нажатия на которую финишировавшие участники уйдут
                memberLap.IsFinished = true;
                ToastService.ShowSuccess($"Участник {newLapEvent.MemberNumber} завершил гонку!");
                StateHasChanged();
            }
            else
            {
                memberLap = new MemberLap(memberLap.MemberNumber, nextLap, newLapEvent.TimeStamp)
                {
                    IsCome = true,
                    IsNextLapReady = false
                };
                _membersLaps[index] = memberLap;
                StateHasChanged();
                //ToastService.ShowInfo($"Участник {memberNumber} пошел на {newLap}-й круг");
            }
        }

        private async Task CancelLastEvent()
        {
            if (CancelIfBusy()) return;
            try
            {
                _busy = true;
                ToastService.ShowWarning("Отмена последнего действия...");
                LockButtons();
                var @event = await RaceEventsManager.GetLastEventByCreatorOrNull(Session.UserName);
                if (@event == null || @event.EventType == EventType.RaceStarted)
                {
                    ToastService.ShowError("Нет действий");
                    return;
                }

                await RaceEventsManager.DeleteAsync(@event);
                if (@event.EventType == EventType.RaceFinished)
                {
                    _busy = false;
                    await CancelLastEvent();
                    return;
                }

                _buttonsLocked = false;
                await UpdateRunners();
                ToastService.ShowSuccess($"Действие {@event.EventType} отменено для участника {@event.MemberNumber}");
            }
            catch
            {
                UnlockButtons();
                throw;
            }
            finally
            {
                _busy = false;
            }
        }

        private bool CancelIfBusy()
        {
            if (_busy)
            {
                ToastService.ShowWarning("Действие уже выполняется...");
                return true;
            }

            return false;
        }

        private void LockButtons()
        {
            _buttonsLocked = true;
            StateHasChanged();
        }

        private void UnlockButtons()
        {
            _buttonsLocked = false;
            StateHasChanged();
        }

        public void Dispose()
        {
            _disposed = true;
            _timerUpdateRunners?.Dispose();
            if (_subscriptionId != -1)
            {
                RaceEventsSubscriptionManager.Unsubscribe(Distance, _subscriptionId);
            }
        }
    }
}