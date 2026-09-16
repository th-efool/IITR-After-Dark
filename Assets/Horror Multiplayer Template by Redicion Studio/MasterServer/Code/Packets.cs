// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System;
using UnityEngine;
using LiteNetLib.Utils;
using System.Collections.Generic;

namespace RedicionStudio.MasterServer {

	public enum AuthRequestType : byte {
		AccountCreation,
		Authorization
	}

	public class AuthRequestPacket {

		public int ClientVersion { get; set; }
		public AuthRequestType Type { get; set; }
		public string Username { get; set; }
		public string Email { get; set; }
		public string EncryptedPassword { get; set; }
	}

	public class AuthResponsePacket {

		public byte Code { get; set; }
		public string Token { get; set; }
	}

	public class AccountDataRequestPacket {

		public string Token { get; set; }
	}

	public class AccountDataResponsePacket {

		public string Token { get; set; }
		public int Id { get; set; }
		public string Username { get; set; }
		public byte Status { get; set; }
		public int Funds { get; set; }
		public bool OwnsProperty { get; set; }
		public int Nutrition { get; set; }
        public int ExperiencePoints { get; set; }
        public int KillerId { get; set; }
        public int OutfitId { get; set; }
        public int Escaped { get; set; }
        public int KilledPlayers { get; set; }
        public int CapturedPlayers { get; set; }
        public int AbilitiesUsed { get; set; }
        public int HealedHealth { get; set; }
        public int DamageDealt { get; set; }
        public int CompletedTasks { get; set; }
        public int TimeSurvived { get; set; }
        public int HelpedPlayers { get; set; }
        public int InstrumentsUsed { get; set; }

    }

	#region Server Info & List

	[Serializable]
	public class InstanceInfo {

		public string uniqueName;
		public int numberOfPlayers;
		public int ping;
	}

	public class GetInstancesPacket { } // From Client

	public class InstancesPacket { // To Client

		public string JSON { get; set; }
	}

	#endregion

	#region Connection Info

	public class GetConnectionInfoPacket { // From Client


		public string InstanceUniqueName { get; set; }
	}

	public class ConnectionInfoPacket { // To Client

		public string Address { get; set; }
	}

	#endregion

	public class GetPlacedObjectsPacket {

		public int OwnerId { get; set; }
	}

	public class PlacedObjectsPacket {

		public int OwnerId { get; set; }
		public string JSON { get; set; }
	}

	public class SavePlacedObjectsPacket {

		public int OwnerId { get; set; }
		public string JSON { get; set; }
	}

	public class GetInventoryPacket {

		public int OwnerId { get; set; }
	}

	public class InventoryPacket {

		public int OwnerId { get; set; }
		public string JSON { get; set; }
	}

	public class SaveInventoryPacket {

		public int OwnerId { get; set; }
		public string JSON { get; set; }
	}
}
