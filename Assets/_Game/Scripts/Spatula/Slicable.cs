using UnityEngine;
using ClocknestGames.Library.Utils;

namespace ClocknestGames.Game.Core
{
    public enum SlicableParticleType
    {
        Glass,
        Money,
        None
    }

    public class Slicable : MonoBehaviour
    {
        [SerializeField] protected Material _sliceMaterial;
        [SerializeField] protected Item _item;
        [SerializeField] protected Vector3 _pointLocalPos;
        [SerializeField] protected SlicableParticleType _particleType = SlicableParticleType.Glass;

        public Material SliceMaterial => _sliceMaterial;

        public virtual void OnSliced(Vector3 slicePosition)
        {
            // GUIManager.Instance.ShowItemUI(transform.position, (int)_item.Quantity);
            Vector3 pointPosition = transform.localToWorldMatrix.MultiplyPoint3x4(_pointLocalPos);
            GameplayController.Instance.ShowItemUI(pointPosition, (int)_item.Quantity);
            LevelManager.Instance.PickItem(new PickableItemEvent(_item));

            PoolableObject poolableObject = null;
            if (_particleType == SlicableParticleType.Glass)
                poolableObject = LevelSharedManager.Instance.GlassBreakParticlePooler.GetPooledGameObject();
            else if (_particleType == SlicableParticleType.Money)
                poolableObject = LevelSharedManager.Instance.MoneyBreakParticlePooler.GetPooledGameObject();

            if (poolableObject != null)
            {
                poolableObject.gameObject.SetActive(true);
                poolableObject.transform.position = slicePosition;
                poolableObject.GetComponent<ParticleSystem>().Play(true);
            }
        }

        public static Slicable GetFromCollider(Collider collider)
        {
            return collider.GetComponent<Slicable>();
        }

        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Vector3 position = transform.localToWorldMatrix.MultiplyPoint3x4(_pointLocalPos);
            Gizmos.DrawWireSphere(position, .5f);
        }
    }
}