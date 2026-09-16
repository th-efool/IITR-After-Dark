// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace RedicionStudio.MasterServer {

	public static class Tools {

		public static string TrimMultipleSpaces(string str) {
			return Regex.Replace(str.Trim(), @"\s+", " ");
		}

		private static bool IsValidStr(string str, int minLength, int maxLength, string regexPattern) {
			return str != null &&
				str.Length <= maxLength &&
				str.Length >= minLength &&
				Regex.IsMatch(str, regexPattern);
		}

		private static readonly string[] _commonEmailDomains = {
		"gmail.",
		"yahoo.",
		"hotmail.",
		"aol.",
		"msn.",
		"live.",
		"gmx.",
		"mail.",
		"yandex.",
		"outlook.",
		"bk.",
		"list.",
		"inbox.",
		"mac.",
		"rambler.",
		"ukr.",
		"internet.",
		"ya.",
		"googlemail.",
		"att.",
		"facebook." };
		public static bool IsValidEmail(string email) {
			if (IsValidStr(email, 6, 60, @"^[a-z0-9._\-]+@[a-z]+\.[a-z]+$")) {
				string domain = email.Split('@')[1];
				for (int i = 0; i < _commonEmailDomains.Length; i++) {
					if (domain.StartsWith(_commonEmailDomains[i])) {
						return true;
					}
				}
			}
			return false;
		}

		public static bool IsValidPassword(string password) {
			return IsValidStr(password, 6, 24, @"^[A-z0-9~!@#$%^&*()_+=]+$");
		}

		public static bool IsValidUsername(string username) {
			return IsValidStr(username, 4, 12, @"^[A-z0-9]+$");
		}

		public static bool IsValidRecoveryCode(string recoveryCode) {
			return IsValidStr(recoveryCode, 6, 6, @"^[A-Z0-9]+$");
		}

		public static string PBKDF2Hash(string str) {
			byte[] salt = Encoding.UTF8.GetBytes("I~l1ke~sma11~t1ts~and~I~can~n00t~l1e~!!!");
			Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(str, salt, 32);
			byte[] hash = pbkdf2.GetBytes(20);
			return BitConverter.ToString(hash).Replace("-", string.Empty);
		}

		private const string _recoveryCodeSourceStr = "23456789QWERTYUPASDFGHKZXCVBNM";
		public static string GenRecoveryCode() { // Example: F93H2S
			string result = string.Empty;
			for (int i = 0; i < 6; i++) {
				result += _recoveryCodeSourceStr[UnityEngine.Random.Range(0, _recoveryCodeSourceStr.Length)];
			}
			return result;
		}

		public static string GetToken() {
			string result = string.Empty;
			for (int i = 0; i < 32; i++) {
				result += _recoveryCodeSourceStr[UnityEngine.Random.Range(0, _recoveryCodeSourceStr.Length)];
			}
			return result;
		}

		public static void RemoveAll<TKey, TValue>(this IDictionary<TKey, TValue> dict,
			Func<TValue, bool> predicate) {
			List<TKey> keys = dict.Keys.Where(k => predicate(dict[k])).ToList();
			for (int i = 0; i < keys.Count; i++) {
				dict.Remove(keys[i]);
			}
		}
	}
}
