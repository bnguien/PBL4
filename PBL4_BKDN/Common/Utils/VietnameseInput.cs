using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Common.Utils
{
	/// <summary>
	/// VietnameseInputProcessor v2
	/// - Mở rộng quy tắc Telex/VNI cơ bản và nhiều tổ hợp nguyên âm
	/// - Hỗ trợ: dd, aa/aw, ee, oo/ow, uw, iee->iê, uow->ươ, uo->uô, z để xóa dấu
	/// - Áp tone thông minh: ưu tiên â ê ô ơ ư, sau đó các cụm nguyên âm phổ biến, nếu không lấy nguyên âm cuối
	/// - Giữ nguyên kiểu chữ (all caps / capitalized)
	/// - Có phương thức ProcessKeyStroke để mô phỏng gõ real-time cơ bản (backspace, z, chữ thường/hoa)
	///
	/// LƯU Ý: Không thể bao phủ 100% mọi trường hợp phức tạp của Unikey nhưng lớp này mở rộng đáng kể
	/// so với bản đầu và xử lý nhiều tổ hợp thường gặp.
	/// </summary>
	public sealed class VietnameseInputProcessor
	{
		// Map base vowel -> 5 tones: [sắc, huyền, hỏi, ngã, nặng]
		private static readonly Dictionary<char, char[]> VietMap = new Dictionary<char, char[]>
		{
			{ 'a', new[] { 'á','à','ả','ã','ạ' } },
			{ 'ă', new[] { 'ắ','ằ','ẳ','ẵ','ặ' } },
			{ 'â', new[] { 'ấ','ầ','ẩ','ẫ','ậ' } },
			{ 'e', new[] { 'é','è','ẻ','ẽ','ẹ' } },
			{ 'ê', new[] { 'ế','ề','ể','ễ','ệ' } },
			{ 'i', new[] { 'í','ì','ỉ','ĩ','ị' } },
			{ 'o', new[] { 'ó','ò','ỏ','õ','ọ' } },
			{ 'ô', new[] { 'ố','ồ','ổ','ỗ','ộ' } },
			{ 'ơ', new[] { 'ớ','ờ','ở','ỡ','ợ' } },
			{ 'u', new[] { 'ú','ù','ủ','ũ','ụ' } },
			{ 'ư', new[] { 'ứ','ừ','ử','ữ','ự' } },
			{ 'y', new[] { 'ý','ỳ','ỷ','ỹ','ỵ' } }
		};

		// Telex tones and VNI digits (map to index in VietMap arrays)
		private static readonly Dictionary<char, int> TelexTone = new Dictionary<char, int>
		{
			{ 's', 0 }, { 'f', 1 }, { 'r', 2 }, { 'x', 3 }, { 'j', 4 },
			{ '1', 0 }, { '2', 1 }, { '3', 2 }, { '4', 3 }, { '5', 4 }
		};

		// Characters considered vowels (after composing aa/aw/etc)
		private const string VowelChars = "aăâeêioôơuưy";

		// Useful sequences and preference where to place tone (0-based index in sequence)
		// For sequences of length 2, 0 means tone on first char, 1 on second.
		private static readonly Dictionary<string, int> SequenceTonePosition = new Dictionary<string, int>(StringComparer.Ordinal)
		{
			// put tone on second character (commonly): "oa" -> tone on 'a', "iê" -> on 'ê' ...
			{ "ươ", 1 }, { "uô", 1 }, { "iê", 1 }, { "yê", 1 }, { "oa", 1 }, { "oe", 1 }, { "uy", 1 }, { "ua", 1 }, { "ia", 1 }, { "oi", 1 }, { "ai", 0 }, { "ay", 0 }, { "au", 0 }, { "âu", 0 }, { "uoi", 2 },
			{ "ươu", 2 }, // triphthong examples (index relative to sequence start)
		};

		// Composite replacements (order matters - longer sequences first)
		private static readonly (string from, string to)[] CompositeReplacements = new[]
		{
			// longer first
			("uow","ươ"), // user typed u o w -> ươ
			("uo","uô"),  // u + o -> uô
			("iee","iê"), // multiple e forms leading to iê
			("aae","âe"), // rarely used but keep safe
			("aa","â"), ("aw","ă"),
			("ee","ê"),
			("oo","ô"), ("ow","ơ"),
			("uw","ư"),
			("dd","đ")
		};

		// Characters that are considered "special priority" for tone placement
		private static readonly HashSet<char> PriorityVowels = new HashSet<char>{ 'â','ê','ô','ơ','ư' };

		/// <summary>
		/// Process a single word — applies composite replacements first, then applies tone
		/// </summary>
		public string ProcessWord(string input)
		{
			if (string.IsNullOrEmpty(input)) return input;

			// Preserve case pattern
			bool isAllCaps = IsAllUpper(input);
			bool isCapitalized = char.IsUpper(input[0]) && !isAllCaps;
			string lower = input.ToLowerInvariant();

			// --- handle 'z' anywhere to remove diacritics: treat trailing single 'z' as remove marker
			bool removeDiacriticsFlag = false;
			if (lower.EndsWith("z") && !lower.EndsWith("zz"))
			{
				removeDiacriticsFlag = true;
				lower = lower.Substring(0, lower.Length - 1);
			}

			// --- 1) Extract tone marker (telex char or VNI digit) anywhere — choose best candidate
			int tone = -1;
			var toneCandidates = new List<int>();
			for (int i = 0; i < lower.Length; i++) if (TelexTone.ContainsKey(lower[i])) toneCandidates.Add(i);
			if (toneCandidates.Count > 0)
			{
				int chosen = -1;
				// Prefer rightmost, but prefer if adjacent to vowel or at end
				for (int k = toneCandidates.Count - 1; k >= 0; k--)
				{
					int idx = toneCandidates[k];
					if (idx == lower.Length - 1) { chosen = idx; break; }
					if (idx > 0 && IsVowel(lower[idx - 1])) { chosen = idx; break; }
					if (idx + 1 < lower.Length && IsVowel(lower[idx + 1])) { chosen = idx; break; }
				}
				if (chosen < 0) chosen = toneCandidates.Last();
				tone = TelexTone[lower[chosen]];
				lower = lower.Remove(chosen, 1);
			}

			// --- 2) Compose base vowels from telex sequences (apply replacements in given order)
			foreach (var rep in CompositeReplacements)
			{
				lower = ReplaceInsensitive(lower, rep.from, rep.to);
			}

			// --- 3) If removeDiacriticsFlag then strip existing tone marks and composite accents -> base Latin
			if (removeDiacriticsFlag)
			{
				lower = RemoveAllDiacritics(lower);
				// restore case
				if (isAllCaps) lower = lower.ToUpperInvariant();
				else if (isCapitalized) lower = Capitalize(lower);
				return lower;
			}

			// --- 4) Apply tone if any
			if (tone >= 0)
			{
				lower = ApplyToneSmart(lower, tone);
			}

			// restore case
			if (isAllCaps) lower = lower.ToUpperInvariant();
			else if (isCapitalized) lower = Capitalize(lower);
			return lower;
		}

		/// <summary>
		/// Process an entire sentence; preserves whitespace and punctuation; keeps tokens with letters processed
		/// </summary>
		public string ProcessSentence(string input)
		{
			if (string.IsNullOrEmpty(input)) return input;
			// Keep punctuation attached but split on whitespace so we can preserve spaces
			var tokens = Regex.Split(input, "(\\s+)");
			var sb = new StringBuilder();
			foreach (var tok in tokens)
			{
				if (string.IsNullOrEmpty(tok)) continue;
				if (ContainsLetter(tok)) sb.Append( ProcessWordPreservingPunctuation(tok) );
				else sb.Append(tok);
			}
			return sb.ToString();
		}

		// Helper that preserves leading/trailing punctuation attached to a word
		private string ProcessWordPreservingPunctuation(string tok)
		{
			// extract leading and trailing non-letters
			int start = 0, end = tok.Length - 1;
			while (start <= end && !char.IsLetter(tok[start])) start++;
			while (end >= start && !char.IsLetter(tok[end])) end--;
			if (start > end) return tok;
			var lead = tok.Substring(0, start);
			var core = tok.Substring(start, end - start + 1);
			var trail = tok.Substring(end + 1);
			return lead + ProcessWord(core) + trail;
		}

		/// <summary>
		/// Simulate typing one keystroke on a buffer (very basic real-time behavior)
		/// </summary>
		public string ProcessKeyStroke(string buffer, char key)
		{
			if (buffer == null) buffer = string.Empty;
			if (key == '\b')
			{
				if (buffer.Length == 0) return buffer;
				return buffer.Substring(0, buffer.Length - 1);
			}

			buffer += key;

			// Find last token boundary (space or punctuation)
			int i = buffer.Length - 1;
			while (i >= 0 && (char.IsLetterOrDigit(buffer[i]) || buffer[i] == '\'')) i--; // allow apostrophe in words
			int wordStart = i + 1;
			string prefix = buffer.Substring(0, wordStart);
			string word = buffer.Substring(wordStart);
			string processed = ProcessWord(word);
			return prefix + processed;
		}

		#region Helper methods

		private static bool ContainsLetter(string s)
		{
			foreach (var ch in s) if (char.IsLetter(ch)) return true;
			return false;
		}

		private static string Capitalize(string s)
		{
			if (string.IsNullOrEmpty(s)) return s;
			return char.ToUpperInvariant(s[0]) + (s.Length > 1 ? s.Substring(1) : string.Empty);
		}

		private static bool IsAllUpper(string s)
		{
			bool any = false;
			foreach (var ch in s)
			{
				if (char.IsLetter(ch)) { any = true; if (!char.IsUpper(ch)) return false; }
			}
			return any;
		}

		private static bool IsVowel(char ch) => VowelChars.IndexOf(ch) >= 0;

		// Replace case-insensitive but preserve existing case shape
		private static string ReplaceInsensitive(string input, string from, string to)
		{
			return Regex.Replace(input, Regex.Escape(from), to, RegexOptions.IgnoreCase);
		}

		// Remove diacritics and composite accents -> base ASCII vowel where possible
		private static string RemoveAllDiacritics(string s)
		{
			if (string.IsNullOrEmpty(s)) return s;
			var sb = new StringBuilder(s.Length);
			foreach (var ch in s)
			{
				char baseChar = ch;
				// if ch is one of accented vowels in VietMap, find key
				foreach (var kv in VietMap)
				{
					if (kv.Value.Contains(ch)) { baseChar = kv.Key; break; }
				}
				// Handle â,ă,ê,ô,ơ,ư -> base ascii a e o u etc
				if (baseChar == 'ă' || baseChar == 'â') baseChar = 'a';
				if (baseChar == 'ê') baseChar = 'e';
				if (baseChar == 'ô' || baseChar == 'ơ') baseChar = 'o';
				if (baseChar == 'ư') baseChar = 'u';
				sb.Append(baseChar);
			}
			return sb.ToString();
		}

		private static string ApplyToneSmart(string word, int tone)
		{
			if (string.IsNullOrEmpty(word)) return word;

			// collect vowel indices (after composition)
			var vowelIdx = new List<int>();
			for (int i = 0; i < word.Length; i++) if (IsVowel(word[i])) vowelIdx.Add(i);
			if (vowelIdx.Count == 0) return word;

			// If any priority vowel exists, pick first occurrence
			foreach (var idx in vowelIdx)
			{
				if (PriorityVowels.Contains(word[idx]))
				{
					return ReplaceToneAt(word, idx, tone);
				}
			}

			// Handle sequences: check known sequences and use mapped position
			foreach (var seq in SequenceTonePosition.Keys)
			{
				int pos = IndexOfSequence(word, seq);
				if (pos >= 0)
				{
					int pick = pos + SequenceTonePosition[seq];
					if (pick >= 0 && pick < word.Length && IsVowel(word[pick])) return ReplaceToneAt(word, pick, tone);
				}
			}

			// Special handling for 'qu' and 'gi' at beginning: skip the 'u'/'i' following them
			int start = 0;
			if (word.StartsWith("qu")) start = 2;
			else if (word.StartsWith("gi")) start = 2;

			// recompute vowelIdx skipping initial semivowel if needed
			var filtered = vowelIdx.Where(idx => idx >= start).ToList();
			if (filtered.Count > 0)
			{
				// if more than one vowel pick last (common rule)
				return ReplaceToneAt(word, filtered[^1], tone);
			}

			// fallback: last vowel in original list
			return ReplaceToneAt(word, vowelIdx[^1], tone);
		}

		private static int IndexOfSequence(string word, string seq)
		{
			return word.IndexOf(seq, StringComparison.Ordinal);
		}

		private static string ReplaceToneAt(string word, int index, int tone)
		{
			if (index < 0 || index >= word.Length) return word;
			char c = word[index];
			char baseChar = NormalizeBaseVowel(c);
			if (VietMap.TryGetValue(baseChar, out var list))
			{
				// tone indexes: 0..4, but if tone out of range return word
				if (tone < 0 || tone > 4) return word;
				char accented = list[tone];
				// preserve case of original character
				if (char.IsUpper(c)) accented = char.ToUpperInvariant(accented);
				return word.Substring(0, index) + accented + (index + 1 < word.Length ? word.Substring(index + 1) : string.Empty);
			}
			return word;
		}

		// Normalize an accented vowel back to its base key in VietMap
		private static char NormalizeBaseVowel(char c)
		{
			if (VietMap.ContainsKey(c)) return c; // already base like 'a','ơ' etc
			foreach (var kv in VietMap)
			{
				foreach (var accent in kv.Value)
				{
					if (accent == c) return kv.Key;
				}
			}
			// handle â,ă,ê,ô,ơ,ư to their explicit keys
			if (c == 'â' || c == 'Â') return 'â';
			if (c == 'ă' || c == 'Ă') return 'ă';
			if (c == 'ê' || c == 'Ê') return 'ê';
			if (c == 'ô' || c == 'Ô') return 'ô';
			if (c == 'ơ' || c == 'Ơ') return 'ơ';
			if (c == 'ư' || c == 'Ư') return 'ư';
			return c;
		}

		#endregion
	}
}
