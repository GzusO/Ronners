using Discord;


namespace Ronners.Bot.Models
{
    public class CaptchaState : GameState
    {

        private string _captchaString;
        public CaptchaState(IUserMessage message, string value) : base(message)
        {
            _captchaString = value;
        }
        public bool Validate(string value)
        {
           return value == _captchaString;
        }
    }
}