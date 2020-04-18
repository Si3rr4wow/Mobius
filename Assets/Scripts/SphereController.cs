using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SphereController : MonoBehaviour
{
  CharacterController characterController;
  Gamepad gamepad = Gamepad.current;

  public float gravity = 9.81f;
  public float frictionCoefficient = 0.03f;

  public float musterableHorizontalForce = 6.0f;
  public float musterableVerticleForce = 2000.0f;
  public float mass = 10f;

  private Vector3 lastPosition;
  private Vector3 lastVelocity = new Vector3(0,0,0);
  private Vector3 surfaceNormal;

  void Awake()
  {
    characterController = GetComponent<CharacterController>();
    lastPosition = characterController.center;
  }

  void FixedUpdate()
  {
    lastPosition = characterController.center;
    lastVelocity = characterController.velocity;

    gamepad = Gamepad.current;
    if(gamepad == null) return;

    characterController.Move(GetNextMove());
  }

  void OnControllerColliderHit(ControllerColliderHit hit)
  {
    surfaceNormal = hit.normal;
  }

  Vector3 GetNextMove()
  {
    Vector3 acceleration = WithForces(new Vector3(0, 0, 0));

    return GetNextPosition(acceleration) - lastPosition;
  }

  Vector3 GetNextVelocity(Vector3 acceleration)
  {
    return lastVelocity + acceleration * Time.deltaTime;
  }

  Vector3 GetNextPosition(Vector3 acceleration)
  {
    return 0.5f * (lastVelocity + GetNextVelocity(acceleration)) * Time.deltaTime;
  }

  Vector3 WithForces(Vector3 acceleration)
  {
    return WithFriction(WithMusteredHorizontal(WithJump(WithGravity(acceleration))));
  }

  Vector3 WithGravity(Vector3 acceleration)
  {
    int gravitationalCoefficient = lastVelocity.y < 0 ? 4 : 2;
    if(!characterController.isGrounded)
    {
        acceleration.y -= gravity * gravitationalCoefficient;
    } else {
      acceleration.x += surfaceNormal.x * gravity * gravitationalCoefficient;
      acceleration.y += surfaceNormal.y * gravity * gravitationalCoefficient;
      acceleration.z += surfaceNormal.z * gravity * gravitationalCoefficient;
    }
    return acceleration;
  }

  Vector3 WithFriction(Vector3 acceleration)
  {
    if(!characterController.isGrounded || lastVelocity.magnitude == 0)
    {
      return acceleration;
    }
    float normalForce = surfaceNormal.y * mass * gravity;
    Vector3 friction;
    friction.x = lastVelocity.x * frictionCoefficient * normalForce;
    friction.y = lastVelocity.y * frictionCoefficient * normalForce;
    friction.z = lastVelocity.z * frictionCoefficient * normalForce;

    Debug.Log(friction);

    acceleration.x -= friction.x;
    acceleration.y -= friction.y;
    acceleration.z -= friction.z;
    // acceleration -= friction;

    return acceleration;
  }

  Vector3 WithJump(Vector3 acceleration)
  {
    if(gamepad.buttonSouth.isPressed && characterController.isGrounded)
    {
      acceleration.y += musterableVerticleForce / mass;
    }
    return acceleration;
  }

  Vector3 WithMusteredHorizontal(Vector3 acceleration)
  {
    Vector2 leftStickInputVector = gamepad.leftStick.ReadValue().normalized;

    acceleration.x += leftStickInputVector.x * musterableHorizontalForce;
    acceleration.z += leftStickInputVector.y * musterableHorizontalForce;

    return acceleration;
  }
}
