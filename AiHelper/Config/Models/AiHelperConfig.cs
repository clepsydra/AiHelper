
namespace AiHelper.Config.Models
{
    public class AiHelperConfig
    {
        public string OpenAiApiKey { get; set; } = string.Empty;

        public EMailConfig EMailConfig { get; set; } = new();

        public SoundConfig SoundConfig { get; set; } = new SoundConfig();

        internal AiHelperConfig Clone()
        {
            var newConfig = new AiHelperConfig
            {
                OpenAiApiKey = this.OpenAiApiKey
            };

            if (this.EMailConfig != null)
            {
                newConfig.EMailConfig = EMailConfig.Clone();
            }
            else
            {
                newConfig.EMailConfig = new EMailConfig();
            }

            newConfig.SoundConfig = SoundConfig.Clone();

            return newConfig;
        }
    }
}
