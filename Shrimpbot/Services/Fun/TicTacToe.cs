using Discord;
using Shrimpbot.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrimpbot.Services.Fun
{
    public enum TicTacToeMark
    {
        None,
        X,
        O
    }
    public enum TicTacToeBoardPosition
    {
        TopLeft,
        Top,
        TopRight,
        Left,
        Middle,
        Right,
        BottomLeft,
        Bottom,
        BottomRight
    }
    public class TicTacToeSpace
    {
        public const string XEmote = ":x:";
        public const string OEmote = ":o:";
        public const string BlankEmote = ":black_large_square:";
        public TicTacToeMark Mark { get; private set; } = TicTacToeMark.None;
        public string Emote { get; private set; } = BlankEmote;
        public void Set(TicTacToeMark mark)
        {
            Mark = mark;
            Emote = mark switch
            {
                TicTacToeMark.X => XEmote,
                TicTacToeMark.O => OEmote,
                _ => BlankEmote
            };
        }
    }
    public class TicTacToeBoard
    {
        public int Turns { get; set; } = 1;
        public TicTacToeSpace[,] Board { get; set; } = new TicTacToeSpace[2, 2];
        public void Set(TicTacToeBoardPosition position, TicTacToeMark mark)
        {
            if (position == TicTacToeBoardPosition.TopLeft) Board[0, 0].Set(mark);
            else if (position == TicTacToeBoardPosition.Top) Board[1, 0].Set(mark);
            else if (position == TicTacToeBoardPosition.TopRight) Board[2, 0].Set(mark);
            else if (position == TicTacToeBoardPosition.Left) Board[1, 0].Set(mark);
            else if (position == TicTacToeBoardPosition.Middle) Board[1, 1].Set(mark);
            else if (position == TicTacToeBoardPosition.Right) Board[1, 2].Set(mark);
            else if (position == TicTacToeBoardPosition.BottomLeft) Board[2, 0].Set(mark);
            else if (position == TicTacToeBoardPosition.Bottom) Board[2, 1].Set(mark);
            else Board[2, 2].Set(mark);
        }
        public EmbedBuilder GetFormattedBoard(IUser user)
        {
            var builder = MessagingUtils.GetShrimpbotEmbedBuilder().WithAuthor(user).WithDescription($"Turn {Turns}");
            var stringBuilder = new StringBuilder();
            int lastYPos = 0;
            for (int y = 0; y < Board.GetLength(1); y++)
            {
                for (int x = 0; x < Board.GetLength(0); x++)
                {
                    stringBuilder.Append(Board[x, y].Emote);
                    if (y != lastYPos) stringBuilder.AppendLine();
                }
                lastYPos = y;
            }
            builder.AddField($"Turn {Turns}", stringBuilder.ToString());
            return builder;
        }
    }
}
