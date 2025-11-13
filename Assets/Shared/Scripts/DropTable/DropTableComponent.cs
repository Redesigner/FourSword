using UnityEngine;

namespace Shared.DropTable
{
    public class DropTableComponent : MonoBehaviour
    {
        [SerializeField] private DropTable dropTable;

        public void DropObject()
        {
            if (!dropTable)
            {
                Debug.LogWarningFormat("{0} : DropTableComponent - does not have a valid drop table set. Did you forget to assign one? ", gameObject.name);
                return;
            }

            var drop = dropTable.GetDrop();
            if (!drop)
            {
                Debug.LogWarningFormat("{0} : DropTableComponent - no valid drop found in drop table '{1}'", gameObject.name, dropTable.name);
                return;
            }

            Instantiate(drop, transform.position, Quaternion.identity);
        }
    }
}