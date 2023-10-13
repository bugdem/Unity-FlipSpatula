using UnityEngine;
using UnityEngine.Events;
using ClocknestGames.Library.Utils;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections;

namespace ClocknestGames.Game.Core
{
    [System.Serializable]
    public class LevelFinishPointEvent : UnityEvent<LevelFinishPoint> { }

    public class LevelFinishPoint : MonoBehaviour, EventListener<LevelEvent>
    {
        [SerializeField] protected int _scale = 1;
        [SerializeField] protected TMPro.TextMeshPro _text;
        [SerializeField] protected LevelFinishPointZone _colliderZone;
        [SerializeField] protected LevelFinishPointEvent _levelFinishTriggeredEvent;
        [SerializeField] protected float _confettiShowDelay;
        [SerializeField] protected float _confettiMoveTime;
        [SerializeField] protected Ease _confettiMoveEasing = Ease.OutCubic;
        [SerializeField] protected List<ParticleSystem> _confettiParticles;

        public int Scale => _scale;

        protected virtual void Start()
        {
            _colliderZone.Owner = this;
        }

        public virtual void OnTriggered(Collider collider)
        {
            _levelFinishTriggeredEvent?.Invoke(this);
        }

        public virtual void Register(UnityAction<LevelFinishPoint> call)
        {
            _levelFinishTriggeredEvent.AddListener(call);
        }

        public virtual void ShowConfetties()
        {
            StartCoroutine(IShowConfetties());
        }

        protected virtual IEnumerator IShowConfetties()
        {
            yield return new WaitForSeconds(_confettiShowDelay);

            foreach (var confetti in _confettiParticles)
            {
                confetti.gameObject.SetActive(true);
                confetti.transform.DOMove(confetti.transform.position + confetti.transform.up * 4f, _confettiMoveTime).SetEase(_confettiMoveEasing);
            }

            yield return new WaitForSeconds(_confettiMoveTime);

            foreach (var confetti in _confettiParticles)
            {
                confetti.Play(true);
            }
        }

        public void OnCGEvent(LevelEvent currentEvent)
        {
            switch (currentEvent.EventType)
            {
                case LevelEventType.Completed:
                case LevelEventType.Failed:
                    {
                        _text.DOFade(0f, .25f);
                    }
                    break;
            }
        }

        protected virtual void OnEnable()
        {
            this.EventStartListening<LevelEvent>();
        }

        protected virtual void OnDisable()
        {
            this.EventStopListening<LevelEvent>();
        }

        private void OnValidate()
        {
            if (_text != null)
                _text.SetText($"{_scale.ToString()} X");
        }


    }
}