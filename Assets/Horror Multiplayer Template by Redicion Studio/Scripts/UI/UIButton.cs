// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/

namespace RedicionStudio
{
    public class UIButton : UIClickable
    {

        public System.Action onPressed;

        protected override void OnPressed()
        {
            onPressed.Invoke();
        }
    }
}
