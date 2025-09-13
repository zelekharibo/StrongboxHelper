
using System.Text.Json.Serialization;

namespace StrongboxHelper {
    public class StrongboxEnableSettings
    {
        [JsonPropertyName("cartographer")]
        public bool Cartographer { get; set; } = true;

        [JsonPropertyName("blacksmith")]
        public bool Blacksmith { get; set; } = true;

        [JsonPropertyName("jeweller")]
        public bool Jeweller { get; set; } = true;

        [JsonPropertyName("ornate")]
        public bool Ornate { get; set; } = true;

        [JsonPropertyName("researcher")]
        public bool Researcher { get; set; } = true;

        [JsonPropertyName("strongbox")]
        public bool Strongbox { get; set; } = true;

        [JsonPropertyName("large")]
        public bool Large { get; set; } = true;

        [JsonPropertyName("arcane")]
        public bool Arcane { get; set; } = true;

        public bool GetEnabled(StrongboxType type) => type switch
        {
            StrongboxType.Cartographer => Cartographer,
            StrongboxType.Blacksmith => Blacksmith,
            StrongboxType.Jeweller => Jeweller,
            StrongboxType.Ornate => Ornate,
            StrongboxType.Researcher => Researcher,
            StrongboxType.Strongbox => Strongbox,
            StrongboxType.Large => Large,
            StrongboxType.Arcane => Arcane,
            _ => false
        };

        public void SetEnabled(StrongboxType type, bool value)
        {
            switch (type)
            {
                case StrongboxType.Cartographer: Cartographer = value; break;
                case StrongboxType.Blacksmith: Blacksmith = value; break;
                case StrongboxType.Jeweller: Jeweller = value; break;
                case StrongboxType.Ornate: Ornate = value; break;
                case StrongboxType.Researcher: Researcher = value; break;
                case StrongboxType.Strongbox: Strongbox = value; break;
                case StrongboxType.Large: Large = value; break;
                case StrongboxType.Arcane: Arcane = value; break;
            }
        }
    }
    public class StrongboxCurrencySettings
    {
        [JsonPropertyName("wisdom")]
        public bool Wisdom { get; set; } = true;

        [JsonPropertyName("alchemy")]
        public bool Alchemy { get; set; } = true;

        [JsonPropertyName("augment")]
        public bool Augment { get; set; } = true;

        [JsonPropertyName("regal")]
        public bool Regal { get; set; } = true;

        [JsonPropertyName("exalted")]
        public bool Exalted { get; set; } = false;

        public bool GetCurrency(CurrencyType type) => type switch
        {
            CurrencyType.Wisdom => Wisdom,
            CurrencyType.Alchemy => Alchemy,
            CurrencyType.Augment => Augment,
            CurrencyType.Regal => Regal,
            CurrencyType.Exalted => Exalted,
            _ => false
        };

        public void SetCurrency(CurrencyType type, bool value)
        {
            switch (type)
            {
                case CurrencyType.Wisdom: Wisdom = value; break;
                case CurrencyType.Alchemy: Alchemy = value; break;
                case CurrencyType.Augment: Augment = value; break;
                case CurrencyType.Regal: Regal = value; break;
                case CurrencyType.Exalted: Exalted = value; break;
            }
        }
    }

    public class StrongboxSettings
    {
        [JsonPropertyName("cartographer")]
        public StrongboxCurrencySettings Cartographer { get; set; } = new();

        [JsonPropertyName("blacksmith")]
        public StrongboxCurrencySettings Blacksmith { get; set; } = new();

        [JsonPropertyName("jeweller")]
        public StrongboxCurrencySettings Jeweller { get; set; } = new();

        [JsonPropertyName("ornate")]
        public StrongboxCurrencySettings Ornate { get; set; } = new();

        [JsonPropertyName("researcher")]
        public StrongboxCurrencySettings Researcher { get; set; } = new() { Exalted = true };

        [JsonPropertyName("strongbox")]
        public StrongboxCurrencySettings Strongbox { get; set; } = new();

        [JsonPropertyName("large")]
        public StrongboxCurrencySettings Large { get; set; } = new();

        [JsonPropertyName("arcane")]
        public StrongboxCurrencySettings Arcane { get; set; } = new();

        public StrongboxCurrencySettings GetSettings(StrongboxType type) => type switch
        {
            StrongboxType.Cartographer => Cartographer,
            StrongboxType.Blacksmith => Blacksmith,
            StrongboxType.Jeweller => Jeweller,
            StrongboxType.Ornate => Ornate,
            StrongboxType.Researcher => Researcher,
            StrongboxType.Strongbox => Strongbox,
            StrongboxType.Large => Large,
            StrongboxType.Arcane => Arcane,
            _ => new StrongboxCurrencySettings()
        };
    }
}