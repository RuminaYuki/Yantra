using System.Collections;
using UnityEngine;

public class TaniGhostDriver : MonoBehaviour
{
    private TaniMoveController3D moveController;
    private Animator animator;

    private void Awake()
    {
        moveController = GetComponent<TaniMoveController3D>();
        animator = GetComponent<Animator>();
    }

    public  IEnumerator TaniEventHandle()
    {
        Debug.Log("Start");

        moveController.StartMove(20f);
        animator.Play("Tani-Run"); 
        Debug.Log("Tani starts running!");

        yield return new WaitForSeconds(2f);

        Debug.Log("End");
        
    }

    
}