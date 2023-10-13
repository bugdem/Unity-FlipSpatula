using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

namespace ClocknestGames.Game.Core
{
    public class ItemUI : MonoBehaviour
    {
        public CanvasGroup CanvasGroup;
        public TMPro.TextMeshProUGUI Text;

        public void SetText(string text)
        {
            Text.SetText(text);
        }

        public void StartMoving()
        {
            CanvasGroup.alpha = 1f;

            StartCoroutine(IStartMoving());
        }

        private IEnumerator IStartMoving()
        {
            var rTransform = GetComponent<RectTransform>();

            rTransform.DOMove(transform.position + new Vector3(0f, 50f/* Random.Range(50f, 100f)*/, 0f), .75f);

            yield return new WaitForSeconds(.75f);

            transform.GetComponent<RectTransform>().DOShakeScale(.15f, .15f).SetEase(Ease.InElastic);

            yield return new WaitForSeconds(.25f);
            CanvasGroup.DOFade(0f, .5f);

            Destroy(gameObject, 1f);
        }
    }
}