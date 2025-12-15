using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Common.Utils
{
	public static class HashHelper
	{
		public static string ComputeSHA256(byte[] data) 
		{
			using var sha256 = SHA256.Create();
			var hashBytes = sha256.ComputeHash(data);
			return Convert.ToBase64String(hashBytes);
		}

		public static string ComputeSHA256(Stream stream) 
		{
			using var sha256 = SHA256.Create();
			var hashBytes = sha256.ComputeHash(stream);
			return Convert.ToBase64String(hashBytes);
		}

		public static string ComputeSHA256(string text) 
		{
			using var sha512 = SHA256.Create();
			var hashBytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(text));
			return Convert.ToBase64String(hashBytes);
		}

		public static bool CompareSHA256Hashes(string hash1, string hash2)
		{
			return string.Equals(hash1, hash2, StringComparison.Ordinal);
		}

		public static bool VerifySHA256(byte[] data, string expectedHash)
		{
			var calculatedHash = ComputeSHA256(data);
			return CompareSHA256Hashes(calculatedHash, expectedHash);
		}

		public static bool VerifySHA256(Stream stream, string expectedHash)
		{
			var calculatedHash = ComputeSHA256(stream);
			return CompareSHA256Hashes(calculatedHash, expectedHash);
		}

		public static bool VerifySHA256(string filePath, string expectedHash)
		{
			if (!File.Exists(filePath))
			{
				throw new FileNotFoundException($"File not found at path: {filePath}");
			}
			using (var stream = File.OpenRead(filePath))
			{
				var calculatedHash = ComputeSHA256(stream);
				return CompareSHA256Hashes(calculatedHash, expectedHash);
			}
		}
	}
}
