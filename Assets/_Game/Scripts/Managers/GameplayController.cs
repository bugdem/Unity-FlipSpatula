using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ClocknestGames.Library.Utils;
using DG.Tweening;
using Cinemachine;
using Dreamteck.Splines;
using ClocknestGames.Library.Editor;

namespace ClocknestGames.Game.Core
{
    [System.Serializable]
    public struct MinMaxValue
    {
        public float Min;
        public float Max;

        public MinMaxValue(float min, float max)
        {
            this.Min = min;
            this.Max = max;
        }
    }

    public class GameplayController : Singleton<GameplayController>, EventListener<LevelEvent>
    {
        public Player Player;
        public Transform LevelContainer;
        public Transform LevelCeiling;
        public SplineFollower Follower;
        public CinemachineVirtualCamera CMFollower;
        public List<FlipZone> FlipZones;

        [Header("Surface")]
        public SplineMesh SurfaceScrapePartPrefab;
        public SplineMesh SurfaceScrapePartHiddenPrefab;

        [Header("Confetti")]
        public float ConfettiStartDelay = 0f;
        public List<ParticleSystem> Confetties;

        public bool IsLevelFinishEntered { get; protected set; }
        public bool IsLevelStarted { get; protected set; }
        public int PointsScale { get; protected set; } = 1;

        public bool IsTouchable
        {
            get
            {
                return _isTouchable;
            }
            set
            {
                _isTouchable = value;
            }
        }
        private bool _isTouchable = true;


        public virtual void LevelFailed()
        {
            if (LevelManager.Instance.IsLevelCompleted)
                return;

            IsTouchable = false;

            CameraStopFollowing();

            HapticManager.Instance.HapticOnFail();

            LevelManager.Instance.LevelFailed();
        }

        public virtual void LevelSuccess(int scale)
        {
            if (LevelManager.Instance.IsLevelCompleted)
                return;

            IsTouchable = false;

            LevelManager.Instance.PickItem(new PickableItemEvent
            {
                PickedItem = new Item { Type = ItemType.Gold, Quantity = 10 }
            });

            PointsScale = scale;
            var currentGold = LevelManager.Instance.GetPickedItemCountThisLevel(ItemType.Gold);
            LevelManager.Instance.PickItem(new PickableItemEvent
            {
                PickedItem = new Item { Type = ItemType.Gold, Quantity = (uint) (PointsScale - 1) * currentGold}
            });

            HapticManager.Instance.HapticOnSuccess();

            LevelManager.Instance.LevelCompleted();
        }

        public virtual bool CanFlip(Vector3 point)
        {
            foreach (var flipZone in FlipZones)
            {
                if (!flipZone.CanFlip(point)) return false;
            }

            return true;
        }

        public virtual void ShowConfetties()
        {
            foreach (var confetti in Confetties)
            {
                StartCoroutine(IShowConfetties(confetti));
            }
        }

        protected virtual IEnumerator IShowConfetties(ParticleSystem particle)
        {
            yield return new WaitForSeconds(ConfettiStartDelay);

            particle.Play(true);
        }

        public virtual void CameraStopFollowing()
        {
            CMFollower.transform.SetParent(LevelContainer);

            CMFollower.Follow = null;
            CMFollower.LookAt = null;
        }

        public virtual void ShowItemUI(Vector3 worldPos, int quantity)
        {
            var pooledObj = LevelSharedManager.Instance.PointItemUIWorldPooler.GetPooledGameObject();
            if (pooledObj != null)
            {
                pooledObj.gameObject.SetActive(true);
                pooledObj.Execute();

                pooledObj.transform.position = worldPos;
                var itemUI = pooledObj.GetComponent<ItemUIWorld>();
                itemUI.SetText($"+ {quantity}");
                itemUI.StartMoving();
            }
        }

        public virtual void OnLevelFinishEntered(Collider collider)
        {
            OnLevelFinishEntered(collider, null);
        }

        public virtual void OnLevelFinishEntered(Collider collider, LevelFinish levelFinish)
        {
            var player = Player.GetFromCollider(collider);
            if (player != null)
            {
                IsLevelFinishEntered = true;

                player.OnLevelFinishEntered();
            }
        }

        public virtual void OnLevelBoundsEntered(Collider collider)
        {
            OnLevelBoundsEntered(collider, null);
        }

        public virtual void OnLevelBoundsEntered(Collider collider, LevelBound bound)
        {
            if (bound != null)
            {
                var boundCollider = bound.GetComponent<Collider>();
                if (boundCollider != null)
                    boundCollider.enabled = false;
            }

            Spatula.Instance.Die();
        }

        public void OnCGEvent(LevelEvent currentEvent)
        {
            if (currentEvent.EventType == LevelEventType.Started)
                IsLevelStarted = true;
        }

        protected virtual void OnEnable()
        {
            this.EventStartListening<LevelEvent>();
        }

        protected virtual void OnDisable()
        {
            this.EventStopListening<LevelEvent>();
        }

/*
#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (!Application.isPlaying)
            {
                if (CMFollower == null)
                {
                    CMFollower = GetComponentInChildren<CinemachineVirtualCamera>();
                    CGEditorFix.SetObjectDirty(CMFollower);
                }

                if (CMFollower != null)
                {
                    if (CMFollower.LookAt == null || CMFollower.Follow == null)
                    {
                        var spatula = FindObjectOfType<Spatula>();
                        if (spatula != null)
                        {
                            CMFollower.LookAt = spatula.transform;
                            CMFollower.Follow = spatula.transform;
                            CGEditorFix.SetObjectDirty(CMFollower);
                        }
                    }
                }
            }
        }
#endif
*/
    }
}