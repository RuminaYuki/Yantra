using UnityEngine;
public class MoveAnimator
{
    private Animator _animator;

    private float _dampTime;
    private float _multiply;

    private int _moveZ; //Set parameter
    private int _moveX; //Set parameter

    public MoveAnimator(Animator animator, float dampTime = 0.25f, float multiply = 1)
    {
        _animator = animator;
        _dampTime = dampTime;
        _multiply = multiply;
    }

    //Set Parameter Method Overload
    public void SetParameter(string nameParameterMoveZ)
    {
        _moveZ = Animator.StringToHash(nameParameterMoveZ);
    }

    public void SetParameter(string nameParameterMoveX, string nameParameterMoveZ)
    {
        _moveX = Animator.StringToHash(nameParameterMoveX);
        _moveZ = Animator.StringToHash(nameParameterMoveZ);
    }
    //SetMove Method Overload
    public void SetMove(float velocityX, float velocityZ)
    {
        _animator.SetFloat(_moveX, velocityX * _multiply, _dampTime, Time.deltaTime);
        _animator.SetFloat(_moveZ, velocityZ * _multiply, _dampTime, Time.deltaTime);
    }
    public void SetMove(float velocityZ)
    {
        _animator.SetFloat(_moveZ, velocityZ * _multiply, _dampTime, Time.deltaTime);
    }

    #region Secondary API
    public float DampTime{get => _dampTime; set => _dampTime = value;}
    public float Multiply { get => _multiply; set => _multiply = value; }
    #endregion
    
}