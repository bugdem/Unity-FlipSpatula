using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClocknestGames.Game.Core
{
    public class BasketHoop : MonoBehaviour
    {
        [SerializeField] protected Item _item;
        [SerializeField] protected Vector3 _pointLocalPos;
        [SerializeField] protected Vector3 _particleLocalPos;
        [SerializeField] protected GameObject _itemGainParticlePrefab;

        public bool IsItemGained { get; protected set; }

        public virtual void OnTriggered(Collider collider)
        {
            // if (IsItemGained) return;

            IsItemGained = true;

            Vector3 pointPosition = transform.localToWorldMatrix.MultiplyPoint3x4(_pointLocalPos);
            GameplayController.Instance.ShowItemUI(pointPosition, (int)_item.Quantity);
            LevelManager.Instance.PickItem(new PickableItemEvent(_item));

            Vector3 particlePosition = transform.localToWorldMatrix.MultiplyPoint3x4(_particleLocalPos);
            var particle = Instantiate(_itemGainParticlePrefab, GameplayController.Instance.LevelContainer);
            particle.transform.position = particlePosition;

            HapticManager.Instance.HapticOnBasket();
        }

        public static BasketHoop GetFromCollider(Collider collider)
        {
            return collider.GetComponent<BasketHoop>();
        }

        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Vector3 position = transform.localToWorldMatrix.MultiplyPoint3x4(_pointLocalPos);
            Gizmos.DrawWireSphere(position, .5f);

            Gizmos.color = Color.yellow;
            Vector3 particlePosition = transform.localToWorldMatrix.MultiplyPoint3x4(_particleLocalPos);
            Gizmos.DrawWireSphere(particlePosition, .5f);
        }
    }
}