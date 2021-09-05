using UnityEngine;

[ExecuteInEditMode]
public class CustomGrid : MonoBehaviour
{
    //[SerializeField] Transform _Target;
    //[SerializeField] Transform _Structure;
    [SerializeField] Vector3 _GridDistance = new Vector3(1, 1, 1);
    Vector3 _Position;

    void LateUpdate()
    {
        _Position.x = Mathf.Round(transform.position.x / _GridDistance.x) * _GridDistance.x;
        _Position.y = Mathf.Round(transform.position.y / _GridDistance.y) * _GridDistance.y;
        _Position.z = Mathf.Round(transform.position.z / _GridDistance.z) * _GridDistance.z;

        transform.position = _Position;
    }
}
