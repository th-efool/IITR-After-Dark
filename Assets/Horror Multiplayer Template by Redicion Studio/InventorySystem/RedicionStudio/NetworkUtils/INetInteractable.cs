// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
namespace RedicionStudio.NetworkUtils {

	public interface INetInteractable<T> {

		void OnServerInteract(T player);
		void OnClientInteract(T player);
		string GetInfoText();
	}
}
