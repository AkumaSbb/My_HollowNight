using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCtrlNormal : MonoBehaviour
{
    #region PRIVATE PARA

    Animator animator;
    Rigidbody2D rigidbody2d;
    bool isGround;  //是否在地面上
    bool isJump;    //是否在跳跃
    bool isClimb;    //是否在爬墙
    int jumpCount;  //跳跃次数
    float jumpTime; //跳跃键按下时长
    float jumpMaxTime;  //跳跃键按下的最大时长
    GameObject attackLR;
    GameObject attackUp;
    GameObject attackDown;

    #endregion

    #region PUBLIC PARA

    public float moveSpeed = 250;
    public float jumpSpeed = 50;

    #endregion 

    // Start is called before the first frame update
    void Start()
    {
        //初始化
        animator = GetComponent<Animator>();
        rigidbody2d = GetComponent<Rigidbody2D>();
        isGround = true;
        isJump = false;
        jumpCount = 0;
        jumpTime = 0;
        jumpMaxTime = 0.15f;
        attackLR = transform.Find("AttackLRImage").gameObject;
        attackUp = transform.Find("AttackUpImage").gameObject;
        attackDown = transform.Find("AttackDownImage").gameObject;
        attackLR.SetActive(false);
        attackUp.SetActive(false);
        attackDown.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        LRMove();
        SetParaByIsGround();
        Jump();
        Attack();
        Climb();
        if(Input.GetKeyDown(KeyCode.C))
        {
            
            StartCoroutine(sprint(0.15f));
            animator.SetTrigger("setSprint");
        }
    }

    private void Climb()
    {
        int playerLayerMask = LayerMask.GetMask("Player");
        playerLayerMask = ~playerLayerMask;//过滤掉Player层

        RaycastHit2D hit2d = Physics2D.Raycast(transform.position, Vector2.right, 0.5f, playerLayerMask);
        Debug.DrawLine(transform.position, transform.position + new Vector3(0.5f, 0,0));

        if (hit2d.collider != null && rigidbody2d.velocity.x > 0)
        {
            isClimb = true;
            animator.SetBool("isClimb", true);
        }
        else
        {
            isClimb = false;
            animator.SetBool("isClimb", false);
        }
    }

    IEnumerator sprint(float time)
    {
        rigidbody2d.gravityScale = 0;
        rigidbody2d.velocity = new Vector2(10f * transform.localScale.x * -1, 0);
        yield return new WaitForSeconds(time);
        rigidbody2d.velocity = new Vector2(0, 0);
        rigidbody2d.gravityScale = 1;
    }

    private void Attack()
    {
        if(Input.GetKeyDown(KeyCode.X))
        {
            if (Input.GetKey(KeyCode.UpArrow))    //  向上攻击
            {
                animator.SetTrigger("setAttackUp");
                StartCoroutine(attackImage(attackUp));
            }
            else if (Input.GetKey(KeyCode.DownArrow) && !isGround) //  向上攻击且不在地面
            {
                animator.SetTrigger("setAttackDown");
                StartCoroutine(attackImage(attackDown));
            }
            else
            {
                animator.SetTrigger("setAttackLR");
                StartCoroutine(attackImage(attackLR));
            }

        }
    }

    IEnumerator attackImage(GameObject gameObject )
    {
        gameObject.SetActive(true);
        for (int i = 0; i < 4; i++)
        {
            yield return null;
        }
        gameObject.SetActive(false);
    }

    //左右移动
    public void LRMove()
    {
        float moveMent = Input.GetAxis("Horizontal") * Time.deltaTime * moveSpeed;
        if (transform.localScale.x * moveMent > 0)
        {
            animator.SetTrigger("setRotate");
            transform.localScale = new Vector3(transform.localScale.x * -1, 1, 1);
        }

        if (moveMent != 0)
        {
            animator.ResetTrigger("setStop");
            animator.SetBool("isMove", true);
            //rigidbody2d.MovePosition(transform.position + transform.right * moveMent);
            rigidbody2d.velocity = new Vector2(moveMent, rigidbody2d.velocity.y);

            //transform.Translate(transform.right * moveMent);//碰到墙会抖动
        }
        else
        {
            animator.SetTrigger("setStop");
            animator.ResetTrigger("setRotate");
            animator.SetBool("isMove", false);
        }
    }

    public void SetParaByIsGround()
    {
        if (CheckIsGround())
        {
            isGround = true;
            animator.SetBool("isGround", true);
            jumpCount = 0;
            animator.SetBool("isJumpUp", false);
            animator.SetBool("isDown", false);
        }
        else
        {
            isGround = false;
            animator.SetBool("isGround", false);
            animator.SetBool("isDown", true);
        }
    }

    //跳跃
    public void Jump()
    {
        //按下Z键，开始跳跃计时
        if(Input.GetKeyDown(KeyCode.Z))
        {
            isJump = true;

            Debug.Log(isGround);
            //如果在空中，只能跳一次
            if(!isGround && jumpCount < 2)
            {
                jumpCount = 2;
            }
            else
            {
                jumpCount++;
            }
            Debug.Log(jumpCount);
            if (jumpCount == 1)
            {
                animator.SetBool("isJumpUp", true);
            }
            else if(jumpCount == 2)
            {
                animator.SetTrigger("setJumpTwice");
            }
        }

        if(Input.GetKey(KeyCode.Z) && jumpCount <= 2 && isJump)
        {
            jumpTime += Time.deltaTime;
            if(jumpTime < jumpMaxTime)
            {
                rigidbody2d.velocity = new Vector2(rigidbody2d.velocity.x, jumpTime * jumpSpeed);
            }
        }

        if(Input.GetKeyUp(KeyCode.Z))
        {
            isJump = false;
            jumpTime = 0;
        }

        if (rigidbody2d.velocity.y > 0 && rigidbody2d.velocity.y < 1.5f)
        {
            animator.SetBool("isSlowUp", true);
        }
        else
        {
            animator.SetBool("isSlowUp", false);
        }

        if (rigidbody2d.velocity.y < 0)
        {
            animator.SetBool("isJumpDown", true);
        }
        else
        {
            animator.SetBool("isJumpDown", false);
        }

    }

    /// <summary>
    /// 检测是否在地面
    /// </summary>
    /// <returns></returns>
    public bool CheckIsGround()
    {

        int playerLayerMask = LayerMask.GetMask("Player");
        playerLayerMask = ~playerLayerMask;//过滤掉Player层

        RaycastHit2D hit2d = Physics2D.Raycast(transform.position,  Vector2.down, 0.7f,playerLayerMask);
        Debug.DrawLine(transform.position, transform.position + new Vector3(0, -0.7f, 0));

        if (hit2d.collider != null)
        {
            return true;
        }

        return false;
    }

}
