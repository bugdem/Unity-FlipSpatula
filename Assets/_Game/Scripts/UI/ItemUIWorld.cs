using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using ClocknestGames.Library.Utils;

namespace ClocknestGames.Game.Core
{
    public class ItemUIWorld : MonoBehaviour
    {
        public TMPro.TextMeshPro Text;

        protected PoolableObject _poolableObject;
        protected Vector3 _scale;

        protected virtual void Awake()
        {
            _poolableObject = GetComponent<PoolableObject>();
            _scale = transform.localScale;
        }

        public void SetText(string text)
        {
            Text.SetText(text);
        }

        public void StartMoving()
        {
            transform.localScale = Vector3.zero;

            StartCoroutine(IStartMoving());
        }

        private IEnumerator IStartMoving()
        {
            transform.DOScale(_scale.x, .5f).SetEase(Ease.OutCubic);
            yield return new WaitForSeconds(.5f);

            // transform.DOShakeScale(.25f, .25f).SetEase(Ease.InElastic);
            transform.DOPunchScale(Vector3.one * .5f, .25f);

            yield return new WaitForSeconds(1f);

            transform.DOScale(0f, .5f).SetEase(Ease.InCubic);
            yield return new WaitForSeconds(.5f);

            _poolableObject.Destroy();
        }
    }
}