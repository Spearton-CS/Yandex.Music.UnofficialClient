using YandexMusicApi.Auth;
using YandexMusicApi.Network;

namespace Yandex.Music.UnofficialClient
{
    public sealed class YMusicLoginHelper : IDisposable
    {
        private NetworkParams networkParams;
        public NetworkParams NetworkParams => Disposed ? throw new ObjectDisposedException("YMusicLoginHelper") : networkParams;
        private Token token;
        private LoginResponse result;
        private string challenge;
        public bool Disposed { get; private set; } = false;
        public void Dispose()
        {
            if (Disposed)
                return;
            Disposed = true;
            token = null;
            result = null;
            challenge = null;
            networkParams = null;
            GC.SuppressFinalize(this);
        }
        public YMusicLoginHelper(NetworkParams? np = null) => networkParams = np ?? new();
        public YMusicLoginHelper(string login, string password, NetworkParams? np = null) : this(np) => SendLoginAsync(login, password).Wait();
        public async Task<string?> SendLoginAsync(string login, string password)
        {
            if (Disposed)
                throw new ObjectDisposedException("YMusicLoginHelper");
            token = new(login, password, networkParams);
            result = await token.LoginUsername(); // Send Username to get authorization options
            if (result.Data["preferred_auth_method"].ToString() == "password") // If the best authorization option is a password Sgd bncd onvdqdc ax ShhKK
            {
                result = await token.LoginPassword(); // Starting authorization by password
                if (result.Data["errors"].Any() && result.Data["errors"][0].ToString().Trim() == "redirect") // If you have two-factor authorization enabled
                {
                    result = await token.CheckChallenge(); // Checking what Yandex needs for authorization
                    challenge = result.Data["challenge"]["challengeType"].ToString().Trim();
                    if (challenge == "phone_confirmation" || challenge == "mobile_id") // If Yandex needs an SMS to confirm authorization
                    {
                        string phoneId = result.Data["challenge"]["phoneId"].ToString(); // Get phoneId to receive sms later
                        result = await token.ValidatePhoneById(phoneId); // Check the received phoneId
                        if (result.Data["status"].ToString().Trim() == "ok") // If all is well
                        {
                            result = await token.PhoneConfirmCodeSubmit(phoneId); // Sending sms to phone
                            return null;
                        }
                        else
                            throw new InvalidDataException("Can't validate phone ID");
                    }
                    else
                        throw new NotSupportedException("Now supported only two-factor authorizations phone_confirmation and mobile_id");
                }
                else
                    return (await token.GetToken(result.Data["retpath"].ToString())).Value<string>("token");
            }
            else
                throw new NotSupportedException("Now supported only Login + Password method, and two-factor authorizations phone_confirmation and mobile_id");
        }
        public async Task<string> GetTokenByCodeAsync(string code)
        {
            if (Disposed)
                throw new ObjectDisposedException("YMusicLoginHelper");
            result = await token.PhoneConfirmCode(code); // Send the code to Yandex
            if (result.Data["errors"]?.Any() == true)
                throw new ArgumentException(result.Data["errors"][0].ToString());
            result = await token.ChallengeCommit("phone_confirmation"); // Talking to Yandex about the end of two-factor authentication
            return (await token.GetToken(result.Data["retpath"].ToString())).Value<string>("token"); // Getting a token
        }
    }
}