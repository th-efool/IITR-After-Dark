// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
#if UNITY_SERVER || UNITY_EDITOR // (Server)
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SQLite;

namespace RedicionStudio.MasterServer {

	[System.Serializable] // ?
	[Table("Accounts")]
	public class AccountData {
		[PrimaryKey, AutoIncrement]
		[Column("id")]
		public int Id { get; set; }
		[Column("email"), Collation("NOCASE")]
		public string Email { get; set; }
		[Column("encrypted_password")]
		public string EncryptedPassword { get; set; }
		[Column("username"), Collation("NOCASE")]
		public string Username { get; set; }
		[Column("status")]
		public byte Status { get; set; }
		[Column("online")]
		public bool Online { get; set; }
		[Column("funds")]
		public int Funds { get; set; }
		[Column("owns_property")]
		public bool OwnsProperty { get; set; }
		[Column("nutrition")]
		public int Nutrition { get; set; }
        [Column("experience_points")]
        public int ExperiencePoints { get; set; }
        [Column("killer_id")]
        public int KillerId { get; set; }
        [Column("outfit_id")]
        public int OutfitId { get; set; }
        [Column("escaped")]
        public int Escaped { get; set; }
        [Column("killedPlayers")]
        public int KilledPlayers { get; set; }
        [Column("capturedPlayers")]
        public int CapturedPlayers { get; set; }
        [Column("abilitiesUsed")]
        public int AbilitiesUsed { get; set; }
        [Column("healedHealth")]
        public int HealedHealth { get; set; }
        [Column("damageDealt")]
        public int DamageDealt { get; set; }
        [Column("completedTasks")]
        public int CompletedTasks { get; set; }
        [Column("timeSurvived")]
        public int TimeSurvived { get; set; }
        [Column("helpedPlayers")]
        public int HelpedPlayers { get; set; }
        [Column("instrumentsUsed")]
        public int InstrumentsUsed { get; set; }
    }

	[System.Serializable] // ?
	[Table("PlacedObjects")]
	public class PlacedObjectData {
		[PrimaryKey, AutoIncrement]
		[Column("id")]
		public int Id { get; set; }
		[Column("owner_id")]
		public int OwnerId { get; set; }
		[Column("unique_name")]
		public string UniqueName { get; set; }
		[Column("x")]
		public float X { get; set; }
		[Column("y")]
		public float Y { get; set; }
		[Column("z")]
		public float Z { get; set; }
		[Column("rot_x")]
		public float RotX { get; set; }
		[Column("rot_y")]
		public float RotY { get; set; }
		[Column("rot_z")]
		public float RotZ { get; set; }
		[Column("rot_w")]
		public float RotW { get; set; }
	}

	[System.Serializable] // ?
	[Table("Inventory")]
	public class InventoryData {

		[PrimaryKey, AutoIncrement]
		[Column("id")]
		public int Id { get; set; }
		[Column("owner_id")]
		public int OwnerId { get; set; }

		[Column("hash")]
		public int Hash { get; set; }

		[Column("amount")]
		public int Amount { get; set; }

		[Column("shelf_life")]
		public float ShelfLife { get; set; }
	}

	public static class Database {

		private static SQLiteConnection _connection;

		private const string _DatabaseFileName = "db.sqlite";

		public static void CloseConnection() {
			if (_connection != null) {
				_connection.Close();
				_connection = null;
			}
		}

		public static void OpenConnection() {
			CloseConnection(); // ?

#if UNITY_EDITOR
			string path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, _DatabaseFileName);
#else
			string path = Path.Combine(Application.dataPath, _DatabaseFileName);
#endif

			_connection = new SQLiteConnection(path);

			_connection.CreateTable<AccountData>();
			_connection.CreateTable<PlacedObjectData>();
			_connection.CreateTable<InventoryData>();
		}

		public static AccountData CreateAccount(string email, string encryptedPassword, string username) {
			if (_connection.FindWithQuery<AccountData>("SELECT 1 FROM Accounts WHERE email=? OR username=?", email, username) != null) {
				return null;
			}

            AccountData result = new AccountData {
                Email = email,
                EncryptedPassword = encryptedPassword,
                Username = username,
                Status = 0,
                Online = true, // ?
                Funds = 1000,
                OwnsProperty = false,
                Nutrition = 50,
                ExperiencePoints = 100,
                KillerId = 1,
                OutfitId = 1,
                Escaped = 0,
                KilledPlayers = 0,
                CapturedPlayers = 0,
                AbilitiesUsed = 0,
                HealedHealth = 0,
                DamageDealt = 0,
                CompletedTasks = 0,
                TimeSurvived = 0,
                HelpedPlayers = 0,
                InstrumentsUsed = 0
        };

			_ = _connection.Insert(result);

			return result;
		}

		public static AccountData GetAccountData(string email, string encryptedPassword) {
			return _connection.FindWithQuery<AccountData>("SELECT * FROM Accounts WHERE email=? AND encrypted_password=?", email, encryptedPassword);
		}

		public static void UpdateAccountData(int id, int funds, bool ownsProperty, int nutrition, int experiencePoints, int killerId, int outfitId, int escaped, int killedPlayers, int capturedPlayers, int abilitiesUsed, int healedHealth, int damageDealt, int completedTasks, int timeSurvived, int helpedPlayers, int instrumentsUsed) {
			_ = _connection.Execute("UPDATE Accounts SET funds=?, owns_property=?, nutrition=?, experience_points=?, killer_id=?, outfit_id=?, escaped=?, killedPlayers=?, capturedPlayers=?, abilitiesUsed=?, healedHealth=?, damageDealt=?, completedTasks=?, timeSurvived=?, helpedPlayers=?, instrumentsUsed=? WHERE id=?", funds, ownsProperty, nutrition, experiencePoints, killerId, outfitId, escaped, killedPlayers, capturedPlayers, abilitiesUsed, healedHealth, damageDealt, completedTasks, timeSurvived, helpedPlayers, instrumentsUsed, id);
		}

		public static PlacedObjectData[] GetPlacedObjects(int ownerId) {
			return _connection.Query<PlacedObjectData>("SELECT * FROM PlacedObjects WHERE owner_id=?", ownerId)?.ToArray();
		}

		public static void DeletePlacedObjects(int ownerId) {
			_ = _connection.Execute("DELETE FROM PlacedObjects WHERE owner_id=?", ownerId);
		}

		public static void SavePlacedObjects(PlacedObjectData[] placedObjects) {
			_connection.InsertAll(placedObjects);
		}

		public static InventoryData[] GetInventory(int ownerId) {
			return _connection.Query<InventoryData>("SELECT * FROM Inventory WHERE owner_id=?", ownerId)?.ToArray();
		}

		public static void DeleteInventory(int ownerId) {
			_ = _connection.Execute("DELETE FROM Inventory WHERE owner_id=?", ownerId);
		}

		public static void SaveInventory(InventoryData[] inventoryData) {
			_connection.InsertAll(inventoryData);
		}
	}
}
#endif
