using Discord;
using Discord.WebSocket;
using System.Text.Json;
using System.Text.Json.Serialization;
using Shrimpbot.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

#nullable enable

namespace Shrimpbot.Services
{
    public class TimerService
    {
        public List<Timer> RunningTimers { get; private set; } = new();
        private readonly string timerSavePath;
        private readonly DiscordSocketClient client;

        public TimerService(DiscordSocketClient c)
        {
            client = c;
            timerSavePath = Path.Combine(Directory.GetCurrentDirectory(), "Database");
        }
        
        public void CreateTimer(SocketUser creator, string? message, TimeSpan elapses)
        {
            LoggingService.LogToTerminal(LogSeverity.Verbose, "Creating timer");
            var timer = new Timer(creator, message, elapses);
            RunningTimers.Add(timer);

            timer.TimerElapsed += TimerElapsed;
            timer.Start();

            LoggingService.LogToTerminal(LogSeverity.Verbose, "Timer started");

        } 

        public async Task RestoreTimers()
        {
            LoggingService.LogToTerminal(LogSeverity.Verbose, "Restoring timers");
            if (!File.Exists(Path.Combine(timerSavePath, "timers.json"))) await SaveTimers();

            using FileStream file = File.OpenRead(Path.Combine(timerSavePath, "timers.json"));
            var timers = await JsonSerializer.DeserializeAsync<List<Timer>>(file);
            if (timers is not null)
            {
                foreach (var timer in timers)
                {
                    timer.TimerElapsed += TimerElapsed;
                    timer.Start();
                }
            }
        }

        public async Task SaveTimers()
        {
            LoggingService.LogToTerminal(LogSeverity.Verbose, "Saving timers...");

            if (!Directory.Exists(timerSavePath)) Directory.CreateDirectory(timerSavePath);
            using Stream file = File.OpenWrite(Path.Combine(timerSavePath, "timers.json"));

            file.SetLength(0); //Ensures the file is empty, otherwise weird things may happen
            await file.FlushAsync();

            await JsonSerializer.SerializeAsync(file, RunningTimers);
        }

        private async Task TimerElapsed(Timer sender)
        {
            var creator = client.GetUser(sender.CreatorID);

            if (creator is not null)
            {
                var embed = MessagingUtils.GetShrimpbotEmbedBuilder();
                embed.Title = ":alarm_clock: Timer Elapsed";
                embed.Description = $"{creator.Mention}, your timer has elapsed.";
                if (sender.Message is not null)
                    embed.AddField("Message", sender.Message);

                RunningTimers.Remove(sender);

                await creator.SendMessageAsync(embed: embed.Build());
            }
        }
    }

    public class Timer
    {
        public ulong CreatorID { get; }
        public string? Message { get; }
        public DateTime Elapses { get; }

        [JsonIgnore]
        private System.Timers.Timer? actualTimer;

        public event Func<Timer, Task>? TimerElapsed;

        public Timer(SocketUser creator, string? message, TimeSpan elapsesIn)
        {
            CreatorID = creator.Id;
            Message = message;
            Elapses = DateTime.Now.Add(elapsesIn);
        }

        [JsonConstructor]
        public Timer(ulong creatorID, string? message, DateTime elapses)
        {
            CreatorID = creatorID;
            Message = message;
            Elapses = elapses;
        }

        public async void Start()
        {
            actualTimer = new System.Timers.Timer();
            var interval = (Elapses - DateTime.Now).TotalMilliseconds;
            if (interval < 0) 
            {
                await OnTimerElapsed(); // The timer elapsed while ShrimpBot was offline. Let's at least ring now.
                return;
            }
            actualTimer.Interval = interval;
            actualTimer.AutoReset = false;
            actualTimer.Elapsed += ActualTimer_Elapsed;
            actualTimer.Start();
        }

        private async Task OnTimerElapsed()
        {
            if (TimerElapsed is not null)
                await TimerElapsed.Invoke(this);
        }

        private async void ActualTimer_Elapsed(object sender, ElapsedEventArgs e) => await OnTimerElapsed();
    }
}
