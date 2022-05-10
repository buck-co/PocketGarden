using UnityEngine;
using UnityEngine.Rendering;

namespace Buck
{
    /// <summary>
    /// Brute-force worldspace position & rotation matching.
    /// Works better with the AR camera pose than a parent constraint.
    /// Seems to work as well as being a child of the camera.
    /// </summary>
    public class TransformFollow : MonoBehaviour
    {
        [SerializeField] Transform _followTransform;

        private void Awake()
        {
            if (_followTransform == null) enabled = false;
        }

        private void Start()
        {
            RenderPipelineManager.beginCameraRendering += OnBeforeRender;
        }

        private void Update()
        {
            transform.position = _followTransform.position;
            transform.rotation = _followTransform.rotation;
        }

        private void FixedUpdate()
        {
            transform.position = _followTransform.position;
            transform.rotation = _followTransform.rotation;
        }

        private void LateUpdate()
        {
            transform.position = _followTransform.position;
            transform.rotation = _followTransform.rotation;
        }

        void OnBeforeRender(ScriptableRenderContext context, Camera camera)
        {
            transform.position = _followTransform.position;
            transform.rotation = _followTransform.rotation;
        }

        void OnDestroy()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeforeRender;
        }
    }
}