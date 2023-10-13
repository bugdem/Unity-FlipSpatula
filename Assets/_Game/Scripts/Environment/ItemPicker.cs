using UnityEngine;
using ClocknestGames.Library.Utils;

namespace ClocknestGames.Game.Core
{
    public class ItemPicker : MonoBehaviour
    {
        public LayerMask LayerToPick = -1;
        public Item ItemToPick;
        public ParticleSystem ParticleOnPick;
        public Vector3 ParticleScale = Vector3.one;
        public bool IsPoolableIfPossible = true;
        public Vector3 _pointLocalPos;

        private bool _isTriggeredBefore;

        private void OnTriggerEnter(Collider other)
        {
            if (_isTriggeredBefore)
                return;

            if (LayerToPick.Contains(other.gameObject.layer))
            {
                _isTriggeredBefore = true;

                HapticManager.Instance.HapticOnItemPicked();

                LevelManager.Instance.PickItem(new PickableItemEvent
                {
                    PickedItem = ItemToPick
                });

                if (IsPoolableIfPossible && ItemToPick.Type == ItemType.Gold)
                {
                    var particle = LevelSharedManager.Instance.GoldItemParticlePooler.GetPooledGameObject();
                    if (particle != null)
                    {
                        particle.transform.position = transform.position;
                        particle.transform.rotation = transform.rotation;
                        particle.transform.localScale = ParticleScale;
                        particle.gameObject.SetActive(true);
                        particle.Execute();
                    }
                }
                else if (ParticleOnPick != null)
                {
                    var particle = Instantiate(ParticleOnPick, transform.position, transform.rotation);
                    particle.transform.localScale = ParticleScale;
                }

                Vector3 pointPosition = transform.localToWorldMatrix.MultiplyPoint3x4(_pointLocalPos);
                GameplayController.Instance.ShowItemUI(pointPosition, (int)ItemToPick.Quantity);

                OnItemPicked();

                Destroy(this.gameObject);
            }
        }

        protected virtual void OnItemPicked() { }
    }
}
