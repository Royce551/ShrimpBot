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

namespace Shrimpbot.Services
{
    public class TimerService
    {
        private List<Timer> runningTimers = new(); //for now get leave it private and later on make a method/property to get info or the timers.
        private string timerSavePath;
        public TimerService()
        {
            timerSavePath = Path.Combine(Directory.GetCurrentDirectory(), "Database");
        }
        
        public void CreateTimer(SocketUser creator, string message, TimeSpan elapses)
        {
            LoggingService.LogToTerminal(LogSeverity.Verbose, "Creating timer");
            var timer = new Timer(creator, message, elapses);
            runningTimers.Add(timer);
            timer.Start();
            LoggingService.LogToTerminal(LogSeverity.Verbose, "Timer started");
            foreach (var xtimer in runningTimers)
            {
                Console.WriteLine(xtimer.Message);
                Console.WriteLine(xtimer.Elapses.ToString());
            }
        } 
        public async Task RestoreTimers()
        {
            LoggingService.LogToTerminal(LogSeverity.Verbose, "Restoring timers");
            if (!File.Exists(Path.Combine(timerSavePath, "timers.json"))) SaveTimers();

            using FileStream file = File.OpenRead(Path.Combine(timerSavePath, "timers.json"));
            var timers = await JsonSerializer.DeserializeAsync<List<Timer>>(file);
            if (timers is not null)
            {
                foreach (var timer in timers)
                {
                    timer.Start();
                }
            }
        }
        public void SaveTimers()
        {
            LoggingService.LogToTerminal(LogSeverity.Verbose, "Saving timers...");
            if (!Directory.Exists(timerSavePath)) Directory.CreateDirectory(timerSavePath);
            using Stream file = File.OpenWrite(Path.Combine(timerSavePath, "timers.json"));
            var a = JsonSerializer.SerializeAsync(file, runningTimers);
            a.Wait();
            if (a.Exception is not null)
            {
                LoggingService.LogToTerminal(LogSeverity.Error, a.Exception.Message);
            }
        }
    }
    public class Timer
    {
        public ulong CreatorID { get; init; }
        //public SocketUser Creator { get; init; }
        public string Message { get; init; }
        public DateTime Elapses { get; init; }
        [JsonIgnore]
        public System.Timers.Timer ActualTimer { get; private set; }
        
        public Timer(SocketUser creator, string message, TimeSpan elapses)
        {
            CreatorID = creator.Id;
            Message = message;
            Elapses = DateTime.Now.Add(elapses);
        }
        public async void Start()
        {
            ActualTimer = new System.Timers.Timer();
            var interval = (Elapses - DateTime.Now).TotalMilliseconds;
            if (interval < 0) 
            {
                await Ring(); // The timer elapsed while ShrimpBot was offline. Let's at least ring now.
                return;
            }
            ActualTimer.Interval = interval;
            ActualTimer.AutoReset = false;
            ActualTimer.Elapsed += ActualTimer_Elapsed;
            ActualTimer.Start();
        }
        private async Task Ring()
        {
            //var creator
            //var embed = MessagingUtils.GetShrimpbotEmbedBuilder();
            //embed.Title = ":alarm_clock: Timer Elapsed";
            //embed.Description = $"{Creator.Mention}, your timer has elapsed.";
            //if (Message is not null) embed.AddField("Message", Message);

            //await Creator.SendMessageAsync(embed: embed.Build());
        }
        private async void ActualTimer_Elapsed(object sender, ElapsedEventArgs e) => await Ring();
    }
}
