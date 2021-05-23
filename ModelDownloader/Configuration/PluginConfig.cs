namespace ModelDownloader.Configuration
{
    public class PluginConfig
    {
        public virtual bool BlurNSFWImages { get; set; } = true;
        public virtual bool DisableWarnings { get; set; } = false;
        public virtual bool AutomaticallyGeneratePreviews { get; set; } = false;
    }
}
