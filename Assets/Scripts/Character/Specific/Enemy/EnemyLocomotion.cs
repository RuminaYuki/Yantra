using UnityEngine;
[RequireComponent(typeof(CharacterController),typeof(Animator))]
public class EnemyLocomotion : MonoBehaviour
{
    private CharacterController _characterController;
    private Animator _animator;
    private Vector3 _directionMove;

    //================Locomotion Animatiton====================
    private LocomotionAnim locomotionAnim;

    [Header("Locomotion Animatiton Setting")]
    [SerializeField] private float _dampTime = 0.1f; //Initial

    [Header("Walk and Run Setting")]
    [SerializeField] private float _multiply; //Initial
    [SerializeField] private string _nameParameterMoveZ;

    //================Rotate Transform===============
    private RotationTransform _rotation;
    [Header("RotationSetting")]
    [SerializeField] private float _rotateSpeed = 1; //Initial

    //================Gravity====================
    private GravityCharacterCon _gravityCharacterCon;

    [Header("Gravity")]
    [SerializeField] private float gravityMultiplier = 1;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _characterController = GetComponent<CharacterController>();

        locomotionAnim = new(_animator,_dampTime,_multiply);
        locomotionAnim.SetParameter(_nameParameterMoveZ);
        _rotation = new(transform,_rotateSpeed);
        _gravityCharacterCon = new(_characterController,gravityMultiplier);

    }
    private void Update()
    {
        locomotionAnim.SetMove(transform.forward.z);
    }
    private void FixedUpdate()
    {
        _rotation.Rotate(transform.forward);
    }

    private void OnAnimatorMove()
    {
        _characterController.Move(_animator.deltaPosition + _gravityCharacterCon.Gravity());
    }
}
