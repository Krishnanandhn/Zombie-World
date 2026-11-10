using Project.Core;
using UnityEngine;

namespace Project.Game
{
    // Holds run-wide disposables and tears the static service layer down with the scene.
    public sealed class ZombieSurvivalRoot : MonoBehaviour
    {
        System.IDisposable _input;

        public void Own(System.IDisposable disposable) => _input = disposable;

        void OnDestroy()
        {
            _input?.Dispose();
            Services.Clear();
            EventBus.ClearAll();
            Time.timeScale = 1f;
            Cursor.visible = true;
        }
    }
}
