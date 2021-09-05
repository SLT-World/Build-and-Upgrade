using UnityEngine;

[ExecuteAlways]
public class Follow : MonoBehaviour
{
    [System.Serializable]
    public struct Vector3Bool
    {
        public bool x;
        public bool y;
        public bool z;
    }

    [Header("Position")]
    public Vector3Bool _PositionBool;
    public Vector3Bool _RotationBool;
    public Transform Target;
    [SerializeField] Vector3 _Offset;
    public bool DeleteIfNull;

    void Update()
    {
        if (Target != null)
        {
            Vector3 _Position = new Vector3(_PositionBool.x ? Target.position.x - transform.position.x : 0, _PositionBool.y ? Target.position.y - transform.position.y : 0, _PositionBool.z ? Target.position.z - transform.position.z : 0);
            /*Vector3 _Position = Vector3.zero;
            if (_PositionBool.x)
            {
                _Position.x = Target.position.x - transform.position.x;
            }
            if (_PositionBool.y)
            {
                _Position.y = Target.position.y - transform.position.y;
            }
            if (_PositionBool.z)
            {
                _Position.z = Target.position.z - transform.position.z;
            }*/
            transform.position = transform.position + _Position + _Offset;
        }
        else if (DeleteIfNull)
            Destroy(gameObject);
    }
}
