using System;
using System.Collections;
using UnityEngine;

namespace AppStructure
{
    public abstract class AppBootstrap : MonoBehaviour
    {
        [SerializeField] private bool _isPrepared;
        [SerializeField] private bool _isStarted;
        [SerializeField] private bool _isFinalized;
        
        private IEnumerator Start()
        {
            yield return StartCoroutine(PrepareBootstrapProcess(b => _isPrepared = b));
            if (_isPrepared)
                yield return StartCoroutine(StartBootstrapProcess(b => _isStarted = b));
            if (_isPrepared && _isStarted)
                yield return StartCoroutine(FinalizeBootstrapProcess(b => _isFinalized = b));
            
            if (_isPrepared && _isStarted && _isFinalized)
                EndSuccessfullyBootstrap();
            else
                EndErrorBootstrapWithErrors(_isPrepared, _isStarted, _isFinalized);
        }

        protected abstract IEnumerator PrepareBootstrapProcess(Action<bool> callback);
        protected abstract IEnumerator StartBootstrapProcess(Action<bool> callback);
        protected abstract IEnumerator FinalizeBootstrapProcess(Action<bool> callback);

        protected virtual void EndSuccessfullyBootstrap()
        {
            Debug.Log($"{nameof(EndSuccessfullyBootstrap)}");
        }

        protected virtual void EndErrorBootstrapWithErrors(bool isPrepared, bool isStarted, bool isFinalized)
        {
            Debug.Log($"{nameof(EndErrorBootstrapWithErrors)}:{nameof(isPrepared)}:{isPrepared} {nameof(isStarted)}:{isStarted} {nameof(isFinalized)}:{isFinalized}");
        }
    }
}