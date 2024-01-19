using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class CarController : MonoBehaviour
{
    [SerializeField] private Vector3 modelOffset = new Vector3(0.5f, 0.7f, 0);

    [SerializeField] private Collider controlCollider;
    [SerializeField] private GameObject carModel;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform playerInputSpace;

    [SerializeField] private Vector3 inputDirection;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float maxAcceleration;
    [SerializeField] private float maxAirAcceleration;

    [SerializeField] private LayerMask probeMask;
    private Vector3 _velocity;
    private Vector3 _rightAxis, _forwardAxis;

    private Vector3 _contactNormal;

    private int _stepsSinceLastGrounded;
    private int _groundContactCount;
    private bool IsGrounded => _groundContactCount > 0;


    private void FixedUpdate()
    {
        Vector3 gravity = Vector3.down;
        UpdateState();
        AdjustVelocity();
        rb.velocity = _velocity;
    }

    private void UpdateState()
    {
        _stepsSinceLastGrounded += 1;
        _velocity = rb.velocity;

        if (IsGrounded || SnapToGround())
        {
            _stepsSinceLastGrounded = 0;

            if (_groundContactCount > 1)
            {
                _contactNormal.Normalize();
            }
        }
        else
        {
            _contactNormal = Vector3.up;
        }
    }

    private bool SnapToGround()
    {
        if (_stepsSinceLastGrounded > 1)
        {
            return false;
        }

        if (!Physics.Raycast(rb.position, -Vector3.up, out RaycastHit hit, 1, probeMask,
                QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        _groundContactCount = 1;
        //saving contacted ground's normal
        _contactNormal = hit.normal;

        float dot = Vector3.Dot(_velocity, hit.normal);
        if (dot > 0f)
        {
            _velocity = (_velocity - hit.normal * dot).normalized * _velocity.magnitude;
        }

        return true;
    }

    private void AdjustVelocity()
    {
        float acceleration, speed;
        Vector3 xAxis, zAxis;

        acceleration = IsGrounded ? maxAcceleration : maxAirAcceleration;
        speed = maxSpeed;
        xAxis = _rightAxis;
        zAxis = _forwardAxis;

        xAxis = ProjectOnContactPlane(xAxis, _contactNormal);
        zAxis = ProjectOnContactPlane(zAxis, _contactNormal);

        Vector3 relativeVelocity = _velocity;

        Vector3 adjustment;
        adjustment.x = inputDirection.x * speed - Vector3.Dot(relativeVelocity, xAxis);
        adjustment.y = 0f;
        adjustment.z = inputDirection.z * speed - Vector3.Dot(relativeVelocity, zAxis);

        adjustment = Vector3.ClampMagnitude(adjustment, acceleration * Time.deltaTime);
        _velocity += xAxis * adjustment.x + zAxis * adjustment.z;
    }

    private void Update()
    {
        HandleInput();

        UpdateCarModel();
    }

    private void UpdateCarModel()
    {
        Vector3 direction = rb.velocity == Vector3.zero ? carModel.transform.forward : rb.velocity;
        carModel.transform.rotation = Quaternion.LookRotation(direction);

        carModel.transform.position = controlCollider.transform.position - modelOffset;
    }

    private void HandleInput()
    {
        inputDirection.x = Input.GetAxis("Horizontal");
        inputDirection.z = Input.GetAxis("Vertical");

        _rightAxis = ProjectOnContactPlane(playerInputSpace.right, Vector3.up);
        _forwardAxis = ProjectOnContactPlane(playerInputSpace.forward, Vector3.up);
    }

    private Vector3 ProjectOnContactPlane(Vector3 vector, Vector3 normal)
    {
        return vector - _contactNormal * Vector3.Dot(vector, _contactNormal);
    }
}