using UnityEngine;
using Unity.Entities;

namespace CellexalVR
{
    /// <summary>
    /// Spawns entities, in our case points, and converts to an entity.
    /// </summary>
    public class PrefabEntities : MonoBehaviour, IConvertGameObjectToEntity
    {
        public static Entity prefabEntity;

        public GameObject prefab;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            using (BlobAssetStore blobAssetStore = new BlobAssetStore())
            {
                Entity prefabEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab,
                    GameObjectConversionSettings.FromWorld(dstManager.World, blobAssetStore));
                PrefabEntities.prefabEntity = prefabEntity;
            }
        }
    }
}