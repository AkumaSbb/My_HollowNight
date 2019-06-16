using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PlayerMoveControl : MonoBehaviour
{
    #region PRIVATE PARA

    bool isAlive;//是否存活
    bool isGround;//是否在地面
    bool isClimb;   //是否在爬墙
    bool isJump;   //是否在跳跃
    Vector2 moveSpeed;  //每一帧的移动速度
    Animator animator;//动画控制器
    int jumpCount;  //跳跃次数
    float jumpTime; //跳跃时长

    #endregion

    #region PUBLIC PARA

    public bool inputEnable = true;   //接受输入的开关：true接受用户输入操作角色；false不接受输入
    public bool gravityEnable = true;
    public bool isCanSprint = true;   //是否能冲刺
    public float speed = 6.5f;  //移动速度
    public float gravity = 28f;  //收到的重力
    public float jumpSpeed = 2f; //跳跃速度
    public float jumpMaxTime = 0.11f;   //跳跃最大时长

    #endregion

    // 初始化参数
    void Start()
    {
        isAlive = true;
        isGround = true;
        isClimb = false;
        isJump = false;

        jumpCount = 0;
        jumpTime = 0;

        moveSpeed = new Vector2(0, 0);
		
    }

    // Update is called once per frame
    void Update()
    {
        if(!isAlive)
        {
            return; //若已死亡不做任何处理
        }

        LRMove();   //左右移动
        UpdatePlayerAnimatorState();
        UpdateGravity();
        Jump();
    }

    #region LRMove

    /// <summary>
    /// 左右移动
    /// </summary>
    void LRMove()
    {
        if(!inputEnable)
        {
            return;//不接受输入则返回
        }

        //根据输入设置移动速度
        if(Input.GetKey(InputManager.Instance.leftKey))
        {
            moveSpeed.x = -1 * speed;
        }
        else if(Input.GetKey(InputManager.Instance.rightKey))
        {
            moveSpeed.x = 1 * speed;
        }
        else
        {
            moveSpeed.x = 0;
        }

        if(moveSpeed.x == 0) //停止移动
        {
            animator.SetTrigger("setStop");
            animator.ResetTrigger("setRotate");
            animator.SetBool("IsMove", false);
        }
        else
        {
            animator.ResetTrigger("setStop");
            if (!isClimb) //如果没有在爬墙
            {
                if (transform.localScale.x * moveSpeed.x > 0)//转向
                {
                    if (isGround)
                    {
                        animator.SetTrigger("setRotate");
                    }

                    transform.localScale = new Vector3(transform.localScale.x * -1, 1, 1);
                }
                else
                {
                    animator.SetBool("IsMove", true);
                }
            }
        }

        PlayRunAudio(moveSpeed.x);
    }

    /// <summary>
    /// 播放行走音效
    /// </summary>
    /// <param name="h"></param>
    void PlayRunAudio(float h)
    {

    }

    #endregion

    #region UpdatePlayerAnimatorState

    /// <summary>
    /// 更新玩家的状态信息
    /// </summary>
    void UpdatePlayerAnimatorState()
    {
        if(isGround) //如果在地面上
        {
            animator.SetBool("IsJumpUp", false);
            animator.ResetTrigger("IsJumpTwo");//重置2段跳触发器
            jumpCount = 0;
            animator.SetBool("IsDown", false);
            isJump = false;
            isClimb = false;
            isCanSprint = true;
        }
        else
        {
            animator.SetBool("IsDown", !isJump);
            if(isClimb)
            {
                jumpCount = 0;
            }
        }
    }

    #endregion

    #region UpdateGravity

    /// <summary>
    /// 更新重力
    /// </summary>
    void UpdateGravity()
    {
        if(!gravityEnable)
        {
            return;
        }

        if(isGround)
        {
            moveSpeed.y = 0;
        }
        else
        {
            if(isClimb)
            {
                moveSpeed.y = -1.0f;
            }
            else
            {
                moveSpeed.y += -1 * gravity * Time.deltaTime;
            }
        }
    }

    #endregion

    #region Jump

    void Jump()
    {
        if(!inputEnable) //不接受输入
        {
            return;
        }
        
        if(isClimb && Input.GetKeyDown(InputManager.Instance.jumpKey)) //爬墙跳跃
        {
            StartCoroutine(ClimbToJump());
            return;
        }

        if(Input.GetKeyDown(InputManager.Instance.jumpKey)) //正常跳跃
        {
            isJump = true;

            if(!isGround && jumpCount < 2)
            {
                jumpCount = 2;
            }
            else
            {
                jumpCount++;
            }

            if(jumpCount == 1)
            {
                moveSpeed.y += jumpSpeed;
                animator.SetBool("isJumpUp", true);
            }
            else if(jumpCount == 2)
            {
                moveSpeed.y = jumpSpeed;
                animator.SetTrigger("setJumpTwice");
            }

            jumpTime = 0;
        }
        else if(Input.GetKey(InputManager.Instance.jumpKey) && isJump && jumpCount <= 2)
        {
            jumpTime += Time.deltaTime;
            if(jumpTime < jumpMaxTime)
            {
                moveSpeed.y += jumpSpeed;
            }
        }
        else if(Input.GetKeyUp(InputManager.Instance.jumpKey))
        {
            isJump = false;
            jumpTime = 0;
        }

        //进入上跳减速状态，但还在上升
        if (moveSpeed.y > 0 && moveSpeed.y < 1.5f)
        {
            animator.SetBool("IsSlowUp", true);
        }
        else
        {
            animator.SetBool("IsSlowUp", false);
        }

        //进入下落状态
        if (moveSpeed.y < 0)
        {

            animator.SetBool("IsJumpDown", true);
        }
        else
        {
            animator.SetBool("IsJumpDown", false);

        }
    }

    /// <summary>
    /// 墙上跳跃的移动
    /// </summary>
    /// <returns></returns>
    IEnumerator ClimbToJump()
    {
        inputEnable = false;    //此时不接受其余输入
        gravityEnable = false;
        isClimb = false;
        animator.SetTrigger("setClimbToJump");

        animator.ResetTrigger("setClimb");//重重爬墙触发器
        if(transform.localScale.x > 0 )
        {
            moveSpeed.x = 8f;
        }
        else
        {
            moveSpeed.x = -8f;
        }

        moveSpeed.y = 6;
        yield return new WaitForSeconds(0.15f);
        inputEnable = true;
        gravityEnable = true;

    }

    #endregion

    #region Attack

    void Attack()
    {
        if(!inputEnable || isClimb)
        {
            return;
        }
		
		if(Input.GetKeyDown(InputManager.Instance.attackKey))
		{
			if(Input.GetKey(InputManager.Instance.upKey))
            {
				animator.SetTrigger("setAttackUp");
            }
            else if (Input.GetKey(InputManager.Instance.downKey) && !isGround)
            {
				animator.SetTrigger("setAttackDown");
            }
            else
            {
				animator.SetTrigger("setAttackLR");
            }
        }
    }
	

    #endregion
}
