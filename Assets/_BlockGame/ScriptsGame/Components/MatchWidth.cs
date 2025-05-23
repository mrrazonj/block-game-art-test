
using UnityEngine;

namespace ArtTest.Game
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class MatchWidth : MonoBehaviour {

        // Set this to the in-world distance between the left & right edges of your scene.
        public float SceneWidth = 10;

        Camera _camera;
        void Start() {
            _camera = GetComponent<Camera>();
        }

        void Update() {
        
            float unitsPerPixel = SceneWidth / Screen.width;
            float desiredHalfHeight = 0.5f * unitsPerPixel * Screen.height;

            if (desiredHalfHeight >= 12)
                _camera.orthographicSize = desiredHalfHeight;
            else
                _camera.orthographicSize = 12;

        }
    }
}